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
using Moralar.Domain.ViewModels.Property;
using Moralar.Domain.ViewModels.ResidencialProperty;
using Moralar.Repository.Interface;
using System;
using System.Collections.Generic;
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

    public class ResidencialPropertyController : Controller
    {

        private readonly IMapper _mapper;
        private readonly IUtilService _utilService;
        private readonly ISenderMailService _senderMailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IResidencialPropertyRepository _residencialPropertyRepository;

        public ResidencialPropertyController(IMapper mapper, IUtilService utilService, ISenderMailService senderMailService, IHttpContextAccessor httpContextAccessor, IResidencialPropertyRepository residencialPropertyRepository)
        {
            _mapper = mapper;
            _utilService = utilService;
            _senderMailService = senderMailService;
            _httpContextAccessor = httpContextAccessor;
            _residencialPropertyRepository = residencialPropertyRepository;
        }
        /// <summary>
        /// BLOQUEAR / DESBLOQUEAR PROPRIEDADE
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         POST
        ///             {
        ///              "id": "string", // required
        ///              "block": true,
        ///              "reason": "" //motivo de bloquear o usuário
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("BlockUnBlock")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> BlockUnBlock([FromBody] BlockViewModel model)
        {
            var typeAction = model.Block == true ? TypeAction.Block : TypeAction.UnBlock;
            try
            {
                model.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelStateOnlyFields(nameof(model.TargetId));

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                var entity = await _residencialPropertyRepository.FindByIdAsync(model.TargetId);

                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ResidencialPropertyNotFound));
                entity.DataBlocked = model.Block ? DateTimeOffset.Now.ToUnixTimeSeconds() : (long?)null;
                entity.Reason = model.Block ? model.Reason : null;

                await _residencialPropertyRepository.UpdateAsync(entity);
                await _utilService.RegisterLogAction(LocalAction.ResidencialProperty, typeAction, TypeResposible.UserAdminstratorGestor, $"Bloqueio do imóvel {entity._id}", Request.GetUserId(), Request.GetUserName().Value, model.TargetId);
                return Ok(Utilities.ReturnSuccess(model.Block ? "Bloqueado com sucesso" : "Desbloqueado com sucesso"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Familia, typeAction, TypeResposible.UserAdminstratorGestor, $"Não foi possível bloquer o imóvel", Request.GetUserId(), Request.GetUserName().Value, model.TargetId, "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }
        /// <summary>
        /// LISTAGEM DOS CLIENTES
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
        public async Task<IActionResult> LoadData([FromForm] DtParameters model, [FromForm] string code, int status, int availableForSale)
        {
            var response = new DtResult<ResidencialPropertyViewModel>();
            try
            {
                var builder = Builders<Data.Entities.ResidencialProperty>.Filter;
                var conditions = new List<FilterDefinition<Data.Entities.ResidencialProperty>>();

                conditions.Add(builder.Where(x => x.Created != null));
                if (!string.IsNullOrEmpty(code))
                    conditions.Add(builder.Where(x => x.Code.ToUpper() == code.ToUpper()));
                if (status == 0)
                    conditions.Add(builder.Where(x => x.DataBlocked == null));
                else if (status == 1)
                    conditions.Add(builder.Where(x => x.DataBlocked != null));
                //var condition = builder.And(conditions);

                var columns = model.Columns.Where(x => x.Searchable && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToArray();

                var sortColumn = !string.IsNullOrEmpty(model.SortOrder) ? model.SortOrder.UppercaseFirst() : model.Columns.FirstOrDefault(x => x.Orderable)?.Name ?? model.Columns.FirstOrDefault()?.Name;
                var totalRecords = (int)await _residencialPropertyRepository.GetCollectionAsync().CountDocumentsAsync(builder.And(conditions));

                var sortBy = model.Order[0].Dir.ToString().ToUpper().Equals("DESC")
                    ? Builders<Data.Entities.ResidencialProperty>.Sort.Descending(sortColumn)
                    : Builders<Data.Entities.ResidencialProperty>.Sort.Ascending(sortColumn);

                var retorno = await _residencialPropertyRepository
                    .LoadDataTableAsync(model.Search.Value, sortBy, model.Start, model.Length, conditions, columns);

                var totalrecordsFiltered = !string.IsNullOrEmpty(model.Search.Value)
                    ? (int)await _residencialPropertyRepository.CountSearchDataTableAsync(model.Search.Value, conditions, columns)
                    : totalRecords;

                response.Data = _mapper.Map<List<ResidencialPropertyViewModel>>(retorno);
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
        /// DETALHES DA PROPRIEDADE
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("Detail/{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Detail([FromRoute] string id)
        {
            try
            {
                if (ObjectId.TryParse(id, out var unused) == false)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredencials));

                var userId = Request.GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredencials));

                var entity = await _residencialPropertyRepository.FindByIdAsync(id).ConfigureAwait(false);

                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ResidencialPropertyNotFound));

                return Ok(Utilities.ReturnSuccess(data: _mapper.Map<ResidencialPropertyViewModel>(entity)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }
        /// <summary>
        /// LISTA TODAS PROPRIEDADE
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("GetAllBy")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllBy([FromQuery] string code, [FromQuery] int status, [FromQuery] int availableForSale)
        {
            try
            {
                //[FromRoute] long startDate, [FromQuery] bool onlyConfigured, [FromQuery] string cityId
                var builder = Builders<Data.Entities.ResidencialProperty>.Filter;
                var conditions = new List<FilterDefinition<Data.Entities.ResidencialProperty>>();

                conditions.Add(builder.Where(x => x.Created != null));
                if (!string.IsNullOrEmpty(code))
                    conditions.Add(builder.Where(x => x.Code.ToUpper() == code.ToUpper()));
                if (status == 0)
                    conditions.Add(builder.Where(x => x.DataBlocked == null));
                else if (status == 1)
                    conditions.Add(builder.Where(x => x.DataBlocked != null));


                var condition = builder.And(conditions);
                var entity = await _residencialPropertyRepository.GetCollectionAsync().FindSync(condition, new FindOptions<Data.Entities.ResidencialProperty>() { }).ToListAsync();
                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ResidencialPropertyNotFound));

                return Ok(Utilities.ReturnSuccess(data: _mapper.Map<List<ResidencialPropertyViewModel>>(entity)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }


        /// <summary>
        /// REGISTRAR NOVA PROPRIEDADE
        /// </summary>
        /// <remarks>
        ///OBJ DE ENVIO
        ///         POST
        ///{
        ///  "code": "fasdfasd",
        ///  "photo": [
        ///    "1.jpg",
        ///    "2.jpg",
        ///    "3.jpg"
        ///  ],
        ///  "project": "project",
        /// "residencialPropertyAdress": {
        ///  "streetAddress": "streetAddress",
        ///  "number": "number",
        ///  "cityName": "cityName",
        ///  "cityId": "cityId",
        ///  "stateName": "stateName",
        ///  "stateUf": "stateUf",
        ///  "stateId": "stateId",
        ///  "neighborhood": "neighborhood",
        ///  "complement": "complement",
        ///  "location": "location",
        ///},
        ///  "residencialPropertyFeatures": {
        ///    "propertyValue": 200,
        ///    "typeProperty": 0,// Enum
        ///    "squareFootage": 1,
        ///    "condominiumValue": 22,
        ///    "iptuValue": 333,
        ///    "neighborhood": "neighborhood",
        ///    "numberFloors": 20,
        ///    "floorLocation": 2,
        ///    "hasElavator": true,
        ///    "numberOfBedrooms": 1,
        ///    "numberOfBathrooms": 1,
        ///    "hasServiceArea": true,
        ///    "hasGarage": true,
        ///    "hasYard": true,
        ///    "hasCistern": true,
        ///    "hasWall": true,
        ///    "hasAccessLadder": true,
        ///    "hasAccessRamp": true,
        ///    "hasAdaptedToPcd": true,
        ///    "propertyRegularization": 1,//Enum
        ///    "typeGasInstallation": 1 //Enum
        ///  }
        ///}
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
        public async Task<IActionResult> Register([FromBody] ResidencialPropertyViewModel model)
        {
            //var claim = Util.SetRole(TypeProfile.Profile);
            //var typeAction = string.IsNullOrEmpty(model.Id) ? TypeAction.Register : TypeAction.Change;
            try
            {
                if (model.Photo.Count() == 0 || model.Photo.Count > 15)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.AmountPhoto));

                var ignoreValidation = new List<string>();
                var isInvalidState = ModelState.ValidModelState(ignoreValidation.ToArray());

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                for (int i = 0; i < model.Photo.Count(); i++)
                    model.Photo[i] = model.Photo[i].SetPathImage();
                var entity = _mapper.Map<Data.Entities.ResidencialProperty>(model);

                var entityId = await _residencialPropertyRepository.CreateAsync(entity).ConfigureAwait(false);
                await _utilService.RegisterLogAction(LocalAction.ResidencialProperty, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Cadastro de novo imóvel {entity.Code}", Request.GetUserId(), Request.GetUserName().Value, entityId, "");

                return Ok(Utilities.ReturnSuccess(data: "Registrado com sucesso!"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.ResidencialProperty, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível cadastrar novo imóvel", Request.GetUserId(), Request.GetUserName().Value, "", "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }

        //[HttpPost("Edit")]
        //[Produces("application/json")]
        //[ProducesResponseType(typeof(ReturnViewModel), 200)]
        //[ProducesResponseType(400)]
        //[ProducesResponseType(401)]
        //[ProducesResponseType(500)]
        //[AllowAnonymous]
        //public async Task<IActionResult> Edit([FromBody] FamilyEditViewModel model)
        //{
        //    try
        //    {
        //        var entityFamily = await _familyRepository.FindByIdAsync(model.Id).ConfigureAwait(false);
        //        if (entityFamily == null)
        //            return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));

        //        var validOnly = _httpContextAccessor.GetFieldsFromBody();

        //        var p = entityFamily.SetIfDifferent(model.Holder, validOnly);
        //        var t = entityFamily.SetIfDifferent(model.Members, validOnly);
        //        var fast = model.SetIfDifferent(entityFamily.Members, validOnly);
        //        var entity = _mapper.Map<Data.Entities.Family>(model);

        //        var entityId = await _familyRepository.UpdateAsync(entity).ConfigureAwait(false);
        //        await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Change, TypeResposible.UserAdminstratorGestor, $"Update de nova família {entity.Holder.Name}", "", "", model.Id);//Request.GetUserName().Value, Request.GetUserId()

        //        return Ok(Utilities.ReturnSuccess(data: "Registrado com sucesso!"));
        //    }
        //    catch (Exception ex)
        //    {
        //        await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível cadastrar nova Família", "", "", "", "", ex);
        //        return BadRequest(ex.ReturnErro());
        //    }
        //}
        /// <summary>
        /// DELETAR FOTO
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///         POST
        ///           {
        ///             "id": "6011d02a4c7c9e71c25df866", Id da propriedade
        ///             "name": "" // nome da foto
        ///            }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("DeletePhoto")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeletePhoto([FromBody] ResidencialPropertyDeletePhotoViewModel model)
        {
            try
            {
                var entity = await _residencialPropertyRepository.FindByIdAsync(model.Id).ConfigureAwait(false);
                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ResidencialPropertyNotFound));
                if (entity.Photo.Count() == 0 || entity.Photo.FindIndex(x => x == model.Name) < 0)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.PhotoNotFound));

                entity.Photo.RemoveAt(entity.Photo.FindIndex(x => x == model.Name));

                await _residencialPropertyRepository.UpdateOneAsync(entity).ConfigureAwait(false);

                await _utilService.RegisterLogAction(LocalAction.ResidencialProperty, TypeAction.Delete, TypeResposible.UserAdminstratorGestor, $"Remover foto  ", Request.GetUserId(), Request.GetUserName().Value, entity._id.ToString());

                return Ok(Utilities.ReturnSuccess(data: "Foto excluída com sucesso!"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.ResidencialProperty, TypeAction.Delete, TypeResposible.UserAdminstratorGestor, $"Não foi possível a foto", Request.GetUserId(), Request.GetUserName().Value, model.Id, "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }
        /// <summary>
        /// REGISTRA NOVA FOTO
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///         POST
        ///           {
        ///             "id": "6011d02a4c7c9e71c25df866", Id da propriedade
        ///             "name": "" // nome da foto
        ///            }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("RegisterNewPhoto")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterNewPhoto([FromBody] ResidencialPropertyAddPhoto model)
        {
            try
            {
                var entity = await _residencialPropertyRepository.FindByIdAsync(model.Id).ConfigureAwait(false);
                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ResidencialPropertyNotFound));

                if (model.Photo.Count() == 0)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.AmountPhoto));

                int amoutPhoto = entity.Photo.Count() + 1;
                if (amoutPhoto > 15)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.AmountPhotoInclude));

                entity.Photo.Add(model.Photo.SetPathImage());

                await _residencialPropertyRepository.UpdateOneAsync(entity).ConfigureAwait(false);

                await _utilService.RegisterLogAction(LocalAction.ResidencialProperty, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Registrar foto  ", Request.GetUserId(), Request.GetUserName().Value, entity._id.ToString());

                return Ok(Utilities.ReturnSuccess(data: "Atualizado com sucesso!"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.ResidencialProperty, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível registrar a foto", Request.GetUserId(), Request.GetUserName().Value, model.Id, "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }


    }
}