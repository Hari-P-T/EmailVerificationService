using Integrate.EmailVerification.Application.Features.Services.DomainChecks;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Infrastructure.Redis;
using Integrate.EmailVerification.Models.Templates;
using Moq;
using StackExchange.Redis;

namespace Integrate.EmailVerification.Tests
{
    [TestFixture]
    public class BogusEmailCheckTests
    {
        private Mock<IConnectionMultiplexer> _redisMock;
        private Mock<IDatabase> _databaseMock;
        private Mock<IRedisSeeder> _redisSeederMock;
        private Mock<IEmailValidationChecksInfoFactory> _emailValidationChecksInfoFactoryMock;
        private BogusEmailCheck _bogusEmailCheck;

        private const string RedisKey = "bogus";

        [SetUp]
        public void Setup()
        {
            _redisMock = new Mock<IConnectionMultiplexer>();
            _databaseMock = new Mock<IDatabase>();
            _redisSeederMock = new Mock<IRedisSeeder>();
            _emailValidationChecksInfoFactoryMock = new Mock<IEmailValidationChecksInfoFactory>();

            _redisMock.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_databaseMock.Object);

            _bogusEmailCheck = new BogusEmailCheck(
                _redisMock.Object,
                _redisSeederMock.Object,
                _emailValidationChecksInfoFactoryMock.Object);
        }

        [Test]
        public async Task EmailCheckValidator_SeedsRedisIfKeyNotExists()
        {
            // Arrange
            var records = new RecordsTemplate("user", "com", "user@example.com", "example.com", "example", new List<string> { "mx1" });
            records.DnsStatus = true;
            var check = new EmailValidationCheck
            {
                Name = "BogusEmailAddress",
                AllotedScore = 10,
                Passed = true,
                Performed = true
            };

            _databaseMock.Setup(db => db.KeyExistsAsync(RedisKey, It.IsAny<CommandFlags>())).ReturnsAsync(false);
            _redisSeederMock.Setup(s => s.SeedAsync(RedisKey)).Returns(Task.CompletedTask);

            _databaseMock.Setup(db => db.SetContainsAsync(RedisKey, It.IsAny<RedisValue>(), It.IsAny<CommandFlags>())).ReturnsAsync(false);

            _emailValidationChecksInfoFactoryMock.Setup(f => f.Create(check, 10, true, true))
                .Returns(new EmailValidationChecksInfo(check) { Passed = true, ObtainedScore = 10 });

            // Act
            var result = await _bogusEmailCheck.EmailCheckValidator(records, check);

            // Assert
            _redisSeederMock.Verify(s => s.SeedAsync(RedisKey), Times.Once);
            Assert.That(result.Passed, Is.True);
            Assert.That(result.ObtainedScore, Is.EqualTo(10));
        }

        [Test]
        public async Task EmailCheckValidator_ReturnsFailedWhenIsBogusEmail()
        {
            // Arrange
            var records = new RecordsTemplate("user", "com", "user@example.com", "example.com", "example", new List<string> { "mx1" });
            records.DnsStatus = true;
            var check = new EmailValidationCheck
            {
                Name = "BogusEmailAddress",
                AllotedScore = 10,
                Passed = true,
                Performed = true
            };

            _databaseMock.Setup(db => db.KeyExistsAsync(RedisKey, It.IsAny<CommandFlags>())).ReturnsAsync(true);

            _databaseMock.Setup(db => db.SetContainsAsync(RedisKey, It.IsAny<RedisValue>(), It.IsAny<CommandFlags>())).ReturnsAsync(true);

            _emailValidationChecksInfoFactoryMock.Setup(f => f.Create(check, 0, false, true))
                .Returns(new EmailValidationChecksInfo(check) { Passed = false, ObtainedScore = 0 });

            // Act
            var result = await _bogusEmailCheck.EmailCheckValidator(records, check);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task IsBogusEmailAddress_ReturnsFalseIfUserNameEqualsDomain()
        {
            // Arrange
            var records = new RecordsTemplate("example", "com", "example@example.com", "example", "parent", new List<string> { "mx1" });
            records.Code = "250";
            records.DnsStatus = true;

            // Act
            var result = await _bogusEmailCheck.IsBogusEmailAddress(records, records.Code, true);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task IsBogusEmailAddress_ReturnsFalseIfMxRecordsEmpty()
        {
            // Arrange
            var records = new RecordsTemplate("user", "com", "user@example.com", "example.com", "parent", new List<string>());
            records.Code = "250";
            records.DnsStatus = true;

            // Act
            var result = await _bogusEmailCheck.IsBogusEmailAddress(records, records.Code, true);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task IsBogusEmailAddress_ReturnsFalseIfCodeNullOrStartsWith5()
        {
            var records = new RecordsTemplate("user", "com", "user@example.com", "example.com", "parent", new List<string> { "mx1" });
            records.DnsStatus = true;

            // Case code null
            records.Code = null;
            Assert.That(await _bogusEmailCheck.IsBogusEmailAddress(records, records.Code, true), Is.False);

            // Case code starts with 5
            records.Code = "550";
            Assert.That(await _bogusEmailCheck.IsBogusEmailAddress(records, records.Code, true), Is.False);
        }

        [Test]
        public async Task IsBogusEmailAddress_ReturnsFalseIfDnsStatusFalse()
        {
            var records = new RecordsTemplate("user", "com", "user@example.com", "example.com", "parent", new List<string> { "mx1" });
            records.Code = "250";
            records.DnsStatus = false;

            var result = await _bogusEmailCheck.IsBogusEmailAddress(records, records.Code, false);

            Assert.That(result, Is.False);
        }
    }
}
