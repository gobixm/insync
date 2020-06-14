using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Gobi.InSync.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(typeof(Program).Assembly.Location));
            
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
        }
    }
}