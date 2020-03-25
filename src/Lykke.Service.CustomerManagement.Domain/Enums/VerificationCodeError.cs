namespace Lykke.Service.CustomerManagement.Domain.Enums
{
    public enum VerificationCodeError
    {
        None,
        AlreadyVerified,
        VerificationCodeDoesNotExist,
        VerificationCodeMismatch,
        VerificationCodeExpired,
        CustomerDoesNotExist,
        ReachedMaximumRequestForPeriod,
        CustomerPhoneIsMissing,
        PhoneAlreadyExists,
    }
}
