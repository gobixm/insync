namespace Gobi.InSync.App.Watchers.Models
{
    public class FileDeleted : IFileEvent
    {
        public string FileName { get; set; }
        public string Path { get; set; }
    }
}