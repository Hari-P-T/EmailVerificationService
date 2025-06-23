using System.Net.Sockets;
using System.Text;
using DnsClient;
using DnsClient.Protocol;
using Integrate.EmailVerification.Application.Features.Services.SMTPChecks;
using Integrate.EmailVerification.Models.Templates;
using Moq;
using NUnit.Framework;

namespace Integrate.EmailVerification.Tests.TestApplication.Features.Services.SMTPChecks
{
    public class MXRecordCheckerTests
    {
        private MXRecordChecker _checker;

        [SetUp]
        public void Setup()
        {
            _checker = new MXRecordChecker();
        }

        [Test]
        public async Task GetMXRecordsAsync_ReturnsRecords_WhenDomainHasMX()
        {
            var result = await _checker.GetMXRecordsAsync("gmail.com");
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.GreaterThan(0));
        }

        [Test]
        public async Task GetMXRecordsAsync_ReturnsEmpty_WhenDomainInvalid()
        {
            var result = await _checker.GetMXRecordsAsync("invalid-domain.abc");
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task HasMXRecords_ReturnsTrue_WhenRecordsExist()
        {
            var result = await _checker.HasMXRecords("gmail.com");
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task HasMXRecords_ReturnsFalse_WhenNoRecords()
        {
            var result = await _checker.HasMXRecords("invalid-domain.abc");
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task GetParentDomain_ReturnsParent_WhenRecordsFound()
        {
            var result = await _checker.GetParentDomain("gmail.com");
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ParentDomain, Is.Not.Null);
        }

        [Test]
        public async Task GetParentDomain_ReturnsEmpty_WhenNoRecords()
        {
            var result = await _checker.GetParentDomain("invalid-domain.abc");
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ParentDomain, Is.EqualTo(""));
        }

        [Test]
        public async Task SendAndReceiveCommand_Works()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream) { AutoFlush = true };
            var reader = new StreamReader(stream);
            var dummyMessage = "220 smtp.test.com ESMTP Postfix\r\n";
            var bytes = Encoding.ASCII.GetBytes(dummyMessage);
            stream.Write(bytes, 0, bytes.Length);
            stream.Position = 0;

            var response = await _checker.ReceiveResponseAsync(stream);
            Assert.That(response.StartsWith("220"), Is.True);
        }

        [Test]
        public async Task IsSMTPValid_ReturnsFalse_WhenNoValidMX()
        {
            var result = await _checker.IsSMTPValid("test@invalid-domain.abc", "invalid-domain.abc", new List<string>());
            Assert.That(result, Is.False);
        }


        [Test]
        public async Task SendCommandAsync_SendsCommandSuccessfully()
        {
            var stream = new MemoryStream();
            var command = "HELO test.com\r\n";

            await _checker.SendCommandAsync(stream, command);

            stream.Position = 0;
            var result = new StreamReader(stream).ReadToEnd();
            Assert.That(result, Is.EqualTo(command));
        }

        [Test]
        public async Task ReceiveResponseAsync_ReturnsResponse()
        {
            var stream = new MemoryStream();
            var responseMessage = "250 OK";
            var bytes = Encoding.ASCII.GetBytes(responseMessage);
            stream.Write(bytes, 0, bytes.Length);
            stream.Position = 0;

            var response = await _checker.ReceiveResponseAsync(stream);
            Assert.That(response, Is.EqualTo(responseMessage.TrimEnd('\r', '\n')));
        }

        [Test]
        public async Task GetMXRecordsAsync_ReturnsEmpty_WhenDomainIsEmpty()
        {
            var result = await _checker.GetMXRecordsAsync(string.Empty);
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task HasMXRecords_ReturnsFalse_WhenDomainIsEmpty()
        {
            var result = await _checker.HasMXRecords(string.Empty);
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task GetParentDomain_ReturnsEmpty_WhenDomainIsEmpty()
        {
            var result = await _checker.GetParentDomain(string.Empty);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ParentDomain, Is.EqualTo(string.Empty));
        }

        [Test]
        public async Task IsSMTPValid_ReturnsFalse_WhenEmailIsEmpty()
        {
            var result = await _checker.IsSMTPValid(string.Empty, "gmail.com", new List<string> { "mx.gmail.com" });
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task IsSMTPValid_ReturnsFalse_WhenDomainIsEmpty()
        {
            var result = await _checker.IsSMTPValid("test@gmail.com", string.Empty, new List<string> { "mx.gmail.com" });
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task IsSMTPValid_ReturnsFalse_WhenMXRecordsIsEmpty()
        {
            var result = await _checker.IsSMTPValid("test@gmail.com", "gmail.com", new List<string>());
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task SendCommandAsync_ThrowsException_WhenStreamIsDisposed()
        {
            var stream = new MemoryStream();
            stream.Dispose();
            Assert.ThrowsAsync<ObjectDisposedException>(async () => await _checker.SendCommandAsync(stream, "HELO test.com\r\n"));
        }

        [Test]
        public async Task ReceiveResponseAsync_ThrowsException_WhenStreamIsDisposed()
        {
            var stream = new MemoryStream();
            stream.Dispose();
            Assert.ThrowsAsync<ObjectDisposedException>(async () => await _checker.ReceiveResponseAsync(stream));
        }

        [Test]
        public async Task CheckSingleMXAsync_ReturnsFalse_WhenMXValidButBlocked()
        {
            var result = await _checker.CheckSingleMXAsync("test@gmail.com", "gmail.com", "gmail-smtp-in.l.google.com");
            Assert.That(result.SmtpCheckValid, Is.False); // Likely blocked or greylisted
        }

        [Test]
        public async Task CheckSingleMXAsync_ReturnsFalse_WhenMXInvalid()
        {
            var result = await _checker.CheckSingleMXAsync("test@invalid-domain.abc", "invalid-domain.abc", "invalid-mx.abc");
            Assert.That(result.SmtpCheckValid, Is.False);
        }

        [Test]
        public async Task CheckSingleMXAsync_ReturnsFalse_WhenEmailIsEmpty()
        {
            var result = await _checker.CheckSingleMXAsync(string.Empty, "gmail.com", "gmail-smtp-in.l.google.com");
            Assert.That(result.SmtpCheckValid, Is.False);
        }

        [Test]
        public async Task CheckSingleMXAsync_ReturnsFalse_WhenDomainIsEmpty()
        {
            var result = await _checker.CheckSingleMXAsync("test@gmail.com", string.Empty, "gmail-smtp-in.l.google.com");
            Assert.That(result.SmtpCheckValid, Is.False);
        }

        [Test]
        public async Task CheckSingleMXAsync_ReturnsFalse_WhenMXRecordIsEmpty()
        {
            var result = await _checker.CheckSingleMXAsync("test@gmail.com", "gmail.com", string.Empty);
            Assert.That(result.SmtpCheckValid, Is.False);
        }

    }
}