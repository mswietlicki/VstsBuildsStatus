using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace VstsBuildsStatus
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseUrls("http://localhost:5000","http://192.168.1.12:5000")
                .Build();

            host.Run();
        }
    }
}
