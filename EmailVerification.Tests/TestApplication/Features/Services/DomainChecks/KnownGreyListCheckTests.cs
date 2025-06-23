using Moq;
using Integrate.EmailVerification.Application.Features.Services.DomainChecks;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Infrastructure.Redis;
using Integrate.EmailVerification.Models.Templates;
using Integrate.EmailVerification.Infrastructure.Constant;
using StackExchange.Redis;

namespace Integrate.EmailVerification.Tests.TestApplication.Features.Services.DomainChecks
{
    [TestFixture]
    public class KnownGreyListerCheckTests
    {
        private Mock<IDatabase> _mockDatabase;
        private Mock<IConnectionMultiplexer> _mockConnection;
        private Mock<IRedisSeeder> _mockSeeder;
        private Mock<IEmailValidationChecksInfoFactory> _mockFactory;

        private KnownGreyListerCheck _greylistCheck;

        [SetUp]
        public void Setup()
        {
            _mockDatabase = new Mock<IDatabase>();
            _mockConnection = new Mock<IConnectionMultiplexer>();
            _mockSeeder = new Mock<IRedisSeeder>();
            _mockFactory = new Mock<IEmailValidationChecksInfoFactory>();

            _mockConnection.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                           .Returns(_mockDatabase.Object);

            _greylistCheck = new KnownGreyListerCheck(_mockConnection.Object, _mockSeeder.Object, _mockFactory.Object);
        }

        [Test]
        public async Task EmailCheckValidator_ShouldSeedIfKeyNotExists()
        {
            var domain = "example.com";
            var parentDomain = "mail.example.com";
            var check = new EmailValidationCheck { Name = CheckNames.GreyListedDomain, AllotedScore = 10 };
            var records = new RecordsTemplate("user","", "user@example.com", domain, parentDomain, new());

            _mockDatabase.Setup(x => x.KeyExistsAsync(ConstantKeys.GreylistedDomains, CommandFlags.None))
                         .ReturnsAsync(false);

            _mockSeeder.Setup(s => s.SeedAsync(ConstantKeys.GreylistedDomains))
                       .Returns(Task.CompletedTask)
                       .Verifiable();

            // Return false for all SetContainsAsync calls so no score deduction happens here
            _mockDatabase.Setup(x => x.SetContainsAsync(ConstantKeys.GreylistedDomains, It.IsAny<RedisValue>(), CommandFlags.None))
                         .ReturnsAsync(false);

            _mockFactory.Setup(f => f.Create(check, check.AllotedScore, true, true))
                        .Returns(new EmailValidationChecksInfo(check) { Passed = true, ObtainedScore = check.AllotedScore });

            var result = await _greylistCheck.EmailCheckValidator(records, check);

            _mockSeeder.Verify(s => s.SeedAsync(ConstantKeys.GreylistedDomains), Times.Once);
            Assert.That(result.Passed, Is.True);
            Assert.That(result.ObtainedScore, Is.EqualTo(check.AllotedScore));
        }

