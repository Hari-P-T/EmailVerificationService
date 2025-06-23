using Integrate.EmailVerification.Api.Middlewares;
using Integrate.EmailVerification.Application.Features.EmailVerification;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Application.Features.Interfaces.SMTPChecks;
using Integrate.EmailVerification.Application.Features.Interfaces.Utility;
using Integrate.EmailVerification.Infrastructure.Redis;
using Integrate.EmailVerification.Infrastructure.Repositories;
using Integrate.EmailVerification.Models.Domains;
using Integrate.EmailVerification.Models.Enum;
using Integrate.EmailVerification.Models.Response;
using Integrate.EmailVerification.Models.Templates;
using Moq;

namespace Integrate.EmailVerification.Tests
{
    [TestFixture]
    public class EmailVerificationHandlerTests
    {
        private Mock<IEmailValidationFactory> _factoryMock;
        private Mock<IEmailHelper> _helperMock;
        private Mock<IMXRecordChecker> _mxCheckerMock;
        private Mock<IEmailValidationResultsRepository> _resultsRepoMock;
        private Mock<IEmailValidationChecksMappingRepository> _mappingRepoMock;
        private Mock<IValidationChecksRepository> _validationRepoMock;
        private Mock<IRedisCache> _redisCacheMock;
        private EmailVerificationHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _factoryMock = new Mock<IEmailValidationFactory>();
            _helperMock = new Mock<IEmailHelper>();
            _mxCheckerMock = new Mock<IMXRecordChecker>();
            _resultsRepoMock = new Mock<IEmailValidationResultsRepository>();
            _mappingRepoMock = new Mock<IEmailValidationChecksMappingRepository>();
            _validationRepoMock = new Mock<IValidationChecksRepository>();
            _redisCacheMock = new Mock<IRedisCache>();

            _handler = new EmailVerificationHandler(
                _factoryMock.Object,
                _helperMock.Object,
                _mxCheckerMock.Object,
                _resultsRepoMock.Object,
                _validationRepoMock.Object,
                _mappingRepoMock.Object,
                _redisCacheMock.Object
            );
        }

        [Test]
        public async Task ValidateEmail_HappyPath_ReturnsValidResponse()
        {
            var email = "test@example.com";
            var domain = "example.com";
            var tld = "com";
            var checkId = Guid.NewGuid();

            _helperMock.Setup(h => h.GetDomain(email)).Returns(domain);
            _helperMock.Setup(h => h.GetTLD(domain)).Returns(tld);
            _helperMock.Setup(h => h.GetUserName(email)).Returns("test");
            _helperMock.Setup(h => h.GetDnsStatus(It.IsAny<RecordsTemplate>())).ReturnsAsync(true);

            _mxCheckerMock.Setup(m => m.GetParentDomain(domain)).ReturnsAsync(
                new MxRecordsTemplate { ParentDomain = domain, mxRecords = new List<string> { "mx1.example.com" } });
            _mxCheckerMock.Setup(m => m.CheckSingleMXAsync(email, domain, It.IsAny<string>())).ReturnsAsync(new SMTPCheckDTO { Code = "OK" });

            var checks = new List<ValidationChecks>
            {
                new() { CheckId = checkId, CheckName = "ValidTopLevelDomain", Weightage = 10 }
            };

            _validationRepoMock.Setup(r => r.RetrieveAllValidationChecks()).ReturnsAsync(checks);

            var validatorMock = new Mock<IEmailValidationChecker>();
            validatorMock.Setup(v => v.EmailCheckValidator(It.IsAny<RecordsTemplate>(), It.IsAny<EmailValidationCheck>()))
                .ReturnsAsync(new EmailValidationChecksInfo(new EmailValidationCheck
                {
                    CheckId = checkId,
                    Name = "ValidTopLevelDomain",
                    AllotedScore = 10,
                    Passed = true,
                    Performed = true
                })
                { ObtainedScore = 100 });

            _factoryMock.Setup(f => f.GetValidator("ValidTopLevelDomain")).Returns(validatorMock.Object);
            _resultsRepoMock.Setup(r => r.CreateEmailValidationResults(It.IsAny<List<EmailValidationResults>>())).ReturnsAsync(true);
            _mappingRepoMock.Setup(m => m.AddEmailValidationCheckMapping(It.IsAny<List<EmailValidationCheckMappings>>())).ReturnsAsync(true);
            _redisCacheMock.Setup(c => c.IsResponseInCache(email)).ReturnsAsync(false);
            _redisCacheMock.Setup(c => c.PutResponseIntoCache(It.IsAny<EmailVerificationResponse>())).ReturnsAsync(true);

            var response = await _handler.ValidateEmail(new EmailValidationInfo
            {
                Email = email,
                Strictness = EStrictness.Basic,
                CreatedBy = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                RequestId = Guid.NewGuid()
            });

            Assert.That(response.Score, Is.EqualTo(100));
            Assert.That(response.Status, Is.EqualTo(EmailValidationStatus.Valid.ToString()));
            Assert.That(response.CheckResult, Has.Count.EqualTo(1));
        }

