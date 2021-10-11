using System.IO;
using Microsoft.AspNetCore.Hosting;

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
                .UseSetting(WebHostDefaults.DetailedErrorsKey, "true")
                //.UseApplicationInsights()
                .Build();

            host.Run();
        }
    }
}
