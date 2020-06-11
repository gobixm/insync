using System;
using CommandLine;
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

    internal class Program
    {
        private static int Main(string[] args)
        {
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            using var channel = GrpcChannel.ForAddress("http://localhost:5000");
            var client = new Service.InSync.InSyncClient(channel);

            return Parser.Default.ParseArguments<AddOptions>(args)
                .MapResult(
                    opts => Add(opts, client),
                    errs => 1);
        }

        private static int Add(AddOptions addOptions, Service.InSync.InSyncClient client)
        {
            var request = new AddSyncRequest
            {
                SourcePath = addOptions.SourceFolder,
                TargetPath = addOptions.TargetFolder
            };

            client.AddSync(request);
            return 0;
        }
    }
}