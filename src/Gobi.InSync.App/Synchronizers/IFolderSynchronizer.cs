namespace Gobi.InSync.App.Synchronizers
{
    public interface IFolderSynchronizer
    {
        void SyncFolder(string sourcePath, string targetPath);
    }
}