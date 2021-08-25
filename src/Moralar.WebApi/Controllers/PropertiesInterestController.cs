using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Moralar.Data.Entities;
using Moralar.Data.Entities.Auxiliar;
using Moralar.Data.Enum;
using Moralar.Domain;
using Moralar.Domain.Services.Interface;
using Moralar.Domain.ViewModels;
using Moralar.Domain.ViewModels.Family;
using Moralar.Domain.ViewModels.PropertiesInterest;
using Moralar.Domain.ViewModels.Property;
using Moralar.Domain.ViewModels.Question;
using Moralar.Domain.ViewModels.Quiz;
using Moralar.Repository.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using UtilityFramework.Application.Core;
using UtilityFramework.Application.Core.JwtMiddleware;
using UtilityFramework.Application.Core.ViewModels;
using UtilityFramework.Services.Core.Interface;


namespace Moralar.WebApi.Controllers
{
    [EnableCors("AllowAllOrigin")]
    [Route("api/v1/[controller]")]
    [Authorize(ActiveAuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class PropertiesInterestController : Controller
    {

        private readonly IMapper _mapper;
        private readonly IFamilyRepository _familyRepository;
        private readonly IResidencialPropertyRepository _residencialPropertyRepository;
        private readonly IPropertiesInterestRepository _propertiesInterestRepository;
        private readonly ISenderMailService _senderMailService;
        private readonly IUtilService _utilService;
        private readonly IProfileRepository _profileRepository;

        public PropertiesInterestController(IMapper mapper, IFamilyRepository familyRepository, IResidencialPropertyRepository residencialPropertyRepository, IPropertiesInterestRepository propertiesInterestRepository, IUtilService utilService, ISenderMailService senderMailService, IProfileRepository profileRepository)
        {
            _mapper = mapper;
            _familyRepository = familyRepository;
            _residencialPropertyRepository = residencialPropertyRepository;
            _propertiesInterestRepository = propertiesInterestRepository;
            _senderMailService = senderMailService;
            _utilService = utilService;
            _profileRepository = profileRepository;
        }

        /// <summary>
        /// DETALHES DOS MATCHS DA FAMÍLIA
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("DetailFamilyMatch/{familyId}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DetailFamilyMatch([FromRoute] string familyId)
        {
            try
            {

                var Entity = await _propertiesInterestRepository.FindByAsync(x => x.FamilyId == familyId).ConfigureAwait(false);
                if (Entity == null)
                    return BadRequest(Utilities.ReturnErro(nameof(DefaultMessages.ResidencialPropertyNotFound)));

                var residencialProperties = await _residencialPropertyRepository.FindIn("_id", Entity.Select(x => ObjectId.Parse(x.ResidencialPropertyId.ToString())).ToList()) as List<ResidencialProperty>;
                if (residencialProperties.Count() == 0)
                    return BadRequest(Utilities.ReturnErro(nameof(DefaultMessages.ResidencialPropertyNotFound)));

                var vieoViewModel = _mapper.Map<List<ResidencialPropertyViewModel>>(residencialProperties);

                return Ok(Utilities.ReturnSuccess(data: vieoViewModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(nameof(DefaultMessages.MessageException)));
            }
        }
        /// <summary>
        /// DETALHES DAS FAMÍLIAS INTERESSADAS NO IMÓVEL
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("DetailFamiliesMatch/{residencialPropertyId}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<IActionResult> DetailFamiliesMatch([FromRoute] string residencialPropertyId)
        {
            try
            {

                var property = await _propertiesInterestRepository.FindByAsync(x => x.ResidencialPropertyId == residencialPropertyId).ConfigureAwait(false);
                if (property == null)
                    return BadRequest(Utilities.ReturnErro(nameof(DefaultMessages.ResidencialPropertyNotFound)));

                var families = await _familyRepository.FindIn("_id", property.Select(x => ObjectId.Parse(x.FamilyId.ToString())).ToList()) as List<Family>;

                var vwFamilies = _mapper.Map<List<FamilyCompleteListViewModel>>(families);

                var residencialProperty = await _residencialPropertyRepository.FindOneByAsync(x => x._id == ObjectId.Parse(residencialPropertyId)).ConfigureAwait(false);
                if (residencialProperty != null && residencialProperty.FamiliIdResidencialChosen != null)
                {
                    vwFamilies.Find(x => x.Id == residencialProperty.FamiliIdResidencialChosen).FamiliIdResidencialChosen = residencialProperty.FamiliIdResidencialChosen;
                    vwFamilies.Find(x => x.Id == residencialProperty.FamiliIdResidencialChosen).TypeStatusResidencial = residencialProperty.TypeStatusResidencialProperty;
                }


                for (int i = 0; i < families.Count(); i++)
                {
                    foreach (var p in families.ToList()[i].Priorization.GetType().GetProperties().Where(p => !p.GetGetMethod().GetParameters().Any()))
                    {
                        var priorityRate = (PriorityRate)p.GetValue(families.ToList()[i].Priorization, null);
                        vwFamilies.Find(x => x.Id == families.ToList()[i]._id.ToString()).Priorization.Add(_mapper.Map<PriorityRateViewModel>(priorityRate));
                    }
                }
                return Ok(Utilities.ReturnSuccess(data: vwFamilies));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(DefaultMessages.MessageException));
            }
        }




        /// <summary>
        /// REGISTRAR UM INTERESSE AO IMÓVEL
        /// </summary>
        /// <remarks>
        ///OBJ DE ENVIO
        ///
        ///        POST
        ///        {
        ///        "familyId": "string",
        ///        "residencialPropertyId": "string"        
        ///        }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("Register")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] PropertiesInterestRegisterViewModel model)
        {

            try
            {
                var ignoreValidation = new List<string>();
                var isInvalidState = ModelState.ValidModelState(ignoreValidation.ToArray());
                if (isInvalidState != null)
                    return BadRequest(isInvalidState);


                if (await _propertiesInterestRepository.CheckByAsync(x => x.FamilyId == model.FamilyId && x.ResidencialPropertyId == model.ResidencialPropertyId).ConfigureAwait(false))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyInUse));

                var familyEntity = await _familyRepository.FindByIdAsync(model.FamilyId).ConfigureAwait(false);
                if (familyEntity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));

                var residencialEntity = await _residencialPropertyRepository.FindByIdAsync(model.ResidencialPropertyId).ConfigureAwait(false);
                if (residencialEntity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ResidencialPropertyNotFound));
               


                /* Regra de escolha de imóvel de ser de no máximo 03  */
                var propertyChoiceFamily = await _propertiesInterestRepository.CountAsync(x => x.FamilyId == model.FamilyId).ConfigureAwait(false);
                if (propertyChoiceFamily > 3)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ChoiceLimitExceeded));
               

                var entityProperty = _mapper.Map<PropertiesInterest>(model);
                entityProperty.HolderName = familyEntity.Holder.Name;
                entityProperty.HolderEmail = familyEntity.Holder.Email;
                entityProperty.HolderCpf = familyEntity.Holder.Cpf;
                entityProperty.HolderNumber = familyEntity.Holder.Number;
                entityProperty.ResidencialCode = residencialEntity.Code;
                entityProperty.Priorization = familyEntity.Priorization;

                await _propertiesInterestRepository.CreateAsync(entityProperty).ConfigureAwait(false);

                var entityResidencial = await _residencialPropertyRepository.FindByIdAsync(model.ResidencialPropertyId).ConfigureAwait(false);
                if (entityResidencial == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ResidencialPropertyNotFound));


