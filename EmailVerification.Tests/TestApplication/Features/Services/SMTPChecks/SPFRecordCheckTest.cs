using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DnsClient;
using DnsClient.Protocol;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Application.Features.Services.SMTPChecks;
using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Models.Templates;
using Moq;
using NUnit.Framework;

namespace Integrate.EmailVerification.Tests.SMTPChecks
{
    [TestFixture]
    public class SPFRecordCheckTests
    {
        private Mock<IEmailValidationChecksInfoFactory> _factoryMock;
        private Mock<ILookupClient> _dnsClientMock;
        private SPFRecordCheck _spfCheck;

        private EmailValidationCheck _check;
        private RecordsTemplate _record;

        [SetUp]
        public void Setup()
        {
            _factoryMock = new Mock<IEmailValidationChecksInfoFactory>();
            _dnsClientMock = new Mock<ILookupClient>();

            _spfCheck = new SPFRecordCheck(_factoryMock.Object, _dnsClientMock.Object);

            _check = new EmailValidationCheck
            {
                Name = CheckNames.SpfRecord,
                AllotedScore = 10
            };

            _record = new RecordsTemplate(
                _userName: "john",
                _tLD: "com",
                _email: "john@example.com",
                _domain: "example.com",
                _parentDomain: "example.org",
                _mxRecords: new List<string> { "mx.example.com" }
            );

            _factoryMock.Setup(f =>
                f.Create(It.IsAny<EmailValidationCheck>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>())
            ).Returns((EmailValidationCheck chk, int score, bool passed, bool valid) =>
                new EmailValidationChecksInfo(chk)
                {
                    CheckName = chk.Name,
                    ObtainedScore = score,
                    Passed = passed,
                    Performed = true,
                    Email = _record.Email
                });
        }
        private IDnsQueryResponse CreateDnsResponseWithSpf(string spfValue)
        {
            var recordInfo = new ResourceRecordInfo(
                domainName: "example.com.",
                recordType: ResourceRecordType.TXT,
                recordClass: QueryClass.IN,
                timeToLive: 3600,
                rawDataLength: (ushort)spfValue.Length
            );

            var txtRecord = new TxtRecord(recordInfo, new[] { spfValue }, new[] { spfValue });

            var mockResponse = new Mock<IDnsQueryResponse>();
            mockResponse.Setup(r => r.Answers).Returns(new List<DnsResourceRecord> { txtRecord });

            return mockResponse.Object;
        }

        private IDnsQueryResponse CreateEmptyDnsResponse()
        {
            var mockResponse = new Mock<IDnsQueryResponse>();
            mockResponse.Setup(r => r.Answers).Returns(new List<DnsResourceRecord>());
            return mockResponse.Object;
        }


        [Test]
        public async Task EmailCheckValidator_ShouldReturnPassed_WhenBothDomainsHaveValidSpf()
        {
            var response = CreateDnsResponseWithSpf("v=spf1 a mx include:_spf.google.com ~all");

            _dnsClientMock.Setup(d =>
                d.QueryAsync(It.IsAny<string>(), QueryType.TXT, QueryClass.IN, It.IsAny<CancellationToken>())
            ).ReturnsAsync(response);

            _dnsClientMock.Setup(d =>
                d.QueryAsync(It.IsAny<string>(), (QueryType)99, QueryClass.IN, It.IsAny<CancellationToken>())
            ).ReturnsAsync(CreateEmptyDnsResponse());

            var result = await _spfCheck.EmailCheckValidator(_record, _check);

            Assert.That(result.Passed, Is.True);
            Assert.That(result.ObtainedScore, Is.EqualTo(10));
        }

        [Test]
        public async Task EmailCheckValidator_ShouldReduceScore_WhenOneDomainFailsSpf()
        {
            var passResponse = CreateDnsResponseWithSpf("v=spf1 a mx include:_spf.google.com ~all");
            var failResponse = CreateEmptyDnsResponse();

            _dnsClientMock.Setup(d =>
                d.QueryAsync("example.com", QueryType.TXT, QueryClass.IN, It.IsAny<CancellationToken>())
            ).ReturnsAsync(passResponse);

            _dnsClientMock.Setup(d =>
                d.QueryAsync("example.org", QueryType.TXT, QueryClass.IN, It.IsAny<CancellationToken>())
            ).ReturnsAsync(failResponse);

            _dnsClientMock.Setup(d =>
                d.QueryAsync(It.IsAny<string>(), (QueryType)99, QueryClass.IN, It.IsAny<CancellationToken>())
            ).ReturnsAsync(CreateEmptyDnsResponse());

            var result = await _spfCheck.EmailCheckValidator(_record, _check);

            Assert.That(result.ObtainedScore, Is.EqualTo(5));
        }

        [Test]
        public async Task EmailCheckValidator_ShouldReturnZero_WhenBothDomainsFailSpf()
        {
            var failResponse = CreateEmptyDnsResponse();

            _dnsClientMock.Setup(d =>
                d.QueryAsync(It.IsAny<string>(), QueryType.TXT, QueryClass.IN, It.IsAny<CancellationToken>())
            ).ReturnsAsync(failResponse);

            _dnsClientMock.Setup(d =>
                d.QueryAsync(It.IsAny<string>(), (QueryType)99, QueryClass.IN, It.IsAny<CancellationToken>())
            ).ReturnsAsync(failResponse);

            var result = await _spfCheck.EmailCheckValidator(_record, _check);

            Assert.That(result.ObtainedScore, Is.EqualTo(0));
        }

        [Test]
        public async Task CheckSPFAsync_ShouldReturnTrue_WhenStrongSpfPolicy()
        {
            var response = CreateDnsResponseWithSpf("v=spf1 a mx include:_spf.google.com -all");

            _dnsClientMock.Setup(d =>
                d.QueryAsync(It.IsAny<string>(), QueryType.TXT, QueryClass.IN, It.IsAny<CancellationToken>())
            ).ReturnsAsync(response);

            _dnsClientMock.Setup(d =>
                d.QueryAsync(It.IsAny<string>(), (QueryType)99, QueryClass.IN, It.IsAny<CancellationToken>())
            ).ReturnsAsync(CreateEmptyDnsResponse());

            var result = await _spfCheck.CheckSPFAsync("example.com");

            Assert.That(result, Is.True);
        }
    }
}
