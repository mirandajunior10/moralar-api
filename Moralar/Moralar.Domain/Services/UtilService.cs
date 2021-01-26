using Moralar.Domain.Services.Interface;
using UtilityFramework.Application.Core;

// ReSharper disable RedundantAnonymousTypePropertyName

namespace Moralar.Domain.Services
{
    public class UtilService : IUtilService
    {
        public string GetFlag(string flag)
        {
            switch (flag)
            {
                case "amex":
                    return $"{BaseConfig.CustomUrls[0]}content/images/flagcard/{flag.ToLower()}.png";
                case "dinners":
                    return $"{BaseConfig.CustomUrls[0]}content/images/flagcard/{flag.ToLower()}.png";
                case "mastercard":
                case "master":
                    return $"{BaseConfig.CustomUrls[0]}content/images/flagcard/mastercard.png";
                case "discover":
                    return $"{BaseConfig.CustomUrls[0]}content/images/flagcard/{flag.ToLower()}.png";
                default:
                    return $"{BaseConfig.CustomUrls[0]}content/images/flagcard/visa.png";
            }
        }
    }
}