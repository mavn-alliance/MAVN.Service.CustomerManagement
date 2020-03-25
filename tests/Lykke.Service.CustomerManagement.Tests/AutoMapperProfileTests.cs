using AutoMapper;
using Lykke.Service.CustomerManagement.AutoMapperProfiles;
using Xunit;

namespace Lykke.Service.CustomerManagement.Tests
{
    public class AutoMapperProfileTests
    {
        [Fact]
        public void Mapping_Configuration_Is_Correct()
        {
            // arrange
            var mockMapper = new MapperConfiguration(cfg => { cfg.AddProfile(new AutoMapperProfile()); });
            var mapper = mockMapper.CreateMapper();

            // act
            mapper.ConfigurationProvider.AssertConfigurationIsValid();

            // assert
            Assert.True(true);
        }
    }
}
