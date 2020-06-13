using System;
using System.Threading.Tasks;

namespace Gobi.InSync.App.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        Task CommitAsync();

        T GetRepository<T>();
    }
}