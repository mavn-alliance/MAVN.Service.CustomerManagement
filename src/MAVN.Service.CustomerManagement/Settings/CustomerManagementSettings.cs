using System;
using JetBrains.Annotations;

namespace MAVN.Service.CustomerManagement.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class CustomerManagementSettings
    {
        public DbSettings Db { get; set; }
        public RabbitMqSettings RabbitMq { get; set; }
        public TimeSpan VerificationEmailExpirePeriod { get; set; }
        public string VerificationThankYouEmailTemplateId { get; set; }
        public string VerificationThankYouEmailSubjectTemplateId { get; set; }
        public string VerificationEmailTemplateId { get; set; }
        public string VerificationEmailSubjectTemplateId { get; set; }
        public string VerificationEmailVerificationLink { get; set; }
        public string PasswordResetEmailTemplateId { get; set; }
        public string PasswordResetEmailSubjectTemplateId { get; set; }
        public string PasswordResetEmailVerificationLinkTemplate { get; set; }
        public string PasswordSuccessfulResetEmailTemplateId { get; set; }
        public string PasswordSuccessfulResetEmailSubjectTemplateId { get; set; }
        public string PasswordSuccessfulChangeEmailTemplateId { get; set; }
        public string PasswordSuccessfulChangeEmailSubjectTemplateId { get; set; }
        public int GetCustomerBlockStatusBatchRequestMaxSize { get; set; }
        
        public string CustomerBlockEmailTemplateId { get; set; }
        public string CustomerBlockSubjectTemplateId { get; set; }
        public string CustomerUnblockEmailTemplateId { get; set; }
        public string CustomerUnblockSubjectTemplateId { get; set; }
        public string CustomerSupportPhoneNumber { get; set; }
        public TimeSpan PhoneVerificationCodeExpirePeriod { get; set; }
        public int PhoneVerificationCodeLength { get; set; }
        public string PhoneVerificationSmsTemplateId { get; set; }

        public RedisSettings Redis { get; set; }
        public LimitationSettings LimitationSettings { get; set; }
        public RegistrationRestrictionsSettings RegistrationRestrictions { get; set; }
    }
}
