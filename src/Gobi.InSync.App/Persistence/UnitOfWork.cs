using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Gobi.InSync.App.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly InSyncDbContext _context;
        private readonly IServiceScope _scope;

        public UnitOfWork(IServiceProvider serviceProvider)
        {
            _scope = serviceProvider.CreateScope();
            _context = _scope.ServiceProvider.GetRequiredService<InSyncDbContext>();
        }

        public void Dispose()
        {
            _context?.Dispose();
            _scope?.Dispose();
        }

        public T GetRepository<T>()
        {
            return _scope.ServiceProvider.GetRequiredService<T>();
        }

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}