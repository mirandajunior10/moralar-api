using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Moralar.Data.Enum;
using Moralar.Domain.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using UtilityFramework.Application.Core;
using UtilityFramework.Application.Core.JwtMiddleware;

namespace Moralar.Domain
{
    public static class Util
    {

        private static IStringLocalizer _localizer { get; set; }

        public static Dictionary<string, string> GetTemplateVariables()
        {

            var dataBody = new Dictionary<string, string>();
            try
            {
                dataBody.Add("__bg-cardbody__", "#F2F3F3");
                dataBody.Add("__bg-cardfooter__", "#A5559A");
                dataBody.Add("__cl-body__", "#000000");
                dataBody.Add("{{ baseUrl }}", $"{BaseConfig.CustomUrls[0]}/content/images");
                dataBody.Add("{{ contact }}", Utilities.GetConfigurationRoot().GetSection("contactEmail").Get<string>());
                dataBody.Add("{{ appName }}", BaseConfig.ApplicationName);
            }
            catch (Exception)
            {

                //unused
            }

            return dataBody;

        }
        public static Claim SetRole(TypeProfile typeProfile) => new Claim(ClaimTypes.Role, Enum.GetName(typeProfile.GetType(), typeProfile));


        public static TypeProfile GetRole(this HttpRequest request)
        {

            var role = request.GetClaimFromToken(ClaimTypes.Role);

            if (string.IsNullOrEmpty(role))
                throw new ArgumentNullException(nameof(ClaimTypes.Role), "Tipo de usuário não identificado");

            return (TypeProfile)Enum.Parse(typeof(TypeProfile), role);
        }
        public static List<SelectItemEnumViewModel> GetMembersOfEnum<T>()
        {
            try
            {
                if (typeof(T).GetTypeInfo().IsEnum == false)
                    throw new ArgumentException("Type must be an enum");

                return Enum.GetValues(typeof(T))
                    .Cast<T>()
                    .Select(x => new SelectItemEnumViewModel()
                    {
                        Value = (int)(object)x,
                        Name = x.ToString(),
                    }).ToList();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static Language GetCurrentLocale(this HttpRequest request)
        {
            try
            {
                if (request.Headers.Keys.Count(x => x == "Accept-Language") > 0)
                {
                    return (Language)Enum.Parse(typeof(Language), request.Headers.GetValue("Accept-Language"), true);
                }
            }
            catch (Exception)
            {
                /**/
            }
            return Language.En;
        }


        private static void SetLocalizer()
        {
            if (_localizer == null)
            {
                var factory = Utilities.HttpContextAccessor.HttpContext.RequestServices.GetService(typeof(IStringLocalizerFactory)) as IStringLocalizerFactory;
                var type = typeof(SharedResource);
                _localizer = factory.Create(type);
            }
        }

        public static string GetTranslate(this string key)
        {
            if (_localizer == null)
                SetLocalizer();

            return _localizer[key]?.ToString();
        }
    }
}