using System.Net.Sockets;
using System.Text;
using DnsClient;
using Integrate.EmailVerification.Application.Features.Interfaces.SMTPChecks;
using Integrate.EmailVerification.Models.Templates;

namespace Integrate.EmailVerification.Application.Features.Services.SMTPChecks
{
    public class MXRecordChecker : IMXRecordChecker
    {

        public async Task<List<string>> GetMXRecordsAsync(string domain)
        {
            Dictionary<int, string> mxRecords = new();
            List<string> sortedMxRecords = new();

            try
            {
                Console.WriteLine($"[DNS] Resolving MX records for: {domain}");
                var client = new LookupClient();
                var queryResult = await client.QueryAsync(domain, QueryType.MX);

                foreach (var record in queryResult.Answers.MxRecords())
                {
                    Console.WriteLine($"[DNS] Found MX: {record.Exchange.Value}, Pref: {record.Preference}");
                    mxRecords[record.Preference] = record.Exchange.Value;
                }

                if (mxRecords.Count == 0)
                {
                    string[] parts = domain.Split('.');
                    if (parts.Length > 2)
                    {
                        string parentDomain = $"{parts[^2]}.{parts[^1]}";
                        Console.WriteLine($"[DNS] No MX. Retrying with parent domain: {parentDomain}");
                        queryResult = await client.QueryAsync(parentDomain, QueryType.MX);
                        foreach (var record in queryResult.Answers.MxRecords())
                        {
                            Console.WriteLine($"[DNS] Found Parent MX: {record.Exchange.Value}, Pref: {record.Preference}");
                            mxRecords[record.Preference] = record.Exchange.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DNS] Exception: {ex.Message}");
                return sortedMxRecords;
            }

            sortedMxRecords = mxRecords.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList();
            return sortedMxRecords;
        }

        public async Task<bool> HasMXRecords(string domain)
        {
            var mxRecords = await GetMXRecordsAsync(domain);
            return mxRecords.Count != 0;
        }

        public async Task<MxRecordsTemplate> GetParentDomain(string domain)
        {
            var mxRecords = await GetMXRecordsAsync(domain);
            var mxRecord = mxRecords?.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(mxRecord))
                return new();

            var host = mxRecord.TrimEnd('.').ToLower();
            var parts = host.Split('.');

            if (parts.Length < 2)
                return new();

            var ParentDomain = $"{parts[^2]}.{parts[^1]}";
            return new MxRecordsTemplate(ParentDomain, mxRecords);
        }

        public async Task<SMTPCheckDTO> CheckSingleMXAsync(string email, string domain, string mxRecord)
        {
            const int port = 25;
            var result = new SMTPCheckDTO { SmtpCheckValid = false };

            try
            {
                Console.WriteLine($"[SMTP] Connecting to {mxRecord}:{port}...");

                using var client = new TcpClient();
                await client.ConnectAsync(mxRecord, port);

                using var stream = client.GetStream();

                string response = await ReceiveResponseAsync(stream);
                Console.WriteLine($"[SMTP] 220 response: {response}");
                result.Code = response.Substring(0, 3);

                if (string.IsNullOrEmpty(response) || result.Code != "220")
                    return result;

                await SendCommandAsync(stream, "HELO verifier.com\r\n");
                response = await ReceiveResponseAsync(stream);
                Console.WriteLine($"[SMTP] HELO response: {response}");
                result.Code = response.Substring(0, 3);
                if (result.Code != "250") return result;

                await SendCommandAsync(stream, $"MAIL FROM:<test@verifier.com>\r\n");
                response = await ReceiveResponseAsync(stream);
                Console.WriteLine($"[SMTP] MAIL FROM response: {response}");
                result.Code = response.Substring(0, 3);
                if (result.Code != "250" && result.Code != "251") return result;

                await SendCommandAsync(stream, $"RCPT TO:<{email}>\r\n");
                response = await ReceiveResponseAsync(stream);
                Console.WriteLine($"[SMTP] RCPT TO response: {response}");
                result.Code = response.Substring(0, 3);

                await SendCommandAsync(stream, "QUIT\r\n");

                if (new[] { "250", "251", "252", "552" }.Contains(result.Code))
                {
                    Console.WriteLine($"[SMTP] Email is deliverable.");
                    result.SmtpCheckValid = true;
                }
                else if (result.Code == "550" || response.Contains("5.1.1") || response.ToLower().Contains("user unknown"))
                {
                    Console.WriteLine($"[SMTP] Email is undeliverable: {response}");
                    result.SmtpCheckValid = false;
                }
                else
                {
                    Console.WriteLine($"[SMTP] Unknown SMTP result: {response}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SMTP] Exception: {ex.Message}");
            }

            return result;
        }


        public async Task SendCommandAsync(Stream stream, string command)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(command);
            await stream.WriteAsync(buffer, 0, buffer.Length);
            await stream.FlushAsync();
        }

        public async Task<string> ReceiveResponseAsync(Stream stream)
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            return Encoding.ASCII.GetString(buffer, 0, bytesRead);
        }

        public async Task<bool> IsSMTPValid(string email, string domain, List<string> mxRecords)
        {
            if (mxRecords.Count > 0)
            {
                foreach (var mxRecord in mxRecords)
                {
                    Console.WriteLine($"[SMTP] Checking SMTP for {email} via MX: {mxRecord}");
                    var result = await CheckSingleMXAsync(email, domain, mxRecord);
                    if (string.IsNullOrEmpty(result.Code))
                    {
                        Console.WriteLine($"[SMTP] No response code for {mxRecord}");
                        continue;
                    }

                    Console.WriteLine($"[SMTP] Result code: {result.Code}, Valid: {result.SmtpCheckValid}");
                    return result.SmtpCheckValid;
                }
            }

            Console.WriteLine($"[SMTP] No MX records to check for {domain}");
            return false;
        }
    }
}
