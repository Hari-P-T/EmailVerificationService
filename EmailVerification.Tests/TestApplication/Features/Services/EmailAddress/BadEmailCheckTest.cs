using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Application.Features.Services.EmailAddress;
using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Models.Enum;
using Integrate.EmailVerification.Models.Templates;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Integrate.EmailVerification.Tests.EmailAddress
{
    [TestFixture]
    public class BadEmailCheckTests
    {
        private Mock<IEmailValidationChecksInfoFactory> _factoryMock;
        private Mock<IServiceProvider> _serviceProviderMock;
        private Mock<IEmailValidationChecker> _spfMock;
        private Mock<IEmailValidationChecker> _dmarcMock;
        private Mock<IEmailValidationChecker> _dkimMock;
        private EmailValidationCheck _check;
        private RecordsTemplate _record;
        private EmailValidationChecksInfo _passingResult;
        private EmailValidationChecksInfo _failingResult;

        [SetUp]
        public void Setup()
        {
            _factoryMock = new Mock<IEmailValidationChecksInfoFactory>();
            _serviceProviderMock = new Mock<IServiceProvider>();

            _spfMock = new Mock<IEmailValidationChecker>();
            _spfMock.Setup(v => v.Name).Returns(CheckNames.SpfRecord);

            _dmarcMock = new Mock<IEmailValidationChecker>();
            _dmarcMock.Setup(v => v.Name).Returns(CheckNames.DmarcRecord);

            _dkimMock = new Mock<IEmailValidationChecker>();
            _dkimMock.Setup(v => v.Name).Returns(CheckNames.DkimRecord);

            _check = new EmailValidationCheck
            {
                AllotedScore = 10,
                Name = CheckNames.BadEmail
            };

            _record = new RecordsTemplate(
                _userName: "test",
                _tLD: "com",
                _email: "test@example.com",
                _domain: "example.com",
                _parentDomain: "example.com",
                _mxRecords: new List<string>());

            _passingResult = new EmailValidationChecksInfo(_check)
            {
                ObtainedScore = 10,
                Passed = true,
                Performed = true
            };

            _failingResult = new EmailValidationChecksInfo(_check)
            {
                ObtainedScore = 0,
                Passed = false,
                Performed = true
            };
        }

        [Test]
        public async Task EmailCheckValidator_AllDependenciesPassed_ReturnsFullScore()
        {
            // Arrange
            _spfMock.Setup(x => x.EmailCheckValidator(_record, _check)).ReturnsAsync(_passingResult);
            _dmarcMock.Setup(x => x.EmailCheckValidator(_record, _check)).ReturnsAsync(_passingResult);
            _dkimMock.Setup(x => x.EmailCheckValidator(_record, _check)).ReturnsAsync(_passingResult);

            var validators = new List<IEmailValidationChecker>
            {
                _spfMock.Object,
                _dmarcMock.Object,
                _dkimMock.Object
            };

            _serviceProviderMock
                .Setup(x => x.GetService(typeof(IEnumerable<IEmailValidationChecker>)))
                .Returns(validators);

            _factoryMock
                .Setup(x => x.Create(_check, 10, true, true))
                .Returns(_passingResult);

            var badEmailCheck = new BadEmailCheck(_factoryMock.Object, _serviceProviderMock.Object);

            // Act
            var result = await badEmailCheck.EmailCheckValidator(_record, _check);

            // Assert
            Assert.That(result.Passed, Is.True);
            Assert.That(result.ObtainedScore, Is.EqualTo(10));
        }

        [Test]
        public async Task EmailCheckValidator_AnyDependencyFails_ReturnsZeroScore()
        {
            // Arrange
            _spfMock.Setup(x => x.EmailCheckValidator(_record, _check)).ReturnsAsync(_passingResult);
            _dmarcMock.Setup(x => x.EmailCheckValidator(_record, _check)).ReturnsAsync(_failingResult); // Failing
            _dkimMock.Setup(x => x.EmailCheckValidator(_record, _check)).ReturnsAsync(_passingResult);

            var validators = new List<IEmailValidationChecker>
            {
                _spfMock.Object,
                _dmarcMock.Object,
                _dkimMock.Object
            };

            _serviceProviderMock
                .Setup(x => x.GetService(typeof(IEnumerable<IEmailValidationChecker>)))
                .Returns(validators);

            _factoryMock
                .Setup(x => x.Create(_check, 0, false, true))
                .Returns(_failingResult);

            var badEmailCheck = new BadEmailCheck(_factoryMock.Object, _serviceProviderMock.Object);

            // Act
            var result = await badEmailCheck.EmailCheckValidator(_record, _check);

            // Assert
            Assert.That(result.Passed, Is.False);
            Assert.That(result.ObtainedScore, Is.EqualTo(0));
        }
    }
}
