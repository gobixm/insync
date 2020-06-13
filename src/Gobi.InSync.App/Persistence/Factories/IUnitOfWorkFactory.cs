namespace Gobi.InSync.App.Persistence.Factories
{
    public interface IUnitOfWorkFactory
    {
        IUnitOfWork Create();
    }
}