        [Test]
        public async Task EmailCheckValidator_ShouldReturnFullScore_WhenDomainAndParentDomainEmpty()
        {
            var check = new EmailValidationCheck { Name = CheckNames.GreyListedDomain, AllotedScore = 10 };
            var records = new RecordsTemplate("user","", "user@", "", "", new());

            _mockDatabase.Setup(x => x.KeyExistsAsync(ConstantKeys.GreylistedDomains, CommandFlags.None))
                         .ReturnsAsync(true);

            // Use the generic setup here (or add in SetUp)
            _mockFactory.Setup(f => f.Create(It.IsAny<EmailValidationCheck>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
                        .Returns((EmailValidationCheck c, int score, bool passed, bool valid) =>
                            new EmailValidationChecksInfo(c) { Passed = passed, ObtainedScore = score });

            var result = await _greylistCheck.EmailCheckValidator(records, check);

            Assert.That(result.Passed, Is.False);
            Assert.That(result.ObtainedScore, Is.EqualTo(0));
        }


        [Test]
        public async Task EmailCheckValidator_ShouldReduceScore_WhenDomainIsInRedis()
        {
            var domain = "grey.com";
            var parentDomain = "mail.grey.com";
            var check = new EmailValidationCheck { Name = CheckNames.GreyListedDomain, AllotedScore = 10 };
            var records = new RecordsTemplate("user", "", "user@grey.com", domain, parentDomain, new());

            _mockDatabase.Setup(x => x.KeyExistsAsync(ConstantKeys.GreylistedDomains, CommandFlags.None))
                         .ReturnsAsync(true);

            _mockDatabase.Setup(x => x.SetContainsAsync(ConstantKeys.GreylistedDomains, domain, CommandFlags.None))
                         .ReturnsAsync(true);

            _mockDatabase.Setup(x => x.SetContainsAsync(ConstantKeys.GreylistedDomains, parentDomain, CommandFlags.None))
                         .ReturnsAsync(false);

            // Expected score after first deduction = 10 - 5 = 5
            _mockFactory.Setup(f => f.Create(check, 5, false, true))
                        .Returns(new EmailValidationChecksInfo(check) { Passed = false, ObtainedScore = 5 });

            var result = await _greylistCheck.EmailCheckValidator(records, check);

            Assert.That(result.Passed, Is.False);
            Assert.That(result.ObtainedScore, Is.EqualTo(5));
        }

        [Test]
        public async Task EmailCheckValidator_ShouldReduceScoreToZero_WhenParentDomainAlsoInRedis_AndScoreLessThanAllotedScore()
        {
            var domain = "grey.com";
            var parentDomain = "mail.grey.com";
            var check = new EmailValidationCheck { Name = CheckNames.GreyListedDomain, AllotedScore = 10 };
            var records = new RecordsTemplate("user", "", "user@grey.com", domain, parentDomain, new());

            _mockDatabase.Setup(x => x.KeyExistsAsync(ConstantKeys.GreylistedDomains, CommandFlags.None))
                         .ReturnsAsync(true);

            // Domain in Redis returns true - first deduction from 10 to 5
            _mockDatabase.SetupSequence(x => x.SetContainsAsync(ConstantKeys.GreylistedDomains, It.IsAny<RedisValue>(), CommandFlags.None))
                         .ReturnsAsync(true)   // for Domain
                         .ReturnsAsync(true);  // for ParentDomain

            // After second deduction, score < AllotedScore => 0
            _mockFactory.Setup(f => f.Create(check, 0, false, true))
                        .Returns(new EmailValidationChecksInfo(check) { Passed = false, ObtainedScore = 0 });

            var result = await _greylistCheck.EmailCheckValidator(records, check);

            Assert.That(result.Passed, Is.False);
            Assert.That(result.ObtainedScore, Is.EqualTo(0));
        }

        [Test]
        public async Task EmailCheckValidator_ShouldReduceScore_WhenParentDomainIsInRedis_AndDomainIsNot()
        {
            var domain = "grey.com";
            var parentDomain = "mail.grey.com";
            var check = new EmailValidationCheck { Name = CheckNames.GreyListedDomain, AllotedScore = 10 };
            var records = new RecordsTemplate("user","", "user@grey.com", domain, parentDomain, new());

            _mockDatabase.Setup(x => x.KeyExistsAsync(ConstantKeys.GreylistedDomains, CommandFlags.None))
                         .ReturnsAsync(true);

            _mockDatabase.Setup(x => x.SetContainsAsync(ConstantKeys.GreylistedDomains, domain, CommandFlags.None))
                         .ReturnsAsync(false);

            _mockDatabase.Setup(x => x.SetContainsAsync(ConstantKeys.GreylistedDomains, parentDomain, CommandFlags.None))
                         .ReturnsAsync(true);

            // After second deduction, score = 10 - 5 = 5
            _mockFactory.Setup(f => f.Create(check, 5, false, true))
                        .Returns(new EmailValidationChecksInfo(check) { Passed = false, ObtainedScore = 5 });

            var result = await _greylistCheck.EmailCheckValidator(records, check);

            Assert.That(result.Passed, Is.False);
            Assert.That(result.ObtainedScore, Is.EqualTo(5));
        }

        [Test]
        public async Task EmailCheckValidator_ShouldReturnPassed_WhenNeitherDomainNorParentDomainInRedis()
        {
            var domain = "clean.com";
            var parentDomain = "mail.clean.com";
            var check = new EmailValidationCheck { Name = CheckNames.GreyListedDomain, AllotedScore = 10 };
            var records = new RecordsTemplate("user","", "user@clean.com", domain, parentDomain, new());

            _mockDatabase.Setup(x => x.KeyExistsAsync(ConstantKeys.GreylistedDomains, CommandFlags.None))
                         .ReturnsAsync(true);

            _mockDatabase.Setup(x => x.SetContainsAsync(ConstantKeys.GreylistedDomains, domain, CommandFlags.None))
                         .ReturnsAsync(false);

            _mockDatabase.Setup(x => x.SetContainsAsync(ConstantKeys.GreylistedDomains, parentDomain, CommandFlags.None))
                         .ReturnsAsync(false);

            _mockFactory.Setup(f => f.Create(check, check.AllotedScore, true, true))
                        .Returns(new EmailValidationChecksInfo(check) { Passed = true, ObtainedScore = check.AllotedScore });

            var result = await _greylistCheck.EmailCheckValidator(records, check);

            Assert.That(result.Passed, Is.True);
            Assert.That(result.ObtainedScore, Is.EqualTo(check.AllotedScore));
        }
    }
}
