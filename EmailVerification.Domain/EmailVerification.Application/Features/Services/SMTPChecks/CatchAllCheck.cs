using System.Net.Sockets;
using System.Text;
using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Application.Features.Interfaces.SMTPChecks;
using Integrate.EmailVerification.Models.Templates;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Integrate.EmailVerification.Application.Features.Services.SMTPChecks
{
    [ExcludeFromCodeCoverage]
    public class CatchAllCheck : IEmailValidationChecker
    {
        private readonly IEmailValidationChecksInfoFactory _emailValidationChecksInfoFactory;
        private readonly IMXRecordChecker _mXRecordChecker;
        private static readonly Random _random = new Random();
        private const string _chars = "abcdefghijklmnopqrstuvwxyz0123456789";

        public CatchAllCheck(IEmailValidationChecksInfoFactory emailValidationChecksInfoFactory, IMXRecordChecker mXRecordChecker)
        {
            _emailValidationChecksInfoFactory = emailValidationChecksInfoFactory;
            _mXRecordChecker = mXRecordChecker;
        }

        public string Name => CheckNames.CatchAll;

        public static string GenerateRandomString(int length = 8)
        {
            char[] buffer = new char[length];
            for (int i = 0; i < length; i++)
            {
                buffer[i] = _chars[_random.Next(_chars.Length)];
            }
            return new string(buffer);
        }
        public async Task<EmailValidationChecksInfo> EmailCheckValidator(RecordsTemplate records, EmailValidationCheck Check)
        {
            int score = Check.AllotedScore;
            bool passed = true;
            bool valid = true;
            if (records == null || records.mxRecords == null || records.mxRecords.Count == 0)
            {
                passed = false;
                valid = true;
                score = 0;
            }
            else
            {
                string Domain = records.Domain;
                var mxHost = records.mxRecords;
                if (!(string.IsNullOrEmpty(Domain) && string.IsNullOrEmpty(mxHost[0])))
                {
                    passed = await IsCatchAllAsync(Domain, mxHost[0]) ?? true;
                }
                else
                {
                    passed = false;
                }

                score = passed ? 0 : score;
            }
            var response = _emailValidationChecksInfoFactory.Create(Check, score, passed, valid);

            return response;
        }
        public virtual async Task<bool?> IsCatchAllAsync(string domain, string mxHost)
        {
            string testEmail = GenerateRandomString() + "@" + domain;

            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(mxHost, 25);
            if (await Task.WhenAny(connectTask, Task.Delay(5000)) != connectTask)
                throw new TimeoutException("Timeout while connecting to SMTP server.");

            try
            {
                using var stream = client.GetStream();
                using var reader = new StreamReader(stream, Encoding.ASCII);
                using var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

                async Task<string> ReadResponseAsync(int timeoutMs = 5000)
                {
                    var readTask = reader.ReadLineAsync();
                    if (await Task.WhenAny(readTask, Task.Delay(timeoutMs)) != readTask)
                        throw new TimeoutException("Timeout waiting for SMTP server response.");
                    return readTask.Result ?? string.Empty;
                }

                async Task SendCommandAsync(string command)
                {
                    await writer.WriteLineAsync(command);
                }

                string rcpResponse = await ReadResponseAsync();

                // HELO handshake  
                await SendCommandAsync($"HELO {domain}");
                string heloResponse = await ReadResponseAsync();

                // MAIL FROM is required by many servers before RCPT TO  
                await SendCommandAsync($"MAIL FROM:<{testEmail}>");
                string mailFromResponse = await ReadResponseAsync();

                // RCPT TO check  
                await SendCommandAsync($"RCPT TO:<{testEmail}>");
                string rcptResponse = await ReadResponseAsync();

                // Determine response code  
                string code = rcptResponse.Length >= 3 ? rcptResponse.Substring(0, 3) : "";

                List<string> acceptable = ["250", "251", "252"];

                return acceptable.Contains(code);
            }
            catch (Exception ex)
            {
                throw new Exception($"SMTP error: {ex.Message}");
            }
        }

    }
}