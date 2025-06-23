using FluentValidation.TestHelper;
using Integrate.EmailVerification.Api.Validators;
using Integrate.EmailVerification.Application.Models.Request;
using Integrate.EmailVerification.Models.Request;
using NUnit.Framework;
using System.Collections.Generic;

namespace Integrate.EmailVerification.Tests.Api.Validators
{
    [TestFixture]
    public class BulkEmailVerificationRequestValidatorTests
    {
        private BulkEmailVerificationRequestValidator _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new BulkEmailVerificationRequestValidator();
        }


        [Test]
        public void Should_Have_Error_When_BulkEmailVerificationList_Is_Empty()
        {
            var model = new BulkEmailVerificationRequest { BulkEmailVerificationList = new List<EmailVerificationRequest>() };

            var result = _validator.TestValidate(model);

            var hasError = result.Errors.Exists(e => e.PropertyName == "BulkEmailVerificationList" &&
                                                    e.ErrorMessage == "BulkEmailVerificationList cannot be empty.");

            Assert.That(hasError, Is.True);
        }

        [Test]
        public void Should_Have_Error_When_BulkEmailVerificationList_Exceeds_25()
        {
            var list = new List<EmailVerificationRequest>();
            for (int i = 0; i < 26; i++)
            {
                list.Add(new EmailVerificationRequest { Email = $"test{i}@example.com",Strictness ="Basic" });
            }

            var model = new BulkEmailVerificationRequest { BulkEmailVerificationList = list };

            var result = _validator.TestValidate(model);

    
            result.ShouldHaveValidationErrorFor(x => x.BulkEmailVerificationList)
                  .WithErrorMessage("Maximum of 25 emails can be verified in a single request.");
        }

        [Test]
        public void Should_Not_Have_Error_For_Valid_Request()
        {
            var list = new List<EmailVerificationRequest>();
            for (int i = 0; i < 5; i++)
            {
                list.Add(new EmailVerificationRequest { Email = $"test{i}@example.com",Strictness ="Basic" });
            }

            var model = new BulkEmailVerificationRequest { BulkEmailVerificationList = list };

            var result = _validator.TestValidate(model);

            Assert.That(result.IsValid, Is.True);
        }
    }
}
