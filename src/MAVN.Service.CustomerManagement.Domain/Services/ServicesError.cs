namespace MAVN.Service.CustomerManagement.Domain.Services
{
    public enum ServicesError
    {
        None = 0,
        LoginNotFound,
        PasswordMismatch,
        RegisteredWithAnotherPassword,
        AlreadyRegistered,
        InvalidLoginFormat,
        InvalidPasswordFormat,
        LoginExistsWithDifferentProvider,
        AlreadyRegisteredWithGoogle,
        CustomerBlocked,
        InvalidCountryOfNationalityId,
        EmailIsNotAllowed,
        CustomerProfileDeactivated,
    }
}