        [Test]
        public void ValidateEmail_NoValidationChecks_ThrowsNullException()
        {
            _validationRepoMock.Setup(r => r.RetrieveAllValidationChecks()).ReturnsAsync(new List<ValidationChecks>());

            var exception = Assert.ThrowsAsync<NullFoundException>(async () =>
                await _handler.ValidateEmail(new EmailValidationInfo { Email = "test@example.com", Strictness = EStrictness.Basic }));

            Assert.That(exception.Message, Does.Contain("No validation checks"));
        }

        [Test]
        public async Task ValidateEmail_LowScore_IsInvalid()
        {
            var email = "test@example.com";
            var domain = "example.com";
            var tld = "com";
            var checkId = Guid.NewGuid();

            _helperMock.Setup(h => h.GetDomain(email)).Returns(domain);
            _helperMock.Setup(h => h.GetTLD(domain)).Returns(tld);
            _helperMock.Setup(h => h.GetUserName(email)).Returns("test");
            _helperMock.Setup(h => h.GetDnsStatus(It.IsAny<RecordsTemplate>())).ReturnsAsync(true);

            _mxCheckerMock.Setup(m => m.GetParentDomain(domain)).ReturnsAsync(new MxRecordsTemplate { ParentDomain = domain, mxRecords = new List<string> { "mx1.example.com" } });
            _mxCheckerMock.Setup(m => m.CheckSingleMXAsync(email, domain, It.IsAny<string>())).ReturnsAsync(new SMTPCheckDTO { Code = "OK" });

            var checks = new List<ValidationChecks> { new() { CheckId = checkId, CheckName = "ValidTopLevelDomain", Weightage = 10 } };
            _validationRepoMock.Setup(r => r.RetrieveAllValidationChecks()).ReturnsAsync(checks);

            var validatorMock = new Mock<IEmailValidationChecker>();
            validatorMock.Setup(v => v.EmailCheckValidator(It.IsAny<RecordsTemplate>(), It.IsAny<EmailValidationCheck>()))
                .ReturnsAsync(new EmailValidationChecksInfo(new EmailValidationCheck
                {
                    CheckId = checkId,
                    Name = "ValidTopLevelDomain",
                    AllotedScore = 10,
                    Passed = false,
                    Performed = true
                })
                { ObtainedScore = 0 });

            _factoryMock.Setup(f => f.GetValidator("ValidTopLevelDomain")).Returns(validatorMock.Object);
            _resultsRepoMock.Setup(r => r.CreateEmailValidationResults(It.IsAny<List<EmailValidationResults>>())).ReturnsAsync(true);
            _mappingRepoMock.Setup(m => m.AddEmailValidationCheckMapping(It.IsAny<List<EmailValidationCheckMappings>>())).ReturnsAsync(true);
            _redisCacheMock.Setup(c => c.IsResponseInCache(email)).ReturnsAsync(false);
            _redisCacheMock.Setup(c => c.PutResponseIntoCache(It.IsAny<EmailVerificationResponse>())).ReturnsAsync(true);

            var result = await _handler.ValidateEmail(new EmailValidationInfo
            {
                Email = email,
                Strictness = EStrictness.Basic,
                CreatedBy = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                RequestId = Guid.NewGuid()
            });

            Assert.That(result.Status, Is.EqualTo(EmailValidationStatus.Invalid.ToString()));
            Assert.That(result.Score, Is.EqualTo(0));
        }
    }
}
