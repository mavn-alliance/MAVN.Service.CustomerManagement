using AutoMapper;
using MAVN.Service.CustomerManagement.Domain;
using MAVN.Service.CustomerManagement.Domain.Models;

namespace MAVN.Service.CustomerManagement.DomainServices.AutoMapperProfiles
{
    public class VerificationEmailAutoMapperProfile : Profile
    {
        public VerificationEmailAutoMapperProfile()
        {
            CreateMap<IVerificationCode, VerificationCodeModel>();
        }
    }
}
