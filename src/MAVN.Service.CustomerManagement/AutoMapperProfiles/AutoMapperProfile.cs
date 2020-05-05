using AutoMapper;
using JetBrains.Annotations;
using MAVN.Service.Credentials.Client.Models.Responses;
using MAVN.Service.CustomerManagement.Client.Models;
using MAVN.Service.CustomerManagement.Client.Models.Requests;
using MAVN.Service.CustomerManagement.Client.Models.Responses;
using MAVN.Service.CustomerManagement.Domain;
using MAVN.Service.CustomerManagement.Domain.Models;
using PasswordResetErrorResponse = MAVN.Service.Credentials.Client.Models.Responses.PasswordResetErrorResponse;

namespace MAVN.Service.CustomerManagement.AutoMapperProfiles
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
