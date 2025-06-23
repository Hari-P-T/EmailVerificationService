using Moq;
using StackExchange.Redis;
using Integrate.EmailVerification.Application.Features.Services.DomainChecks;
using Integrate.EmailVerification.Infrastructure.Redis;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Models.Templates;
using Integrate.EmailVerification.Infrastructure.Constant;

namespace Integrate.EmailVerification.Tests.TestApplication.Features.Services.DomainChecks
{
    [TestFixture]
    public class BlackListedDomainCheckTests
    {
        private Mock<IConnectionMultiplexer> _mockConnectionMultiplexer;
        private Mock<IDatabase> _mockDatabase;
        private Mock<IEmailValidationChecksInfoFactory> _mockFactory;
        private Mock<IRedisSeeder> _mockSeeder;
        private BlackListedDomainCheck _check;

        [SetUp]
        public void Setup()
        {
            _mockConnectionMultiplexer = new Mock<IConnectionMultiplexer>();
            _mockDatabase = new Mock<IDatabase>();
            _mockFactory = new Mock<IEmailValidationChecksInfoFactory>();
            _mockSeeder = new Mock<IRedisSeeder>();

            _mockConnectionMultiplexer.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                                      .Returns(_mockDatabase.Object);

            _check = new BlackListedDomainCheck(
                _mockConnectionMultiplexer.Object,
                _mockFactory.Object,
                _mockSeeder.Object);
        }

        [Test]
        public async Task EmailCheckValidator_ShouldSeedIfKeyNotExists()
        {
            // Arrange
            var domain = "spam.com";
            var parentDomain = "parent.spam.com";

            var records = new RecordsTemplate("user", "", "", domain, parentDomain, new());
            var check = new EmailValidationCheck { Name = "BlackListedDomain", AllotedScore = 10 };

            _mockDatabase.Setup(x => x.KeyExistsAsync(ConstantKeys.BlacklistedDomains, CommandFlags.None))
                         .ReturnsAsync(false);

            _mockSeeder.Setup(s => s.SeedAsync(ConstantKeys.BlacklistedDomains))
                       .Returns(Task.CompletedTask);

            // Domain and parent domain are NOT blacklisted
            _mockDatabase.Setup(x => x.SetContainsAsync(ConstantKeys.BlacklistedDomains, domain, CommandFlags.None))
                         .ReturnsAsync(false);
            _mockDatabase.Setup(x => x.SetContainsAsync(ConstantKeys.BlacklistedDomains, parentDomain, CommandFlags.None))
                         .ReturnsAsync(false);

            // Because domain is NOT blacklisted, passed = true
            _mockFactory.Setup(f => f.Create(It.IsAny<EmailValidationCheck>(), It.IsAny<int>(), true, true))
                        .Returns((EmailValidationCheck c, int s, bool p, bool v) =>
                            new EmailValidationChecksInfo(c)
                            {
                                ObtainedScore = s,
                                Passed = p,
                                Performed = true,
                                Email = records.Email
                            });

            // Act
            var result = await _check.EmailCheckValidator(records, check);

            // Assert
            _mockSeeder.Verify(s => s.SeedAsync(ConstantKeys.BlacklistedDomains), Times.Once);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Passed, Is.True); // Now it will pass
        }

