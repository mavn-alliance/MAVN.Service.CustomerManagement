using System.Data.SqlClient;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using MAVN.Persistence.PostgreSQL.Legacy;
using MAVN.Service.CustomerManagement.Domain.Models;
using MAVN.Service.CustomerManagement.Domain.Repositories;
using MAVN.Service.CustomerManagement.MsSqlRepositories.Contexts;
using MAVN.Service.CustomerManagement.MsSqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MAVN.Service.CustomerManagement.MsSqlRepositories.Repositories
{
    public class CustomersRegistrationReferralDataRepository : ICustomersRegistrationReferralDataRepository
    {
        private readonly PostgreSQLContextFactory<CmContext> _contextFactory;
        private readonly ILog _log;

        public CustomersRegistrationReferralDataRepository(
            PostgreSQLContextFactory<CmContext> contextFactory,
            ILogFactory logFactory)
        {
            _contextFactory = contextFactory;
            _log = logFactory.CreateLog(this);
        }

        public async Task AddAsync(string customerId, string referralCode)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var entity = CustomerRegistrationReferralDataEntity.Create(customerId, referralCode);

                context.CustomersRegistrationReferralData.Add(entity);

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateException e)
                {
                    if (e.InnerException is PostgresException sqlException
                        && sqlException.SqlState == PostgresErrorCodes.UniqueViolation)
                    {
                        _log.Error(e, "Error on customer referral data context saving");
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        public async Task<ICustomerRegistrationReferralData> GetAsync(string customerId)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var result = await context.CustomersRegistrationReferralData.FindAsync(customerId);

                return result;
            }
        }

        public async Task DeleteAsync(string customerId)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var entity = new CustomerRegistrationReferralDataEntity { CustomerId = customerId };

                context.CustomersRegistrationReferralData.Attach(entity);

                context.Remove(entity);

                await context.SaveChangesAsync();
            }
        }
    }
}
