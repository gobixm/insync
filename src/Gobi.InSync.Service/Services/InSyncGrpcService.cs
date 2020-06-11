using System.Threading.Tasks;
using Gobi.InSync.App.Services;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Gobi.InSync.Service.Services
{
    public class InSyncGrpcService : InSync.InSyncBase
    {
        private readonly ILogger<InSyncGrpcService> _logger;
        private readonly ISyncService _syncService;

        public InSyncGrpcService(ILogger<InSyncGrpcService> logger, ISyncService syncService)
        {
            _logger = logger;
            _syncService = syncService;
        }

        public override Task<AddSyncResponse> AddSync(AddSyncRequest request, ServerCallContext context)
        {
            var watch = _syncService.AddSyncFolder(request.SourcePath, request.TargetPath);

            return Task.FromResult(new AddSyncResponse
            {
                Result = ErrorCode.Ok,
                ErrorMessage = string.Empty,
                WatchFolder = new WatchFolder
                {
                    SourcePath = watch.Source,
                    TargetPath = watch.Target
                }
            });
        }
    }
}