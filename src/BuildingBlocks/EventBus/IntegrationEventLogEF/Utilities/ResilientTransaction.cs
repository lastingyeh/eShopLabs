using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.eShopOnContainers.BuildingBlocks.IntegrationEventLogEF.Utilities
{
    public class ResilientTransaction
    {
        private DbContext _context;
        private ResilientTransaction(DbContext context) =>
            _context = context;

        public static ResilientTransaction New(DbContext context) =>
            new ResilientTransaction(context);

        public async Task ExecuteAsync(Func<Task> action)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = _context.Database.BeginTransaction();

                await action();
                transaction.Commit();
            });
        }
    }
}
