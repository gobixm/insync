using System.Linq;
using System.Threading.Tasks;
using Gobi.InSync.App.Persistence.Factories;
using Gobi.InSync.App.Persistence.Models;
using Gobi.InSync.App.Services;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Gobi.InSync.Service.Services
{
    public class InSyncGrpcService : InSync.InSyncBase
    {
        private readonly ILogger<InSyncGrpcService> _logger;
        private readonly ISyncService _syncService;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public InSyncGrpcService(
            ILogger<InSyncGrpcService> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            ISyncService syncService)
        {
            _logger = logger;
            _unitOfWorkFactory = unitOfWorkFactory;
            _syncService = syncService;
        }

        public override async Task<AddSyncResponse> AddSync(AddSyncRequest request, ServerCallContext context)
        {
            using var unitOfWork = _unitOfWorkFactory.Create();
            var syncWatch = new SyncWatch
            {
                SourcePath = request.SourcePath,
                TargetPath = request.TargetPath
            };
            await _syncService.AddWatchAsync(unitOfWork, syncWatch);
            await unitOfWork.CommitAsync();

            return new AddSyncResponse
            {
                Result = ErrorCode.Ok,
                ErrorMessage = string.Empty,
                Sync = new Sync
                {
                    SourcePath = syncWatch.SourcePath,
                    TargetPath = syncWatch.TargetPath
                }
            };
        }

        public override async Task<ListSyncResponse> ListSync(ListSyncRequest request, ServerCallContext context)
        {
            using var unitOfWork = _unitOfWorkFactory.Create();
            var watches = await _syncService.GetWatchesAsync(unitOfWork);
            await unitOfWork.CommitAsync();

            return new ListSyncResponse
            {
                Result = ErrorCode.Ok,
                Watches =
                {
                    watches.Select(x => new Sync
                    {
                        SourcePath = x.SourcePath,
                        TargetPath = x.TargetPath
                    })
                }
            };
        }

        public override async Task<RemoveSyncResponse> RemoveSync(RemoveSyncRequest request, ServerCallContext context)
        {
            using var unitOfWork = _unitOfWorkFactory.Create();
            var watches = await _syncService.DeleteWatchesAsync(unitOfWork, request.SourcePath, request.TargetPath);
            await unitOfWork.CommitAsync();

            return new RemoveSyncResponse
            {
                Result = ErrorCode.Ok,
                Removed =
                {
                    watches.Select(x => new Sync
                    {
                        SourcePath = x.SourcePath,
                        TargetPath = x.TargetPath
                    })
                }
            };
        }
    }
}