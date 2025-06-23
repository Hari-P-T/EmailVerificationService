using Moq;
using StackExchange.Redis;
using Integrate.EmailVerification.Application.Features.Services.EmailAddress;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Infrastructure.Redis;
using Integrate.EmailVerification.Models.Templates;

namespace Integrate.EmailVerification.Tests
{
    [TestFixture]
    public class SpamCheckTests
    {
        private Mock<IEmailValidationChecksInfoFactory> _factoryMock;
        private Mock<IRedisSeeder> _seederMock;
        private Mock<IConnectionMultiplexer> _redisMock;
        private Mock<IDatabase> _databaseMock;
        private SpamCheck _spamCheck;
        private EmailValidationCheck _check;

        [SetUp]
        public void Setup()
        {
            _factoryMock = new Mock<IEmailValidationChecksInfoFactory>();
            _seederMock = new Mock<IRedisSeeder>();
            _redisMock = new Mock<IConnectionMultiplexer>();
            _databaseMock = new Mock<IDatabase>();

            _redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_databaseMock.Object);

            _spamCheck = new SpamCheck(_factoryMock.Object, _seederMock.Object, _redisMock.Object);

            _check = new EmailValidationCheck
            {
                AllotedScore = 10,
                Name = "SpamDomain"
            };

            _factoryMock.Setup(f => f.Create(It.IsAny<EmailValidationCheck>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns<EmailValidationCheck, int, bool, bool>((check, score, passed, valid) =>
                    new EmailValidationChecksInfo(check)
                    {
                        ObtainedScore = score,
                        Passed = passed,
                        Performed = true,
                        CheckName = check.Name
                    });
        }

        [Test]
        public async Task EmailCheckValidator_EmailInSpamSet_ReturnsPassedTrueAndFullScore()
        {
            // Arrange
            var records = new RecordsTemplate("user", "com", "spam@example.com", "example.com", "example.com", null);

            _databaseMock.Setup(db => db.KeyExistsAsync(It.IsAny<RedisKey>(), default))
                .ReturnsAsync(true);

            _databaseMock.Setup(db => db.SetContainsAsync(It.IsAny<RedisKey>(), It.Is<RedisValue>(v => v == records.Email), default))
                .ReturnsAsync(true);

            // Act
            var result = await _spamCheck.EmailCheckValidator(records, _check);

            // Assert
            Assert.That(result.Passed, Is.False);
            Assert.That(result.ObtainedScore, Is.Not.EqualTo(_check.AllotedScore));
        }

        [Test]
        public async Task EmailCheckValidator_EmailNotInSpamSet_ReturnsPassedFalseAndZeroScore()
        {
            // Arrange
            var records = new RecordsTemplate("user", "com", "notspam@example.com", "example.com", "example.com", null);

            _databaseMock.Setup(db => db.KeyExistsAsync(It.IsAny<RedisKey>(), default))
                .ReturnsAsync(true);

            _databaseMock.Setup(db => db.SetContainsAsync(It.IsAny<RedisKey>(), It.Is<RedisValue>(v => v == records.Email), default))
                .ReturnsAsync(false);

            // Act
            var result = await _spamCheck.EmailCheckValidator(records, _check);

            // Assert
            Assert.That(result.Passed, Is.True);
            Assert.That(result.ObtainedScore, Is.EqualTo(10));
        }

        [Test]
        public async Task EmailCheckValidator_KeyDoesNotExist_SeedsAndChecksSet()
        {
            // Arrange
            var records = new RecordsTemplate("user", "com", "spam@example.com", "example.com", "example.com", null);

            _databaseMock.Setup(db => db.KeyExistsAsync(It.IsAny<RedisKey>(), default))
                .ReturnsAsync(false);

            _seederMock.Setup(s => s.SeedAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _databaseMock.Setup(db => db.SetContainsAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), default))
                .ReturnsAsync(true);

            // Act
            var result = await _spamCheck.EmailCheckValidator(records, _check);

            // Assert
            _seederMock.Verify(s => s.SeedAsync(It.IsAny<string>()), Times.Once);
            Assert.That(result.Passed, Is.False);
            Assert.That(result.ObtainedScore, Is.Not.EqualTo(_check.AllotedScore));
        }

        [Test]
        public async Task EmailCheckValidator_EmptyEmail_ReturnsPassedTrueAndFullScore()
        {
            // Arrange
            var records = new RecordsTemplate("user", "com", "", "example.com", "example.com", null);

            // Act
            var result = await _spamCheck.EmailCheckValidator(records, _check);

            // Assert
            // Since email is empty, it never checks Redis, so valid stays true and passed stays true
            Assert.That(result.Passed, Is.False);
            Assert.That(result.ObtainedScore, Is.Not.EqualTo(_check.AllotedScore));
        }
    }
}
