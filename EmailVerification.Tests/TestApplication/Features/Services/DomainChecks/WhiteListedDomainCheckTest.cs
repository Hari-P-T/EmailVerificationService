using Integrate.EmailVerification.Application.Features.Services.DomainChecks;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Infrastructure.Redis;
using Integrate.EmailVerification.Models.Templates;
using Moq;
using StackExchange.Redis;


namespace Integrate.EmailVerification.Tests.TestApplication.Features.Services.DomainChecks
{
    [TestFixture]
    public class WhiteListedDomainCheckTests
    {
        private Mock<IDatabase> _mockRedisDb;
        private Mock<IConnectionMultiplexer> _mockRedis;
        private Mock<IRedisSeeder> _mockRedisSeeder;
        private Mock<IEmailValidationChecksInfoFactory> _mockFactory;
        private WhiteListedDomainCheck _whiteListedDomainCheck;

        [SetUp]
        public void Setup()
        {
            _mockRedisDb = new Mock<IDatabase>();
            _mockRedis = new Mock<IConnectionMultiplexer>();
            _mockRedisSeeder = new Mock<IRedisSeeder>();
            _mockFactory = new Mock<IEmailValidationChecksInfoFactory>();

            _mockRedis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_mockRedisDb.Object);

            _whiteListedDomainCheck = new WhiteListedDomainCheck(_mockRedis.Object, _mockRedisSeeder.Object, _mockFactory.Object);
        }

        [Test]
        public async Task EmailCheckValidator_ShouldReturnPassed_WhenDomainIsWhitelisted()
        {
            var records = new RecordsTemplate("example", "com", "test@example.com", "example.com", "example.com", new List<string>());
            var check = new EmailValidationCheck { AllotedScore = 100 };

            _mockRedisDb.Setup(db => db.KeyExistsAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>())).ReturnsAsync(true);
            _mockRedisDb.Setup(db => db.SetContainsAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>())).ReturnsAsync(true);

            var expectedResponse = new EmailValidationChecksInfo(check)
            {
                ObtainedScore = 100,
                Passed = true,
                Performed = true
            };

            _mockFactory.Setup(f => f.Create(check, 100, true, true)).Returns(expectedResponse);

            var result = await _whiteListedDomainCheck.EmailCheckValidator(records, check);

            Assert.That(result.ObtainedScore, Is.EqualTo(expectedResponse.ObtainedScore));
            Assert.That(result.Passed, Is.True);
            Assert.That(result.Performed, Is.True);
        }

        [Test]
        public async Task EmailCheckValidator_ShouldReturnFailed_WhenDomainNotWhitelisted()
        {
            var records = new RecordsTemplate("notlisted", "com", "test@notlisted.com", "notlisted.com", "notlisted.com", new List<string>());
            var check = new EmailValidationCheck { AllotedScore = 100 };

            _mockRedisDb.Setup(db => db.KeyExistsAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>())).ReturnsAsync(true);
            _mockRedisDb.SetupSequence(db => db.SetContainsAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
                        .ReturnsAsync(false)
                        .ReturnsAsync(false);

            var expectedResponse = new EmailValidationChecksInfo(check)
            {
                ObtainedScore = 0,
                Passed = false,
                Performed = true
            };

            _mockFactory.Setup(f => f.Create(check, 0, false, true)).Returns(expectedResponse);

            var result = await _whiteListedDomainCheck.EmailCheckValidator(records, check);

            Assert.That(result.ObtainedScore, Is.EqualTo(expectedResponse.ObtainedScore));
            Assert.That(result.Passed, Is.False);
            Assert.That(result.Performed, Is.True);
        }

        [Test]
        public async Task EmailCheckValidator_ShouldSeedRedis_WhenKeyDoesNotExist()
        {
            var records = new RecordsTemplate("example", "com", "test@example.com", "example.com", "example.com", new List<string>());
            var check = new EmailValidationCheck { AllotedScore = 100 };

            _mockRedisDb.Setup(db => db.KeyExistsAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>())).ReturnsAsync(false);
            _mockRedisSeeder.Setup(seeder => seeder.SeedAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
            _mockRedisDb.Setup(db => db.SetContainsAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>())).ReturnsAsync(true);

            var expectedResponse = new EmailValidationChecksInfo(check)
            {
                ObtainedScore = 100,
                Passed = true,
                Performed = true
            };

            _mockFactory.Setup(f => f.Create(check, 100, true, true)).Returns(expectedResponse);

            var result = await _whiteListedDomainCheck.EmailCheckValidator(records, check);

            _mockRedisSeeder.Verify(s => s.SeedAsync(It.IsAny<string>()), Times.Once);
            Assert.That(result.ObtainedScore, Is.EqualTo(expectedResponse.ObtainedScore));
            Assert.That(result.Passed, Is.True);
            Assert.That(result.Performed, Is.True);
        }
    }
}
