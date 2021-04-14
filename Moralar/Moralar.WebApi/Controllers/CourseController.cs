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
        private readonly INotificationRepository  _notificationRepository;
        private readonly INotificationSendedRepository _notificationSendedRepository;
        private readonly IFamilyRepository _familyRepository;
        private readonly ICourseFamilyRepository _courseFamilyRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUtilService _utilService;
        private readonly ISenderMailService _senderMailService;

        public CourseController(IMapper mapper, ICourseRepository courseRepository, INotificationRepository notificationRepository, INotificationSendedRepository notificationSendedRepository, IFamilyRepository familyRepository, ICourseFamilyRepository courseFamilyRepository, IHttpContextAccessor httpContextAccessor, IUtilService utilService, ISenderMailService senderMailService)
        {
            _mapper = mapper;
            _courseRepository = courseRepository;
            _notificationRepository = notificationRepository;
            _notificationSendedRepository = notificationSendedRepository;
            _familyRepository = familyRepository;
            _courseFamilyRepository = courseFamilyRepository;
            _httpContextAccessor = httpContextAccessor;
            _utilService = utilService;
            _senderMailService = senderMailService;
        }
        /// <summary>
        /// BLOQUEAR / DESBLOQUEAR CURSO 
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
                await _utilService.RegisterLogAction(LocalAction.Curso, typeAction, TypeResposible.UserAdminstratorGestor, $"Bloqueou/Desbloqueu o curso {entity.Title}", Request.GetUserId(), Request.GetUserName().Value, model.TargetId);
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
            var response = new DtResult<CourseListViewModel>();
            try
            {
                var builder = Builders<Data.Entities.Course>.Filter;
                var conditions = new List<FilterDefinition<Data.Entities.Course>>();

                conditions.Add(builder.Where(x => x.Created != null));
                if (!string.IsNullOrEmpty(title))
                    conditions.Add(builder.Where(x => x.Title.ToUpper().Contains(title.ToUpper())));
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

                response.Data = _mapper.Map<List<CourseListViewModel>>(retorno);
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
        /// DETALHES DO CURSO 
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         GET
        ///             {
        ///              "id": "string", // required
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
        /// LISTA TODOS OS CURSOS
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
                
                var entity = await _courseRepository.FindAllAsync().ConfigureAwait(false);

                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ResidencialPropertyNotFound));

                return Ok(Utilities.ReturnSuccess(data: _mapper.Map<List<CourseViewModel>>(entity)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// REGISTRAR UM NOVO CURSO
        /// </summary>
        /// <remarks>
        ///OBJ DE ENVIO
        ///
        ///        POST
        ///        {
        ///          "title": "string",
        ///          "img": "string",
        ///          "startDate": 0,
        ///          "endDate": 0,
        ///          "schedule": "string",
        ///          "place": "string",
        ///          "workLoad": "string",
        ///          "description": "string",
        ///          "startTargetAudienceAge": 0,
        ///          "endTargetAudienceAge": 0,
        ///          "typeGenre": "Feminino",
        ///          "numberOfVacancies": 0,
        ///          "id": "string"
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

              
                await _utilService.RegisterLogAction(LocalAction.Curso, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Cadastrou novo curso {model.Title}", Request.GetUserId(), Request.GetUserName().Value, entityId, "");

                return Ok(Utilities.ReturnSuccess(data: "Registrado com sucesso!"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Curso, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível cadastrar novo curso", "", "", "", "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// REGISTRAR FAMÍLIA PARA O CURSO
        /// </summary>
        /// <remarks>
        ///OBJ DE ENVIO
        ///
        ///         POST
        ///        {
        ///          "familyId": "string",
        ///          "courseId": "string",
        ///          "waitInTheQueue": true, AGUARDAR NA FILA DE ESPERA
        ///          "id": "string"
        ///        }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("RegisterFamilyToTrainning")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterFamilyToTrainning([FromBody] CourseFamilyViewModel model)
        {
            try
            {
                var entityFamily = await _familyRepository.FindByIdAsync(model.FamilyId).ConfigureAwait(false);
                if (entityFamily == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));

                if (await _courseFamilyRepository.CheckByAsync(x => x.FamilyId == model.FamilyId).ConfigureAwait(false))
                    return BadRequest(Utilities.ReturnErro("Família já realizou o curso"));
                var entityCourse = await _courseRepository.FindByIdAsync(model.CourseId).ConfigureAwait(false);
                if (entityCourse == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.CourseNotFound));

                if (!entityFamily.Members.Exists(x => x.Birthday.TimeStampToDateTime().CalculeAge() >= entityCourse.StartTargetAudienceAge && x.Birthday.TimeStampToDateTime().CalculeAge() <= entityCourse.EndTargetAudienceAge))
                    return BadRequest(Utilities.ReturnErro("Não é possível realizar a inscrição neste curso, por não condizer com o público a que é destinado"));

                var qtdCourse = await _courseFamilyRepository.FindByAsync(x => x.CourseId == model.CourseId).ConfigureAwait(false) as List<CourseFamily>;
                if (qtdCourse.Count() >= entityCourse.NumberOfVacancies && model.WaitInTheQueue == false)
                    return BadRequest(Utilities.ReturnErro("Deseja aguardar na fila de espera?"));

                var entity = _mapper.Map<Data.Entities.CourseFamily>(model);
                entity.TypeStatusCourse = model.WaitInTheQueue == false ? TypeStatusCourse.Inscrito : TypeStatusCourse.ListaEspera;
                var entityId = await _courseFamilyRepository.CreateAsync(entity).ConfigureAwait(false);


                await _notificationSendedRepository.CreateAsync(new NotificationSended
                {
                    Title = "Confirmação de inscrição em curso",
                    Description = $"Olá {  entityFamily.Holder.Name }!"+
                                  $"Sua inscrição no curso {  entityCourse.Title }"+
                                  "foi realizada com sucesso!Fique atento, seu curso começará em breve.",
                    FamilyId=entity._id.ToString(),
                });
                await _utilService.RegisterLogAction(LocalAction.Curso, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Cadastrou família  {entityFamily.Holder.Name} para fazer o curso", Request.GetUserId(), Request.GetUserName().Value, entityId, "");
                return Ok(Utilities.ReturnSuccess(data: "Registrado com sucesso!"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Curso, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível cadastrar nova Família", "", "", "", "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// CANCELA CURSO DA FAMÍLIA
        /// </summary>
        /// <remarks>
        ///OBJ DE ENVIO
        ///
        ///         POST
        ///        {
        ///        "familyId": "string",
        ///        "courseId": "string"
        ///        }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("CancelFamilyToTrainning")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CancelFamilyToTrainning([FromBody] CourseCancelViewModel model)
        {
            try
            {
                var entityFamily = await _familyRepository.FindByIdAsync(model.FamilyId).ConfigureAwait(false);
                if (entityFamily == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));

                var entityCourse = await _courseRepository.FindByIdAsync(model.CourseId).ConfigureAwait(false);
                if (entityCourse == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.CourseNotFound));

                var courses = await _courseFamilyRepository.FindByAsync(x => x.CourseId == model.CourseId).ConfigureAwait(false);
                var entityCourseFamily = courses.Where(x => x.FamilyId == model.FamilyId && x.CourseId == model.CourseId).FirstOrDefault();
                if (entityCourseFamily == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.CourseFamilyNotFound));

                await _courseFamilyRepository.DeleteOneAsync(entityCourseFamily._id.ToString()).ConfigureAwait(false);

               
                if (courses.ToList().Count(x => x.TypeStatusCourse == TypeStatusCourse.ListaEspera) > 0)
                {
                    var familyToCourseAvaliable = courses.ToList().Where(x => x.TypeStatusCourse == TypeStatusCourse.ListaEspera).OrderBy(x => x.Created).FirstOrDefault();
                    if (familyToCourseAvaliable != null)
                    {
                        familyToCourseAvaliable.TypeStatusCourse = TypeStatusCourse.Inscrito;
                        await _courseFamilyRepository.UpdateOneAsync(familyToCourseAvaliable).ConfigureAwait(false);


                        await _notificationSendedRepository.CreateAsync(new NotificationSended
                        {
                            Title = "Liberação de vaga em curso",
                            Description = $"Olá {  entityFamily.Holder.Name }!" +
                                          $" Sua inscrição para o curso {  entityCourse.Title }" +
                                           " que estava aguardando na lista de espera foi confirmada com sucesso! Fique atento, seu curso começará em breve.",
                            FamilyId = entityFamily._id.ToString(),
                        });
                    }
                }
                await _notificationSendedRepository.CreateAsync(new NotificationSended
                {
                    Title = "Cancelamento de inscrição em curso",
                    Description = $"Olá {  entityFamily.Holder.Name }!" +
                                 $" Seu curso {  entityCourse.Title }" +
                                 " foi realizada com sucesso!Fique atento, seu curso começará em breve.",
                    FamilyId = entityFamily._id.ToString(),
                });
                await _utilService.RegisterLogAction(LocalAction.Curso, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Cancelou o curso {entityFamily.Holder.Name} para fazer o curso", Request.GetUserId(), Request.GetUserName().Value, model.CourseId, "");
                return Ok(Utilities.ReturnSuccess(data: "Registrado com sucesso!"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Curso, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível cadastrar nova Família", "", "", "", "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }
    }
}