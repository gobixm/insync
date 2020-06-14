using System.IO;
using Gobi.Bootstrap.AspNetCore.Extensions;
using Gobi.InSync.App.Dispatchers;
using Gobi.InSync.App.Persistence;
using Gobi.InSync.App.Persistence.Configurations;
using Gobi.InSync.App.Persistence.Factories;
using Gobi.InSync.App.Persistence.Repositories;
using Gobi.InSync.App.Services;
using Gobi.InSync.App.Synchronizers;
using Gobi.InSync.App.Watchers;
using Gobi.InSync.Service.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gobi.InSync.Service
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
            services.Configure<DbConfiguration>(Configuration.GetSection("Db"));
            services.AddDbContext<InSyncDbContext>(options =>
            {
                var dbConfig = Configuration.GetSection("Db").Get<DbConfiguration>() ?? new DbConfiguration();
                var connectionString = $"Data Source={dbConfig.DbFolder}/insync.db";
                options.UseSqlite(connectionString,
                    o => o.MigrationsAssembly(typeof(InSyncDbContext).Assembly.FullName));
            });

            services.AddSingleton<IWatchService, WatchService>();
            services.AddTransient<IFileEventDispatcher, FileEventDispatcher>();
            services.AddTransient<IFolderSynchronizer, FolderSynchronizer>();
            services.AddTransient<IFileWatcherFactory, FileWatcherFactory>();
            services.AddSingleton<ISyncService, SyncService>();
            services.AddTransient<IUnitOfWorkFactory, UnitOfWorkFactory>();
            services.AddTransient<ISyncWatchRepository, SyncWatchRepository>();

            services.AddBootstrap();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<InSyncGrpcService>();

                endpoints.MapGet("/",
                    async context =>
                    {
                        await context.Response.WriteAsync(
                            "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                    });
            });

            app.UseBootstrap(async (provider, state, cancel) =>
            {
                var dbConfig = provider.GetRequiredService<IOptions<DbConfiguration>>().Value;
                if (!Directory.Exists(dbConfig.DbFolder)) Directory.CreateDirectory(Path.Combine(dbConfig.DbFolder));

                await provider.GetRequiredService<InSyncDbContext>().Database.MigrateAsync(cancel);
                var unitOfWorkFactory = provider.GetRequiredService<IUnitOfWorkFactory>();
                using var unitOfWork = unitOfWorkFactory.Create();
                await provider.GetRequiredService<ISyncService>().StartAsync(unitOfWork);
            });
        }
    }
}