using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Application.Features.Services.Factory;
using Moq;
using NUnit.Framework;

namespace Integrate.EmailVerification.Tests.TestApplication.Features.Services.Factory
{
    [TestFixture]
    public class EmailValidationFactoryTests
    {
        private Mock<IEmailValidationChecker> _checkerMock1;
        private Mock<IEmailValidationChecker> _checkerMock2;
        private EmailValidationFactory _factory;

        [SetUp]
        public void Setup()
        {
            _checkerMock1 = new Mock<IEmailValidationChecker>();
            _checkerMock1.Setup(c => c.Name).Returns("CheckerOne");

            _checkerMock2 = new Mock<IEmailValidationChecker>();
            _checkerMock2.Setup(c => c.Name).Returns("CheckerTwo");

            var validators = new List<IEmailValidationChecker> { _checkerMock1.Object, _checkerMock2.Object };
            _factory = new EmailValidationFactory(validators);
        }

        [Test]
        public void GetValidator_ShouldReturnValidator_WhenNameExists()
        {
            // Act
            var validator1 = _factory.GetValidator("CheckerOne");
            var validator2 = _factory.GetValidator("checkerTwo"); // test case-insensitivity

            // Assert
            Assert.That(validator1, Is.EqualTo(_checkerMock1.Object));
            Assert.That(validator2, Is.EqualTo(_checkerMock2.Object));
        }

        //[Test]
        public void GetValidator_ShouldThrowArgumentException_WhenNameDoesNotExist()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _factory.GetValidator("NonExistentChecker"));
            Assert.That(ex.Message, Does.Contain("Validator for type 'NonExistentChecker' not found."));
        }
    }
}