        [Test]
        public async Task EmailCheckValidator_ShouldFail_WhenDomainIsBlacklisted()
        {
            // Arrange
            var domain = "spam.com";
            var parentDomain = "parent.com";
            var records = new RecordsTemplate("user","", "user@spam.com", domain, parentDomain, new());
            var check = new EmailValidationCheck
            {
                Name = "BlackListedDomain",
                AllotedScore = 10,
                Performed = true,
                Passed = true,
                CheckId = Guid.NewGuid()
            };

            _mockDatabase.Setup(x => x.KeyExistsAsync(ConstantKeys.BlacklistedDomains, CommandFlags.None))
                         .ReturnsAsync(true);

            // Domain is blacklisted
            _mockDatabase.Setup(x => x.SetContainsAsync(ConstantKeys.BlacklistedDomains, domain, CommandFlags.None))
                         .ReturnsAsync(true);

            // Parent domain check will be skipped in this case because domain is already blacklisted
            _mockFactory.Setup(f => f.Create(It.IsAny<EmailValidationCheck>(), It.IsAny<int>(), false, true))
                        .Returns((EmailValidationCheck c, int s, bool p, bool v) =>
                            new EmailValidationChecksInfo(c)
                            {
                                ObtainedScore = s,
                                Passed = p,
                                Performed = true,
                                Email = records.Email,
                                CheckName = c.Name
                            });

            // Act
            var result = await _check.EmailCheckValidator(records, check);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Passed, Is.False); // Because domain was blacklisted
            Assert.That(result.ObtainedScore, Is.EqualTo(5)); // 10 - 5 penalty
        }

        [Test]
        public async Task EmailCheckValidator_ShouldFail_WhenParentDomainIsBlacklisted()
        {
            // Arrange
            var domain = "safe.com";
            var parentDomain = "blacklisted.com";
            var check = new EmailValidationCheck { Name = "BlackListedDomain", AllotedScore = 10 };
            var records = new RecordsTemplate("user", "", "", domain, parentDomain, new());

            _mockDatabase.Setup(x => x.KeyExistsAsync(ConstantKeys.BlacklistedDomains, CommandFlags.None))
                         .ReturnsAsync(true);

            _mockDatabase.Setup(x => x.SetContainsAsync(ConstantKeys.BlacklistedDomains, domain, CommandFlags.None))
                         .ReturnsAsync(false);

            _mockDatabase.Setup(x => x.SetContainsAsync(ConstantKeys.BlacklistedDomains, parentDomain, CommandFlags.None))
                         .ReturnsAsync(true);

            _mockFactory.Setup(f => f.Create(
                It.Is<EmailValidationCheck>(c => c.Name == check.Name),
                It.Is<int>(s => s == 5), // score deducted once
                It.Is<bool>(p => p == false),
                It.IsAny<bool>()
            )).Returns(new EmailValidationChecksInfo(check) { Passed = false, ObtainedScore = 5 });

            // Act
            var result = await _check.EmailCheckValidator(records, check);

            // Assert
            Assert.That(result.Passed, Is.False);
            Assert.That(result.ObtainedScore, Is.EqualTo(5));
        }

        [Test]
        public async Task EmailCheckValidator_ShouldSetScoreZero_WhenBothDomainAndParentDomainAreBlacklisted()
        {
            // Arrange
            var domain = "blacklisted.com";
            var parentDomain = "blacklistedparent.com";
            var check = new EmailValidationCheck { Name = "BlackListedDomain", AllotedScore = 10 };
            var records = new RecordsTemplate("user", "", "", domain, parentDomain, new());

            _mockDatabase.Setup(x => x.KeyExistsAsync(ConstantKeys.BlacklistedDomains, CommandFlags.None))
                         .ReturnsAsync(true);

            _mockDatabase.SetupSequence(x => x.SetContainsAsync(ConstantKeys.BlacklistedDomains, It.IsAny<RedisValue>(), CommandFlags.None))
                         .ReturnsAsync(true)  // Domain is blacklisted
                         .ReturnsAsync(true); // Parent domain also blacklisted

            _mockFactory.Setup(f => f.Create(
                It.Is<EmailValidationCheck>(c => c.Name == check.Name),
                It.Is<int>(s => s == 0), // score is reduced to 0
                It.Is<bool>(p => p == false),
                It.IsAny<bool>()
            )).Returns(new EmailValidationChecksInfo(check) { Passed = false, ObtainedScore = 0 });

            // Act
            var result = await _check.EmailCheckValidator(records, check);

            // Assert
            Assert.That(result.Passed, Is.False);
            Assert.That(result.ObtainedScore, Is.EqualTo(0));
        }
    }
}
