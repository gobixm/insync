namespace Gobi.InSync.App.Watchers.Models
{
    public class FileCreated : IFileEvent
    {
        public string FileName { get; set; }
        public string Path { get; set; }
    }
}