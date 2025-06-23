using Moq;
using StackExchange.Redis;
using Integrate.EmailVerification.Models.Templates;
using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Application.Features.Services.DomainChecks;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Infrastructure.Redis;

namespace Integrate.EmailVerification.Tests.TestApplication.Features.Services.DomainChecks
{
    [TestFixture]
    public class DisposableDomainCheckTests
    {
        private Mock<IConnectionMultiplexer> _mockConnection;
        private Mock<IDatabase> _mockDatabase;
        private Mock<IRedisSeeder> _mockSeeder;
        private Mock<IEmailValidationChecksInfoFactory> _mockFactory;
        private DisposableDomainCheck _disposableDomainCheck;

        [SetUp]
        public void Setup()
        {
            _mockConnection = new Mock<IConnectionMultiplexer>();
            _mockDatabase = new Mock<IDatabase>();
            _mockSeeder = new Mock<IRedisSeeder>();
            _mockFactory = new Mock<IEmailValidationChecksInfoFactory>();

            _mockConnection.Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                           .Returns(_mockDatabase.Object);

            _disposableDomainCheck = new DisposableDomainCheck(_mockConnection.Object, _mockSeeder.Object, _mockFactory.Object);
        }

        [Test]
        public async Task EmailCheckValidator_ShouldSeed_WhenKeyDoesNotExist()
        {
            // Arrange
            var domain = "temp.com";
            var parentDomain = "mail.temp.com";
            var records = new RecordsTemplate("user", "", "user@temp.com", domain, parentDomain, new());
            var check = new EmailValidationCheck { Name = "DisposableDomain", AllotedScore = 10 };

            _mockDatabase.Setup(db => db.KeyExistsAsync(ConstantKeys.DisposableDomains, CommandFlags.None))
                         .ReturnsAsync(false);

            _mockDatabase.Setup(db => db.SetContainsAsync(ConstantKeys.DisposableDomains, parentDomain, CommandFlags.None))
                         .ReturnsAsync(false);

            _mockDatabase.Setup(db => db.SetContainsAsync(ConstantKeys.DisposableDomains, domain, CommandFlags.None))
                         .ReturnsAsync(false);

            _mockSeeder.Setup(s => s.SeedAsync(ConstantKeys.DisposableDomains))
                       .Returns(Task.CompletedTask);

            _mockFactory.Setup(f => f.Create(check, 10, true, true))
                        .Returns(new EmailValidationChecksInfo(check) { Passed = true, ObtainedScore = 10 });

            // Act
            var result = await _disposableDomainCheck.EmailCheckValidator(records, check);

            // Assert
            _mockSeeder.Verify(s => s.SeedAsync(ConstantKeys.DisposableDomains), Times.Once);
            Assert.That(result.Passed, Is.True);
            Assert.That(result.ObtainedScore, Is.EqualTo(10));
        }

        [Test]
        public async Task EmailCheckValidator_ShouldFail_WhenParentDomainIsDisposable()
        {
            // Arrange
            var domain = "normal.com";
            var parentDomain = "disposable.com";
            var records = new RecordsTemplate("user", "", "user@normal.com", domain, parentDomain, new());
            var check = new EmailValidationCheck { Name = "DisposableDomain", AllotedScore = 10 };

            _mockDatabase.Setup(db => db.KeyExistsAsync(ConstantKeys.DisposableDomains, CommandFlags.None))
                         .ReturnsAsync(true);

            _mockDatabase.Setup(db => db.SetContainsAsync(ConstantKeys.DisposableDomains, parentDomain, CommandFlags.None))
                         .ReturnsAsync(true);

            _mockDatabase.Setup(db => db.SetContainsAsync(ConstantKeys.DisposableDomains, domain, CommandFlags.None))
                         .ReturnsAsync(false);

            _mockFactory.Setup(f => f.Create(check, 0, false, true))
                        .Returns(new EmailValidationChecksInfo(check) { Passed = false, ObtainedScore = 0 });

            // Act
            var result = await _disposableDomainCheck.EmailCheckValidator(records, check);

            // Assert
            Assert.That(result.Passed, Is.False);
            Assert.That(result.ObtainedScore, Is.EqualTo(0));
        }

        [Test]
        public async Task EmailCheckValidator_ShouldFail_WhenDomainIsDisposable()
        {
            // Arrange
            var domain = "disposable.com";
            var parentDomain = "parent.com";
            var records = new RecordsTemplate("user","", "user@disposable.com", domain, parentDomain, new());
            var check = new EmailValidationCheck { Name = "DisposableDomain", AllotedScore = 10 };

            _mockDatabase.Setup(db => db.KeyExistsAsync(ConstantKeys.DisposableDomains, CommandFlags.None))
                         .ReturnsAsync(true);

            _mockDatabase.Setup(db => db.SetContainsAsync(ConstantKeys.DisposableDomains, parentDomain, CommandFlags.None))
                         .ReturnsAsync(false);

            _mockDatabase.Setup(db => db.SetContainsAsync(ConstantKeys.DisposableDomains, domain, CommandFlags.None))
                         .ReturnsAsync(true);

            _mockFactory.Setup(f => f.Create(check, 0, false, true))
                        .Returns(new EmailValidationChecksInfo(check) { Passed = false, ObtainedScore = 0 });

            // Act
            var result = await _disposableDomainCheck.EmailCheckValidator(records, check);

            // Assert
            Assert.That(result.Passed, Is.False);
            Assert.That(result.ObtainedScore, Is.EqualTo(0));
        }

        [Test]
        public async Task EmailCheckValidator_ShouldPass_WhenDomainAndParentDomainAreValid()
        {
            // Arrange
            var domain = "valid.com";
            var parentDomain = "mail.valid.com";
            var records = new RecordsTemplate("user", "", "user@valid.com", domain, parentDomain, new());
            var check = new EmailValidationCheck { Name = "DisposableDomain", AllotedScore = 10 };

            _mockDatabase.Setup(db => db.KeyExistsAsync(ConstantKeys.DisposableDomains, CommandFlags.None))
                         .ReturnsAsync(true);

            _mockDatabase.Setup(db => db.SetContainsAsync(ConstantKeys.DisposableDomains, parentDomain, CommandFlags.None))
                         .ReturnsAsync(false);

            _mockDatabase.Setup(db => db.SetContainsAsync(ConstantKeys.DisposableDomains, domain, CommandFlags.None))
                         .ReturnsAsync(false);

            _mockFactory.Setup(f => f.Create(check, 10, true, true))
                        .Returns(new EmailValidationChecksInfo(check) { Passed = true, ObtainedScore = 10 });

            // Act
            var result = await _disposableDomainCheck.EmailCheckValidator(records, check);

            // Assert
            Assert.That(result.Passed, Is.True);
            Assert.That(result.ObtainedScore, Is.EqualTo(10));
        }
    }
}
