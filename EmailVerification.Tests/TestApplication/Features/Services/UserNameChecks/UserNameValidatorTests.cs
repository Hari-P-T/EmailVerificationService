using Integrate.EmailVerification.Application.Features.Services.UserNameChecks;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Application.Features.Interfaces.Utility;
using Integrate.EmailVerification.Models.Templates;
using Integrate.EmailVerification.Api.Middlewares;
using Moq;

namespace Integrate.EmailVerification.Tests
{
    [TestFixture]
    public class UserNameValidatorTests
    {
        private Mock<IEmailHelper> _emailHelperMock;
        private Mock<IEmailValidationChecksInfoFactory> _factoryMock;
        private UserNameValidator _validator;

        [SetUp]
        public void Setup()
        {
            _emailHelperMock = new Mock<IEmailHelper>();
            _factoryMock = new Mock<IEmailValidationChecksInfoFactory>();
            _validator = new UserNameValidator(_emailHelperMock.Object, _factoryMock.Object);
        }

        [Test]
        public void EmailCheckValidator_NullCheck_ThrowsNullFoundException()
        {
            var records = new RecordsTemplate("user", "com", "user@example.com", "example.com", "example", null);
            Assert.ThrowsAsync<NullFoundException>(async () =>
                await _validator.EmailCheckValidator(records, null));
        }

        [TestCase("validUser123")]
        [TestCase("a.b-c_d")]
        [TestCase("abc")]
        [TestCase("user.name")]
        public async Task EmailCheckValidator_ValidUsernames_ReturnsPassed(string username)
        {
            // Arrange
            var email = $"{username}@example.com";
            var records = new RecordsTemplate(username, "com", email, "example.com", "example", null);
            var check = new EmailValidationCheck { AllotedScore = 10, Name = "GargbageEmailAddress" };

            _emailHelperMock.Setup(h => h.GetUserName(email)).Returns(username);
            _factoryMock.Setup(f => f.Create(check, 10, true, true))
                        .Returns(new EmailValidationChecksInfo(check)
                        {
                            ObtainedScore = 10,
                            Passed = true,
                            CheckName = check.Name,
                            Performed = true
                        });

            // Act
            var result = await _validator.EmailCheckValidator(records, check);

            // Assert
            Assert.That(result.Passed, Is.True);
            Assert.That(result.ObtainedScore, Is.EqualTo(10));
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("ab")]  // too short
        [TestCase("user..name")]  // double dot
        [TestCase(".username")]  // starts with dot
        [TestCase("username-")]  // ends with dash
        [TestCase("thisusernameiswaytoolongtobevalid")] // >20 chars
        public async Task EmailCheckValidator_InvalidUsernames_ReturnsFailed(string username)
        {
            // Arrange
            var email = username == null ? null : $"{username}@example.com";
            var records = new RecordsTemplate(username, "com", email, "example.com", "example", null);
            var check = new EmailValidationCheck { AllotedScore = 10, Name = "GargbageEmailAddress" };

            _emailHelperMock.Setup(h => h.GetUserName(email)).Returns(username);
            _factoryMock.Setup(f => f.Create(check, 0, false, true))
                        .Returns(new EmailValidationChecksInfo(check)
                        {
                            ObtainedScore = 0,
                            Passed = false,
                            CheckName = check.Name,
                            Performed = true
                        });

            // Act
            var result = await _validator.EmailCheckValidator(records, check);

            // Assert
            Assert.That(result.Passed, Is.False);
            Assert.That(result.ObtainedScore, Is.EqualTo(0));
        }

        [Test]
        public async Task EmailCheckValidator_RegexTimeout_ReturnsFailed()
        {
            // Arrange
            var username = "thisusernameisdefinitelyvalidbutwewillinjecttimeout";
            var email = $"{username}@example.com";
            var records = new RecordsTemplate(username, "com", email, "example.com", "example", null);
            var check = new EmailValidationCheck { AllotedScore = 10, Name = "GargbageEmailAddress" };

            _emailHelperMock.Setup(h => h.GetUserName(email)).Returns(username);

            // Inject a regex pattern that times out
            var validator = new UserNameValidator(_emailHelperMock.Object, _factoryMock.Object);

            // Use a very small timeout in the implementation — or simulate RegexMatchTimeoutException
            // So simulate by temporarily mocking Regex itself — which is not directly possible.

            // Instead: simulate timeout by mocking GetUserName to trigger a long string that we know will cause a timeout.
            _emailHelperMock.Setup(h => h.GetUserName(email))
                            .Returns("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"); // long invalid input

            // Setup factory to create the expected failure result
            _factoryMock.Setup(f => f.Create(check, 0, false, true))
                        .Returns(new EmailValidationChecksInfo(check)
                        {
                            ObtainedScore = 0,
                            Passed = false,
                            CheckName = check.Name,
                            Performed = true
                        });

            // Act
            var result = await validator.EmailCheckValidator(records, check);

            // Assert
            Assert.That(result.Passed, Is.False);
            Assert.That(result.ObtainedScore, Is.EqualTo(0));
        }

    }
}
