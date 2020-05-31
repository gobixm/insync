namespace Gobi.InSync.App.Watchers.Models
{
    public class FileRenamed : IFileEvent
    {
        public string OldFileName { get; set; }
        public string OldPath { get; set; }
        public string FileName { get; set; }
        public string Path { get; set; }
    }
}