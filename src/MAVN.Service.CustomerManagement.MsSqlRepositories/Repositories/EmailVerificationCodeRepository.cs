using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Lykke.Common.MsSql;
using MAVN.Service.CustomerManagement.MsSqlRepositories.Contexts;
using MAVN.Service.CustomerManagement.MsSqlRepositories.Entities;
using MAVN.Service.CustomerManagement.Domain.Models;
using MAVN.Service.CustomerManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MAVN.Service.CustomerManagement.MsSqlRepositories.Repositories
{
    public class EmailVerificationCodeRepository : IEmailVerificationCodeRepository
    {
        private readonly MsSqlContextFactory<CmContext> _contextFactory;
        private readonly TimeSpan _verificationEmailExpirePeriod;

        public EmailVerificationCodeRepository(MsSqlContextFactory<CmContext> contextFactory, TimeSpan verificationEmailExpirePeriod)
        {
            _contextFactory = contextFactory;
            _verificationEmailExpirePeriod = verificationEmailExpirePeriod;
        }

        public async Task<IVerificationCode> CreateOrUpdateAsync(string customerId, string verificationCode)
        {
            var entity =
                EmailVerificationCodeEntity.Create(customerId, verificationCode, _verificationEmailExpirePeriod);
            
            using (var context = _contextFactory.CreateDataContext())
            {
                await context.EmailVerificationCodes.AddAsync(entity);

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateException e)
                {
                    if (e.InnerException is SqlException sqlException &&
                        sqlException.Number == MsSqlErrorCodes.PrimaryKeyConstraintViolation)
                    {
                        context.EmailVerificationCodes.Update(entity);

                        await context.SaveChangesAsync();
                    }
                    else throw;
                }
            }

            return entity;
        }

        public async Task<IVerificationCode> GetByValueAsync(string value)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var entity = await context.EmailVerificationCodes.SingleOrDefaultAsync(x => x.VerificationCode == value);

                return entity;
            }
        }

        public async Task<IVerificationCode> GetByCustomerAsync(string customerId)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var entity = await context.EmailVerificationCodes.FindAsync(customerId);

                return entity;
            }
        }

        public async Task SetAsVerifiedAsync(string value)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var entity = await context.EmailVerificationCodes.SingleOrDefaultAsync(x => x.VerificationCode == value);

                if (entity == null)
                    throw new InvalidOperationException($"Verification code {value} doesn't exist");

                entity.IsVerified = true;

                context.EmailVerificationCodes.Update(entity);

                await context.SaveChangesAsync();
            }
        }
    }
}
