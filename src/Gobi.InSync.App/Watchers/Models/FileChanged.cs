namespace Gobi.InSync.App.Watchers.Models
{
    public class FileChanged : IFileEvent
    {
        public string FileName { get; set; }
        public string Path { get; set; }
    }
}