using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Moralar.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                //.UseUrls("http://0.0.0.0:5000") /*CASO NECESSARIO COMPARTILHAR LOCALHOST*/
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();

            host.Run();
        }
    }
}