                var sendInformation = await _propertiesInterestRepository.FindByAsync(x => x.ResidencialPropertyId == model.ResidencialPropertyId).ConfigureAwait(false);
                foreach (var item in sendInformation)
                {

                    var dataBody = Util.GetTemplateVariables();
                    dataBody.Add("{{ title }}", "Interesse em Imóvel");
                    dataBody.Add("{{ message }}", $"<p>Caro(a) {item.HolderName.GetFirstName()}</p>" +
                                                $"<p> Uma família manifestou interesse pelo imóvel {entityResidencial.ResidencialPropertyAdress.Location} "
                                                );

                    var body = _senderMailService.GerateBody("custom", dataBody);

                    var unused = Task.Run(async () =>
                    {
                        await _senderMailService.SendMessageEmailAsync(Startup.ApplicationName, item.HolderEmail, body, "Registro de interesse em imóvel").ConfigureAwait(false);
                    });
                }


                /* Informa os gestores sobre o término do processo de escolha de imóveis  */
                var propertyChoice = await _propertiesInterestRepository.CountAsync(x => x.FamilyId == model.FamilyId).ConfigureAwait(false);
                if (propertyChoice == 3)
                {                   


                    var entityProfile = await _profileRepository.FindByAsync(x => x.TypeProfile == TypeUserProfile.Gestor || x.TypeProfile == TypeUserProfile.TTS && x.DataBlocked != null).ConfigureAwait(false);
                    foreach (var item in entityProfile)
                    {
                        var dataBody = Util.GetTemplateVariables();
                        dataBody.Add("{{ title }}", "Processo de escolha de imóvel finalizado");
                        dataBody.Add("{{ message }}", $"<p>Caro(a) {item.Name.GetFirstName()}</p>" +
                                                    $"<p> A família {familyEntity.Holder.Name} completou o processo de escolha de imóvel."
                                                    );

                        var body = _senderMailService.GerateBody("custom", dataBody);

                        var unused = Task.Run(async () =>
                        {
                            await _senderMailService.SendMessageEmailAsync(Startup.ApplicationName, item.Email, body, "Registro de interesse em imóvel").ConfigureAwait(false);
                        });

                    }
                }

