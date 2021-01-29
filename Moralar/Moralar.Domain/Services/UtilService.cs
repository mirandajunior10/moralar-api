using AutoMapper;
using Moralar.Data.Entities;
using Moralar.Data.Enum;
using Moralar.Domain.Services.Interface;
using Moralar.Repository.Interface;
using System;
using System.Threading.Tasks;
using UtilityFramework.Application.Core;

// ReSharper disable RedundantAnonymousTypePropertyName

namespace Moralar.Domain.Services
{

    public class UtilService : IUtilService
    {
        private readonly IMapper _mapper;
        private readonly ILogActionRepository _logActionRepository;

        public UtilService(IMapper mapper, ILogActionRepository logActionRepository)
        {
            _mapper = mapper;
            _logActionRepository = logActionRepository;
        }

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

        public async Task LogUserAdministrationAction(string userId, string message, TypeAction typeAction, LocalAction localAction, string referenceId = null)
        {
            try
            {

                //var userAdministratorEntity = await _userAdministratorRepository.FindByIdAsync(userId);

                //var entityLogAction = new LogAction()
                //{
                //    TypeAction = typeAction,
                //    TypeResposible = TypeResposible.UserAdminstrator,
                //    ReferenceId = referenceId,
                //    Message = message,
                //    ResponsibleId = userId,
                //    ResponsibleName = userAdministratorEntity.Name,
                //    Justification = null,
                //    LocalAction = localAction,
                //    ClientIp = Util.GetClientIp()
                //};

                //await _logActionRepository.CreateReturnAsync(entityLogAction);
             


            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RegisterLogAction(LocalAction localAction, TypeAction typeAction, TypeResposible typeResposible, string message, string responsibleId = null, string responsibleName = null, string referenceId = null, string justification = null, Exception ex = null)
        {
            try
            {
                var entityLogAction = new LogAction()
                {
                    TypeAction = typeAction,
                    TypeResposible = typeResposible,
                    ReferenceId = referenceId,
                    Message = message,
                    ResponsibleId = responsibleId,
                    ResponsibleName = responsibleName,
                    Justification = justification,
                    StackTrace = ex?.StackTrace,
                    MessageEx = ex != null ? $"{ex.InnerException} {ex.Message}".Trim() : null,
                    LocalAction = localAction,
                    ClientIp = Util.GetClientIp()
                };

                await _logActionRepository.CreateReturnAsync(entityLogAction);

            }
            catch (Exception)
            {
                //unused
            }
        }

        public void UpdateCascate(Family familyEntity)
        {
            try
            {
                //var id = entity._id.ToString();

                //_userAdministratorRepository.UpdateMultiple(Query<Family>.EQ(x => x.AgentId, id),
                //new UpdateBuilder<UserAdministrator>().Set(x => x.AgentName, entity.ContactName),
                //UpdateFlags.Multi);
            }
            catch (Exception)
            {
                /*unused*/
            }
        }
    }
}