using System;
using System.Linq;
using CommandLine;
using CommandLine.Text;
using Gobi.InSync.Service;
using Grpc.Net.Client;

namespace Gobi.InSync.Cli
{
    [Verb("add", HelpText = "Add folder synchronization")]
    internal class AddOptions
    {
        [Option('s', "source", Required = true, HelpText = "Source folder.")]
        public string SourceFolder { get; set; }

        [Option('t', "target", Required = true, HelpText = "Target folder.")]
        public string TargetFolder { get; set; }
    }

    [Verb("list", HelpText = "Show synchronized folders")]
    internal class ListOptions
    {
    }

    [Verb("remove", HelpText = "Remove folder synchronization")]
    internal class RemoveOptions
    {
        [Option('s', "source", Required = true, HelpText = "Source folder.")]
        public string SourceFolder { get; set; }

        [Option('t', "target", Required = false,
            HelpText = "Target folder. Or empty to remove all target of specified source.")]
        public string TargetFolder { get; set; }
    }

    internal class Program
    {
        private static int Main(string[] args)
        {
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            using var channel = GrpcChannel.ForAddress("http://localhost:5000");
            var client = new Service.InSync.InSyncClient(channel);
            
            return Parser.Default.ParseArguments<AddOptions, RemoveOptions, ListOptions>(args)
                .MapResult(
                    (AddOptions opts) => Add(opts, client),
                    (RemoveOptions opts) => Remove(opts, client),
                    (ListOptions opts) => List(opts, client),
                    errs => 1);
        }

        private static int Add(AddOptions addOptions, Service.InSync.InSyncClient client)
        {
            var request = new AddSyncRequest
            {
                SourcePath = addOptions.SourceFolder ?? string.Empty,
                TargetPath = addOptions.TargetFolder ?? string.Empty
            };

            var result = client.AddSync(request);
            return 0;
        }

        private static int Remove(RemoveOptions removeOptions, Service.InSync.InSyncClient client)
        {
            var request = new RemoveSyncRequest
            {
                SourcePath = removeOptions.SourceFolder ?? string.Empty,
                TargetPath = removeOptions.TargetFolder ?? string.Empty
            };

            var result = client.RemoveSync(request);
            result.Removed
                .ToList()
                .ForEach(x => Console.WriteLine($"{x.SourcePath} => {x.TargetPath} (removed)"));
            return 0;
        }

        private static int List(ListOptions listOptions, Service.InSync.InSyncClient client)
        {
            var request = new ListSyncRequest();

            var result = client.ListSync(request);
            result.Watches
                .ToList()
                .ForEach(x => Console.WriteLine($"{x.SourcePath} => {x.TargetPath}"));
            return 0;
        }
    }
}