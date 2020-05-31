namespace Gobi.InSync.App.Watchers.Models
{
    public interface IFileEvent
    {
        public string FileName { get; set; }
        public string Path { get; set; }
    }
}