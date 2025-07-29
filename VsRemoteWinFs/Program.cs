using System;
using System.Linq;
using System.Threading.Tasks;
using DokanNet;
using DokanNet.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VsRemoteWinFs;

internal class Program
{
    private const string UriKey = "-uri";
    private const string MountKey = "-where";

    private static async Task Main(string[] args)
    {
        try
        {
            var arguments = args
               .Select(x => x.Split([ '=' ], 2, StringSplitOptions.RemoveEmptyEntries))
               .Where(x => x.Length > 1)
               .ToDictionary(x => x[0], x => x[1], StringComparer.OrdinalIgnoreCase);

            if (!arguments.TryGetValue(UriKey, out var vsRemoteUri))
                vsRemoteUri = "http://172.17.0.1:5229";

            if (!arguments.TryGetValue(MountKey, out var mountPath))
               mountPath = @"N:\";

            var dokanLogger = new NullLogger();
            using (var consoleLogger = new ConsoleLogger("[VsRemote] "))
            using (var dokan = new Dokan(dokanLogger))
            {
                var mirror = new VsRemoteFS(new VsRemoteFSOptions(vsRemoteUri, Logger: consoleLogger));

                var dokanBuilder = new DokanInstanceBuilder(dokan)
                    .ConfigureLogger(() => dokanLogger)
                    .ConfigureOptions(options =>
                    {
                        options.Options = DokanOptions.DebugMode | DokanOptions.RemovableDrive;
                        options.MountPoint = mountPath;
                    });

                using var dokanInstance = dokanBuilder.Build(mirror);
                Console.CancelKeyPress += (object? sender, ConsoleCancelEventArgs e) =>
                {
                    e.Cancel = true;
                    dokan.RemoveMountPoint(mountPath);
                };

                await dokanInstance.WaitForFileSystemClosedAsync(uint.MaxValue);
            }

            Console.WriteLine(@"Success");
        }
        catch (DokanException ex)
        {
            Console.WriteLine(@"Error: " + ex.Message);
        }
    }
}
