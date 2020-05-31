using Microsoft.Extensions.Logging;

namespace Gobi.InSync.Service.Services
{
    public class InSyncGrpcService : InSync.InSyncBase
    {
        private readonly ILogger<InSyncGrpcService> _logger;

        public InSyncGrpcService(ILogger<InSyncGrpcService> logger)
        {
            _logger = logger;
        }
    }
}