using MAVN.Service.CustomerManagement.DomainServices;
using Xunit;

namespace MAVN.Service.CustomerManagement.Tests
{
    public class EmailRestrictionsServiceTests
    {
        private const string FakeEmail = "email@mail.com";
        private const string InvalidEmail = "emailmail.com";
        private const string AllowedEmail = "string@mail.com";
        private const string NotAllowedEmail = "string@mail1.com";
        private const string FakeEmailDomain = "mail.com";

        [Theory]
        [InlineData(FakeEmail)]
        [InlineData("email@mail.bg")]
        [InlineData("EMAIL@test.com")]
        [InlineData("email@lykke-business.ch")]
        public void IsEmailAllowed_AllowedEmailDomainsEmpty_AllEmailsAreAllowed(string email)
        {
            var sut = new EmailRestrictionsService(new string[0], new string[0]);

            var result = sut.IsEmailAllowed(email);

            Assert.True(result);
        }

        [Fact]
        public void IsEmailAllowed_InvalidEmailFormat_EmailNotAllowed()
        {
            var sut = new EmailRestrictionsService(new[] { FakeEmailDomain }, new string[0]);

            var result = sut.IsEmailAllowed(InvalidEmail);

            Assert.False(result);
        }

        [Fact]
        public void IsEmailAllowed_EmailDomainNotAllowedAndSpecificEmailNotAllowed_EmailNotAllowed()
        {
            var sut = new EmailRestrictionsService(new[] { FakeEmailDomain }, new[] { AllowedEmail });

            var result = sut.IsEmailAllowed(NotAllowedEmail);

            Assert.False(result);
        }

        [Fact]
        public void IsEmailAllowed_EmailDomainAllowed_EmailAllowed()
        {
            var sut = new EmailRestrictionsService(new[] { FakeEmailDomain }, new[] { AllowedEmail });

            var result = sut.IsEmailAllowed(FakeEmail);

            Assert.True(result);
        }

        [Fact]
        public void IsEmailAllowed_EmailDomainNotAllowedButTheSpecificEmailIs_EmailAllowed()
        {
            var sut = new EmailRestrictionsService(new[] { FakeEmailDomain }, new[] { AllowedEmail });

            var result = sut.IsEmailAllowed(AllowedEmail);

            Assert.True(result);
        }
    }
}
