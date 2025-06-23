using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Application.Features.Services.Factory;
using Integrate.EmailVerification.Models.Templates;
using NUnit.Framework;

namespace Integrate.EmailVerification.Tests.TestApplication.Features.Services.Factory
{
    [TestFixture]
    public class EmailValidationChecksInfoFactoryTests
    {
        private IEmailValidationChecksInfoFactory _factory;

        [SetUp]
        public void Setup()
        {
            _factory = new EmailValidationChecksInfoFactory();
        }

        [Test]
        public void Create_ShouldSet_AllProperties_Correctly_WhenPassedTrue()
        {
            var check = new EmailValidationCheck { AllotedScore = 10 };
            int obtainedScore = 10;
            bool passed = true;
            bool performed = true;

            var result = _factory.Create(check, obtainedScore, passed, performed);

            Assert.That(result.CheckName, Is.EqualTo(check.Name));
            Assert.That(result.ObtainedScore, Is.EqualTo(obtainedScore));
            Assert.That(result.Passed, Is.EqualTo(passed));
            Assert.That(result.Performed, Is.EqualTo(performed));
        }

        [Test]
        public void Create_ShouldSet_AllProperties_Correctly_WhenPassedFalse()
        {
            var check = new EmailValidationCheck { AllotedScore = 5 };
            int obtainedScore = 0;
            bool passed = false;
            bool performed = false;

            var result = _factory.Create(check, obtainedScore, passed, performed);

            Assert.That(result.CheckName, Is.EqualTo(check.Name));
            Assert.That(result.ObtainedScore, Is.EqualTo(obtainedScore));
            Assert.That(result.Passed, Is.EqualTo(passed));
            Assert.That(result.Performed, Is.EqualTo(performed));
        }

        [Test]
        public void Create_ShouldReturnNewInstance_EveryTime()
        {
            var check = new EmailValidationCheck { AllotedScore = 3 };
            var result1 = _factory.Create(check, 1, false, true);
            var result2 = _factory.Create(check, 3, true, true);

            Assert.That(result1, Is.Not.SameAs(result2));
        }

        [Test]
        public void Create_ShouldRetainReferenceToOriginalCheckObject()
        {
            var check = new EmailValidationCheck { AllotedScore = 7 };
            var result = _factory.Create(check, 5, true, true);

            Assert.That(result.AllotedScore, Is.EqualTo(7));
        }
    }
}
