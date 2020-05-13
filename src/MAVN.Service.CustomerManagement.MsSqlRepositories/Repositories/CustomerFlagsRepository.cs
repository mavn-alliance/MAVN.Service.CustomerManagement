using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using MAVN.Common.MsSql;
using MAVN.Service.CustomerManagement.Domain.Models;
using MAVN.Service.CustomerManagement.Domain.Repositories;
using MAVN.Service.CustomerManagement.MsSqlRepositories.Contexts;
using MAVN.Service.CustomerManagement.MsSqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace MAVN.Service.CustomerManagement.MsSqlRepositories.Repositories
{
    public class CustomerFlagsRepository : ICustomerFlagsRepository
    {
        private readonly MsSqlContextFactory<CmContext> _contextFactory;

        public CustomerFlagsRepository(
            MsSqlContextFactory<CmContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }
        
        public async Task<ICustomerFlags> CreateOrUpdateAsync(string customerId, bool isBlocked)
        {
            var entity = CustomerFlagsEntity.Create(customerId, isBlocked);
            
            using (var context = _contextFactory.CreateDataContext())
            {
                await context.CustomerFlags.AddAsync(entity);

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateException e)
                {
                    if (e.InnerException is SqlException sqlException &&
                        sqlException.Number == MsSqlErrorCodes.PrimaryKeyConstraintViolation)
                    {
                        context.CustomerFlags.Update(entity);

                        await context.SaveChangesAsync();
                    }
                    else throw;
                }
            }

            return entity;
        }

        public async Task<ICustomerFlags> GetByCustomerIdAsync(string customerId)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var entity = await context.CustomerFlags.FindAsync(customerId);

                return entity;
            }
        }

        public async Task<IEnumerable<ICustomerFlags>> GetByCustomerIdsAsync(string[] customerIds)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var entity = await context.CustomerFlags
                    .Where(x => customerIds.Contains(x.CustomerId))
                    .ToListAsync();

                return entity;
            }
        }
    }
}
