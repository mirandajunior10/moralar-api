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
using Moralar.Domain.ViewModels.Course;
using Moralar.Domain.ViewModels.Family;
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

    public class CourseController : Controller
    {

        private readonly IMapper _mapper;
        private readonly ICourseRepository _courseRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUtilService _utilService;

        public CourseController(IMapper mapper, ICourseRepository courseRepository, IHttpContextAccessor httpContextAccessor, IUtilService utilService)
        {
            _mapper = mapper;
            _courseRepository = courseRepository;
            _httpContextAccessor = httpContextAccessor;
            _utilService = utilService;
        }


        /// <summary>
        /// BLOQUEAR / DESBLOQUEAR AGENTE - ADMIN
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
            try
            {
                model.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelStateOnlyFields(nameof(model.TargetId));

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                var entity = await _courseRepository.FindByIdAsync(model.TargetId);

                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound));
                entity.DataBlocked = model.Block ? DateTimeOffset.Now.ToUnixTimeSeconds() : (long?)null;

                await _courseRepository.UpdateAsync(entity);
                var typeAction = model.Block == true ? TypeAction.Block : TypeAction.UnBlock;
                await _utilService.RegisterLogAction(LocalAction.Curso, typeAction, TypeResposible.UserAdminstratorGestor, $"Bloqueio de família { Request.GetUserName().Value}", Request.GetUserId(), Request.GetUserName().Value, model.TargetId);
                return Ok(Utilities.ReturnSuccess(model.Block ? "Bloqueado com sucesso" : "Desbloqueado com sucesso"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível bloquer Família", Request.GetUserId(), Request.GetUserName().Value, model.TargetId, "", ex);
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
        [AllowAnonymous]
        public async Task<IActionResult> LoadData([FromForm] DtParameters model, [FromForm] string title, long startDate, long endDate)
        {
            var response = new DtResult<CourseViewModel>();
            try
            {
                var builder = Builders<Data.Entities.Course>.Filter;
                var conditions = new List<FilterDefinition<Data.Entities.Course>>();

                conditions.Add(builder.Where(x => x.Created != null));
                if (!string.IsNullOrEmpty(title))
                    conditions.Add(builder.Where(x => x.Title.ToUpper().Contains(title)));
                //if (!string.IsNullOrEmpty(nameHolder))
                //    conditions.Add(builder.Where(x => x.Holder.Name.ToUpper().Contains(nameHolder.ToUpper())));
                //if (!string.IsNullOrEmpty(cpf))
                //    conditions.Add(builder.Where(x => x.Holder.Cpf == cpf));

                var columns = model.Columns.Where(x => x.Searchable && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToArray();

                var sortColumn = !string.IsNullOrEmpty(model.SortOrder) ? model.SortOrder.UppercaseFirst() : model.Columns.FirstOrDefault(x => x.Orderable)?.Name ?? model.Columns.FirstOrDefault()?.Name;
                var totalRecords = (int)await _courseRepository.GetCollectionAsync().CountDocumentsAsync(builder.And(conditions));

                var sortBy = model.Order[0].Dir.ToString().ToUpper().Equals("DESC")
                    ? Builders<Data.Entities.Course>.Sort.Descending(sortColumn)
                    : Builders<Data.Entities.Course>.Sort.Ascending(sortColumn);

                var retorno = await _courseRepository
                    .LoadDataTableAsync(model.Search.Value, sortBy, model.Start, model.Length, conditions, columns);

                var totalrecordsFiltered = !string.IsNullOrEmpty(model.Search.Value)
                    ? (int)await _courseRepository.CountSearchDataTableAsync(model.Search.Value, conditions, columns)
                    : totalRecords;

                response.Data = _mapper.Map<List<CourseViewModel>>(retorno);
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
        /// DETALHES DA FAMÍLIA
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
        [AllowAnonymous]
        public async Task<IActionResult> Detail([FromRoute] string id)
        {
            try
            {
                //if (ObjectId.TryParse(id, out var unused) == false)
                //    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredencials));

                //var userId = Request.GetUserId();

                //if (string.IsNullOrEmpty(userId))
                //    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredencials));

                var entity = await _courseRepository.FindByIdAsync(id).ConfigureAwait(false);

                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.CourseNotFound));

                return Ok(Utilities.ReturnSuccess(data: _mapper.Map<CourseViewModel>(entity)));
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
        [HttpGet("GetAll")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                //    if (ObjectId.TryParse(id, out var unused) == false)
                //        return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredencials));

                //    var userId = Request.GetUserId();

                //    if (string.IsNullOrEmpty(userId))
                //        return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredencials));

                var entity = await _courseRepository.FindAllAsync().ConfigureAwait(false);

                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ResidencialPropertyNotFound));

                return Ok(Utilities.ReturnSuccess(data: _mapper.Map<List<FamilyHolderListViewModel>>(entity)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// REGISTRAR FAMÍLIA
        /// </summary>
        /// <remarks>
        ///OBJ DE ENVIO
        ///         POST
        ///        
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
        public async Task<IActionResult> Register([FromBody] CourseViewModel model)
        {
            //var claim = Util.SetRole(TypeProfile.Profile);
            //var typeAction = string.IsNullOrEmpty(model.Id) ? TypeAction.Register : TypeAction.Change;
            try
            {
                model.TrimStringProperties();
                var ignoreValidation = new List<string>();
                var isInvalidState = ModelState.ValidModelState(ignoreValidation.ToArray());

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                var entity = _mapper.Map<Data.Entities.Course>(model);
                entity.Img = model.Img.SetPathImage();
                var entityId = await _courseRepository.CreateAsync(entity).ConfigureAwait(false);
                //await _utilService.RegisterLogAction(LocalAction.Curso, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Cadastro de nova família {Request.GetUserName().Value}", Request.GetUserId(), Request.GetUserName().Value, entityId, "");

                return Ok(Utilities.ReturnSuccess(data: "Registrado com sucesso!"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Curso, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível cadastrar nova Família", "", "", "", "", ex);
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
        //        var entityFamily = await _courseRepository.FindByIdAsync(model.Id).ConfigureAwait(false);
        //        if (entityFamily == null)
        //            return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));

        //        var validOnly = _httpContextAccessor.GetFieldsFromBody();

        //        var p = entityFamily.SetIfDifferent(model.Holder, validOnly);
        //        var t = entityFamily.SetIfDifferent(model.Members, validOnly);

        //        var entity = _mapper.Map<Data.Entities.Family>(model);

        //        var entityId = await _courseRepository.UpdateAsync(entity).ConfigureAwait(false);
        //        await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Change, TypeResposible.UserAdminstratorGestor, $"Update de nova família {entity.Holder.Name}", "", "", model.Id);//Request.GetUserName().Value, Request.GetUserId()

        //        return Ok(Utilities.ReturnSuccess(data: "Registrado com sucesso!"));
        //    }
        //    catch (Exception ex)
        //    {
        //        await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível cadastrar nova Família", "", "", "", "", ex);
        //        return BadRequest(ex.ReturnErro());
        //    }
        //}

    }
}