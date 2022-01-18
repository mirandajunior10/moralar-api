﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moralar.Data.Entities;
using Moralar.Data.Enum;
using Moralar.Domain.Services.Interface;
using Moralar.Domain.ViewModels;
using Moralar.Repository.Interface;
using RestSharp;
using System;
using System.Threading.Tasks;
using UtilityFramework.Application.Core;

// ReSharper disable RedundantAnonymousTypePropertyName

namespace Moralar.Domain.Services
{

    public class UtilService :ControllerBase, IUtilService
    {
        private readonly IMapper _mapper;
        private readonly ILogActionRepository _logActionRepository;
        private readonly ICityRepository _cityRepository;

        public UtilService(IMapper mapper, ILogActionRepository logActionRepository, ICityRepository cityRepository)
        {
            _mapper = mapper;
            _logActionRepository = logActionRepository;
            _cityRepository = cityRepository;
        }
        public string GetEscolasMunicipaisEnsinoFundamental() {

            return "";
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
        public async Task<InfoAddressViewModel> GetInfoFromZipCode(string zipCode)
        {
            try
            {
                if (string.IsNullOrEmpty(zipCode))
                     Utilities.ReturnErro("Informe o CEP");

                var client = new RestSharp.RestClient($"http://viacep.com.br/ws/{zipCode.OnlyNumbers()}/json/");

                var request = new RestSharp.RestRequest(Method.GET);

                var infoZipCode = await client.Execute<AddressInfoViewModel>(request).ConfigureAwait(false);

                if (infoZipCode.Data.Erro)
                   Utilities.ReturnErro(DefaultMessages.ZipCodeNotFound);

                var response = _mapper.Map<InfoAddressViewModel>(infoZipCode.Data);

                //var city = await _cityRepository.FindOneByAsync(x => x.Name == infoZipCode.Data.Localidade && x.StateUf == infoZipCode.Data.Uf).ConfigureAwait(false);

                //if (city == null)
                //   Utilities.ReturnSuccess(data: response);

                //response.CityId = city._id.ToString();
                //response.CityName = city.Name;
                //response.StateId = city.StateId;
                //response.StateUf = infoZipCode.Data.Uf;
                //response.StateName = city.StateName;


                return  response;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}