                    return Ok(Utilities.ReturnSuccess(data: "Registrado com sucesso!"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Question, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível cadastrar/atualizar novo questionário", "", "", "", "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// LISTAGEM DAS FAMÍLIAS
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("LoadData")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        //[ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> LoadData([FromForm] DtParameters model, [FromForm] string number, [FromForm] string holderName, [FromForm] string holderCpf)
        {
            //, [FromForm] string number, [FromForm] string holderName, [FromForm] string holderCpf
            var response = new DtResult<PropertiesInterestViewModel>();
            //
            try
            {
                var builder = Builders<Data.Entities.PropertiesInterest>.Filter;
                var conditions = new List<FilterDefinition<Data.Entities.PropertiesInterest>>();

                conditions.Add(builder.Where(x => x.Created != null));

                if (!string.IsNullOrEmpty(number))
                    conditions.Add(builder.Where(x => x.HolderNumber == number));
                if (!string.IsNullOrEmpty(holderName))
                    conditions.Add(builder.Where(x => x.HolderName.ToUpper().Contains(holderName.ToUpper())));
                if (!string.IsNullOrEmpty(holderCpf))
                    conditions.Add(builder.Where(x => x.HolderCpf == holderCpf.OnlyNumbers()));

                var columns = model.Columns.Where(x => x.Searchable && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToArray();
                

                //var sortColumn = !string.IsNullOrEmpty(model.SortOrder) ? model.SortOrder.UppercaseFirst() : model.Columns.FirstOrDefault(x => x.Orderable)?.Name ?? model.Columns.FirstOrDefault()?.Name;
                var sortColumn = !string.IsNullOrEmpty(model.SortOrder) ? model.SortOrder.UppercaseFirst() : model.Columns.FirstOrDefault(x => x.Orderable)?.Name ?? model.Columns.FirstOrDefault()?.Name;
                var totalRecords = (int)await _propertiesInterestRepository.GetCollectionAsync().CountDocumentsAsync(builder.And(conditions));

                var sortBy =
                model.Order[0].Dir.ToString().ToUpper().Equals("DESC")
                ? Builders<Data.Entities.PropertiesInterest>.Sort.Descending(sortColumn)
                : Builders<Data.Entities.PropertiesInterest>.Sort.Ascending(sortColumn);

                var retorno = await _propertiesInterestRepository
                    .LoadDataTableAsync(model.Search.Value, sortBy, model.Start, model.Length, conditions, columns);


                //var t = retorno.ToList();
                var propertiesEntity = _mapper.Map<List<PropertiesInterestViewModel>>(retorno);
                for (int i = 0; i < retorno.Count(); i++)
                {
                    var s = retorno.ToList()[i].Priorization;

                    foreach (var p in s.GetType().GetProperties().Where(p => !p.GetGetMethod().GetParameters().Any()))
                    {
                        var g = (PriorityRate)p.GetValue(s, null);
                        if (g.Value == true)
                            propertiesEntity.Find(x => x.Id == retorno.ToList()[i]._id.ToString()).PriorityRates.Add(g);
                    }
                    propertiesEntity.Find(x => x.Id == retorno.ToList()[i]._id.ToString()).Interest = retorno.Count(x => x.ResidencialPropertyId == retorno.ToList()[i].ResidencialPropertyId.ToString());

                }

                var totalrecordsFiltered = !string.IsNullOrEmpty(model.Search.Value)
                    ? (int)await _propertiesInterestRepository.CountSearchDataTableAsync(model.Search.Value, conditions, columns)
                    : totalRecords;

                var teste = !string.IsNullOrEmpty(model.SortOrder) ? model.SortOrder.UppercaseFirst() : model.Columns.FirstOrDefault(x => x.Orderable)?.Name ?? model.Columns.FirstOrDefault()?.Name;

                var residencialEntity = await _residencialPropertyRepository.FindIn("_id", retorno.Select(x => ObjectId.Parse(x.ResidencialPropertyId.ToString())).ToList()) as List<ResidencialProperty>;
                for (int i = 0; i < propertiesEntity.Count(); i++)
                {
                    var objResidencial = residencialEntity.FirstOrDefault(x => x._id == ObjectId.Parse(propertiesEntity[i].ResidencialPropertyId));
                    if (objResidencial != null)
                        propertiesEntity[i].ResidencialPropertyAdress = _mapper.Map<ResidencialPropertyAdress>(objResidencial.ResidencialPropertyAdress);
                }

                response.Data = propertiesEntity.OrderBy(c => c.PriorityRates.Min(x => x.Rate)).ToList();
                response.Draw = model.Draw;
                response.RecordsFiltered = totalrecordsFiltered;
                response.RecordsTotal = totalRecords;

                return Ok(response);

            }
            catch (Exception ex)
            {
                response.Erro = true;
                response.MessageEx = $"{ex.InnerException} {ex.Message}".Trim();

                return Ok(response);
            }
        }

        /// <summary>
        /// REMOVER INTERESSE DA FAMÍLIA
        /// </summary>
        /// <remarks>
        ///OBJ DE ENVIO
        ///
        ///         POST
        ///        {
        ///        "familyId": "string",
        ///        "residencialPropertyId": "string"
        ///        }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("InterestCancel")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> InterestCancel([FromBody] PropertiesInterestCancelViewModel model)
        {
            try
            {
                var entityFamily = await _familyRepository.FindByIdAsync(model.FamilyId).ConfigureAwait(false);
                if (entityFamily == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));

                var entityResidencial = await _residencialPropertyRepository.FindByIdAsync(model.ResidencialPropertyId).ConfigureAwait(false);
                if (entityResidencial == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ResidencialPropertyNotFound));

                var property = await _propertiesInterestRepository.FindByAsync(x => x.ResidencialPropertyId == model.ResidencialPropertyId).ConfigureAwait(false);
                var entityPropertyFamily = property.Where(x => x.FamilyId == model.FamilyId && x.ResidencialPropertyId == model.ResidencialPropertyId).FirstOrDefault();
                if (entityPropertyFamily == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ResidencialPropertyNotFound));

               await _propertiesInterestRepository.DeleteOneAsync(entityPropertyFamily._id.ToString()).ConfigureAwait(false);

                                
                
                return Ok(Utilities.ReturnSuccess(data: "Cancelado com sucesso!"));
            }
            catch (Exception ex)
            {
               return BadRequest(ex.ReturnErro());
            }
        }

    }

}