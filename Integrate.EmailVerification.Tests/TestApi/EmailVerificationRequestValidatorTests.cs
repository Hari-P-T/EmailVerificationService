using FluentValidation.TestHelper;
using Integrate.EmailVerification.Api;
using Integrate.EmailVerification.Models.Request;
using NUnit.Framework;

namespace Integrate.EmailVerification.Tests.Api
{
    [TestFixture]
    public class EmailVerificationRequestValidatorTests
    {
        private EmailVerificationRequestValidator _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new EmailVerificationRequestValidator();
        }

        [Test]
        public void Should_Have_Error_When_Email_Is_Null_Or_Empty()
        {
            var model = new EmailVerificationRequest { Email = null };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("Email is required.");

            model.Email = "";
            result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("Email is required.");
        }

        [TestCase("invalid-email")]
        [TestCase("missingatsign.com")]
        [TestCase("missingdomain@")]
        public void Should_Have_Error_When_Email_Is_Invalid(string invalidEmail)
        {
            var model = new EmailVerificationRequest { Email = invalidEmail };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("A valid email address is required.");
        }

        [TestCase("test@example.com")]
        [TestCase("user.name+tag+sorting@example.com")]
        public void Should_Not_Have_Error_When_Email_Is_Valid(string validEmail)
        {
            var model = new EmailVerificationRequest { Email = validEmail };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Email);
        }

        [Test]
        public void Should_Have_Error_When_Strictness_Is_Null_Or_Empty()
        {
            var model = new EmailVerificationRequest { Strictness = null };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Strictness)
                  .WithErrorMessage("Strictness is required.");

            model.Strictness = "";
            result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Strictness)
                  .WithErrorMessage("Strictness is required.");
        }

        [TestCase("invalid")]
        [TestCase("STRICT")]
        [TestCase("none")]
        public void Should_Have_Error_When_Strictness_Is_Not_Allowed_Value(string strictness)
        {
            var model = new EmailVerificationRequest { Strictness = strictness };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Strictness)
                  .WithErrorMessage("Strictness must be one of: basic, intermediate, advanced.");
        }

        [TestCase("basic")]
        [TestCase("Basic")]
        [TestCase("INTERMEDIATE")]
        [TestCase("advanced")]
        public void Should_Not_Have_Error_When_Strictness_Is_Allowed_Value(string strictness)
        {
            var model = new EmailVerificationRequest { Strictness = strictness };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Strictness);
        }

        //[Test]
        //public void Should_Have_Error_When_Timeout_Is_Less_Than_Or_Equal_To_Zero()
        //{
        //    var model = new EmailVerificationRequest { Timeout = 0 };
        //    var result = _validator.TestValidate(model);
        //    result.ShouldHaveValidationErrorFor(x => x.Timeout)
        //          .WithErrorMessage("Timeout must be greater than 0.");

        //    model.Timeout = -1;
        //    result = _validator.TestValidate(model);
        //    result.ShouldHaveValidationErrorFor(x => x.Timeout)
        //          .WithErrorMessage("Timeout must be greater than 0.");
        //}

        //[Test]
        //public void Should_Have_Error_When_Timeout_Exceeds_10000()
        //{
        //    var model = new EmailVerificationRequest { Timeout = 10001 };
        //    var result = _validator.TestValidate(model);
        //    result.ShouldHaveValidationErrorFor(x => x.Timeout)
        //          .WithErrorMessage("Timeout must not exceed 10000 milliseconds.");
        //}

        //[TestCase(1)]
        //[TestCase(5000)]
        //[TestCase(10000)]
        //public void Should_Not_Have_Error_When_Timeout_Is_Valid(int timeout)
        //{
        //    var model = new EmailVerificationRequest { Timeout = timeout };
        //    var result = _validator.TestValidate(model);
        //    result.ShouldNotHaveValidationErrorFor(x => x.Timeout);
        //}

        [Test]
        public void Should_Have_Errors_For_Multiple_Invalid_Properties()
        {
            var model = new EmailVerificationRequest
            {
                Email = "",
                Strictness = "invalid",
                
            };
            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("Email is required.");

            result.ShouldHaveValidationErrorFor(x => x.Strictness)
                  .WithErrorMessage("Strictness must be one of: basic, intermediate, advanced.");

            
        }
    }
}
