using System;

namespace Gobi.InSync.App.Persistence.Configurations
{
    public sealed class DbConfiguration
    {
        public string DbFolder { get; set; } =
            $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.insync/db";
    }
}