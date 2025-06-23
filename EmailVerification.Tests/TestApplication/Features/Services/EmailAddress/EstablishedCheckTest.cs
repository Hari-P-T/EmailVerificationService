using Integrate.EmailVerification.Application.Features.Services.EmailAddress;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Infrastructure.Redis;
using Integrate.EmailVerification.Models.Templates;
using Moq;
using NUnit.Framework;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Integrate.EmailVerification.Tests.Features.Services.EmailAddress
{
    [TestFixture]
    public class EstablishedCheckTests
    {
        private Mock<IConnectionMultiplexer> _mockRedis;
        private Mock<IDatabase> _mockDatabase;
        private Mock<IRedisSeeder> _mockRedisSeeder;
        private Mock<IEmailValidationChecksInfoFactory> _mockFactory;
        private EstablishedCheck _establishedCheck;
        private EmailValidationCheck _check;

        [SetUp]
        public void Setup()
        {
            _mockRedis = new Mock<IConnectionMultiplexer>();
            _mockDatabase = new Mock<IDatabase>();
            _mockRedisSeeder = new Mock<IRedisSeeder>();
            _mockFactory = new Mock<IEmailValidationChecksInfoFactory>();

            _mockRedis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                      .Returns(_mockDatabase.Object);

            _check = new EmailValidationCheck
            {
                Name = CheckNames.Established,
                AllotedScore = 10
            };

            _establishedCheck = new EstablishedCheck(_mockRedis.Object, _mockRedisSeeder.Object, _mockFactory.Object);
        }

        [Test]
        public async Task EmailCheckValidator_EmailExistsInRedis_ShouldFail() // <- Updated name and logic
        {
            // Arrange
            var record = new RecordsTemplate("user", "com", "user@example.com", "example.com", "example.com", new List<string>());

            _mockDatabase.Setup(db => db.KeyExistsAsync(ConstantKeys.Established, CommandFlags.None))
                         .ReturnsAsync(true);

            _mockDatabase.Setup(db => db.SetContainsAsync(ConstantKeys.Established, record.Email, CommandFlags.None))
                         .ReturnsAsync(true); // Email exists → Should fail based on new logic

            _mockFactory.Setup(f => f.Create(_check, 0, false, true))
                        .Returns(new EmailValidationChecksInfo(_check)
                        {
                            Email = record.Email,
                            ObtainedScore = 0,
                            Passed = false,
                            Performed = true
                        });

            // Act
            var result = await _establishedCheck.EmailCheckValidator(record, _check);

            // Assert
            Assert.That(result.ObtainedScore, Is.EqualTo(0));
            Assert.That(result.Passed, Is.False);
        }

        [Test]
        public async Task EmailCheckValidator_EmailDoesNotExistInRedis_ShouldPass()
        {
            var record = new RecordsTemplate("user", "com", "user@fake.com", "fake.com", "fake.com", new List<string>());

            _mockDatabase.Setup(db => db.KeyExistsAsync(ConstantKeys.Established, CommandFlags.None))
                         .ReturnsAsync(true);

            _mockDatabase.Setup(db => db.SetContainsAsync(ConstantKeys.Established, record.Email, CommandFlags.None))
                         .ReturnsAsync(false); // Email doesn't exist → Should pass

            _mockFactory.Setup(f => f.Create(_check, 10, true, true))
                        .Returns(new EmailValidationChecksInfo(_check)
                        {
                            Email = record.Email,
                            ObtainedScore = 10,
                            Passed = true,
                            Performed = true
                        });

            // Act
            var result = await _establishedCheck.EmailCheckValidator(record, _check);

            // Assert
            Assert.That(result.ObtainedScore, Is.EqualTo(10));
            Assert.That(result.Passed, Is.True);
        }

        [Test]
        public async Task EmailCheckValidator_RedisKeyDoesNotExist_ShouldSeed_AndPassIfNotFound()
        {
            var record = new RecordsTemplate("user", "com", "user@unknown.com", "unknown.com", "unknown.com", new List<string>());

            _mockDatabase.Setup(db => db.KeyExistsAsync(ConstantKeys.Established, CommandFlags.None))
                         .ReturnsAsync(false);

            _mockRedisSeeder.Setup(s => s.SeedAsync(ConstantKeys.Established))
                            .Returns(Task.CompletedTask);

            _mockDatabase.Setup(db => db.SetContainsAsync(ConstantKeys.Established, record.Email, CommandFlags.None))
                         .ReturnsAsync(false); // Not found → Should pass

            _mockFactory.Setup(f => f.Create(_check, 10, true, true))
                        .Returns(new EmailValidationChecksInfo(_check)
                        {
                            Email = record.Email,
                            ObtainedScore = 10,
                            Passed = true,
                            Performed = true
                        });

            var result = await _establishedCheck.EmailCheckValidator(record, _check);

            Assert.That(result.Passed, Is.True);
            _mockRedisSeeder.Verify(s => s.SeedAsync(ConstantKeys.Established), Times.Once);
        }
    }
}
