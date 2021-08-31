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
using Moralar.Domain.ViewModels.Informative;
using Moralar.Domain.ViewModels.InformativeSended;
using Moralar.Domain.ViewModels.Notification;
using Moralar.Domain.ViewModels.NotificationSended;
using Moralar.Domain.ViewModels.Question;
using Moralar.Domain.ViewModels.Quiz;
using Moralar.Repository.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    public class NotificationController : Controller
    {

        private readonly IMapper _mapper;
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationSendedRepository _notificationSendedRepository;
        private readonly IFamilyRepository _familyRepository;
        private readonly ISenderMailService _senderMailService;
        private readonly IUtilService _utilService;

        public NotificationController(IMapper mapper, INotificationRepository notificationRepository, INotificationSendedRepository notificationSendedRepository, IFamilyRepository familyRepository, ISenderMailService senderMailService, IUtilService utilService)
        {
            _mapper = mapper;
            _notificationRepository = notificationRepository;
            _notificationSendedRepository = notificationSendedRepository;
            _familyRepository = familyRepository;
            _senderMailService = senderMailService;
            _utilService = utilService;
        }



        ///// <summary>
        ///// METODO DE DETALHES DO ITEM
        ///// </summary>
        ///// <response code="200">Returns success</response>
        ///// <response code="400">Custom Error</response>
        ///// <response code="401">Unauthorize Error</response>
        ///// <response code="500">Exception Error</response>
        ///// <returns></returns>
        [HttpGet("DetailNotification/{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<IActionResult> DetailNotification([FromRoute] string id)
        {
            try
            {
                var Entity = await _notificationRepository.FindByIdAsync(id).ConfigureAwait(false);
                if (Entity == null)
                    return BadRequest(Utilities.ReturnErro(nameof(DefaultMessages.InformativeNotFound)));

                var vieoViewModel = _mapper.Map<NotificationListViewModel>(Entity);

                return Ok(Utilities.ReturnSuccess(data: vieoViewModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(nameof(DefaultMessages.MessageException)));
            }
        }
        ///// <summary>
        ///// METODO DE DETALHES DAS NOTIFICAÇÕES ENVIADAS
        ///// </summary>
        ///// <response code="200">Returns success</response>
        ///// <response code="400">Custom Error</response>
        ///// <response code="401">Unauthorize Error</response>
        ///// <response code="500">Exception Error</response>
        ///// <returns></returns>
        [HttpGet("DetailNotificationSended/{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<IActionResult> DetailNotificationSended([FromRoute] string id)
        {
            try
            {

                var entity = await _notificationSendedRepository.FindByIdAsync(id).ConfigureAwait(false);
                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(nameof(DefaultMessages.InformativeNotFound)));

                var informativeViewModel = _mapper.Map<NotificationSendedListViewModel>(entity);

                return Ok(Utilities.ReturnSuccess(data: informativeViewModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(nameof(DefaultMessages.MessageException)));
            }
        }
        ///// <summary>
        ///// LISTA TODAS AS NOTIFICAÇÕES
        ///// </summary>
        ///// <response code="200">Returns success</response>
        ///// <response code="400">Custom Error</response>
        ///// <response code="401">Unauthorize Error</response>
        ///// <response code="500">Exception Error</response>
        ///// <returns></returns>
        [HttpGet("GetAllNotification")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllNotification()
        {
            try
            {
                var entity = await _notificationRepository.FindByAsync(x => x.DataBlocked == null).ConfigureAwait(false);
                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(nameof(DefaultMessages.NotificationNotFound)));

                var list = _mapper.Map<List<NotificationListViewModel>>(entity);

                return Ok(Utilities.ReturnSuccess(data: list));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(nameof(DefaultMessages.MessageException)));
            }
        }
        /// <summary>
        /// LISTA TODOS OS NOTIFICAÇÕES POR FAMÍLIA
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("GetAllNotificationByFamily")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllNotificationByFamily()
        {
            try
            {
                var familyId = Request.GetUserId();
                var entity = await _notificationSendedRepository.FindByAsync(x => x.FamilyId == familyId).ConfigureAwait(false);
                if (entity.Count() == 0)
                    return BadRequest(Utilities.ReturnErro(nameof(DefaultMessages.InformativeNotFound)));

                var vieoViewModel = _mapper.Map<List<NotificationSendedListViewModel>>(entity);

                return Ok(Utilities.ReturnSuccess(data: vieoViewModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(nameof(DefaultMessages.MessageException)));
            }
        }

        /// <summary>
        /// CADASTRAR UMA NOTIFICAÇÃO POR FAMÍLIA OU TODAS
        /// </summary>
        /// <remarks>
        ///OBJ DE ENVIO
        ///
        /// 
        ///         POST
        ///         {
        ///
        ///              "familyId": [
        ///                 "string"
        ///              ]
        ///              },
        ///              "allFamily": "false",  // INFORME TRUE PARA TODAS AS FAMILIAS
        ///              "image": "210831110311.jpg",
        ///              "title": "string",
        ///              "description": "string"
        ///          }    
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
        public async Task<IActionResult> Register([FromBody] NotificationViewModel model)
        {
            try
            {
                model.TrimStringProperties();
                //var ignoreValidation = new List<string>();

                //var isInvalidState = ModelState.ValidModelState(ignoreValidation.ToArray());

                //if (isInvalidState != null)
                //    return BadRequest(isInvalidState);  


                var entityFamily = new List<Family>();
                if (model.AllFamily == true)
                    entityFamily = await _familyRepository.FindByAsync(x => x.Disabled == null).ConfigureAwait(false) as List<Family>;
                else
                {
                    entityFamily = await _familyRepository.FindIn("_id", model.FamilyId.Select(x => ObjectId.Parse(x)).ToList()) as List<Family>;
                    if (model.FamilyId.Count() != entityFamily.Count())
                        return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));
                }

                foreach (var item in entityFamily)
                {
                    var entityNotification = new Notification()
                    {
                        Created = DateTimeOffset.Now.ToUnixTimeSeconds(),
                        FamilyId = item._id.ToString(),
                        Title = model.Title,
                        Description = model.Description,
                        Image = model.Image.SetPathImage()
                    };
                    
                    
                    var informativeId = await _notificationRepository.CreateAsync(entityNotification).ConfigureAwait(false);

                    await _utilService.RegisterLogAction(LocalAction.Notificacao, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Cadastrou uma notificação {Request.GetUserName()}", Request.GetUserId(), Request.GetUserName().Value, informativeId);

                }

                //var entity = _mapper.Map<Data.Entities.Notification>(model);
                //var informativeId = await _notificationRepository.CreateAsync(entity).ConfigureAwait(false);
                //  await _utilService.RegisterLogAction(LocalAction.Notificacao, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Cadatrou uma notrificação {Request.GetUserName()}", Request.GetUserId(), Request.GetUserName().Value, informativeId);
                return Ok(Utilities.ReturnSuccess(nameof(DefaultMessages.Registred)));

            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Notificacao, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível cadastrar nova notificação", "", "", "", "", ex);
                return BadRequest(ex.ReturnErro(nameof(DefaultMessages.MessageException)));
            }
        }

        /// <summary>
        /// ENVIA UMA NOTIFICAÇÃO
        /// </summary>
        /// <remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("Send/{notificationId}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<IActionResult> Send(string notificationId)
        {
            try
            {

                var entityNotification = await _notificationRepository.FindByIdAsync(notificationId).ConfigureAwait(false);
                if (entityNotification == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InformativeNotFound));

                var entityFamily = await _familyRepository.FindAllAsync().ConfigureAwait(false);
                foreach (var item in entityFamily)
                {
                    var notificationSended = new NotificationSended()
                    {
                        FamilyId = item._id.ToString(),
                        NotificationId = notificationId
                    };
                    await _notificationSendedRepository.CreateAsync(notificationSended);
                }
                await _utilService.RegisterLogAction(LocalAction.Notificacao, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Cadatrou uma notrificação {Request.GetUserName()}", Request.GetUserId(), Request.GetUserName().Value, "");
                return Ok(Utilities.ReturnSuccess(nameof(DefaultMessages.Registred)));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Notificacao, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível cadastrar nova notificação", "", "", "", "", ex);
                return BadRequest(ex.ReturnErro(nameof(DefaultMessages.MessageException)));
            }
        }
        /// <summary>
        /// MODIFICA UMA NOTIFICAÇÃO PARA LIDO
        /// </summary>
        /// <remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("ChangeToRead/{notificationId}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ChangeToRead(string notificationId)
        {
            try
            {

                var entityNotificationSended = await _notificationSendedRepository.FindByIdAsync(notificationId).ConfigureAwait(false);
                if (entityNotificationSended == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.NotificationNotFound));

                entityNotificationSended.DateViewed = Utilities.ToTimeStamp(DateTime.Now);
                var entityFamily = await _notificationSendedRepository.UpdateOneAsync(entityNotificationSended).ConfigureAwait(false);
                await _utilService.RegisterLogAction(LocalAction.Notificacao, TypeAction.Change, TypeResposible.UserAdminstratorGestor, $"Leu a notificação{Request.GetUserName()}", Request.GetUserId(), Request.GetUserName().Value, entityFamily);
                return Ok(Utilities.ReturnSuccess(DefaultMessages.Updated));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Notificacao, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi ler a notificação", "", "", "", "", ex);
                return BadRequest(ex.ReturnErro(nameof(DefaultMessages.MessageException)));
            }
        }

        /// <summary>
        /// METODO DE LISTAGEM COM DATATABLE
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("LoadDataNotification")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<IActionResult> LoadDataNotification([FromForm] DtParameters model)
        {
            var response = new DtResult<NotificationViewModel>();
            try
            {
                var builder = Builders<Notification>.Filter;
                var conditions = new List<FilterDefinition<Notification>>();

                conditions.Add(builder.Where(x => x.Disabled == null));

                var columns = model.Columns.Where(x => x.Searchable && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToArray();

                var sortColumn = model.SortOrder;
                var totalRecords = (int)await _notificationRepository.GetCollectionAsync().CountDocumentsAsync(builder.And(conditions));

                var sortBy = model.Order[0].Dir.ToString().ToUpper().Equals("DESC")
                   ? Builders<Notification>.Sort.Descending(sortColumn)
                   : Builders<Notification>.Sort.Ascending(sortColumn);

                var retorno = await _notificationRepository
                 .LoadDataTableAsync(model.Search.Value, sortBy, model.Start, model.Length, conditions, fields: columns);

                var totalrecordsFiltered = !string.IsNullOrEmpty(model.Search.Value)
                   ? (int)await _notificationRepository.CountSearchDataTableAsync(model.Search.Value, conditions, columns)
                   : totalRecords;

                response.Data = _mapper.Map<List<NotificationViewModel>>(retorno.OrderBy(x => x.Created));
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
        /// METODO DE LISTAGEM COM DATATABLE DAS NOTIFICAÇÕES ENVIADAS
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("LoadDataInformativeSended")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<IActionResult> LoadDataNotificationSended([FromForm] DtParameters model, [FromForm] string description, [FromForm] long startDate, [FromForm] long endDate)
        {
            var response = new DtResult<NotificationSendedViewModel>();
            try
            {
                var builder = Builders<NotificationSended>.Filter;
                var conditions = new List<FilterDefinition<NotificationSended>>();

                conditions.Add(builder.Where(x => x.Created != null));
                //if (!string.IsNullOrEmpty(number))
                //    conditions.Add(builder.Where(x => x.Holder.Number == number));
                //if (!string.IsNullOrEmpty(description))
                //    conditions.Add(builder.Where(x => x.Holder.Name.ToUpper().Contains(description.ToUpper())));
                //if (!string.IsNullOrEmpty(holderCpf))
                //    conditions.Add(builder.Where(x => x.Holder.Cpf == holderCpf.OnlyNumbers()));
                var columns = model.Columns.Where(x => x.Searchable && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToArray();

                var sortColumn = model.SortOrder;
                var totalRecords = (int)await _notificationSendedRepository.GetCollectionAsync().CountDocumentsAsync(builder.And(conditions));

                var sortBy = model.Order[0].Dir.ToString().ToUpper().Equals("DESC")
                   ? Builders<NotificationSended>.Sort.Descending(sortColumn)
                   : Builders<NotificationSended>.Sort.Ascending(sortColumn);

                var retorno = await _notificationSendedRepository
                 .LoadDataTableAsync(model.Search.Value, sortBy, model.Start, model.Length, conditions, fields: columns);

                var totalrecordsFiltered = !string.IsNullOrEmpty(model.Search.Value)
                   ? (int)await _notificationSendedRepository.CountSearchDataTableAsync(model.Search.Value, conditions, columns)
                   : totalRecords;

                response.Data = _mapper.Map<List<NotificationSendedViewModel>>(retorno);
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
        /// METODO DE REMOVER ITEM
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("Delete/{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            try
            {
                if (!await _notificationRepository.CheckByAsync(x => x._id == ObjectId.Parse(id)).ConfigureAwait(false))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InformativeNotFound));

                 await _notificationRepository.DeleteOneAsync(id).ConfigureAwait(false);
                await _utilService.RegisterLogAction(LocalAction.Notificacao, TypeAction.Delete, TypeResposible.UserAdminstratorGestor, $"Deletou uma notrificação {Request.GetUserName()}", Request.GetUserId(), Request.GetUserName().Value, id);
                return Ok(Utilities.ReturnSuccess(nameof(DefaultMessages.Deleted)));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Notificacao, TypeAction.Delete, TypeResposible.UserAdminstratorGestor, $"Não foi possível cadastrar nova notificação", "", "", "", "", ex);
                return BadRequest(ex.ReturnErro(nameof(DefaultMessages.MessageException)));
            }
        }
        /// <summary>
        /// BLOQUEIA OU DESBLOQUEIA UMA NOTIFICAÇÃO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("BlockUnblock")]
        [Produces("application/json")]
        //[ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> BlockUnblock([FromBody] BlockViewModel model)
        {

            try
            {
                if (ObjectId.TryParse(model.TargetId, out var unused) == false)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidIdentifier));

                var entityVideo = await _notificationRepository.FindOneByAsync(x => x._id == ObjectId.Parse(model.TargetId)).ConfigureAwait(false);

                if (entityVideo == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.VideoNotFound));

                entityVideo.DataBlocked = model.Block ? DateTimeOffset.Now.ToUnixTimeSeconds() : (long?)null;


                var entityId = await _notificationRepository.UpdateOneAsync(entityVideo).ConfigureAwait(false);
                await _utilService.RegisterLogAction(LocalAction.Notificacao, model.Block==true? TypeAction.Block:TypeAction.UnBlock, TypeResposible.UserAdminstratorGestor, $"Bloqueou uma notificação {Request.GetUserName()}", Request.GetUserId(), Request.GetUserName().Value, entityVideo._id.ToString());
                return Ok(Utilities.ReturnSuccess("Registrado com sucesso"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Notificacao, TypeAction.Block, TypeResposible.UserAdminstratorGestor, $"Não foi bloquear a notificação", "", "", "", "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }


    }

}