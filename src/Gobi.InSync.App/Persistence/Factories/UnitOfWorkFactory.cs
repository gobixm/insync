using System;

namespace Gobi.InSync.App.Persistence.Factories
{
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public UnitOfWorkFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IUnitOfWork Create()
        {
            return new UnitOfWork(_serviceProvider);
        }
    }
}