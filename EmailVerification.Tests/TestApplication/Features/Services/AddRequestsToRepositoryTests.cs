using System;
using System.Threading.Tasks;
using Integrate.EmailVerification.Application.Features.Services;
using Integrate.EmailVerification.Infrastructure.Repositories;
using Integrate.EmailVerification.Models.Domains;
using Integrate.EmailVerification.Models.Templates;
using Moq;
using NUnit.Framework;

namespace Integrate.EmailVerification.Application.Tests.Features.Services
{
    [TestFixture]
    public class AddRequestUserToRepositoryTests
    {
        private Mock<IRequestsRepository> _mockRequestsRepository;
        private AddRequestUserToRepository _service;

        [SetUp]
        public void SetUp()
        {
            _mockRequestsRepository = new Mock<IRequestsRepository>();
            _service = new AddRequestUserToRepository(_mockRequestsRepository.Object);
        }

        [Test]
        public async Task AddRequestToRespository_ValidInput_CallsRepositoryAndReturnsTrue()
        {
            // Arrange
            var emailValidationInfo = new EmailValidationInfo
            {
                RequestId = Guid.NewGuid(),
                CreatedBy = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            _mockRequestsRepository
                .Setup(r => r.AddRequest(It.IsAny<Requests>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.AddRequestToRespository(emailValidationInfo);

            // Assert
            Assert.That(result, Is.True);

            _mockRequestsRepository.Verify(r =>
                r.AddRequest(It.Is<Requests>(req =>
                    req.Id == emailValidationInfo.RequestId &&
                    req.CreatedBy == emailValidationInfo.CreatedBy &&
                    req.CreatedAt == emailValidationInfo.CreatedAt)), Times.Once);
        }

    }
}
