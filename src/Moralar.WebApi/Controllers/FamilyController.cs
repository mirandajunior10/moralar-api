using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MimeTypes.Core;
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
using Moralar.Domain.ViewModels.Schedule;
using Moralar.Domain.ViewModels.Shared;
using Moralar.Repository.Interface;
using Newtonsoft.Json;
using OfficeOpenXml;
using UtilityFramework.Application.Core;
using UtilityFramework.Application.Core.JwtMiddleware;
using UtilityFramework.Application.Core.ViewModels;
using UtilityFramework.Services.Core.Interface;


namespace Moralar.WebApi.Controllers
{
    [EnableCors("AllowAllOrigin")]
    [Route("api/v1/[controller]")]
    [Authorize(ActiveAuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class FamilyController : Controller
    {
        private readonly IHostingEnvironment _env;
        private readonly IMapper _mapper;
        private readonly IFamilyRepository _familyRepository;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IScheduleHistoryRepository _scheduleHistoryRepository;
        private readonly ICourseFamilyRepository _courseFamilyRepository;
        private readonly IQuizFamilyRepository _quizFamilyRepository;
        private readonly IResidencialPropertyRepository _residencialPropertyRepository;
        private readonly IPropertiesInterestRepository _propertiesInterestRepository;
        private readonly ICityRepository _cityRepository;
        private readonly IUtilService _utilService;
        private readonly ISenderMailService _senderMailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public readonly List<string> _acceptedFiles = new List<string>() { ".xls", ".xlsx" };


        public FamilyController(IHostingEnvironment env, IMapper mapper, IFamilyRepository familyRepository, IScheduleRepository scheduleRepository, IScheduleHistoryRepository scheduleHistoryRepository, ICourseFamilyRepository courseFamilyRepository, IQuizFamilyRepository quizFamilyRepository, IResidencialPropertyRepository residencialPropertyRepository, IPropertiesInterestRepository propertiesInterestRepository, ICityRepository cityRepository, IUtilService utilService, ISenderMailService senderMailService, IHttpContextAccessor httpContextAccessor)
        {
            _env = env;
            _mapper = mapper;
            _familyRepository = familyRepository;
            _scheduleRepository = scheduleRepository;
            _scheduleHistoryRepository = scheduleHistoryRepository;
            _courseFamilyRepository = courseFamilyRepository;
            _quizFamilyRepository = quizFamilyRepository;
            _residencialPropertyRepository = residencialPropertyRepository;
            _propertiesInterestRepository = propertiesInterestRepository;
            _cityRepository = cityRepository;
            _utilService = utilService;
            _senderMailService = senderMailService;
            _httpContextAccessor = httpContextAccessor;
        }
        /// <summary>
        /// BLOQUEAR / DESBLOQUEAR FAMÍLIA
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

                var entity = await _familyRepository.FindByIdAsync(model.TargetId);

                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound));
                entity.DataBlocked = model.Block ? DateTimeOffset.Now.ToUnixTimeSeconds() : (long?)null;
                entity.Reason = model.Block ? model.Reason : null;

                await _familyRepository.UpdateAsync(entity);
                var typeAction = model.Block == true ? TypeAction.Block : TypeAction.UnBlock;
                await _utilService.RegisterLogAction(LocalAction.Familia, typeAction, TypeResposible.UserAdminstratorGestor, $"Bloqueio de família {entity.Holder.Name}", Request.GetUserId(), Request.GetUserName()?.Value, model.TargetId);
                return Ok(Utilities.ReturnSuccess(model.Block ? "Bloqueado com sucesso" : "Desbloqueado com sucesso"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível bloquer Família", Request.GetUserId(), Request.GetUserName()?.Value, model.TargetId, "", ex);
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
            //, 
            var response = new DtResult<FamilyHolderListViewModel>();

            try
            {
                var builder = Builders<Data.Entities.Family>.Filter;
                var conditions = new List<FilterDefinition<Data.Entities.Family>>();

                conditions.Add(builder.Where(x => x.Created != null));
                if (!string.IsNullOrEmpty(number))
                    conditions.Add(builder.Where(x => x.Holder.Number == number));
                if (!string.IsNullOrEmpty(holderName))
                    conditions.Add(builder.Where(x => x.Holder.Name.ToUpper().Contains(holderName.ToUpper())));
                if (!string.IsNullOrEmpty(holderCpf))
                    conditions.Add(builder.Where(x => x.Holder.Cpf == holderCpf.OnlyNumbers()));

                var columns = model.Columns.Where(x => x.Searchable && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToArray();

                var sortColumn = !string.IsNullOrEmpty(model.SortOrder) ? model.SortOrder.UppercaseFirst() : model.Columns.FirstOrDefault(x => x.Orderable)?.Name ?? model.Columns.FirstOrDefault()?.Name;
                var totalRecords = (int)await _familyRepository.GetCollectionAsync().CountDocumentsAsync(builder.And(conditions));

                var sortBy = model.Order[0].Dir.ToString().ToUpper().Equals("DESC")
                    ? Builders<Data.Entities.Family>.Sort.Descending(sortColumn)
                    : Builders<Data.Entities.Family>.Sort.Ascending(sortColumn);

                var retorno = await _familyRepository
                    .LoadDataTableAsync(model.Search.Value, sortBy, model.Start, model.Length, conditions, columns);

                var totalrecordsFiltered = !string.IsNullOrEmpty(model.Search.Value)
                    ? (int)await _familyRepository.CountSearchDataTableAsync(model.Search.Value, conditions, columns)
                    : totalRecords;

                response.Data = _mapper.Map<List<FamilyHolderListViewModel>>(retorno);
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
        /// TIMELINE DAS FAMÍLIAS
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("TimeLine/LoadData")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        //[ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> TimeLineLoadData([FromForm] DtParameters model, [FromForm] string number, [FromForm] string holderName, [FromForm] string holderCpf, [FromForm] TypeSubject typeSubject)
        {
            var response = new DtResult<FamilyHolderListViewModel>();

            try
            {
                // var listEnumsOfTimeLine = new List<int> { 2, 4, 7, 8 };//verificar se o parâmetro corresponde a um dos types

                // if (!listEnumsOfTimeLine.Exists(x => x == (int)typeSubject))
                //     return BadRequest(Utilities.ReturnErro(DefaultMessages.EnumInvalid));

                var builder = Builders<Data.Entities.Family>.Filter;
                var conditions = new List<FilterDefinition<Data.Entities.Family>>();

                conditions.Add(builder.Where(x => x.Created != null));

                if (string.IsNullOrEmpty(number) == false)
                    conditions.Add(builder.Where(x => x.Holder.Number == number));

                if (string.IsNullOrEmpty(holderName) == false)
                    conditions.Add(builder.Regex(x => x.Holder.Name, new BsonRegularExpression(holderName, "i")));

                if (string.IsNullOrEmpty(holderCpf) == false)
                    conditions.Add(builder.Where(x => x.Holder.Cpf == holderCpf.OnlyNumbers()));

                var schedule = await _scheduleRepository.FindByAsync(x => x.TypeSubject == typeSubject).ConfigureAwait(false) as List<Schedule>;
                var columns = model.Columns.Where(x => x.Searchable && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToArray();

                var sortColumn = !string.IsNullOrEmpty(model.SortOrder) ? model.SortOrder.UppercaseFirst() : model.Columns.FirstOrDefault(x => x.Orderable)?.Name ?? model.Columns.FirstOrDefault()?.Name;
                var totalRecords = (int)await _familyRepository.GetCollectionAsync().CountDocumentsAsync(builder.And(conditions));

                var sortBy = model.Order[0].Dir.ToString().ToUpper().Equals("DESC")
                    ? Builders<Data.Entities.Family>.Sort.Descending(sortColumn)
                    : Builders<Data.Entities.Family>.Sort.Ascending(sortColumn);

                var retorno = await _familyRepository
                    .LoadDataTableAsync(model.Search.Value, sortBy, model.Start, model.Length, conditions, columns) as List<Family>;

                var filterFamilyWithSchedule = retorno.FindAll(x => schedule.Exists(c => c.FamilyId == x._id.ToString()));
                var totalrecordsFiltered = !string.IsNullOrEmpty(model.Search.Value)
                    ? (int)await _familyRepository.CountSearchDataTableAsync(model.Search.Value, conditions, columns)
                    : totalRecords;

                var _vwFamiliHolder = _mapper.Map<List<FamilyHolderListViewModel>>(filterFamilyWithSchedule);
                var _schedules = await _scheduleRepository.FindIn("FamilyId", retorno.Select(x => ObjectId.Parse(x._id.ToString())).ToList()) as List<Schedule>;

                for (int i = 0; i < _vwFamiliHolder.Count(); i++)
                {
                    if (_schedules.Find(x => x.FamilyId == _vwFamiliHolder[i].Id) != null)
                    {
                        _vwFamiliHolder[i].TypeScheduleStatus = _schedules.Find(x => x.FamilyId == _vwFamiliHolder[i].Id).TypeScheduleStatus;
                        _vwFamiliHolder[i].TypeSubject = _schedules.Find(x => x.FamilyId == _vwFamiliHolder[i].Id).TypeSubject;
                    }
                }

                response.Data = _vwFamiliHolder;
                response.Draw = model.Draw;
                response.RecordsFiltered = _vwFamiliHolder.Count(); //totalrecordsFiltered;
                response.RecordsTotal = _vwFamiliHolder.Count(); //totalRecords;

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
        /// EXPORTAR TIMELINE DAS FAMÍLIAS
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("TimeLine/Export")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ExportTimeLine([FromForm] DtParameters model, [FromForm] string number, [FromForm] string holderName, [FromForm] string holderCpf, [FromForm] TypeSubject typeSubject)
        {
            var response = new DtResult<FamilyHolderExportViewModel>();

            try
            {
                // var listEnumsOfTimeLine = new List<int> { 2, 4, 7, 8 };//verificar se o parâmetro corresponde a um dos types

                // if (!listEnumsOfTimeLine.Exists(x => x == (int)typeSubject))
                //     return BadRequest(Utilities.ReturnErro(DefaultMessages.EnumInvalid));

                var builder = Builders<Data.Entities.Family>.Filter;
                var conditions = new List<FilterDefinition<Data.Entities.Family>>();

                conditions.Add(builder.Where(x => x.Created != null));
                if (!string.IsNullOrEmpty(number))
                    conditions.Add(builder.Where(x => x.Holder.Number == number));
                if (!string.IsNullOrEmpty(holderName))
                    conditions.Add(builder.Where(x => x.Holder.Name.ToUpper().Contains(holderName.ToUpper())));
                if (!string.IsNullOrEmpty(holderCpf))
                    conditions.Add(builder.Where(x => x.Holder.Cpf == holderCpf.OnlyNumbers()));

                var schedule = await _scheduleRepository.FindByAsync(x => x.TypeSubject == typeSubject).ConfigureAwait(false) as List<Schedule>;
                var columns = model.Columns.Where(x => x.Searchable && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToArray();

                var sortColumn = !string.IsNullOrEmpty(model.SortOrder) ? model.SortOrder.UppercaseFirst() : model.Columns.FirstOrDefault(x => x.Orderable)?.Name ?? model.Columns.FirstOrDefault()?.Name;
                var totalRecords = (int)await _familyRepository.GetCollectionAsync().CountDocumentsAsync(builder.And(conditions));

                var sortBy = model.Order[0].Dir.ToString().ToUpper().Equals("DESC")
                    ? Builders<Data.Entities.Family>.Sort.Descending(sortColumn)
                    : Builders<Data.Entities.Family>.Sort.Ascending(sortColumn);

                var retorno = await _familyRepository
                    .LoadDataTableAsync(model.Search.Value, sortBy, model.Start, model.Length, conditions, columns) as List<Family>;

                var filterFamilyWithSchedule = retorno.FindAll(x => schedule.Exists(c => c.FamilyId == x._id.ToString()));

                var _vwFamiliHolder = _mapper.Map<List<FamilyHolderExportViewModel>>(filterFamilyWithSchedule);
                var _schedules = await _scheduleRepository.FindIn("FamilyId", retorno.Select(x => ObjectId.Parse(x._id.ToString())).ToList()) as List<Schedule>;

                for (int i = 0; i < _vwFamiliHolder.Count(); i++)
                {
                    if (_schedules.Find(x => x.FamilyId == _vwFamiliHolder[i].Id) != null)
                    {
                        _vwFamiliHolder[i].TypeSubject = _schedules.Find(x => x.FamilyId == _vwFamiliHolder[i].Id).TypeSubject;
                    }
                }

                var fileName = "Linha do Tempo Familias_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";
                var path = Path.Combine($"{Directory.GetCurrentDirectory()}\\", @"ExportFiles");
                if (Directory.Exists(path) == false)
                    Directory.CreateDirectory(path);

                var fullPathFile = Path.Combine(path, fileName);
                var listViewModel = _mapper.Map<List<FamilyHolderExportViewModel>>(retorno);
                Utilities.ExportToExcel(listViewModel, path, fileName: fileName.Split('.')[0]);
                if (System.IO.File.Exists(fullPathFile) == false)
                    return BadRequest(Utilities.ReturnErro("Ocorreu um erro fazer download do arquivo"));

                var fileBytes = System.IO.File.ReadAllBytes(@fullPathFile);
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(Utilities.ReturnErro(ex.Message));
            }
        }

        /// <summary>
        /// BUSCAR LINHA DO TEMPO DAS FAMILIAS
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         POST
        ///             {
        ///                "searchTerm": "string",  Ex: (nome, cpf ou nº de cadastro)
        ///                "typeSubject": 2         Ex: ReuniaoPGM = 2 - EscolhaDoImovel = 4 - Mudanca = 7 - AcompanhamentoPosMudança = 8
        ///                
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("SearchTimeLine")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(typeof(IEnumerable<ScheduleListViewModel>), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]

        public async Task<IActionResult> SearchTimeLine([FromBody] SearchViewModel model)
        {
            try
            {

                model.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelState();

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);


                var builder = Builders<Data.Entities.Schedule>.Filter;
                var conditions = new List<FilterDefinition<Data.Entities.Schedule>>();

                //var cpfSearch = model.SearchTerm.OnlyNumbers();

                conditions.Add(builder.Where(x => x.Created != null));

                if (string.IsNullOrEmpty(model.SearchTerm) == false)
                    conditions.Add(
                        builder.Or(
                        builder.Regex(x => x.HolderName, new BsonRegularExpression(model.SearchTerm, "i")),
                        builder.Regex(x => x.HolderNumber, new BsonRegularExpression(model.SearchTerm, "i")),
                        builder.Regex(x => x.HolderCpf, new BsonRegularExpression(model.SearchTerm, "i"))
                )
                );

                if (model.TypeSubject != null)
                    conditions.Add(builder.Where(x => x.TypeSubject == model.TypeSubject));

                //var query = _scheduleRepository.PrintQuery(builder.And(conditions));

                var retorno = await _scheduleRepository.GetCollectionAsync().FindSync(builder.And(conditions), new FindOptions<Schedule>()).ToListAsync();

                var _vwFamiliHolder = _mapper.Map<List<ScheduleListViewModel>>(retorno);

                var response = _vwFamiliHolder;

                return Ok(Utilities.ReturnSuccess(data: response));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(responseList: true));
            }
        }

        /// <summary>
        /// BUSCAR LINHA DO TEMPO DAS FAMILIAS
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         POST
        ///             {
        ///                "searchTerm": "string",  Ex: (nome, cpf ou nº de cadastro)
        ///                "typeSubject": 2         Ex: ReuniaoPGM = 2 - EscolhaDoImovel = 4 - Mudanca = 7 - AcompanhamentoPosMudança = 8
        ///                
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("SearchTimeLineExport")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(typeof(IEnumerable<ScheduleListViewModel>), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]

        public async Task<IActionResult> SearchTimeLineExport([FromBody] SearchViewModel model)
        {
            try
            {

                model.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelState();

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);


                var builder = Builders<Data.Entities.Schedule>.Filter;
                var conditions = new List<FilterDefinition<Data.Entities.Schedule>>();

                //var cpfSearch = model.SearchTerm.OnlyNumbers();

                conditions.Add(builder.Where(x => x.Created != null));

                if (string.IsNullOrEmpty(model.SearchTerm) == false)
                    conditions.Add(
                        builder.Or(
                        builder.Regex(x => x.HolderName, new BsonRegularExpression(model.SearchTerm, "i")),
                        builder.Regex(x => x.HolderNumber, new BsonRegularExpression(model.SearchTerm, "i")),
                        builder.Regex(x => x.HolderCpf, new BsonRegularExpression(model.SearchTerm, "i"))
                )
                );

                if (model.TypeSubject != null)
                    conditions.Add(builder.Where(x => x.TypeSubject == model.TypeSubject));

                var retorno = await _scheduleRepository.GetCollectionAsync().FindSync(builder.And(conditions), new FindOptions<Schedule>()).ToListAsync();

                var now = DateTimeOffset.Now.ToUnixTimeSeconds();
                var fileName = $"linhadotempo_{now}.xlsx";

                var path = Path.Combine($"{Directory.GetCurrentDirectory()}\\", @"ExportFiles");
                if (Directory.Exists(path) == false)
                    Directory.CreateDirectory(path);

                var fullPathFile = Path.Combine(path, fileName);
                var listViewModel = _mapper.Map<List<ScheduleExportViewModel>>(retorno);
                Utilities.ExportToExcel(listViewModel, path, fileName: fileName.Split('.')[0]);
                if (System.IO.File.Exists(fullPathFile) == false)
                    return BadRequest(Utilities.ReturnErro("Ocorreu um erro fazer download do arquivo"));

                var fileBytes = System.IO.File.ReadAllBytes(@fullPathFile);
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);


            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(responseList: true));
            }
        }

        /// <summary>
        /// EXPORTAR LINHA DO TEMPO DAS FAMILIAS USO NO APP
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("TimeLineExport")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> TimeLineExport([FromQuery] SearchViewModel model)
        {
            try
            {
                //model.TrimStringProperties();
                //var isInvalidState = ModelState.ValidModelState();

                //if (isInvalidState != null)
                //    return BadRequest(isInvalidState);


                var builder = Builders<Data.Entities.Schedule>.Filter;
                var conditions = new List<FilterDefinition<Data.Entities.Schedule>>();


                conditions.Add(builder.Where(x => x.Created != null));

                if (string.IsNullOrEmpty(model.SearchTerm) == false)
                    conditions.Add(
                        builder.Or(
                        builder.Regex(x => x.HolderName, new BsonRegularExpression(model.SearchTerm, "i")),
                        builder.Regex(x => x.HolderNumber, new BsonRegularExpression(model.SearchTerm, "i")),
                        builder.Regex(x => x.HolderCpf, new BsonRegularExpression(model.SearchTerm, "i"))
                )
                );

                if (model.TypeSubject != null)
                    conditions.Add(builder.Where(x => x.TypeSubject == model.TypeSubject.GetValueOrDefault()));

                var retorno = await _scheduleRepository.GetCollectionAsync().FindSync(builder.And(conditions), new FindOptions<Schedule>()).ToListAsync();

                var now = DateTimeOffset.Now.ToUnixTimeSeconds();
                var fileName = $"linhadotempo_{now}.xlsx";

                var path = Path.Combine($"{Directory.GetCurrentDirectory()}\\", @"ExportFiles");
                if (Directory.Exists(path) == false)
                    Directory.CreateDirectory(path);

                var fullPathFile = Path.Combine(path, fileName);
                var listViewModel = _mapper.Map<List<ScheduleExportViewModel>>(retorno);
                Utilities.ExportToExcel(listViewModel, path, fileName: fileName.Split('.')[0]);
                if (System.IO.File.Exists(fullPathFile) == false)
                    return BadRequest(Utilities.ReturnErro("Ocorreu um erro fazer download do arquivo"));

                var fileBytes = System.IO.File.ReadAllBytes(@fullPathFile);
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }


        /// <summary>
        /// TIME LINE DO PROCESSO OPCIONAL
        /// </summary>
        ///  <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         POST
        ///             {
        ///              "familyId": "", // required
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("TimeLineProcessOptional/{familyId}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> TimeLineProcessOptional([FromRoute] string familyId)
        {
            try
            {
                bool course = await _courseFamilyRepository.CheckByAsync(x => x.FamilyId == familyId).ConfigureAwait(false);
                bool enquete = await _quizFamilyRepository.CheckByAsync(x => x.FamilyId == familyId && x.TypeStatus == TypeStatus.Respondido).ConfigureAwait(false);

                var objReturn = new
                {
                    course,
                    enquete
                };

                return Ok(Utilities.ReturnSuccess(data: objReturn));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// DETALHES DA FAMÍLIA
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         POST
        ///             {
        ///              "id": "", // required
        ///             }
        /// </remarks>
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
                var entity = await _familyRepository.FindByIdAsync(id).ConfigureAwait(false);

                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound));

                return Ok(Utilities.ReturnSuccess(data: _mapper.Map<FamilyCompleteViewModel>(entity)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }
        /// <summary>
        /// IMPORTAÇÃO DA FAMÍLIA
        /// </summary>
        /// <remarks>
        ///  OBJ DE ENVIO
        /// 
        ///         GET
        ///             {
        ///              "file": "", // Arquivo a ser importado
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("Import")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Import([FromForm] IFormFile file) //[FromRoute] string file
        {
            try
            {
                //var folder = $"{_env.ContentRootPath}\\content\\upload\\".Trim();

                //var exists = Directory.Exists(folder);

                //if (!exists)
                //    Directory.CreateDirectory(folder);

                //var fi = new System.IO.FileInfo(folder + "\\" + file);

                if (file == null || file.Length <= 0)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FileNotFound));

                var extension = MimeTypeMap.GetExtension(file.ContentType).ToLower();

                if (_acceptedFiles.Count(x => x == extension) == 0)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FileNotAllowed));




                using (var package = new ExcelPackage(file.OpenReadStream()))
                {
                    ExcelWorksheet sheet = package.Workbook.Worksheets[1];


                    for (int linha = 2; linha < 300; linha++)
                    {
                        if (string.IsNullOrEmpty(package.Workbook.Worksheets[1].Cells[linha, 1].Value.ToString()))
                            break;

                        var vw = new FamilyCompleteViewModel();
                        vw.Holder = new FamilyHolderViewModel();
                        vw.Spouse = new FamilySpouseViewModel();
                        vw.Financial = new FamilyFinancialViewModel();
                        vw.Priorization = new FamilyPriorizationViewModel();
                        vw.Members = new List<FamilyMemberViewModel>();

                        vw.Holder.Number = package.Workbook.Worksheets[1].Cells[linha, 1].Value.ToString();
                        vw.Holder.Name = package.Workbook.Worksheets[1].Cells[linha, 2].Value.ToString();
                        vw.Holder.Cpf = package.Workbook.Worksheets[1].Cells[linha, 3].Value.ToString();
                        vw.Holder.Birthday = Utilities.ToTimeStamp(DateTime.Parse(package.Workbook.Worksheets[1].Cells[linha, 4].Value.ToString()));
                        vw.Holder.Genre = (TypeGenre)Enum.Parse(typeof(TypeGenre), package.Workbook.Worksheets[1].Cells[linha, 5].Value.ToString());
                        vw.Holder.Email = package.Workbook.Worksheets[1].Cells[linha, 6].Value.ToString();
                        vw.Holder.Phone = package.Workbook.Worksheets[1].Cells[linha, 7].Value.ToString();
                        vw.Holder.Scholarity = Utilities.ToEnum<TypeScholarity>(package.Workbook.Worksheets[1].Cells[linha, 8].Value.ToString()); ;

                        //conjuge
                        vw.Spouse.Name = package.Workbook.Worksheets[1].Cells[linha, 9].Value.ToString();
                        vw.Spouse.Birthday = Utilities.ToTimeStamp(DateTime.Parse(package.Workbook.Worksheets[1].Cells[linha, 10].Value.ToString()));
                        vw.Spouse.Genre = (TypeGenre)Enum.Parse(typeof(TypeGenre), package.Workbook.Worksheets[1].Cells[linha, 11].Value.ToString());
                        vw.Spouse.SpouseScholarity = Utilities.ToEnum<TypeScholarity>(package.Workbook.Worksheets[1].Cells[linha, 12].Value.ToString());

                        //membro 1
                        if (package.Workbook.Worksheets[1].Cells[linha, 13].Value != null)
                        {
                            vw.Members.Add(new FamilyMemberViewModel()
                            {
                                Name = package.Workbook.Worksheets[1].Cells[linha, 13].Value.ToString(),
                                Birthday = Utilities.ToTimeStamp(DateTime.Parse(package.Workbook.Worksheets[1].Cells[linha, 14].Value.ToString())),
                                Genre = (TypeGenre)Enum.Parse(typeof(TypeGenre), package.Workbook.Worksheets[1].Cells[linha, 15].Value.ToString()),
                                KinShip = Utilities.ToEnum<TypeKingShip>(package.Workbook.Worksheets[1].Cells[linha, 16].Value.ToString()),
                                Scholarity = Utilities.ToEnum<TypeScholarity>(package.Workbook.Worksheets[1].Cells[linha, 17].Value.ToString())
                            });
                        }

                        //membro 2
                        if (package.Workbook.Worksheets[1].Cells[linha, 18].Value != null)
                        {
                            vw.Members.Add(new FamilyMemberViewModel()
                            {
                                Name = package.Workbook.Worksheets[1].Cells[linha, 18].Value.ToString(),
                                Birthday = Utilities.ToTimeStamp(DateTime.Parse(package.Workbook.Worksheets[1].Cells[linha, 19].Value.ToString())),
                                Genre = (TypeGenre)Enum.Parse(typeof(TypeGenre), package.Workbook.Worksheets[1].Cells[linha, 20].Value.ToString()),
                                KinShip = Utilities.ToEnum<TypeKingShip>(package.Workbook.Worksheets[1].Cells[linha, 21].Value.ToString()),
                                Scholarity = Utilities.ToEnum<TypeScholarity>(package.Workbook.Worksheets[1].Cells[linha, 22].Value.ToString())
                            });
                        }

                        //membro 3
                        if (package.Workbook.Worksheets[1].Cells[linha, 23].Value != null)
                        {
                            vw.Members.Add(new FamilyMemberViewModel()
                            {
                                Name = package.Workbook.Worksheets[1].Cells[linha, 23].Value.ToString(),
                                Birthday = Utilities.ToTimeStamp(DateTime.Parse(package.Workbook.Worksheets[1].Cells[linha, 24].Value.ToString())),
                                Genre = (TypeGenre)Enum.Parse(typeof(TypeGenre), package.Workbook.Worksheets[1].Cells[linha, 25].Value.ToString()),
                                KinShip = Utilities.ToEnum<TypeKingShip>(package.Workbook.Worksheets[1].Cells[linha, 26].Value.ToString()),
                                Scholarity = Utilities.ToEnum<TypeScholarity>(package.Workbook.Worksheets[1].Cells[linha, 27].Value.ToString())
                            });
                        }


                        //membro 4
                        if (package.Workbook.Worksheets[1].Cells[linha, 28].Value != null)
                        {
                            vw.Members.Add(new FamilyMemberViewModel()
                            {
                                Name = package.Workbook.Worksheets[1].Cells[linha, 28].Value.ToString(),
                                Birthday = Utilities.ToTimeStamp(DateTime.Parse(package.Workbook.Worksheets[1].Cells[linha, 29].Value.ToString())),
                                Genre = (TypeGenre)Enum.Parse(typeof(TypeGenre), package.Workbook.Worksheets[1].Cells[linha, 30].Value.ToString()),
                                KinShip = Utilities.ToEnum<TypeKingShip>(package.Workbook.Worksheets[1].Cells[linha, 31].Value.ToString()),
                                Scholarity = Utilities.ToEnum<TypeScholarity>(package.Workbook.Worksheets[1].Cells[linha, 32].Value.ToString())
                            });
                        }

                        //membro 5
                        if (package.Workbook.Worksheets[1].Cells[linha, 33].Value != null)
                        {
                            vw.Members.Add(new FamilyMemberViewModel()
                            {
                                Name = package.Workbook.Worksheets[1].Cells[linha, 33].Value.ToString(),
                                Birthday = Utilities.ToTimeStamp(DateTime.Parse(package.Workbook.Worksheets[1].Cells[linha, 34].Value.ToString())),
                                Genre = (TypeGenre)Enum.Parse(typeof(TypeGenre), package.Workbook.Worksheets[1].Cells[linha, 35].Value.ToString()),
                                KinShip = Utilities.ToEnum<TypeKingShip>(package.Workbook.Worksheets[1].Cells[linha, 36].Value.ToString()),
                                Scholarity = Utilities.ToEnum<TypeScholarity>(package.Workbook.Worksheets[1].Cells[linha, 37].Value.ToString())
                            });
                        }

                        //Financeiro
                        vw.Financial.FamilyIncome = decimal.Parse(package.Workbook.Worksheets[1].Cells[linha, 38].Value.ToString());
                        vw.Financial.PropertyValueForDemolished = decimal.Parse(package.Workbook.Worksheets[1].Cells[linha, 39].Value.ToString());
                        vw.Financial.MaximumPurchase = decimal.Parse(package.Workbook.Worksheets[1].Cells[linha, 40].Value.ToString());
                        vw.Financial.IncrementValue = decimal.Parse(package.Workbook.Worksheets[1].Cells[linha, 41].Value.ToString());

                        //Priorização
                        vw.Priorization.WorkFront = new PriorityRateViewModel()
                        {
                            Rate = 1,
                            Value = package.Workbook.Worksheets[1].Cells[linha, 42].Value.ToString() == "Sim" ? true : false
                        };
                        vw.Priorization.PermanentDisabled = new PriorityRateViewModel()
                        {
                            Rate = 2,
                            Value = package.Workbook.Worksheets[1].Cells[linha, 43].Value.ToString() == "Sim" ? true : false
                        };
                        vw.Priorization.ElderlyOverEighty = new PriorityRateViewModel()
                        {
                            Rate = 3,
                            Value = package.Workbook.Worksheets[1].Cells[linha, 44].Value.ToString() == "Sim" ? true : false
                        };
                        vw.Priorization.WomanServedByProtectiveMeasure = new PriorityRateViewModel()
                        {
                            Rate = 4,
                            Value = package.Workbook.Worksheets[1].Cells[linha, 46].Value.ToString() == "Sim" ? true : false
                        };
                        vw.Priorization.FemaleBreadWinner = new PriorityRateViewModel()
                        {
                            Rate = 5,
                            Value = package.Workbook.Worksheets[1].Cells[linha, 48].Value.ToString() == "Sim" ? true : false
                        };
                        vw.Priorization.SingleParent = new PriorityRateViewModel()
                        {
                            Rate = 6,
                            Value = package.Workbook.Worksheets[1].Cells[linha, 47].Value.ToString() == "Sim" ? true : false
                        };

                        vw.Priorization.FamilyWithMoreThanFivePeople = new PriorityRateViewModel()
                        {
                            Rate = 7,
                            Value = package.Workbook.Worksheets[1].Cells[linha, 49].Value.ToString() == "Sim" ? true : false
                        };

                        vw.Priorization.ChildUnderEighteen = new PriorityRateViewModel()
                        {
                            Rate = 8,
                            Value = package.Workbook.Worksheets[1].Cells[linha, 50].Value.ToString() == "Sim" ? true : false
                        };
                        vw.Priorization.HeadOfHouseholdWithoutIncome = new PriorityRateViewModel()
                        {
                            Rate = 9,
                            Value = package.Workbook.Worksheets[1].Cells[linha, 51].Value.ToString() == "Sim" ? true : false
                        };

                        vw.Priorization.BenefitOfContinuedProvision = new PriorityRateViewModel()
                        {
                            Rate = 10,
                            Value = package.Workbook.Worksheets[1].Cells[linha, 52].Value.ToString() == "Sim" ? true : false
                        };

                        vw.Priorization.FamilyPurse = new PriorityRateViewModel()
                        {
                            Rate = 11,
                            Value = package.Workbook.Worksheets[1].Cells[linha, 53].Value.ToString() == "Sim" ? true : false
                        };
                        vw.Priorization.InvoluntaryCohabitation = new PriorityRateViewModel()
                        {
                            Rate = 12,
                            Value = package.Workbook.Worksheets[1].Cells[linha, 54].Value.ToString() == "Sim" ? true : false
                        };
                        vw.Priorization.FamilyIncomeOfUpTwoMinimumWages = new PriorityRateViewModel()
                        {
                            Rate = 13,
                            Value = package.Workbook.Worksheets[1].Cells[linha, 55].Value.ToString() == "Sim" ? true : false
                        };
                        vw.Priorization.YearsInSextyAndSeventyNine = new PriorityRateViewModel()
                        {
                            Rate = 14,
                            Value = package.Workbook.Worksheets[1].Cells[linha, 45].Value.ToString() == "Sim" ? true : false
                        };

                        var family = _mapper.Map<Data.Entities.Family>(vw);
                        if (vw.Holder.Birthday.HasValue)
                        {
                            var dateBir = Utilities.TimeStampToDateTime(vw.Holder.Birthday.Value);
                            var dateUnix = Utilities.ToTimeStamp(dateBir.Date);
                            family.Holder.Birthday = dateUnix;
                        }
                        family.Holder.Cpf = vw.Holder.Cpf.OnlyNumbers();

                        var entityId = await _familyRepository.CreateAsync(family).ConfigureAwait(false);
                    }
                }
                //package.Save();
                await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.ImportFamily, TypeResposible.UserAdminstratorGestor, $"Importou as famílias {Request.GetUserName()}", Request.GetUserId(), Request.GetUserName()?.Value, "");

                return Ok(Utilities.ReturnSuccess());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }



        /// <summary>
        /// BUSCA OS DADOS DO USUÁRIO LOGADO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("GetUser")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                var userId = Request.GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredencials));

                var entity = await _familyRepository.FindByIdAsync(userId).ConfigureAwait(false);

                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound));

                return Ok(Utilities.ReturnSuccess(data: _mapper.Map<FamilyCompleteViewModel>(entity)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }
        /// <summary>
        /// DISTÂNCIA DA RESIDÊNCIA ATÉ O IMÓVEL ESCOLHIDO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("DisplacementMap/{familyId}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DisplacementMap([FromRoute] string familyId)
        {
            try
            {

                var family = await _familyRepository.FindByIdAsync(familyId).ConfigureAwait(false);
                if (family == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));

                var infoOrigin = await _utilService.GetInfoFromZipCode(family.Address.CEP).ConfigureAwait(false);
                if (infoOrigin == null)
                    return BadRequest(Utilities.ReturnErro("Cep não encontrado"));

                var residencialOrigin = Utilities.GetInfoFromAdressLocation(infoOrigin.StreetAddress + " " + infoOrigin.Complement + " " + infoOrigin.Neighborhood + " " + infoOrigin.CityName + " " + infoOrigin.StateUf);
                if (residencialOrigin.Erro == true)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.LocationNotFound));



                var propertyInterest = await _propertiesInterestRepository.FindByAsync(x => x.FamilyId == familyId).ConfigureAwait(false);
                var residencialDestination = await _residencialPropertyRepository.FindIn(c => c.TypeStatusResidencialProperty == TypeStatusResidencial.Vendido, "_id", propertyInterest.Select(x => ObjectId.Parse(x.ResidencialPropertyId.ToString())).ToList(), Builders<ResidencialProperty>.Sort.Ascending(nameof(ResidencialProperty.LastUpdate))) as List<ResidencialProperty>;
                if (residencialDestination.FirstOrDefault() == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.PropertySaledNotFound));
                if (residencialDestination.Count() > 1)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ResidencialSaled));

                var infoDestination = await _utilService.GetInfoFromZipCode(residencialDestination.FirstOrDefault().ResidencialPropertyAdress.CEP).ConfigureAwait(false);
                if (infoDestination == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.PropertySaledNotFound));

                var destination = Utilities.GetInfoFromAdressLocation(infoDestination.StreetAddress + " " + infoDestination.Complement + " " + infoDestination.Neighborhood + " " + infoDestination.CityName + " " + infoDestination.StateUf);
                var distanceM = Utilities.GetDistance(residencialOrigin.Geometry.Location.Lat, residencialOrigin.Geometry.Location.Lng, destination.Geometry.Location.Lat, destination.Geometry.Location.Lng, 'M');
                var distanceK = Utilities.GetDistance(residencialOrigin.Geometry.Location.Lat, residencialOrigin.Geometry.Location.Lng, destination.Geometry.Location.Lat, destination.Geometry.Location.Lng, 'K');

                var familyVw = _mapper.Map<FamilyHolderDistanceViewModel>(family);
                familyVw.AddressPropertyDistanceMeters = distanceM;
                familyVw.AddressPropertyDistanceKilometers = distanceK;
                familyVw.AddressPropertyOrigin = residencialOrigin.FormatedAddress;
                familyVw.AddressPropertyDestination = destination.FormatedAddress;
                return Ok(Utilities.ReturnSuccess(data: familyVw));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// DISTÂNCIA DA RESIDÊNCIA ATÉ O IMÓVEL ESCOLHIDO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("DisplacementMapAllFamilies")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DisplacementMapAllFamilies()
        {
            try
            {

                var family = await _familyRepository.FindAllAsync().ConfigureAwait(false);
                if (family.Count() == 0)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));

                var listDisplacement = new List<FamilyHolderDistanceViewModel>();

                foreach (var item in family)
                {
                    var propertyInterest = await _propertiesInterestRepository.FindByAsync(x => x.FamilyId == item._id.ToString()).ConfigureAwait(false);
                    var residencialDestination = await _residencialPropertyRepository.FindIn(c => c.TypeStatusResidencialProperty == TypeStatusResidencial.Vendido, "_id", propertyInterest.Select(x => ObjectId.Parse(x.ResidencialPropertyId.ToString())).ToList(), Builders<ResidencialProperty>.Sort.Ascending(nameof(ResidencialProperty.LastUpdate))) as List<ResidencialProperty>;
                    if (residencialDestination.Count() == 1)
                    {
                        var infoOrigin = await _utilService.GetInfoFromZipCode(item.Address.CEP).ConfigureAwait(false);
                        if (infoOrigin == null)
                            return BadRequest(Utilities.ReturnErro("Cep não encontrado"));

                        var residencialOrigin = Utilities.GetInfoFromAdressLocation(infoOrigin.StreetAddress + " " + infoOrigin.Complement + " " + infoOrigin.Neighborhood + " " + infoOrigin.CityName + " " + infoOrigin.StateUf);
                        if (residencialOrigin.Erro == true)
                            return BadRequest(Utilities.ReturnErro(DefaultMessages.LocationNotFound));


                        if (residencialDestination.FirstOrDefault() == null)
                            return BadRequest(Utilities.ReturnErro(DefaultMessages.PropertySaledNotFound));
                        if (residencialDestination.Count() > 1)
                            return BadRequest(Utilities.ReturnErro(DefaultMessages.ResidencialSaled));

                        var infoDestination = await _utilService.GetInfoFromZipCode(residencialDestination.FirstOrDefault().ResidencialPropertyAdress.CEP).ConfigureAwait(false);
                        if (infoDestination == null)
                            return BadRequest(Utilities.ReturnErro(DefaultMessages.PropertySaledNotFound));

                        var destination = Utilities.GetInfoFromAdressLocation(infoDestination.StreetAddress + " " + infoDestination.Complement + " " + infoDestination.Neighborhood + " " + infoDestination.CityName + " " + infoDestination.StateUf);
                        var distanceM = Utilities.GetDistance(residencialOrigin.Geometry.Location.Lat, residencialOrigin.Geometry.Location.Lng, destination.Geometry.Location.Lat, destination.Geometry.Location.Lng, 'M');
                        var distanceK = Utilities.GetDistance(residencialOrigin.Geometry.Location.Lat, residencialOrigin.Geometry.Location.Lng, destination.Geometry.Location.Lat, destination.Geometry.Location.Lng, 'K');

                        var familyVw = _mapper.Map<FamilyHolderDistanceViewModel>(item);
                        familyVw.AddressPropertyDistanceMeters = distanceM;
                        familyVw.AddressPropertyDistanceKilometers = distanceK;
                        familyVw.AddressPropertyOrigin = residencialOrigin.FormatedAddress;
                        familyVw.AddressPropertyDestination = destination.FormatedAddress;
                        listDisplacement.Add(familyVw);
                    }

                }

                return Ok(Utilities.ReturnSuccess(data: listDisplacement));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }
        /// <summary>
        /// LISTA FAMÍLIAS POR FILTRO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("GetFamilyByFilter")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetFamilyByFilter(string number, string name, string cpf)
        {
            try
            {
                var builder = Builders<Data.Entities.Family>.Filter;
                var conditions = new List<FilterDefinition<Data.Entities.Family>>();

                conditions.Add(builder.Where(x => x.Created != null));
                if (!string.IsNullOrEmpty(number))
                    conditions.Add(builder.Where(x => x.Holder.Number.ToUpper() == number.ToUpper()));
                if (!string.IsNullOrEmpty(name))
                    conditions.Add(builder.Where(x => x.Holder.Name.ToUpper().Contains(name.ToUpper())));
                if (!string.IsNullOrEmpty(cpf))
                    conditions.Add(builder.Where(x => x.Holder.Cpf == cpf));


                var condition = builder.And(conditions);
                var entity = await _familyRepository.GetCollectionAsync().FindSync(condition, new FindOptions<Data.Entities.Family>() { }).ToListAsync();
                if (entity.Count() == 0)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));

                return Ok(Utilities.ReturnSuccess(data: _mapper.Map<List<FamilyHolderListViewModel>>(entity)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// LISTA TODAS FAMÍLIAS
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
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var entity = await _familyRepository.FindAllAsync().ConfigureAwait(false);

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
        /// ESQUECI A SENHA
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         POST
        ///             {
        ///                "motherName" :"",
        ///                "motherCityBorned" :"",
        ///                "cpf": ""
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("ForgotPassword")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ForgotPassword([FromBody] FamilyForgotPasswordViewModel model)
        {
            try
            {
                model?.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelStateOnlyFields(nameof(model.Cpf), nameof(model.MotherCityBorned), nameof(model.MotherName));
                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                var builder = Builders<Data.Entities.Family>.Filter;
                var conditions = new List<FilterDefinition<Data.Entities.Family>>();

                conditions.Add(builder.Where(x => x.Created != null));
                if (!string.IsNullOrEmpty(model.Cpf))
                    conditions.Add(builder.Where(x => x.Holder.Cpf.Contains(model.Cpf.OnlyNumbers())));
                if (!string.IsNullOrEmpty(model.MotherCityBorned))
                    conditions.Add(builder.Where(x => x.MotherCityBorned.ToUpper().Contains(model.MotherCityBorned.RemoveAccents().ToUpper())));
                if (!string.IsNullOrEmpty(model.MotherName))
                    conditions.Add(builder.Where(x => x.MotherName.ToUpper().Contains(model.MotherName.RemoveAccents().ToUpper())));

                var condition = builder.And(conditions);

                var profile = await _familyRepository.GetCollectionAsync().FindSync(condition, new FindOptions<Data.Entities.Family>() { }).ToListAsync();
                if (profile.Count() == 0 || profile.Count() > 1)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));



                var dataBody = Util.GetTemplateVariables();

                var newPassword = Utilities.RandomInt(8);

                dataBody.Add("{{ title }}", "Lembrete de senha");
                dataBody.Add("{{ message }}", $"<p>Caro(a) {profile.FirstOrDefault().Holder.Name.GetFirstName()}</p>" +
                                            $"<p>Segue sua senha de acesso ao {Startup.ApplicationName}</p>" +
                                            $"<p><b>CPF</b> : {profile.FirstOrDefault().Holder.Cpf}</p>" +
                                            $"<p><b>Senha</b> :{newPassword}</p>");

                var body = _senderMailService.GerateBody("custom", dataBody);

                var unused = Task.Run(async () =>
                {
                    await _senderMailService.SendMessageEmailAsync(Startup.ApplicationName, profile.FirstOrDefault().Holder.Email, body, "Lembrete de senha").ConfigureAwait(false);

                    profile.FirstOrDefault().Password = newPassword;
                    await _familyRepository.UpdateAsync(profile.FirstOrDefault());

                });
                await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Change, TypeResposible.UserAdminstratorGestor, $"Atualizou a senha {model.Cpf}", "", "", "");
                return Ok(Utilities.ReturnSuccess("Verifique seu e-mail"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Change, TypeResposible.UserAdminstratorGestor, $"Não foi possível atualizar a senha", "", "", "", "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }
        /// <summary>
        /// REGISTRAR UMA FAMÍLIA
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         POST
        ///          "address": {
        ///            "streetAddress": "string",
        ///            "number": "string",
        ///            "cityName": "string",
        ///            "cityId": "string",
        ///            "stateName": "string",
        ///            "stateUf": "string",
        ///            "neighborhood": "string",
        ///            "complement": "string",
        ///            "location": "string"
        ///          },
        ///         "holder": {
        ///          "number": "",
        ///          "name": "name",
        ///          "cpf": "cpf",
        ///          "birthday": 1611776217,
        ///          "genre":  Enum,
        ///          "email": "email",
        ///          "phone": "phone",
        ///          "scholarity": Enum
        ///        },
        ///        "spouse": {
        ///          "name": "name",
        ///          "birthday": 1611776217,
        ///          "genre": Enum,
        ///          "scholarity": Enum
        ///        },
        ///        "members": [
        ///          {
        ///            "name": "name",
        ///            "birthday":  1611776217,
        ///            "genre":  Enum,
        ///            "kinShip": 1,
        ///            "scholarity": Enum
        ///          }, {
        ///            "name": "name2",
        ///            "birthday":  1611776217,
        ///            "genre":  Enum,
        ///            "kinShip": Enum,
        ///            "scholarity": Enum
        ///          }
        ///        ],
        ///        "financial": {
        ///          "familyIncome": decimal,
        ///          "propertyValueForDemolished": decimal,
        ///          "maximumPurchase": decimal,
        ///          "incrementValue": decimal
        ///        },
        ///        "priorization": {
        ///          "workFront": bool,
        ///          "permanentDisabled": bool,
        ///          "elderlyOverEighty": bool,
        ///          "yearsInSextyAndSeventyNine": bool,
        ///          "womanServedByProtectiveMeasure": bool,
        ///          "femaleBreadWinner": bool,
        ///          "singleParent": bool,
        ///          "familyWithMoreThanFivePeople": bool,
        ///          "childUnderEighteen": bool,
        ///          "headOfHouseholdWithoutIncome": bool,
        ///          "benefitOfContinuedProvision": bool,
        ///          "familyPurse": bool,
        ///          "involuntaryCohabitation": bool,
        ///          "familyIncomeOfUpTwoMinimumWages": bool
        ///        },
        ///        "motherName": "string",
        ///        "motherCityBorned": "string",
        ///        "password": "",
        ///        "isFirstAcess": bool,
        ///        "providerId": "string"
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
        public async Task<IActionResult> Register([FromBody] FamilyCompleteViewModel model)
        {
            //var claim = Util.SetRole(TypeProfile.Profile);
            //var typeAction = string.IsNullOrEmpty(model.Id) ? TypeAction.Register : TypeAction.Change;
            try
            {
                if (model.Holder.Email.ValidEmail() == false)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.EmailInvalid));

                var family = await _familyRepository.FindOneByAsync(x => x.Holder.Cpf == model.Holder.Cpf.OnlyNumbers()).ConfigureAwait(false);
                if (family == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));
                var familiId = family._id;


                family = _mapper.Map<Data.Entities.Family>(model);
                family._id = familiId;
                if (model.Holder.Birthday.HasValue)
                {
                    var dateBir = Utilities.TimeStampToDateTime(model.Holder.Birthday.Value);
                    var dateUnix = Utilities.ToTimeStamp(dateBir.Date);
                    family.Holder.Birthday = dateUnix;
                }
                family.Holder.Cpf = model.Holder.Cpf.OnlyNumbers();


                var entityId = await _familyRepository.UpdateOneAsync(family).ConfigureAwait(false);
                await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Registrou uma família {Request.GetUserName()}", Request.GetUserId(), Request.GetUserName()?.Value, "");
                return Ok(Utilities.ReturnSuccess(data: "Registrado com sucesso!"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível cadastrar nova Família", "", "", "", "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }


        /// <summary>
        /// REGISTRAR UMA FAMÍLIA
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         POST
        ///         "holder": {
        ///          "number": "",
        ///          "name": "name",
        ///          "cpf": "cpf",
        ///          "birthday": 1611776217,
        ///          "genre":  Enum,
        ///          "email": "email",
        ///          "phone": "phone",
        ///          "scholarity": Enum
        ///        },
        ///        "spouse": {
        ///          "name": "name",
        ///          "birthday": 1611776217,
        ///          "genre": Enum,
        ///          "scholarity": Enum
        ///        },
        ///        "members": [
        ///          {
        ///            "name": "name",
        ///            "birthday":  1611776217,
        ///            "genre":  Enum,
        ///            "kinShip": 1,
        ///            "scholarity": Enum
        ///          }, {
        ///            "name": "name2",
        ///            "birthday":  1611776217,
        ///            "genre":  Enum,
        ///            "kinShip": Enum,
        ///            "scholarity": Enum
        ///          }
        ///        ],
        ///        "financial": {
        ///          "familyIncome": decimal,
        ///          "propertyValueForDemolished": decimal,
        ///          "maximumPurchase": decimal,
        ///          "incrementValue": decimal
        ///        },
        ///        "priorization": {
        ///          "workFront": bool,
        ///          "permanentDisabled": bool,
        ///          "elderlyOverEighty": bool,
        ///          "yearsInSextyAndSeventyNine": bool,
        ///          "womanServedByProtectiveMeasure": bool,
        ///          "femaleBreadWinner": bool,
        ///          "singleParent": bool,
        ///          "familyWithMoreThanFivePeople": bool,
        ///          "childUnderEighteen": bool,
        ///          "headOfHouseholdWithoutIncome": bool,
        ///          "benefitOfContinuedProvision": bool,
        ///          "familyPurse": bool,
        ///          "involuntaryCohabitation": bool,
        ///          "familyIncomeOfUpTwoMinimumWages": bool
        ///        }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("RegisterWeb")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterWeb([FromBody] FamilyCompleteWebViewModel model)
        {
            //var claim = Util.SetRole(TypeProfile.Profile);
            //var typeAction = string.IsNullOrEmpty(model.Id) ? TypeAction.Register : TypeAction.Change;
            try
            {
                if (model.Holder.Email.ValidEmail() == false)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.EmailInvalid));

                if (!model.Holder.Cpf.OnlyNumbers().ValidCpf())
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.CpfInvalid));

                if (await _familyRepository.CheckByAsync(x => x.Holder.Cpf == model.Holder.Cpf.OnlyNumbers()).ConfigureAwait(false))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.CpfInUse));

                var family = _mapper.Map<Data.Entities.Family>(model);
                if (model.Holder.Birthday.HasValue)
                {
                    var dateBir = Utilities.TimeStampToDateTime(model.Holder.Birthday.Value);
                    var dateUnix = Utilities.ToTimeStamp(dateBir.Date);
                    family.Holder.Birthday = dateUnix;
                }
                family.Holder.Cpf = model.Holder.Cpf.OnlyNumbers();

                //var newPassword = Utilities.RandomString(8);
                //family.Password = newPassword;
                var entityId = await _familyRepository.CreateAsync(family).ConfigureAwait(false);
                //////await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Cadastro de nova família {entity.Holder.Name}", Request.GetUserId(), Request.GetUserName()?.Value, entityId, "");

                //var dataBody = Util.GetTemplateVariables();
                //dataBody.Add("{{ title }}", "Lembrete de senha");
                //dataBody.Add("{{ message }}", $"<p>Caro(a) {model.Holder.Name.GetFirstName()}</p>" +
                //                            $"<p>Segue sua senha de acesso ao {Startup.ApplicationName}</p>" +
                //                            //$"<p><b>Login</b> : {profile.Login}</p>" +
                //                            $"<p><b>Senha</b> :{newPassword}</p>"
                //                            );

                //var body = _senderMailService.GerateBody("custom", dataBody);

                //var unused = Task.Run(async () =>
                //{
                //    await _senderMailService.SendMessageEmailAsync(Startup.ApplicationName, model.Holder.Email, body, "Lembrete de senha").ConfigureAwait(false);
                //});

                await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Criadoa família {model.Holder.Name}", Request.GetUserId(), Request.GetUserName()?.Value, entityId);
                return Ok(Utilities.ReturnSuccess(data: "Registrado com sucesso!"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível cadastrar nova Família", "", "", "", "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// VERIFICA SE A FAMÍLIA JÁ ESTÁ CADASTRADA ATRAVÉS DO CPF
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         POST
        ///         {
        ///          ExistCpf?cpf=25645377079
        ///          }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("ExistCpf")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<IActionResult> ExistCpf([FromQuery] string cpf)
        {
            try
            {
                if (cpf.OnlyNumbers().ValidCpf() == false)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.CpfInvalid));

                var family = await _familyRepository.FindOneByAsync(x => x.Holder.Cpf == cpf.OnlyNumbers()).ConfigureAwait(false);
                if (family == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.CpfNotFound));
                family.Password = "";
                return Ok(Utilities.ReturnSuccess(data: family));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// EDITA FAMÍLIA
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         POST
        ///         {
        ///       "holder": {
        ///       "name": "string",
        ///       "genre": "Feminino",
        ///       "email": "string",
        ///       "phone": "string",
        ///       "scholarity": "Não possui"
        ///       },       
        ///       "spouse": {
        ///       "name": "string",
        ///       "birthday": 0,
        ///       "genre": "Feminino",
        ///       "spouseScholarity": "Não possui"
        ///       },
        ///       "members": [
        ///       {
        ///       "name": "string",
        ///       "birthday": 0,
        ///       "genre": "Feminino",
        ///       "kinShip": "Filha",
        ///       "scholarity": "Não possui"
        ///       }
        ///       ],
        ///       "financial": {
        ///       "familyIncome": 0,
        ///       "propertyValueForDemolished": 0,
        ///       "maximumPurchase": 0,
        ///       "incrementValue": 0
        ///       },
        ///       "priorization": {
        ///       "workFront": {
        ///       "rate": 0,
        ///       "value": true
        ///       },
        ///       "permanentDisabled": {
        ///       "rate": 0,
        ///       "value": true
        ///       },
        ///       "elderlyOverEighty": {
        ///       "rate": 0,
        ///       "value": true
        ///       },
        ///       "yearsInSextyAndSeventyNine": {
        ///       "rate": 0,
        ///       "value": true
        ///       },
        ///       "womanServedByProtectiveMeasure": {
        ///       "rate": 0,
        ///       "value": true
        ///       },
        ///       "femaleBreadWinner": {
        ///       "rate": 0,
        ///       "value": true
        ///       },
        ///       "singleParent": {
        ///       "rate": 0,
        ///       "value": true
        ///       },
        ///       "familyWithMoreThanFivePeople": {
        ///       "rate": 0,
        ///       "value": true
        ///       },
        ///       "childUnderEighteen": {
        ///       "rate": 0,
        ///       "value": true
        ///       },
        ///       "headOfHouseholdWithoutIncome": {
        ///       "rate": 0,
        ///       "value": true
        ///       },
        ///       "benefitOfContinuedProvision": {
        ///       "rate": 0,
        ///       "value": true
        ///       },
        ///       "familyPurse": {
        ///       "rate": 0,
        ///       "value": true
        ///       },
        ///       "involuntaryCohabitation": {
        ///       "rate": 0,
        ///       "value": true
        ///       },
        ///       "familyIncomeOfUpTwoMinimumWages": {
        ///       "rate": 0,
        ///       "value": true
        ///       }
        ///       },
        ///       "id": "string",
        ///       "isFirstAcess": false
        ///       }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("Edit")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Edit([FromBody] FamilyEditViewModel model)
        {
            try
            {
                var validOnly = _httpContextAccessor.GetFieldsFromBodyCustom();

                model.TrimStringProperties();

                Family entityFamily = null;

                var isInvalidState = ModelState.ValidModelStateOnlyFields(validOnly);

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                entityFamily = await _familyRepository.FindByIdAsync(model.Id).ConfigureAwait(false);

                if (entityFamily == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));

                entityFamily.SetIfDifferent(model, validOnly);

                entityFamily.IsFirstAcess = model.IsFirstAcess;


                if (validOnly.Count(x => x == nameof(Family.Holder)) > 0)
                {
                    entityFamily.Holder.SetIfDifferentCustom(model.Holder);
                }

                if (validOnly.Count(x => x == nameof(Family.Address)) > 0)
                {
                    entityFamily.Address.SetIfDifferentCustom(model.Address);
                }

                if (validOnly.Count(x => x == nameof(Family.Members)) > 0)
                {
                    entityFamily.Members = _mapper.Map<List<FamilyMember>>(model.Members);
                }

                if (validOnly.Count(x => x == nameof(Family.Spouse)) > 0)
                {
                    entityFamily.Spouse.SetIfDifferentCustom(model.Spouse);
                }

                if (validOnly.Count(x => x == nameof(Family.Financial)) > 0)
                {
                    entityFamily.Financial.SetIfDifferentCustom(model.Financial);
                }

                if (validOnly.Count(x => x == nameof(Family.Priorization)) > 0)
                {
                    entityFamily.Priorization = _mapper.Map<FamilyPriorization>(model.Priorization);
                }


                //if (model.Financial.FamilyIncome > 0)
                //    entityFamily.Financial.FamilyIncome = model.Financial.FamilyIncome;                    

                //if (model.Financial.PropertyValueForDemolished > 0)
                //    entityFamily.Financial.PropertyValueForDemolished = model.Financial.PropertyValueForDemolished;

                //if (model.Financial.MaximumPurchase > 0)
                //    entityFamily.Financial.MaximumPurchase = model.Financial.MaximumPurchase;

                //if (model.Financial.IncrementValue > 0)
                //    entityFamily.Financial.IncrementValue = model.Financial.IncrementValue;

                //}

                await _familyRepository.UpdateAsync(entityFamily).ConfigureAwait(false);

                await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Change, TypeResposible.UserAdminstratorGestor, $"Update de nova família {entityFamily.Holder.Name}", "", "", model.Id);//Request.GetUserName()?.Value, Request.GetUserId()

                return Ok(Utilities.ReturnSuccess(data: "Atualizado com sucesso!"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível cadastrar nova Família", "", "", "", "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// DELETAR MEMBRO DA FAMÍLIA
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         POST
        ///             {
        ///             "familyId": "6011d02a4c7c9e71c25df866", Id da família
        ///             "name": "" // nome
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("DeleteMember")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteMember([FromBody] FamilyDeleteMember model)
        {
            try
            {
                var entityFamily = await _familyRepository.FindByIdAsync(model.FamilyId).ConfigureAwait(false);
                if (entityFamily == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));
                if (entityFamily.Members.FindIndex(x => x.Name == model.Name) < 0)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.MemberNotFound));

                entityFamily.Members.RemoveAt(entityFamily.Members.FindIndex(x => x.Name == model.Name));
                await _familyRepository.UpdateOneAsync(entityFamily).ConfigureAwait(false);

                await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Delete, TypeResposible.UserAdminstratorGestor, $"Remover membro de nova família {model.Name}", Request.GetUserId(), Request.GetUserName()?.Value, entityFamily._id.ToString());

                return Ok(Utilities.ReturnSuccess(data: "Atualizado com sucesso!"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Delete, TypeResposible.UserAdminstratorGestor, $"Não foi possível remover membro de nova família", Request.GetUserId(), Request.GetUserName()?.Value, model.FamilyId, "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }


        /// <summary>
        /// REGISTRA MEMBRO DA FAMÍLIA
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         POST
        ///             {
        ///           "familyId": "6011d02a4c7c9e71c25df866",
        ///            "members": [
        ///              {
        ///                      "Name" : "name",
        ///                      "Birthday" : 1611776217,
        ///                      "Genre" : 0,
        ///                      "Scholarity" : 0,
        ///                      "KinShip" : 0
        ///              }
        ///            ]
        ///         }  
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("RegisterMember")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> RegisterMember([FromBody] FamilyRegisterMember model)
        {
            //var claim = Util.SetRole(TypeProfile.Profile);
            //var typeAction = string.IsNullOrEmpty(model.Id) ? TypeAction.Register : TypeAction.Change;
            try
            {
                if (model.Members.Count() == 0)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.MemberNotFound));


                var entityFamily = await _familyRepository.FindByIdAsync(model.FamilyId).ConfigureAwait(false);
                if (entityFamily == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));

                var existMemberWithSameName = entityFamily.Members.Find(x => model.Members.Exists(c => c.Name == x.Name));
                if (existMemberWithSameName != null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.MemberInUse));

                var namesMembers = string.Join(";", model.Members.Select(x => x.Name).ToArray());

                entityFamily.Members.AddRange(_mapper.Map<List<FamilyMember>>(model.Members));
                await _familyRepository.UpdateOneAsync(entityFamily).ConfigureAwait(false);

                await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Inclusão de membro(s) de nova família {namesMembers}", Request.GetUserId(), Request.GetUserName()?.Value, entityFamily._id.ToString());//Request.GetUserName()?.Value Request.GetUserId()

                return Ok(Utilities.ReturnSuccess(data: "Atualizado com sucesso!"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Delete, TypeResposible.UserAdminstratorGestor, $"Não foi possível Inclusão de membro(s) de nova família", Request.GetUserId(), Request.GetUserName()?.Value, model.FamilyId, "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }
        /// <summary>
        /// LOGIN
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         POST
        ///             {
        ///              "holderCpf":"string", //obrigatório
        ///              "password":"string", //obrigatório  
        ///              "UseNewDevice":false  /// true para forçar o login de outro dispositivo
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("Token")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Token([FromBody] LoginFamilyViewModel model)
        {
            var claims = new List<Claim>();
            var claim = Util.SetRole(TypeProfile.Familia);
            var claimUserName = Request.GetUserName();
            claims.Add(claim);

            var response = new ResponseCheckDevice();

            try
            {
                if (claimUserName != null)
                    claims.Add(claimUserName);

                var deviceId = Request.Headers["deviceId"].ToString();

                model.TrimStringProperties();

                if (string.IsNullOrEmpty(model.RefreshToken) == false)
                    return TokenProviderMiddleware.RefreshToken(model.RefreshToken, false, claims.ToArray());

                Data.Entities.Family familyEntity;

                model.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelStateOnlyFields(nameof(model.HolderCpf), nameof(model.Password));

                if (isInvalidState != null)
                {
                    isInvalidState.Data = response;
                    return BadRequest(isInvalidState);
                }

                familyEntity = await _familyRepository.FindOneByAsync(x => x.Holder.Cpf == model.HolderCpf && x.Password == model.Password).ConfigureAwait(false);

                if (familyEntity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidLogin, data: response));

                /* VERIFICA SE EXISTE UM DISPOSITIVO LOGADO  */
                if (familyEntity != null && string.IsNullOrEmpty(deviceId) == false && familyEntity.DeviceId != null && familyEntity.DeviceId.Count() > 0 && familyEntity.DeviceId[0] != deviceId && model.UseNewDevice == false)
                {
                    response.HasAnotherSession = true;
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.DeviceInUse, data: response));
                }

                if (claims.Count(x => x.Type == "UserName") == 0)
                    claims.Add(new Claim("UserName", familyEntity.Holder.Name));

                if (string.IsNullOrEmpty(deviceId) == false && (familyEntity.DeviceId.Count() == 0 || model.UseNewDevice))
                {
                    familyEntity.DeviceId = new List<string>() { deviceId };

                    await _familyRepository.UpdateAsync(familyEntity).ConfigureAwait(false);
                }

                return Ok(Utilities.ReturnSuccess(data: TokenProviderMiddleware.GenerateToken(familyEntity._id.ToString(), false, claims.ToArray())));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(data: response));
            }
        }
        /// <summary>
        /// LOGIN POR CPF / DATA DE ANIVERSÁRIO
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         POST
        ///             {
        ///              "holderCpf":"string", //obrigatório
        ///              "holderBirthday":long //obrigatório
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("TokenByBirthday")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> TokenByBirthday([FromBody] LoginFamilyBirthdayViewModel model)
        {
            var claims = new List<Claim>();
            var claim = Util.SetRole(TypeProfile.Familia);
            var claimUserName = Request.GetUserName();
            claims.Add(claim);

            try
            {
                if (claimUserName != null)
                    claims.Add(claimUserName);

                model.TrimStringProperties();



                Data.Entities.Family entity;

                model.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelStateOnlyFields(nameof(model.HolderCpf), nameof(model.HolderBirthday));

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                var dateBir = Utilities.TimeStampToDateTime(model.HolderBirthday);
                var dateUnix = Utilities.ToTimeStamp(dateBir.Date);
                entity = await _familyRepository.FindOneByAsync(x => x.Holder.Cpf == model.HolderCpf && x.Holder.Birthday == dateUnix).ConfigureAwait(false);
                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidLogin));


                claims.Add(new Claim("UserName", entity.Holder.Name));
                await _familyRepository.UpdateAsync(entity).ConfigureAwait(false);


                return Ok(Utilities.ReturnSuccess(data: TokenProviderMiddleware.GenerateToken(entity._id.ToString(), false, claims.ToArray())));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }


        /// <summary>
        /// ALTERAR PASSWORD
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         POST
        ///             {
        ///              "currentPassword":"string",
        ///              "newPassword":"string",
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("ChangePassword")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            try
            {
                var userId = Request.GetUserId();


                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro());

                model.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelState();

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                var entity = await _familyRepository.FindByIdAsync(userId).ConfigureAwait(false);

                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));

                if (entity.Password != model.CurrentPassword)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.PasswordNoMatch));

                entity.Password = model.NewPassword;

                _familyRepository.Update(entity);
                await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Change, TypeResposible.UserAdminstratorGestor, $"Alteração de senha {entity.Holder.Name}", userId, "", userId);
                return Ok(Utilities.ReturnSuccess("Senha alterada com sucesso."));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Curso, TypeAction.Change, TypeResposible.UserAdminstratorGestor, $"Não foi possível alterar a senha", "", "", "", "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// PRIMEIRO ACESSO
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         POST
        ///             {
        ///               "cpf": "string",
        ///               "password": "string",
        ///               "motherName": "string",
        ///               "cityBorned": "string"        
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("FirstAccess")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> FirstAccess([FromBody] FamilyFirstAccessViewModel model)
        {
            try
            {

                model.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelState();

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);


                var family = _mapper.Map<Data.Entities.Family>(model);

                var entity = await _familyRepository.FindOneByAsync(x => x.Holder.Cpf == model.Cpf).ConfigureAwait(false);

                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));

                entity.Password = model.Password;
                entity.MotherName = model.MotherName.RemoveAccents();
                entity.MotherCityBorned = model.MotherCityBorned.RemoveAccents();


                _familyRepository.Update(entity);

                return Ok(Utilities.ReturnSuccess("Primeiro acesso realizado com sucesso."));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// EXPORTAR FAMÍLIA PARA O EXCEL
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


        public async Task<IActionResult> Export([FromForm] DtParameters model, [FromForm] string number, [FromForm] string holderName, [FromForm] string holderCpf)
        {
            var response = new DtResult<FamilyExportViewModel>();
            try
            {
                var conditions = new List<FilterDefinition<Data.Entities.Family>>();
                var builder = Builders<Data.Entities.Family>.Filter;

                conditions.Add(builder.Where(x => x.Created != null && x._id != null));

                if (!string.IsNullOrEmpty(number))
                    conditions.Add(builder.Where(x => x.Holder.Number == number));
                if (!string.IsNullOrEmpty(holderName))
                    conditions.Add(builder.Where(x => x.Holder.Name.ToUpper().Contains(holderName.ToUpper())));
                if (!string.IsNullOrEmpty(holderCpf))
                    conditions.Add(builder.Where(x => x.Holder.Cpf == holderCpf.OnlyNumbers()));


                var condition = builder.And(conditions);
                var fileName = "Familias_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";
                var allData = await _familyRepository.GetCollectionAsync().FindSync(condition, new FindOptions<Data.Entities.Family>() { }).ToListAsync();

                var path = Path.Combine($"{Directory.GetCurrentDirectory()}\\", @"ExportFiles");
                if (Directory.Exists(path) == false)
                    Directory.CreateDirectory(path);

                var fullPathFile = Path.Combine(path, fileName);
                var listViewModel = _mapper.Map<List<FamilyExportViewModel>>(allData);
                Utilities.ExportToExcel(listViewModel, path, fileName: fileName.Split('.')[0]);
                if (System.IO.File.Exists(fullPathFile) == false)
                    return BadRequest(Utilities.ReturnErro("Ocorreu um erro fazer download do arquivo"));

                var fileBytes = System.IO.File.ReadAllBytes(@fullPathFile);
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(Utilities.ReturnErro(ex.Message));
            }
        }

        /// <summary>
        /// REGISTRAR E REMOVER DEVICE ID ONESIGNAL OU FCM | CHAMAR APOS LOGIN E LOGOUT
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         POST
        ///             {
        ///              "deviceId":"string",
        ///              "isRegister":true  // true => registrar  | false => remover 
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("RegisterUnRegisterDeviceId")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public IActionResult RegisterUnRegisterDeviceId([FromBody] PushViewModel model)
        {
            try
            {
                var userId = Request.GetUserId();

                model.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelState();

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));


                Task.Run(() =>
                {
                    if (model.IsRegister)
                    {

                        _familyRepository.UpdateMultiple(Query<Data.Entities.Family>.Where(x => x._id == ObjectId.Parse(userId)),
                            new UpdateBuilder<Data.Entities.Family>().AddToSet(x => x.DeviceId, model.DeviceId), UpdateFlags.None);
                    }
                    else
                    {
                        _familyRepository.UpdateMultiple(Query<Data.Entities.Profile>.Where(x => x._id == ObjectId.Parse(userId)),
                            new UpdateBuilder<Data.Entities.Family>().Pull(x => x.DeviceId, model.DeviceId), UpdateFlags.None);
                    }
                });

                return Ok(Utilities.ReturnSuccess());
            }
            catch (Exception ex)
            {
                return BadRequest(Utilities.ReturnErro(ex.Message));
            }
        }

        /// <summary>
        /// DOWNLOAD DE ARQUIVO DE EXEMPLO DE IMPORTAÇÃO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("ExampleFileImport")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<IActionResult> ExampleFileImport()
        {
            try
            {
                var fileName = "Modelo Importar Familias.xlsx";
                var path = Path.Combine($"{Directory.GetCurrentDirectory()}/Content", @"ExportFiles");
                if (Directory.Exists(path) == false)
                    Directory.CreateDirectory(path);

                var fullPathFile = Path.Combine(path, fileName);
                var listViewModel = new List<FamilyImportViewModel>() { new FamilyImportViewModel() };
                var listMemberViewModel = new List<FamilyMemberImportViewModel>() { new FamilyMemberImportViewModel() };


                Util.ExportToExcelMultiWorksheet(path, new List<string>() { "Familia", "Membros" }, fileName: fileName.Split('.')[0]);
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