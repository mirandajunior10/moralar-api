using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Moralar.Domain;
using Moralar.Domain.ViewModels;
using Moralar.Repository.Interface;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UtilityFramework.Application.Core;
using UtilityFramework.Application.Core.ViewModels;

namespace Moralar.WebApi.Controllers
{
    [EnableCors("AllowAllOrigin")]
    [Route("api/v1/[controller]")]
    [Authorize(ActiveAuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiExplorerSettings(IgnoreApi = true)]

    public class CityController : Controller
    {
        private readonly ICityRepository _cityRepository;
        private readonly IStateRepository _stateRepository;
        private readonly IMapper _mapper;

        public CityController(ICityRepository cityRepository, IStateRepository stateRepository, IMapper mapper)
        {
            _cityRepository = cityRepository;
            _stateRepository = stateRepository;
            _mapper = mapper;
        }




        /// <summary>
        /// LISTAR CIDADES POR ESTADOS
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{stateId}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Get([FromRoute] string stateId)
        {
            try
            {
                var listCity = await _cityRepository.FindByAsync(x => x.StateId == stateId, Builders<Data.Entities.City>.Sort.Ascending(nameof(Data.Entities.City.Name))).ConfigureAwait(false);

                return Ok(Utilities.ReturnSuccess(data: _mapper.Map<IEnumerable<CityDefaultViewModel>>(listCity)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(responseList: true));
            }
        }

        /// <summary>
        /// LISTAR ESTADOS 
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("ListState")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ListState()
        {
            try
            {
                var listState = await _stateRepository.FindAllAsync(Builders<Data.Entities.State>.Sort.Ascending(nameof(Data.Entities.State.Name))).ConfigureAwait(false);

                return Ok(Utilities.ReturnSuccess(data: _mapper.Map<IEnumerable<StateDefaultViewModel>>(listState)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// BUSCAR INFORMAÇÕES DE DETERMINADO CEP
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("GetInfoFromZipCode/{zipCode}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetInfoFromZipCode([FromRoute] string zipCode)
        {
            try
            {
                if (string.IsNullOrEmpty(zipCode))
                    return BadRequest(Utilities.ReturnErro("Informe o CEP"));

                var client = new RestClient($"http://viacep.com.br/ws/{zipCode.OnlyNumbers()}/json/");

                var request = new RestRequest(Method.GET);

                var infoZipCode = await client.Execute<AddressInfoViewModel>(request).ConfigureAwait(false);

                if (infoZipCode.Data.Erro)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ZipCodeNotFound));

                var response = _mapper.Map<InfoAddressViewModel>(infoZipCode.Data);

                var city = await _cityRepository.FindOneByAsync(x => x.Name == infoZipCode.Data.Localidade && x.StateUf == infoZipCode.Data.Uf).ConfigureAwait(false);

                if (city == null)
                    return Ok(Utilities.ReturnSuccess(data: response));

                response.CityId = city._id.ToString();
                response.CityName = city.Name;
                response.StateId = city.StateId;
                response.StateUf = infoZipCode.Data.Uf;
                response.StateName = city.StateName;

                return Ok(Utilities.ReturnSuccess(data: response));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

    }
}