using Microsoft.Extensions.DependencyInjection;
using Moralar.Domain.Services;
using Moralar.Domain.Services.Interface;
using System.Linq;
using System.Reflection;
using UtilityFramework.Services.Core;
using UtilityFramework.Services.Core.Interface;
using UtilityFramework.Services.Iugu.Core;
using UtilityFramework.Services.Iugu.Core.Interface;

namespace Moralar.WebApi.Services
{
    public static class IocContainer
    {
        /// <summary>
        /// INJEÇÃO DE DEPENDENCIAS DE REPOSITORIO DE ACESSO A DADOS DO BANCO
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddRepositoryInjection(this IServiceCollection services)
        {
            /*REPOSITORIES - INJEÇÃO AUTOMATICA DE DEPENDÊNCIAS*/
            var assemblyName = Assembly.GetEntryAssembly().GetReferencedAssemblies().FirstOrDefault(x => x.Name.Contains($"{Startup.ApplicationName}.Repository"));

            var assembly = Assembly.Load(assemblyName);
            var types = assembly.GetTypes().ToList();
            var interfacesTypes = assembly.GetTypes().Where(x => x.GetTypeInfo().IsInterface).ToList();

            for (var i = 0; i < interfacesTypes.Count(); i++)
            {
                var className = interfacesTypes[i].Name.StartsWith("II") ? interfacesTypes[i].Name.Substring(1) : interfacesTypes[i].Name.TrimStart('I');
                var classType = types.Find(x => x.Name == className);
                if (classType != null)
                    services.AddSingleton(interfacesTypes[i], classType);
            }

            return services;
        }

        /// <summary>
        /// INJEÇÃO DE DEPENDENCIAS DE SERVIÇOS DE API
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddServicesInjection(this IServiceCollection services)
        {

            /*IUGU*/
            services.AddSingleton(typeof(IIuguChargeServices), typeof(IuguService));
            services.AddSingleton(typeof(IIuguMarketPlaceServices), typeof(IuguService));
            services.AddSingleton(typeof(IIuguPaymentMethodService), typeof(IuguService));
            services.AddSingleton(typeof(IIuguCustomerServices), typeof(IuguService));
            services.AddSingleton(typeof(IIuguService), typeof(IuguService));

            /* NOTIFICAÇÕES & EMAIL*/
            services.AddSingleton(typeof(ISenderMailService), typeof(SendService));
            services.AddSingleton(typeof(ISenderNotificationService), typeof(SendService));

            /*FIREBASE*/
            services.AddScoped(typeof(IFirebaseServices), typeof(FirebaseServices));

            /*UTILIDADES */
            services.AddSingleton(typeof(IUtilService), typeof(UtilService));

            return services;
        }
    }
}