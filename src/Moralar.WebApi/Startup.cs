using System;
using System.IO;
using System.Reflection;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

using AutoMapper;

using Hangfire;
using Hangfire.Console;
using Hangfire.Mongo;
using Hangfire.Storage;

using Moralar.WebApi.Filter;
using Moralar.WebApi.Services;

using UtilityFramework.Application.Core;
using Moralar.WebApi.HangFire.Interface;
using Moralar.WebApi.HangFire;
using MongoDB.Driver;

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
            EnableService = Configuration.GetSection("EnableService").Get<bool>();

            /* CRIAR NO Settings json prop com array de cultures ["pt","pt-br"] */
            //var cultures = Utilities.GetConfigurationRoot().GetSection("TranslateLanguages").Get<List<string>>();
            //SupportedCultures = cultures.Select(x => new CultureInfo(x)).ToList();

        }

        public IConfigurationRoot Configuration { get; }
        public static string ApplicationName { get; set; }
        public static bool EnableSwagger { get; set; }
        public static bool EnableService { get; set; }

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
            if (EnableService)
            {
                var remoteDatabase = Configuration.GetSection("DATABASE:LOCAL").Get<string>();
                var dataBaseName = Configuration.GetSection("DATABASE:NAME").Get<string>();

                var mongoUrlBuilder = new MongoUrlBuilder($"mongodb://{remoteDatabase}/{dataBaseName}");
                var mongoClient = new MongoClient(mongoUrlBuilder.ToMongoUrl());

                var migrationOptions = new MongoMigrationOptions
                {
                    Strategy = MongoMigrationStrategy.Drop,
                    BackupStrategy = MongoBackupStrategy.Collections
                };
                var storageOptions = new MongoStorageOptions
                {
                    MigrationOptions = migrationOptions,
                    Prefix = "HangFire",
                    CheckConnection = false
                };

                services.AddHangfire(configuration =>
                {
                    configuration.UseConsole();
                    configuration.UseMongoStorage(mongoClient, mongoUrlBuilder.DatabaseName, storageOptions);
                });
            }
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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddFile($"Log/" + ApplicationName + "-{Date}.txt", LogLevel.Warning, retainedFileCountLimit: 5);

            Utilities.SetHttpContext(httpContextAccessor);

            /* PARA USO DO HANG FIRE ROTINAS*/
            if (EnableService)
            {
                GlobalConfiguration.Configuration.UseActivator(new HangfireActivator(serviceProvider));
                app.UseHangfireServer();
                app.UseHangfireDashboard("/jobs", new DashboardOptions
                {
                    Authorization = new[] { new MyAuthorizationFilter() }
                });
            }

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
            app.UseRequestResponseLoggingLite();
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
                   c.EnableFilter();
               });
            }

            if (EnableService)
            {
                using (var connection = JobStorage.Current.GetConnection())
                    foreach (var recurringJob in connection.GetRecurringJobs())
                        RecurringJob.RemoveIfExists(recurringJob.Id);

                var timeZoneBrazil = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");

                /*RODA A CADA MINUTO*/
                RecurringJob.AddOrUpdate<IHangFireService>(
                    "MAKEQUESTIONAVAILABLE",
                    services => services.MakeQuestionAvailable(null),
                    Cron.Minutely(), timeZoneBrazil);

                /*RODA TODO DIA AS 9 AM*/
                RecurringJob.AddOrUpdate<IHangFireService>(
                    "ALERTA_AGENDAMENTO",
                    services => services.ScheduleAlert(null),                    
                    Cron.Daily(9), timeZoneBrazil);
                //Cron.MinuteInterval(5), timeZoneBrazil);

            }
        }
    }
}