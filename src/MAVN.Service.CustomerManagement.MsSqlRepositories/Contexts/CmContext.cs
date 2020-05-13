using System.Data.Common;
using MAVN.Common.MsSql;
using MAVN.Service.CustomerManagement.MsSqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace MAVN.Service.CustomerManagement.MsSqlRepositories.Contexts
{
    public class CmContext : MsSqlContext
    {
        private const string Schema = "customer_management";

        internal DbSet<EmailVerificationCodeEntity> EmailVerificationCodes { get; set; }

        internal DbSet<PhoneVerificationCodeEntity> PhoneVerificationCodes { get; set; }

        internal DbSet<CustomerFlagsEntity> CustomerFlags { get; set; }

        internal DbSet<CustomerRegistrationReferralDataEntity> CustomersRegistrationReferralData { get; set; }

        public CmContext() : base(Schema)
        {
        }

        public CmContext(string connectionString, bool isTraceEnabled)
            : base(Schema, connectionString, isTraceEnabled)
        {
        }

        public CmContext(DbConnection dbConnection)
            : base(Schema, dbConnection)
        {
        }

        protected override void OnLykkeModelCreating(ModelBuilder modelBuilder)
        {
            var verificationCodeEntityBuilder = modelBuilder.Entity<EmailVerificationCodeEntity>();
            verificationCodeEntityBuilder.HasIndex(c => c.VerificationCode).IsUnique();

            var phoneVerificationCodeEntityBuilder = modelBuilder.Entity<PhoneVerificationCodeEntity>();
            phoneVerificationCodeEntityBuilder.HasIndex(c => new{c.CustomerId, c.VerificationCode}).IsUnique();
        }
    }
}
