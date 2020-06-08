using System.Threading.Tasks;

namespace Gobi.InSync.App.Synchronizers
{
    public interface IFolderSynchronizer
    {
        Task SyncFolder(string sourcePath, string targetPath);
    }
}