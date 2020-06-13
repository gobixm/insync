using Gobi.InSync.App.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Gobi.InSync.App.Persistence
{
    public class InSyncDbContext : DbContext
    {
        public InSyncDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<SyncWatch> Watches { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SyncWatch>()
                .HasKey(x => new {x.SourcePath, x.TargetPath});
        }
    }
}