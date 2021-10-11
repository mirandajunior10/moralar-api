using System.IO;
using System.Linq;
using System.Reflection;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Moralar.WebApi.Filter;
using Moralar.WebApi.Services;
using UtilityFramework.Application.Core;


namespace Moralar.WebApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", reloadOnChange: true, optional: false)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            ApplicationName = Assembly.GetEntryAssembly().GetName().Name?.Split('.')[0];
            EnableSwagger = Configuration.GetSection("EnableSwagger").Get<bool>();

            /* CRIAR NO Settings json prop com array de cultures ["pt","pt-br"] */
            //var cultures = Utilities.GetConfigurationRoot().GetSection("TranslateLanguages").Get<List<string>>();
            //SupportedCultures = cultures.Select(x => new CultureInfo(x)).ToList();

        }

        public IConfigurationRoot Configuration { get; }
        public static string ApplicationName { get; set; }
        public static bool EnableSwagger { get; set; }

        /* PARA TRANSLATE*/
        //public static List<CultureInfo> SupportedCultures { get; set; }


        // This method gets Race by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(opt =>
            {
                opt.Filters.Add(typeof(CheckJson));
                opt.Filters.Add(typeof(PreventSpanFilter));
                opt.Filters.Add(typeof(CheckCurrentDevice));
                opt.Filters.Add(new FilterAsyncToken());
            });

            /*TRANSLATE I18N*/
            //services.AddLocalization(options => options.ResourcesPath = "Resources");
            //services.AddMvc()
            //       .AddDataAnnotationsLocalization(options =>
            //       options.DataAnnotationLocalizerProvider = (type, factory) =>
            //       factory.Create(typeof(SharedResource)));

            //services.Configure<RequestLocalizationOptions>(options =>
            //{
            //    options.DefaultRequestCulture = new RequestCulture("pt");
            //    options.SupportedCultures = SupportedCultures;
            //    options.SupportedUICultures = SupportedCultures;
            //});

            /*HANGFIRE*/
            // services.AddHangfire(x => x.UseMemoryStorage());

            /*ENABLE CORS*/
            services.AddCors(options =>
           {
               options.AddPolicy("AllowAllOrigin",
                   builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().AllowCredentials().Build());
           });
            services.AddAutoMapper();
            /*CROP IMAGE*/
            services.AddImageResizer();

            /*ADD SWAGGER*/
            services.AddJwtSwagger(ApplicationName, enableSwaggerAuth: true);

            /*INJEÇÃO DE DEPENDENCIAS DE BANCO*/
            services.AddRepositoryInjection();

            /*INJEÇÃO DE DEPENDENCIAS DE SERVIÇOS*/
            services.AddServicesInjection();
        }

        // This method gets Race by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IHttpContextAccessor httpContextAccessor)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddFile($"Log/" + ApplicationName + "-{Date}.txt", LogLevel.Warning, retainedFileCountLimit: 5);

            Utilities.SetHttpContext(httpContextAccessor);

            /* PARA USO DO HANG FIRE ROTINAS*/
            // app.UseHangfireServer();
            // app.UseHangfireDashboard("/jobs", new DashboardOptions
            // {
            //     Authorization = new[] { new MyAuthorizationFilter() }
            // }); 

            /*CROP IMAGE*/
            app.UseImageResizer();

            app.UseStaticFiles();

            var path = Path.Combine(Directory.GetCurrentDirectory(), @"Content");

            if (Directory.Exists(path) == false)
                Directory.CreateDirectory(path);

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(path),
                RequestPath = new PathString("/content")
            });


            app.UseCors("AllowAllOrigin");
            /*LOG BASICO*/
            // app.UseRequestResponseLoggingLite();
            /*RETORNO COM GZIP*/
            app.UseResponseCompression();

            /* TRANSLATE API */
            // app.UseRequestLocalization(new RequestLocalizationOptions
            // {
            //     DefaultRequestCulture = new RequestCulture("pt"),
            /*  Formatting numbers, dates, etc.*/
            //     SupportedCultures = SupportedCultures,
            /* UI strings that we have localized. */
            //     SupportedUICultures = SupportedCultures
            // });

            /*JWT TOKEN*/
            app.UseJwtTokenApiAuth(Configuration);

            app.UseMvc();

            if (EnableSwagger)
            {
                app.UseSwagger();

                app.UseSwaggerUI(c =>
               {
                   c.SwaggerEndpoint($"../swagger/v1/swagger.json".Trim(), $"API {ApplicationName} {env.EnvironmentName}");
                   c.EnableDeepLinking();
                   c.DocExpansion(DocExpansion.None);

               });
            }


        }
    }
}