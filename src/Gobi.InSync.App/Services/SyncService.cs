using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gobi.InSync.App.Persistence;
using Gobi.InSync.App.Persistence.Models;
using Gobi.InSync.App.Persistence.Repositories;

namespace Gobi.InSync.App.Services
{
    public class SyncService : ISyncService
    {
        private readonly IWatchService _watchService;

        public SyncService(IWatchService watchService)
        {
            _watchService = watchService;
        }

        public async Task StartAsync(IUnitOfWork unitOfWork)
        {
            await StartSavedWatches(unitOfWork);
        }

        public async Task AddWatchAsync(IUnitOfWork unitOfWork, SyncWatch syncWatch)
        {
            var syncWatchRepository = unitOfWork.GetRepository<ISyncWatchRepository>();
            await syncWatchRepository.AddAsync(syncWatch);
        }

        public async Task<List<SyncWatch>> DeleteWatchesAsync(IUnitOfWork unitOfWork, string sourceFolder,
            string targetFolder)
        {
            if (string.IsNullOrEmpty(sourceFolder)) throw new ArgumentNullException(nameof(sourceFolder));

            var syncWatchRepository = unitOfWork.GetRepository<ISyncWatchRepository>();

            List<SyncWatch> watchesToDelete;
            if (string.IsNullOrEmpty(targetFolder))
            {
                watchesToDelete = await syncWatchRepository.GetBySourceAsync(sourceFolder);
            }
            else
            {
                var watchToDelete = await syncWatchRepository.GetAsync(sourceFolder, targetFolder);
                if (watchToDelete == null) throw new ArgumentException("Watch not found");
                watchesToDelete = new List<SyncWatch> {watchToDelete};
            }

            if (!watchesToDelete.Any()) throw new ArgumentException("No watches found");

            syncWatchRepository.Delete(watchesToDelete);

            return watchesToDelete;
        }

        public async Task<List<SyncWatch>> GetWatchesAsync(IUnitOfWork unitOfWork)
        {
            var syncWatchRepository = unitOfWork.GetRepository<ISyncWatchRepository>();
            return await syncWatchRepository.GetAllAsync();
        }

        private async Task StartSavedWatches(IUnitOfWork unitOfWork)
        {
            var syncWatchRepository = unitOfWork.GetRepository<ISyncWatchRepository>();
            var savedWatches = await GetSavedWatches(syncWatchRepository);
            savedWatches.ForEach(x => _watchService.StartWatching(x.SourcePath, x.TargetPath));
        }

        private async Task<List<SyncWatch>> GetSavedWatches(ISyncWatchRepository repository)
        {
            return await repository.GetAllAsync();
        }
    }
}