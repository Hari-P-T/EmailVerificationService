using Integrate.EmailVerification.Application.Features.Services.EmailAddress;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Application.Features.Interfaces.Utility;
using Integrate.EmailVerification.Infrastructure.Redis;
using Integrate.EmailVerification.Models.Templates;
using Moq;
using StackExchange.Redis;

namespace Integrate.EmailVerification.Tests
{
    [TestFixture]
    public class VulgarCheckTests
    {
        private Mock<IEmailHelper> _emailHelperMock;
        private Mock<IDatabase> _databaseMock;
        private Mock<IRedisSeeder> _redisSeederMock;
        private Mock<IEmailValidationChecksInfoFactory> _factoryMock;
        private Mock<IConnectionMultiplexer> _redisMock;

        private VulgarCheck _vulgarCheck;
        private const string VulgarKey = "vulgar";

        [SetUp]
        public void Setup()
        {
            _emailHelperMock = new Mock<IEmailHelper>();
            _databaseMock = new Mock<IDatabase>();
            _redisSeederMock = new Mock<IRedisSeeder>();
            _factoryMock = new Mock<IEmailValidationChecksInfoFactory>();
            _redisMock = new Mock<IConnectionMultiplexer>();

            _redisMock.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                      .Returns(_databaseMock.Object);

            _vulgarCheck = new VulgarCheck(
                _emailHelperMock.Object,
                _redisSeederMock.Object,
                _redisMock.Object,
                _factoryMock.Object);
        }

        [Test]
        public void EmailCheckValidator_ThrowsArgumentNullException_WhenCheckIsNull()
        {
            var records = new RecordsTemplate("user", "com", "user@example.com", "example.com", "example", null);

            Assert.That(async () => await _vulgarCheck.EmailCheckValidator(records, null),
                        Throws.ArgumentNullException);
        }

        [Test]
public async Task EmailCheckValidator_SeedsRedis_WhenKeyDoesNotExist()
{
    var records = new RecordsTemplate("user", "com", "user@example.com", "example.com", "example", null);

    var check = new EmailValidationCheck
    {
        AllotedScore = 10,
        Name = "Vulgar",
        Passed = true,
        Performed = true
    };

    _databaseMock.Setup(db => db.KeyExistsAsync(VulgarKey, It.IsAny<CommandFlags>()))
                 .ReturnsAsync(false);

    _databaseMock.Setup(db => db.SetContainsAsync(VulgarKey, It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
                 .ReturnsAsync(false);

    _redisSeederMock.Setup(s => s.SeedAsync(VulgarKey))
                    .Returns(Task.CompletedTask)
                    .Verifiable();

    _factoryMock.Setup(f => f.Create(
        It.IsAny<EmailValidationCheck>(),
        It.IsAny<int>(),
        It.IsAny<bool>(),
        It.IsAny<bool>()))
    .Returns((EmailValidationCheck chk, int score, bool passed, bool valid) =>
        new EmailValidationChecksInfo(chk)
        {
            ObtainedScore = score,
            Passed = passed,
            CheckName = chk.Name,
            Performed = true
        });

    var result = await _vulgarCheck.EmailCheckValidator(records, check);

    _redisSeederMock.Verify(s => s.SeedAsync(VulgarKey), Times.AtLeastOnce);
    Assert.That(result.Passed, Is.True);
    Assert.That(result.ObtainedScore, Is.EqualTo(10));
}

[Test]
public async Task EmailCheckValidator_ReturnsFailed_WhenUserNameAndDomainExistInRedisSet()
{
    var records = new RecordsTemplate("vulgarUser", "com", "vulgarUser@example.com", "example", "exampleParent", null);

    var check = new EmailValidationCheck
    {
        AllotedScore = 10,
        Name = "Vulgar",
        Passed = true,
        Performed = true
    };

    _databaseMock.Setup(db => db.KeyExistsAsync(VulgarKey, It.IsAny<CommandFlags>()))
                 .ReturnsAsync(true);

    _databaseMock.SetupSequence(db => db.SetContainsAsync(VulgarKey, It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
                 .ReturnsAsync(true)   // username exists
                 .ReturnsAsync(true);  // domain exists

    _factoryMock.Setup(f => f.Create(
        It.IsAny<EmailValidationCheck>(),
        It.IsAny<int>(),
        It.IsAny<bool>(),
        It.IsAny<bool>()))
    .Returns((EmailValidationCheck chk, int score, bool passed, bool valid) =>
        new EmailValidationChecksInfo(chk)
        {
            ObtainedScore = score,
            Passed = passed,
            CheckName = chk.Name,
            Performed = true
        });

    var result = await _vulgarCheck.EmailCheckValidator(records, check);

    Assert.That(result.Passed, Is.False);
    Assert.That(result.ObtainedScore, Is.Not.EqualTo(10));
}


        [Test]
        public async Task EmailCheckValidator_ReturnsPassed_WhenUserNameOrDomainNotInRedisSet()
        {
            // Arrange
            var records = new RecordsTemplate("user", "com", "user@example.com", "example.com", "exampleParent", null);

            var check = new EmailValidationCheck
            {
                AllotedScore = 10,
                Name = "Vulgar",
                Passed = true,
                Performed = true
            };

            _databaseMock.Setup(db => db.KeyExistsAsync(VulgarKey, It.IsAny<CommandFlags>()))
                         .ReturnsAsync(true);

            // Username exists but domain does not, so valid is false in the code
            _databaseMock.SetupSequence(db => db.SetContainsAsync(VulgarKey, It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
                         .ReturnsAsync(true)   // username exists
                         .ReturnsAsync(false); // domain does NOT exist

            _factoryMock.Setup(f => f.Create(check, 10, true, true))
                        .Returns(new EmailValidationChecksInfo(check)
                        {
                            ObtainedScore = 10,
                            Passed = true,
                            CheckName = check.Name,
                            Performed = true
                        });

            // Act
            var result = await _vulgarCheck.EmailCheckValidator(records, check);

            // Assert
            Assert.That(result.Passed, Is.True);
            Assert.That(result.ObtainedScore, Is.EqualTo(10));
        }
    }
}
