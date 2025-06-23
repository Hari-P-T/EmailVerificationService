using System.Threading.Tasks;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Application.Features.Services.SMTPChecks;
using Integrate.EmailVerification.Models.Templates;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using static NUnit.Framework.Assert;

namespace Integrate.EmailVerification.Tests.SMTPChecks
{
    [TestFixture]
    public class DMARCRecordCheckTests
    {
        private Mock<IEmailValidationChecksInfoFactory> _factoryMock;
        private Mock<DMARCRecordCheck> _mockedCheck;
        private EmailValidationCheck _check;
        private RecordsTemplate _record;

        [SetUp]
        public void SetUp()
        {
            _factoryMock = new Mock<IEmailValidationChecksInfoFactory>();

            _mockedCheck = new Mock<DMARCRecordCheck>(_factoryMock.Object)
            {
                CallBase = true
            };

            _check = new EmailValidationCheck
            {
                Name = "DmarcRecord",
                AllotedScore = 10
            };

            _record = new RecordsTemplate(
                _userName: "john",
                _tLD: "com",
                _email: "john@example.com",
                _domain: "example.com",
                _parentDomain: "example.com",
                _mxRecords: new List<string> { "mx.example.com" }
            );

            _factoryMock.Setup(f =>
                f.Create(It.IsAny<EmailValidationCheck>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>())
            ).Returns((EmailValidationCheck chk, int score, bool passed, bool valid) =>
                new EmailValidationChecksInfo(chk)
                {
                    CheckName = chk.Name,
                    ObtainedScore = score,
                    Passed = passed,
                    Performed = true,
                    Email = _record.Email
                });
        }

        [TestCase(10, true)]
        [TestCase(7, true)]
        [TestCase(5, true)]
        [TestCase(4, false)]
        [TestCase(0, false)]
        public async Task EmailCheckValidator_ShouldReturn_CorrectScoreBasedOnPolicy(int policyScore, bool expectedPassed)
        {
            _mockedCheck.Setup(x => x.EvaluateDmarcPolicyScore(It.IsAny<string>()))
                        .ReturnsAsync(policyScore);

            var result = await _mockedCheck.Object.EmailCheckValidator(_record, _check);

            Assert.That(result.Passed, Is.EqualTo(expectedPassed));
            Assert.That(result.ObtainedScore, Is.EqualTo(expectedPassed ? 10 : 0));
        }

        [Test]
        public async Task NameProperty_ShouldReturn_DmarcRecord()
        {
            Assert.That(_mockedCheck.Object.Name, Is.EqualTo("HasADmarcRecord"));
        }


        private class FailingDmarcCheck : DMARCRecordCheck
        {
            public FailingDmarcCheck(IEmailValidationChecksInfoFactory factory) : base(factory) { }

            public async Task<int> CallEvaluateDmarcPolicyScore(string domain)
            {
                throw new System.Exception("DNS error");
            }
        }
    }
}
