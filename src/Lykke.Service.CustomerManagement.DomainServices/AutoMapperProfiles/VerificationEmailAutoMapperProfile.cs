using AutoMapper;
using Lykke.Service.CustomerManagement.Domain;
using Lykke.Service.CustomerManagement.Domain.Models;

namespace Lykke.Service.CustomerManagement.DomainServices.AutoMapperProfiles
{
    public class VerificationEmailAutoMapperProfile : Profile
    {
        public VerificationEmailAutoMapperProfile()
        {
            CreateMap<IVerificationCode, VerificationCodeModel>();
        }
    }
}
