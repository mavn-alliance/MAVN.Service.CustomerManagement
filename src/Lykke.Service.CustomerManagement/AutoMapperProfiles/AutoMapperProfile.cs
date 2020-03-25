using AutoMapper;
using JetBrains.Annotations;
using Lykke.Service.Credentials.Client.Models.Responses;
using Lykke.Service.CustomerManagement.Client.Models;
using Lykke.Service.CustomerManagement.Client.Models.Requests;
using Lykke.Service.CustomerManagement.Client.Models.Responses;
using Lykke.Service.CustomerManagement.Domain;
using Lykke.Service.CustomerManagement.Domain.Models;
using PasswordResetErrorResponse = Lykke.Service.Credentials.Client.Models.Responses.PasswordResetErrorResponse;

namespace Lykke.Service.CustomerManagement.AutoMapperProfiles
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<AuthResultModel, AuthenticateResponseModel>();
            CreateMap<RegistrationResultModel, RegistrationResponseModel>();

            CreateMap<ConfirmVerificationCodeResultModel, VerificationCodeConfirmationResponseModel>();

            CreateMap<ChangePasswordResultModel, ChangePasswordResponseModel>();
            CreateMap<PasswordResetResponseModel, PasswordResetError>()
                .ForMember(x => x.Error, c => c.MapFrom(s => s.ErrorCode));

            CreateMap<PasswordResetErrorResponse, PasswordResetError>();
            CreateMap<PasswordResetError, Client.Models.PasswordResetErrorResponse>();

            CreateMap<VerificationCodeResult, VerificationCodeResponseModel>();

            CreateMap<ResetIdentifierValidationResponse, ValidateResetIdentifierModel>();

            CreateMap<ValidateResetIdentifierModel, ValidateResetIdentifierResponse>();
            CreateMap<BatchCustomerStatusesModel, BatchOfCustomerStatusesResponse>();

            CreateMap<RegistrationRequestModel, RegistrationRequestDto>();
        }
    }
}
