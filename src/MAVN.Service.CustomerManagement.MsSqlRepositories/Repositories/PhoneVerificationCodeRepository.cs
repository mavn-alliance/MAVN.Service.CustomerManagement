using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using MAVN.Persistence.PostgreSQL.Legacy;
using MAVN.Service.CustomerManagement.Domain.Models;
using MAVN.Service.CustomerManagement.Domain.Repositories;
using MAVN.Service.CustomerManagement.MsSqlRepositories.Contexts;
using MAVN.Service.CustomerManagement.MsSqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MAVN.Service.CustomerManagement.MsSqlRepositories.Repositories
{
    public class PhoneVerificationCodeRepository : IPhoneVerificationCodeRepository
    {
        private readonly PostgreSQLContextFactory<CmContext> _contextFactory;

        public PhoneVerificationCodeRepository(PostgreSQLContextFactory<CmContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IPhoneVerificationCode> CreateOrUpdateAsync(string customerId, string verificationCode, TimeSpan expirationPeriod)
        {
            var entity =
                PhoneVerificationCodeEntity.Create(customerId, verificationCode, expirationPeriod);

            using (var context = _contextFactory.CreateDataContext())
            {
                await context.PhoneVerificationCodes.AddAsync(entity);

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateException e)
                {
                    if (e.InnerException is PostgresException sqlException &&
                        sqlException.SqlState == PostgresErrorCodes.UniqueViolation)
                    {
                        context.PhoneVerificationCodes.Update(entity);

                        await context.SaveChangesAsync();
                    }
                    else throw;
                }
            }

            return entity;
        }

        public async Task<IPhoneVerificationCode> GetByCustomerAndCodeAsync(string customerId, string verificationCode)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var entity = await context.PhoneVerificationCodes.SingleOrDefaultAsync(x =>
                    x.VerificationCode == verificationCode && x.CustomerId == customerId);

                return entity;
            }
        }

        public async Task<IPhoneVerificationCode> GetByCustomerAsync(string customerId)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var entity = await context.PhoneVerificationCodes.FindAsync(customerId);

                return entity;
            }
        }

        public async Task RemoveAsync(string customerId, string verificationCode)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var entity = await context.PhoneVerificationCodes.SingleOrDefaultAsync(x =>
                    x.VerificationCode == verificationCode && x.CustomerId == customerId);

                if (entity == null)
                    throw new InvalidOperationException(
                        $"Verification code {verificationCode} for customer {customerId} doesn't exist");

                context.PhoneVerificationCodes.Remove(entity);

                await context.SaveChangesAsync();
            }
        }
    }
}
