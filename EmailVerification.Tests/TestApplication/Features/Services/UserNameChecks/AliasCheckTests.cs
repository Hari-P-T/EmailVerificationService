using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using StackExchange.Redis;
using Integrate.EmailVerification.Application.Features.Services.UserNameChecks;
using Integrate.EmailVerification.Application.Features.Interfaces.Utility;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Infrastructure.Redis;
using Integrate.EmailVerification.Models.Templates;
using Integrate.EmailVerification.Models.Domains;
using Integrate.EmailVerification.Infrastructure.Constant;

namespace Integrate.EmailVerification.Tests.UserNameChecks
{
    [TestFixture]
    public class AliasCheckTests
    {
        private Mock<IConnectionMultiplexer> _redisMock;
        private Mock<IDatabase> _dbMock;
        private Mock<IEmailHelper> _emailHelperMock;
        private Mock<IRedisSeeder> _redisSeederMock;
        private Mock<IEmailValidationChecksInfoFactory> _factoryMock;
        private AliasCheck _aliasCheck;

        [SetUp]
        public void Setup()
        {
            _redisMock = new Mock<IConnectionMultiplexer>();
            _dbMock = new Mock<IDatabase>();
            _emailHelperMock = new Mock<IEmailHelper>();
            _redisSeederMock = new Mock<IRedisSeeder>();
            _factoryMock = new Mock<IEmailValidationChecksInfoFactory>();

            _redisMock.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_dbMock.Object);

            _aliasCheck = new AliasCheck(
                _redisMock.Object,
                _emailHelperMock.Object,
                _redisSeederMock.Object,
                _factoryMock.Object
            );
        }

        private RecordsTemplate GetRecordsTemplate(string email, string username)
        {
            return new RecordsTemplate(
                _userName: username,
                _tLD: "com",
                _email: email,
                _domain: "example.com",
                _parentDomain: "example.com",
                _mxRecords: new List<string> { "mx1.example.com" }
            );
        }

        [Test]
        public async Task EmailCheckValidator_AliasFoundInRedis_ReturnsPassedResult()
        {
            var check = new EmailValidationCheck { AllotedScore = 10, Name = CheckNames.Alias };
            var records = GetRecordsTemplate("alias@example.com", "alias");

            _emailHelperMock.Setup(x => x.GetUserName(records.Email)).Returns(records.UserName);
            _dbMock.Setup(x => x.KeyExistsAsync(ConstantKeys.AliasNames, CommandFlags.None)).ReturnsAsync(true);
            _dbMock.Setup(x => x.SetContainsAsync(ConstantKeys.AliasNames, records.UserName, CommandFlags.None)).ReturnsAsync(true);

            _factoryMock.Setup(x => x.Create(check, 10, true, true)).Returns(new EmailValidationChecksInfo(check));

            var result = await _aliasCheck.EmailCheckValidator(records, check);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.CheckName, Is.EqualTo(CheckNames.Alias));
            Assert.That(result.ObtainedScore, Is.EqualTo(10));
            Assert.That(result.Passed, Is.False);
        }

        [Test]
        public async Task EmailCheckValidator_AliasNotFoundInRedis_ReturnsFailedResult()
        {
            var check = new EmailValidationCheck { AllotedScore = 10, Name = CheckNames.Alias };
            var records = GetRecordsTemplate("nonalias@example.com", "nonalias");

            _emailHelperMock.Setup(x => x.GetUserName(records.Email)).Returns(records.UserName);
            _dbMock.Setup(x => x.KeyExistsAsync(ConstantKeys.AliasNames, CommandFlags.None)).ReturnsAsync(true);
            _dbMock.Setup(x => x.SetContainsAsync(ConstantKeys.AliasNames, records.UserName, CommandFlags.None)).ReturnsAsync(false);

            _factoryMock.Setup(x => x.Create(check, 0, false, true)).Returns(new EmailValidationChecksInfo(check));

            var result = await _aliasCheck.EmailCheckValidator(records, check);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ObtainedScore, Is.EqualTo(10));
            Assert.That(result.Passed, Is.False);
        }

        [Test]
        public async Task EmailCheckValidator_RedisKeyMissing_SeedsRedis_ThenChecks()
        {
            var check = new EmailValidationCheck { AllotedScore = 10, Name = CheckNames.Alias };
            var records = GetRecordsTemplate("seed@example.com", "seed");

            _emailHelperMock.Setup(x => x.GetUserName(records.Email)).Returns(records.UserName);
            _dbMock.Setup(x => x.KeyExistsAsync(ConstantKeys.AliasNames, CommandFlags.None)).ReturnsAsync(false);
            _redisSeederMock.Setup(x => x.SeedAsync(ConstantKeys.AliasNames)).Returns(Task.CompletedTask);
            _dbMock.Setup(x => x.SetContainsAsync(ConstantKeys.AliasNames, records.UserName, CommandFlags.None)).ReturnsAsync(true);

            _factoryMock.Setup(x => x.Create(check, 10, true, true)).Returns(new EmailValidationChecksInfo(check));

            var result = await _aliasCheck.EmailCheckValidator(records, check);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ObtainedScore, Is.EqualTo(10));
            Assert.That(result.Passed, Is.False);
            _redisSeederMock.Verify(x => x.SeedAsync(ConstantKeys.AliasNames), Times.Once);
        }
    }
}
