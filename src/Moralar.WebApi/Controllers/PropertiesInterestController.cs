﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Moralar.Data.Entities;
using Moralar.Data.Entities.Auxiliar;
using Moralar.Data.Enum;
using Moralar.Domain;
using Moralar.Domain.Services.Interface;
using Moralar.Domain.ViewModels.Family;
using Moralar.Domain.ViewModels.PropertiesInterest;
using Moralar.Domain.ViewModels.Property;
using Moralar.Repository.Interface;
using UtilityFramework.Application.Core;
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
        private readonly INotificationRepository _notificationRepository;
        private readonly ISenderNotificationService _senderNotificationService;

        public PropertiesInterestController(IMapper mapper, IFamilyRepository familyRepository, IResidencialPropertyRepository residencialPropertyRepository, IPropertiesInterestRepository propertiesInterestRepository, IUtilService utilService, ISenderMailService senderMailService, IProfileRepository profileRepository, INotificationRepository notificationRepository, ISenderNotificationService senderNotificationService)
        {
            _mapper = mapper;
            _familyRepository = familyRepository;
            _residencialPropertyRepository = residencialPropertyRepository;
            _propertiesInterestRepository = propertiesInterestRepository;
            _senderMailService = senderMailService;
            _utilService = utilService;
            _profileRepository = profileRepository;
            _notificationRepository = notificationRepository;
            _senderNotificationService = senderNotificationService;
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

                var listPropertiesInterestEntity = await _propertiesInterestRepository.FindByAsync(x => x.FamilyId == familyId).ConfigureAwait(false);
                if (listPropertiesInterestEntity.Count() == 0)
                    return Ok(Utilities.ReturnSuccess(data: new List<object>()));

                var listEntity = await _residencialPropertyRepository.FindIn(c => c.TypeStatusResidencialProperty != TypeStatusResidencial.Vendido, "_id", listPropertiesInterestEntity.Select(x => ObjectId.Parse(x.ResidencialPropertyId.ToString())).ToList(), Builders<ResidencialProperty>.Sort.Descending(nameof(ResidencialProperty.Created))) as List<ResidencialProperty>;

                if (listEntity.Count() == 0)
                    return Ok(Utilities.ReturnSuccess(data: new List<object>()));

                var listInterest = await _propertiesInterestRepository.FindIn(nameof(PropertiesInterest.ResidencialPropertyId), listEntity.Select(x => x._id.ToString()).ToList());
                var response = _mapper.Map<List<ResidencialProperty>, List<ResidencialPropertyViewModel>>(listEntity, opt => opt.AfterMap((src, dest) =>
                {
                    for (int i = 0; i < src.Count(); i++)
                    {
                        dest[i].InterestedFamilies = listInterest.LongCount(x => x.ResidencialPropertyId == dest[i].Id);
                    }
                }));

                return Ok(Utilities.ReturnSuccess(data: response));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(DefaultMessages.MessageException, responseList: true));
            }
        }
        /// <summary>
        /// DETALHES DAS FAMÍLIAS INTERESSADAS NO IMÓVEL
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>'
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("DetailFamiliesMatch/{residencialPropertyId}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DetailFamiliesMatch([FromRoute] string residencialPropertyId)
        {
            try
            {
                var listPropertiesInterest = await _propertiesInterestRepository.FindByAsync(x => x.ResidencialPropertyId == residencialPropertyId).ConfigureAwait(false);
                if (listPropertiesInterest.Count() == 0)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.NoPropertiesInterest));

                var listFamilies = await _familyRepository.FindIn("_id", listPropertiesInterest.Select(x => ObjectId.Parse(x.FamilyId)).ToList()) as List<Family>;

                var vwFamilies = _mapper.Map<List<FamilyCompleteListViewModel>>(listFamilies);

                var residencialProperty = await _residencialPropertyRepository.FindOneByAsync(x => x._id == ObjectId.Parse(residencialPropertyId)).ConfigureAwait(false);
                if (residencialProperty?.FamiliIdResidencialChosen != null)
                {
                    for (int i = 0; i < vwFamilies.Count(); i++)
                    {
                        vwFamilies[i].FamiliIdResidencialChosen = residencialProperty.FamiliIdResidencialChosen;
                        vwFamilies[i].TypeStatusResidencial = residencialProperty.TypeStatusResidencialProperty;
                    }
                }

                for (int i = 0; i < listFamilies.Count(); i++)
                {
                    foreach (var p in listFamilies.ToList()[i].Priorization.GetType().GetProperties().Where(p => !p.GetGetMethod().GetParameters().Any()))
                    {
                        var priorityRate = (PriorityRate)p.GetValue(listFamilies.ToList()[i].Priorization, null);
                        var item = vwFamilies.Find(x => x.Id == listFamilies.ToList()[i]._id.ToString());
                        if (item != null)
                        {
                            item.Priorization.Add(_mapper.Map<PriorityRateViewModel>(priorityRate));
                            item.TotalPoints = item.Priorization.Where(x => x.Value == true).Sum(x => x.Rate);
                        }
                    }
                }
                return Ok(Utilities.ReturnSuccess(data: vwFamilies.OrderBy(x => x.TotalPoints)));
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

                /* Regra de escolha de imóvel tem que ser de no máximo 03  */
                var propertyChoiceFamily = await _propertiesInterestRepository.CountAsync(x => x.FamilyId == model.FamilyId).ConfigureAwait(false);
                if (propertyChoiceFamily >= 3)
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

                var title = "Interesse em Imóvel";
                var content = $"Uma família manifestou interesse pelo imóvel {entityResidencial.ResidencialPropertyAdress.StreetAddress}";

                var sendInformation = await _propertiesInterestRepository.FindByAsync(x => x.ResidencialPropertyId == model.ResidencialPropertyId && x.FamilyId != familyEntity._id.ToString()).ConfigureAwait(false);
                foreach (var item in sendInformation)
                {

                    var dataBody = Util.GetTemplateVariables();
                    dataBody.Add("{{ title }}", title);
                    dataBody.Add("{{ message }}", $"<p>Olá {item.HolderName.GetFirstName()}</p>" +
                                                  $"<p>Uma família manifestou interesse pelo imóvel {entityResidencial.ResidencialPropertyAdress.StreetAddress} "
                                                );

                    var body = _senderMailService.GerateBody("custom", dataBody);

                    var unused = Task.Run(async () =>
                    {
                        await _senderMailService.SendMessageEmailAsync(Startup.ApplicationName, item.HolderEmail, body, "Registro de interesse em imóvel").ConfigureAwait(false);
                    });
                }

                var families = await _familyRepository.FindIn("_id", sendInformation.Select(x => ObjectId.Parse(x.FamilyId)).ToList()) as List<Family>;

                var listNotification = new List<Notification>();
                for (int i = 0; i < families.Count(); i++)
                {
                    var familyItem = families[i];

                    listNotification.Add(new Notification()
                    {
                        For = ForType.Family,
                        FamilyId = familyItem._id.ToString(),
                        Title = title,
                        Description = content
                    });
                }

                await _notificationRepository.CreateAsync(listNotification);

                dynamic payloadPush = Util.GetPayloadPush();
                dynamic settingPush = Util.GetSettingsPush();

                await _senderNotificationService.SendPushAsync(title, content, families.SelectMany(x => x.DeviceId).ToList(), data: payloadPush, settings: settingPush, priority: 10);

                /* Informa os gestores sobre o término do processo de escolha de imóveis  */
                var propertyChoice = await _propertiesInterestRepository.CountAsync(x => x.FamilyId == model.FamilyId).ConfigureAwait(false);
                if (propertyChoice == 3)
                {
                    var unused = Task.Run(async () =>
                    {
                        var entityProfile = await _profileRepository.FindByAsync(x => x.TypeProfile == TypeUserProfile.Gestor || x.TypeProfile == TypeUserProfile.TTS && x.DataBlocked != null).ConfigureAwait(false);
                        foreach (var item in entityProfile)
                        {
                            var dataBody = Util.GetTemplateVariables();
                            dataBody.Add("{{ title }}", "Processo de escolha de imóvel finalizado");
                            dataBody.Add("{{ message }}", $"<p>Olá {item.Name.GetFirstName()}</p>" +
                                                          $"<p> A família {familyEntity.Holder.Name} completou o processo de escolha de imóvel.");

                            var body = _senderMailService.GerateBody("custom", dataBody);

                            await _senderMailService.SendMessageEmailAsync(Startup.ApplicationName, item.Email, body, "Registro de interesse em imóvel").ConfigureAwait(false);

                        }
                        await _notificationRepository.CreateAsync(new Notification()
                        {
                            Title = "Processo de escolha de imóvel finalizado",
                            Description = $"O processo de interesse no imóvel {residencialEntity.Code} foi concluído",
                        });

                    });
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
        public async Task<IActionResult> LoadData([FromForm] DtParameters model, [FromForm] string number, [FromForm] string holderName, [FromForm] string holderCpf)
        {
            var response = new DtResult<PropertiesInterestViewModel>();
            //
            try
            {
                var builder = Builders<PropertiesInterest>.Filter;
                var conditions = new List<FilterDefinition<PropertiesInterest>>();

                conditions.Add(builder.Where(x => x.Created != null));

                if (string.IsNullOrEmpty(number) == false)
                    conditions.Add(builder.Where(x => x.HolderNumber == number));

                if (string.IsNullOrEmpty(holderName) == false)
                    conditions.Add(builder.Where(x => x.HolderName.ToUpper().Contains(holderName.ToUpper())));

                if (string.IsNullOrEmpty(holderCpf) == false)
                    conditions.Add(builder.Where(x => x.HolderCpf == holderCpf.OnlyNumbers()));

                var columns = model.Columns.Where(x => x.Searchable && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToArray();

                var sortColumn = !string.IsNullOrEmpty(model.SortOrder) ? model.SortOrder.UppercaseFirst() : model.Columns.FirstOrDefault(x => x.Orderable)?.Name ?? model.Columns.FirstOrDefault()?.Name;
                var totalRecords = (int)await _propertiesInterestRepository.GetCollectionAsync().CountDocumentsAsync(builder.And(conditions));

                var sortBy =
                model.Order[0].Dir.ToString().ToUpper().Equals("DESC")
                ? Builders<PropertiesInterest>.Sort.Descending(sortColumn)
                : Builders<PropertiesInterest>.Sort.Ascending(sortColumn);

                var retorno = await _propertiesInterestRepository
                    .LoadDataTableAsync(model.Search.Value, sortBy, model.Start, model.Length, conditions, columns) as List<PropertiesInterest>;

                var propertiesEntity = _mapper.Map<List<PropertiesInterestViewModel>>(retorno);

                if (propertiesEntity.Count() > 0)
                {
                    var listInterest = await _propertiesInterestRepository.FindIn("ResidencialPropertyId", retorno.Select(x => x.ResidencialPropertyId).Distinct().ToList()) as List<PropertiesInterest>;
                    var listResidencialProperty = await _residencialPropertyRepository.FindIn("_id", retorno.Select(x => ObjectId.Parse(x.ResidencialPropertyId.ToString())).ToList()) as List<ResidencialProperty>;

                    for (int i = 0; i < propertiesEntity.Count(); i++)
                    {
                        propertiesEntity[i].Interest = listInterest.Count(x => x.ResidencialPropertyId == propertiesEntity[i].ResidencialPropertyId);

                        var residencialEntity = listResidencialProperty.Find(x => x._id == ObjectId.Parse(propertiesEntity[i].ResidencialPropertyId));

                        if (residencialEntity != null)
                            propertiesEntity[i].ResidencialPropertyAdress = _mapper.Map<ResidencialPropertyAdress>(residencialEntity.ResidencialPropertyAdress);
                    }
                }

                var totalrecordsFiltered = !string.IsNullOrEmpty(model.Search.Value)
                    ? (int)await _propertiesInterestRepository.CountSearchDataTableAsync(model.Search.Value, conditions, columns)
                    : totalRecords;

                response.Data = propertiesEntity;
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
        /// BUSCAR MATCHS DAS FAMILIAS
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         POST
        ///             {
        ///                "searchTerm": "string",
        ///                "residencialCode": "string"                
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("SearchMatch")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(typeof(IEnumerable<PropertiesInterestViewModel>), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]

        public async Task<IActionResult> SearchMatch([FromBody] SearchMatchsViewModel model)
        {
            try
            {
                model.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelState();

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);


                var builder = Builders<PropertiesInterest>.Filter;
                var conditions = new List<FilterDefinition<PropertiesInterest>>();

                conditions.Add(builder.Where(x => x.Created != null));

                if (string.IsNullOrEmpty(model.Search) == false)
                    conditions.Add(
                        builder.Or(
                        builder.Regex(x => x.HolderNumber, new BsonRegularExpression(model.Search, "i")),
                        builder.Regex(x => x.HolderName, new BsonRegularExpression(model.Search, "i")),
                        builder.Regex(x => x.HolderCpf, new BsonRegularExpression(model.Search, "i"))

                )
                );

                if (string.IsNullOrEmpty(model.ResidencialCode) == false)
                    conditions.Add(builder.Where(x => x.ResidencialCode == model.ResidencialCode));

                var retorno = await _propertiesInterestRepository.GetCollectionAsync().FindSync(builder.And(conditions), new FindOptions<PropertiesInterest>()
                {
                    Sort = Builders<PropertiesInterest>.Sort.Ascending(nameof(PropertiesInterest.Created))

                }).ToListAsync();


                var propertiesEntity = _mapper.Map<List<PropertiesInterestViewModel>>(retorno);


                var residencialEntity = await _residencialPropertyRepository.FindIn("_id", retorno.Select(x => ObjectId.Parse(x.ResidencialPropertyId)).ToList()) as List<ResidencialProperty>;
                for (int i = 0; i < propertiesEntity.Count(); i++)
                {
                    var objResidencial = residencialEntity.FirstOrDefault(x => x._id == ObjectId.Parse(propertiesEntity[i].ResidencialPropertyId));
                    if (objResidencial != null)
                        propertiesEntity[i].ResidencialPropertyAdress = _mapper.Map<ResidencialPropertyAdress>(objResidencial.ResidencialPropertyAdress);

                    propertiesEntity[i].Interest = retorno.Count(x => x.ResidencialPropertyId == propertiesEntity[i].ResidencialPropertyId);
                }


                return Ok(Utilities.ReturnSuccess(data: propertiesEntity));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(responseList: true));
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