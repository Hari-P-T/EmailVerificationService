using Integrate.EmailVerification.Application.Features.Utility;
using Integrate.EmailVerification.Application.Features.Interfaces.SMTPChecks;
using Integrate.EmailVerification.Models.Templates;
using Moq;
using NUnit.Framework.Legacy;

namespace Integrate.EmailVerification.Tests.TestApplication.Features.Utility
{
    [TestFixture]
    public class EmailHelperTests
    {
        private Mock<IMXRecordChecker> _mxCheckerMock;
        private EmailHelper _emailHelper;

        [SetUp]
        public void Setup()
        {
            _mxCheckerMock = new Mock<IMXRecordChecker>();
            _emailHelper = new EmailHelper(_mxCheckerMock.Object);
        }

        
        [TestCase("")]
        [TestCase("invalidEmailWithoutAt")]
        public void GetUserName_ShouldReturnEmpty_WhenEmailIsInvalid(string email)
        {
            var result = _emailHelper.GetUserName(email);
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void GetUserName_ShouldReturnUserPart_WhenEmailIsValid()
        {
            var result = _emailHelper.GetUserName("john.doe@example.com");
            Assert.That(result, Is.EqualTo("john.doe"));
        }

        [Test]
        public void GetTLD_ShouldReturnTLD_WhenDomainIsValid()
        {
            var result = _emailHelper.GetTLD("example.co.uk");
            Assert.That(result, Is.EqualTo("uk"));
        }

        [Test]
        public void GetTLD_ShouldReturnEmpty_WhenDomainIsEmpty()
        {
            var result = _emailHelper.GetTLD("");
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void GetDomainParts_ShouldReturnEmptyList_WhenEmailIsInvalid()
        {
            var result = _emailHelper.GetDomainParts("invalidEmail");
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetDomainParts_ShouldReturnParts_WhenEmailIsValid()
        {
            var result = _emailHelper.GetDomainParts("user@mail.example.com");
            CollectionAssert.AreEqual(new List<string> { "mail", "example", "com" }, result);
        }

        [Test]
        public void GetDomain_ShouldReturnEmpty_WhenEmailIsInvalid()
        {
            var result = _emailHelper.GetDomain("invalidEmail");
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void GetDomain_ShouldReturnDomain_WhenEmailIsValid()
        {
            var result = _emailHelper.GetDomain("john@sub.example.com");
            Assert.That(result, Is.EqualTo("sub.example.com"));
        }

        [Test]
        public async Task GetParentDomain_ShouldCallMXChecker_AndReturnResult()
        {
            var expected = new MxRecordsTemplate { ParentDomain = "parent.com" };
            _mxCheckerMock.Setup(m => m.GetParentDomain("example.com")).ReturnsAsync(expected);

            var result = await _emailHelper.GetParentDomain("example.com");

            Assert.That(result, Is.EqualTo(expected));
            _mxCheckerMock.Verify(m => m.GetParentDomain("example.com"), Times.Once);
        }
    }
}
