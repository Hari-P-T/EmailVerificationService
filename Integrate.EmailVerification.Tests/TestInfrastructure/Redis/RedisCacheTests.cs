using Integrate.EmailVerification.Infrastructure.Redis;
using Integrate.EmailVerification.Models.Response;
using Moq;
using NUnit.Framework;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Integrate.EmailVerification.Tests.Infrastructure.Redis
{
    [TestFixture]
    public class RedisCacheTests
    {
        private Mock<IConnectionMultiplexer> _mockConnectionMultiplexer;
        private Mock<IDatabase> _mockDatabase;
        private RedisCache _redisCache;

        [SetUp]
        public void SetUp()
        {
            _mockConnectionMultiplexer = new Mock<IConnectionMultiplexer>();
            _mockDatabase = new Mock<IDatabase>();
            _mockConnectionMultiplexer.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                                      .Returns(_mockDatabase.Object);
            _redisCache = new RedisCache(_mockConnectionMultiplexer.Object);
        }

        [Test]
        public async Task GetResponseFromCache_KeyDoesNotExist_ReturnsNewResponse()
        {
            _mockDatabase.Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                         .ReturnsAsync(RedisValue.Null);

            var result = await _redisCache.GetResponseFromCache("nonexistent_key");

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<EmailVerificationResponse>());
        }

        [Test]
        public async Task GetResponseFromCache_KeyExists_ReturnsDeserializedResponse()
        {
            var expectedResponse = new EmailVerificationResponse { Email = "test@example.com", Score = 100 };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(expectedResponse);
            _mockDatabase.Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                         .ReturnsAsync(json);

            var result = await _redisCache.GetResponseFromCache("test@example.com");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Email, Is.EqualTo("test@example.com"));
            Assert.That(result.Score, Is.EqualTo(100));
        }

        [Test]
        public async Task IsResponseInCache_KeyExists_ReturnsTrue()
        {
            _mockDatabase.Setup(db => db.KeyExistsAsync("key", It.IsAny<CommandFlags>()))
                         .ReturnsAsync(true);

            var exists = await _redisCache.IsResponseInCache("key");

            Assert.That(exists, Is.True);
        }

        [Test]
        public async Task IsResponseInCache_KeyDoesNotExist_ReturnsFalse()
        {
            _mockDatabase.Setup(db => db.KeyExistsAsync("key", It.IsAny<CommandFlags>()))
                         .ReturnsAsync(false);

            var exists = await _redisCache.IsResponseInCache("key");

            Assert.That(exists, Is.False);
        }

        [Test]
        public async Task PutResponseIntoCache_KeyAlreadyExists_ReturnsTrue()
        {
            var response = new EmailVerificationResponse { Email = "test@example.com" };
            _mockDatabase.Setup(db => db.KeyExistsAsync(response.Email, It.IsAny<CommandFlags>()))
                         .ReturnsAsync(true);

            var result = await _redisCache.PutResponseIntoCache(response);

            Assert.That(result, Is.True);
            _mockDatabase.Verify(db => db.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), When.Always, CommandFlags.None), Times.Never);
        }

    }
}
