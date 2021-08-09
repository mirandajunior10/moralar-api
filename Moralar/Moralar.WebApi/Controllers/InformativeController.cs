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
using Moralar.Domain.ViewModels.Question;
using Moralar.Domain.ViewModels.Quiz;
using Moralar.Repository.Interface;
using Moralar.WebApi.Services;
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

    public class InformativeController : Controller
    {

        private readonly IMapper _mapper;
        private readonly IInformativeRepository _informativeRepository;
        private readonly IInformativeSendedRepository _informativeSendedRepository;
        private readonly IFamilyRepository _familyRepository;
        private readonly ISenderMailService _senderMailService;
        private readonly IUtilService _utilService;

        public InformativeController(IMapper mapper, IInformativeRepository informativeRepository, IInformativeSendedRepository informativeSendedRepository, IFamilyRepository familyRepository, ISenderMailService senderMailService, IUtilService utilService)
        {
            _mapper = mapper;
            _informativeRepository = informativeRepository;
            _informativeSendedRepository = informativeSendedRepository;
            _familyRepository = familyRepository;
            _senderMailService = senderMailService;
            _utilService = utilService;
        }





        ///// <summary>
        ///// DETALHES DO INFORMATIVO
        ///// </summary>
        ///// <response code="200">Returns success</response>
        ///// <response code="400">Custom Error</response>
        ///// <response code="401">Unauthorize Error</response>
        ///// <response code="500">Exception Error</response>
        ///// <returns></returns>
        [HttpGet("DetailInformative/{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DetailInformative([FromRoute] string id)
        {
            try
            {
                var Entity = await _informativeRepository.FindByIdAsync(id).ConfigureAwait(false);
                if (Entity == null)
                    return BadRequest(Utilities.ReturnErro(nameof(DefaultMessages.InformativeNotFound)));

                var vieoViewModel = _mapper.Map<InformativeViewModel>(Entity);

                return Ok(Utilities.ReturnSuccess(data: vieoViewModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(nameof(DefaultMessages.MessageException)));
            }
        }
        ///// <summary>
        ///// METODO DE DETALHES DO INFORMATIVO ENVIADO
        ///// </summary>
        ///// <response code="200">Returns success</response>
        ///// <response code="400">Custom Error</response>
        ///// <response code="401">Unauthorize Error</response>
        ///// <response code="500">Exception Error</response>
        ///// <returns></returns>
        [HttpGet("DetailInformativeSended/{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<IActionResult> DetailInformativeSended([FromRoute] string id)
        {
            try
            {

                var entity = await _informativeSendedRepository.FindByIdAsync(id).ConfigureAwait(false);
                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(nameof(DefaultMessages.InformativeNotFound)));

                var informativeViewModel = _mapper.Map<InformativeSendedDetailViewModel>(entity);

                return Ok(Utilities.ReturnSuccess(data: informativeViewModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(nameof(DefaultMessages.MessageException)));
            }
        }
        ///// <summary>
        ///// LISTA TODOS OS INFORMATIVOS
        ///// </summary>
        ///// <response code="200">Returns success</response>
        ///// <response code="400">Custom Error</response>
        ///// <response code="401">Unauthorize Error</response>
        ///// <response code="500">Exception Error</response>
        ///// <returns></returns>
        [HttpGet("GetAllInformative")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllInformative()
        {
            try
            {
                var entity = await _informativeRepository.FindByAsync(x => x.DataBlocked == null).ConfigureAwait(false);
                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(nameof(DefaultMessages.InformativeNotFound)));

                var vieoViewModel = _mapper.Map<List<InformativeViewModel>>(entity);

                return Ok(Utilities.ReturnSuccess(data: vieoViewModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(nameof(DefaultMessages.MessageException)));
            }
        }
        ///// <summary>
        ///// LISTA TODOS OS INFORMATIVOS POR FAMÍLIA
        ///// </summary>
        ///// <response code="200">Returns success</response>
        ///// <response code="400">Custom Error</response>
        ///// <response code="401">Unauthorize Error</response>
        ///// <response code="500">Exception Error</response>
        ///// <returns></returns>
        [HttpGet("GetAllInformativeByFamily")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllInformativeByFamily()
        {
            try
            {
                var familyId = Request.GetUserId();
                var entity = await _informativeSendedRepository.FindByAsync(x => x.FamilyId == familyId).ConfigureAwait(false);
                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(nameof(DefaultMessages.InformativeNotFound)));

                var vieoViewModel = _mapper.Map<List<InformativeViewModel>>(entity);

                return Ok(Utilities.ReturnSuccess(data: vieoViewModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(nameof(DefaultMessages.MessageException)));
            }
        }

        /// <summary>
        /// CADASTRAR UM INFORMATIVE
        /// </summary>
        /// <remarks>
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
        public async Task<IActionResult> Register([FromBody] InformativeViewModel model)
        {
            try
            {
                var entity = _mapper.Map<Data.Entities.Informative>(model);
                entity.Image = model.Image.SetPathImage();
                entity.Status = TypeStatusActiveInactive.Ativo;
                var informativeId = await _informativeRepository.CreateAsync(entity).ConfigureAwait(false);

                var entityFamily = await _familyRepository.FindAllAsync().ConfigureAwait(false);
                foreach (var item in entityFamily)
                {
                    var informationSended = new InformativeSended()
                    {
                        FamilyId = item._id.ToString(),
                        InformativeId = informativeId
                    };
                    await _informativeSendedRepository.CreateAsync(informationSended);
                }
                await _utilService.RegisterLogAction(LocalAction.Informativo, TypeAction.Change, TypeResposible.UserAdminstratorGestor, $"Bloqueio de família {Request.GetUserName().Value}", Request.GetUserId(), Request.GetUserName().Value, informativeId);
                return Ok(Utilities.ReturnSuccess(DefaultMessages.Registred));

            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Informativo, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível cadastrar nova Família", "", "", "", "", ex);
                return BadRequest(ex.ReturnErro(nameof(DefaultMessages.MessageException)));
            }
        }
        /// <summary>
        /// MUDAR A SITUAÇÃO PARA INFORMATIVO VISTO
        /// </summary>
        /// <remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("ChangeStatusViewed/{informativeId}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ChangeStatusViewed([FromRoute] string informativeId)
        {
            try
            {

                var informativeSended = await _informativeSendedRepository.FindByIdAsync(informativeId).ConfigureAwait(false);
                if (informativeSended == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InformativeNotFound));
                informativeSended.DateViewed = Utilities.ToTimeStamp(DateTime.Now);
                await _informativeSendedRepository.UpdateOneAsync(informativeSended).ConfigureAwait(false);
                await _utilService.RegisterLogAction(LocalAction.Informativo, TypeAction.Change, TypeResposible.UserAdminstratorGestor, $"Mudou a situação para visualizada {Request.GetUserName().Value}", Request.GetUserId(), Request.GetUserName().Value, informativeId);
                return Ok(Utilities.ReturnSuccess(DefaultMessages.Updated));

            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Informativo, TypeAction.Change, TypeResposible.UserAdminstratorGestor, $"Não foi possível mudar a situação para visualizada", "", "", "", "", ex);
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
        [HttpPost("LoadData")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<IActionResult> LoadData([FromForm] DtParameters model)
        {
            var response = new DtResult<InformativeListViewModel>();
            try
            {
                var builder = Builders<Informative>.Filter;
                var conditions = new List<FilterDefinition<Informative>>();

                conditions.Add(builder.Where(x => x.Disabled == null));

                var columns = model.Columns.Where(x => x.Searchable && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToArray();

                var sortColumn = model.SortOrder;
                var totalRecords = (int)await _informativeRepository.GetCollectionAsync().CountDocumentsAsync(builder.And(conditions));

                var sortBy = model.Order[0].Dir.ToString().ToUpper().Equals("DESC")
                   ? Builders<Informative>.Sort.Descending(sortColumn)
                   : Builders<Informative>.Sort.Ascending(sortColumn);

                var retorno = await _informativeRepository
                 .LoadDataTableAsync(model.Search.Value, sortBy, model.Start, model.Length, conditions, fields: columns);

                var totalrecordsFiltered = !string.IsNullOrEmpty(model.Search.Value)
                   ? (int)await _informativeRepository.CountSearchDataTableAsync(model.Search.Value, conditions, columns)
                   : totalRecords;

                response.Data = _mapper.Map<List<InformativeListViewModel>>(retorno.OrderBy(x => x.Created)).ToList();
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
        /// METODO DE LISTAGEM COM DATATABLE
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
        public async Task<IActionResult> LoadDataInformativeSended([FromForm] DtParameters model, [FromForm] string description, [FromForm] long startDate, [FromForm] long endDate)
        {
            var response = new DtResult<InformativeSendedViewModel>();
            try
            {
                var builder = Builders<InformativeSended>.Filter;
                var conditions = new List<FilterDefinition<InformativeSended>>();

                conditions.Add(builder.Where(x => x.Created != null));
                //if (!string.IsNullOrEmpty(number))
                //    conditions.Add(builder.Where(x => x.Holder.Number == number));
                //if (!string.IsNullOrEmpty(description))
                //    conditions.Add(builder.Where(x => x.Holder.Name.ToUpper().Contains(description.ToUpper())));
                //if (!string.IsNullOrEmpty(holderCpf))
                //    conditions.Add(builder.Where(x => x.Holder.Cpf == holderCpf.OnlyNumbers()));
                var columns = model.Columns.Where(x => x.Searchable && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToArray();

                var sortColumn = model.SortOrder;
                var totalRecords = (int)await _informativeSendedRepository.GetCollectionAsync().CountDocumentsAsync(builder.And(conditions));

                var sortBy = model.Order[0].Dir.ToString().ToUpper().Equals("DESC")
                   ? Builders<InformativeSended>.Sort.Descending(sortColumn)
                   : Builders<InformativeSended>.Sort.Ascending(sortColumn);

                var retorno = await _informativeSendedRepository
                 .LoadDataTableAsync(model.Search.Value, sortBy, model.Start, model.Length, conditions, fields: columns);

                var totalrecordsFiltered = !string.IsNullOrEmpty(model.Search.Value)
                   ? (int)await _informativeSendedRepository.CountSearchDataTableAsync(model.Search.Value, conditions, columns)
                   : totalRecords;

                response.Data = _mapper.Map<List<InformativeSendedViewModel>>(retorno);
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
                if (!await _informativeRepository.CheckByAsync(x => x._id == ObjectId.Parse(id)).ConfigureAwait(false))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InformativeNotFound));

                await _informativeRepository.DeleteOneAsync(id).ConfigureAwait(false);

                return Ok(Utilities.ReturnSuccess(nameof(DefaultMessages.Deleted)));
                await _utilService.RegisterLogAction(LocalAction.Informativo, TypeAction.Delete, TypeResposible.UserAdminstratorGestor, $"Deletou o registro {Request.GetUserName().Value}", Request.GetUserId(), Request.GetUserName().Value, id);
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Informativo, TypeAction.Delete, TypeResposible.UserAdminstratorGestor, $"Não foi possível excluir informativo", "", "", "", "", ex);
                return BadRequest(ex.ReturnErro(nameof(DefaultMessages.MessageException)));
            }
        }
        /// <summary>
        /// BLOQUEIA OU DESBLOQUEIA O INFORMATIVO
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

                var entityInformative = await _informativeRepository.FindOneByAsync(x => x._id == ObjectId.Parse(model.TargetId)).ConfigureAwait(false);

                if (entityInformative == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.VideoNotFound));

                entityInformative.DataBlocked = model.Block ? DateTimeOffset.Now.ToUnixTimeSeconds() : (long?)null;


                var entityId = await _informativeRepository.UpdateOneAsync(entityInformative).ConfigureAwait(false);
                await _utilService.RegisterLogAction(LocalAction.Informativo, model.Block==true? TypeAction.Block: TypeAction.UnBlock, TypeResposible.UserAdminstratorGestor, $"Cadastrou bloqueou o informativo {entityInformative.Description}", Request.GetUserId(), Request.GetUserName().Value, entityId, "");
                return Ok(Utilities.ReturnSuccess("Registrado com sucesso"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Informativo, model.Block == true ? TypeAction.Block : TypeAction.UnBlock, TypeResposible.UserAdminstratorGestor, $"Não foi bloquear/desbloquear o informativo", "", "", "", "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// EXPORTAR PARA EXCEL
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("Export")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [OnlyAdministrator]
        public async Task<IActionResult> Export()
        {
            try
            {
                var allData = await _informativeRepository.FindAllAsync().ConfigureAwait(false);
                var fileName = $"informative.xlsx";
                var path = Path.Combine($"{Directory.GetCurrentDirectory()}/Content", @"ExportFiles");
                if (Directory.Exists(path) == false)
                    Directory.CreateDirectory(path);
                var fullPathFile = Path.Combine(path, fileName);
                var listViewModel = _mapper.Map<List<InformativeExportViewModel>>(allData);
                Utilities.ExportToExcel(listViewModel, path, fileName: fileName.Split('.')[0]);
                if (System.IO.File.Exists(fullPathFile) == false)
                    return BadRequest(Utilities.ReturnErro("Ocorreu um erro fazer download do arquivo"));

                var fileBytes = System.IO.File.ReadAllBytes(fullPathFile);
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }
    }
}