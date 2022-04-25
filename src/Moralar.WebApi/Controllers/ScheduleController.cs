using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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
using Moralar.Domain.ViewModels.Question;
using Moralar.Domain.ViewModels.Quiz;
using Moralar.Domain.ViewModels.Schedule;
using Moralar.Domain.ViewModels.ScheduleHistory;
using Moralar.Repository.Interface;
using UtilityFramework.Application.Core;
using UtilityFramework.Application.Core.JwtMiddleware;
using UtilityFramework.Application.Core.ViewModels;
using UtilityFramework.Services.Core.Interface;


namespace Moralar.WebApi.Controllers
{
    [EnableCors("AllowAllOrigin")]
    [Route("api/v1/[controller]")]
    [Authorize(ActiveAuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class ScheduleController : Controller
    {

        private readonly IMapper _mapper;
        private readonly IFamilyRepository _familyRepository;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IScheduleHistoryRepository _scheduleHistoryRepository;
        private readonly IUtilService _utilService;
        private readonly ISenderMailService _senderMailService;
        private readonly IQuizRepository _quizRepository;
        private readonly IQuizFamilyRepository _quizFamilyRepository;
        private readonly ICourseFamilyRepository _courseFamilyRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IPropertiesInterestRepository _propertiesInterestRepository;
        private readonly IResidencialPropertyRepository _residencialPropertyRepository;
        private readonly INotificationSendedRepository _notificationSendedRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly ISenderNotificationService _senderNotificationService;

        public ScheduleController(IMapper mapper, IFamilyRepository familyRepository, IScheduleRepository scheduleRepository, IScheduleHistoryRepository scheduleHistoryRepository, IUtilService utilService, ISenderMailService senderMailService, IQuizRepository quizRepository, IQuizFamilyRepository quizFamilyRepository, ICourseFamilyRepository courseFamilyRepository, ICourseRepository courseRepository, IPropertiesInterestRepository propertiesInterestRepository, IResidencialPropertyRepository residencialPropertyRepository, INotificationSendedRepository notificationSendedRepository, INotificationRepository notificationRepository, ISenderNotificationService senderNotificationService, IProfileRepository profileRepository)
        {
            _mapper = mapper;
            _familyRepository = familyRepository;
            _scheduleRepository = scheduleRepository;
            _scheduleHistoryRepository = scheduleHistoryRepository;
            _utilService = utilService;
            _senderMailService = senderMailService;
            _quizRepository = quizRepository;
            _quizFamilyRepository = quizFamilyRepository;
            _courseFamilyRepository = courseFamilyRepository;
            _courseRepository = courseRepository;
            _propertiesInterestRepository = propertiesInterestRepository;
            _residencialPropertyRepository = residencialPropertyRepository;
            _notificationSendedRepository = notificationSendedRepository;
            _notificationRepository = notificationRepository;
            _senderNotificationService = senderNotificationService;
            _profileRepository = profileRepository;
        }




        /// <summary>
        /// RETORNA O HISTÓRICO DOS AGENDAMENTOS POR FAMÍLIA
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("GetHistoryByFamily/{familyId}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetHistoryByFamily([FromRoute] string familyId)
        {
            try
            {
                var entity = await _scheduleHistoryRepository.FindByAsync(x => x.FamilyId == familyId).ConfigureAwait(false);

                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ScheduleNotFound));

                return Ok(Utilities.ReturnSuccess(data: _mapper.Map<List<ScheduleHistoryViewModel>>(entity.OrderBy(x => x.TypeSubject).ThenBy(x => x.Created).ToList())));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// RETORNA OS PRÓXIMOS AGENDAMENTOS POR FAMÍLIA
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("GetScheduleByFamily/{familyId}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetScheduleByFamily([FromRoute] string familyId)
        {
            try
            {
                var entity = await _scheduleRepository.FindByAsync(x => x.FamilyId == familyId).ConfigureAwait(false);

                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ScheduleNotFound));

                return Ok(Utilities.ReturnSuccess(data: _mapper.Map<List<ScheduleListViewModel>>(entity.OrderBy(x => x.Created).ToList())));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// LISTAGEM DOS QUESTINÁRIOS DATATABLE
        /// </summary>
        /// <response code = "200" > Returns success</response>
        /// <response code = "400" > Custom Error</response>
        /// <response code = "401" > Unauthorize Error</response>
        /// <response code = "500" > Exception Error</response>
        /// <returns></returns>
        [HttpPost("LoadData")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        //[ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> LoadData([FromForm] DtParameters model, [FromForm] string number, [FromForm] string name, [FromForm] string cpf, [FromForm] long? startDate, [FromForm] long? endDate, [FromForm] string place, [FromForm] string description, [FromForm] TypeScheduleStatus? status, [FromForm] TypeSubject? type)
        {
            var response = new DtResult<ScheduleListViewModel>();
            try
            {
                var builder = Builders<Data.Entities.Schedule>.Filter;
                var conditions = new List<FilterDefinition<Data.Entities.Schedule>>();

                conditions.Add(builder.Where(x => x.Created != null && x.Disabled == null));

                if (!string.IsNullOrEmpty(number))
                    conditions.Add(builder.Where(x => x.HolderNumber == number));
                if (!string.IsNullOrEmpty(name))
                    conditions.Add(builder.Where(x => x.HolderName.ToUpper().Contains(name.ToUpper())));
                if (!string.IsNullOrEmpty(cpf))
                    conditions.Add(builder.Where(x => x.HolderCpf == cpf.OnlyNumbers()));
                if (startDate.HasValue)
                    conditions.Add(builder.Where(x => x.Date >= startDate));
                if (endDate.HasValue)
                    conditions.Add(builder.Where(x => x.Date <= endDate));
                if (!string.IsNullOrEmpty(place))
                    conditions.Add(builder.Where(x => x.Place.ToUpper().Contains(place.ToUpper())));
                if (!string.IsNullOrEmpty(description))
                    conditions.Add(builder.Where(x => x.Description.ToUpper().Contains(description.ToUpper())));

                if (status != null)
                    conditions.Add(builder.Where(x => x.TypeScheduleStatus == status));

                if (type != null)
                    conditions.Add(builder.Where(x => x.TypeSubject == type));


                var columns = model.Columns.Where(x => x.Searchable && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToArray();

                var sortColumn = !string.IsNullOrEmpty(model.SortOrder) ? model.SortOrder.UppercaseFirst() : model.Columns.FirstOrDefault(x => x.Orderable)?.Name ?? model.Columns.FirstOrDefault()?.Name;
                var totalRecords = (int)await _scheduleRepository.GetCollectionAsync().CountDocumentsAsync(builder.And(conditions));

                var sortBy = model.Order[0].Dir.ToString().ToUpper().Equals("DESC")
                    ? Builders<Data.Entities.Schedule>.Sort.Descending(sortColumn)
                    : Builders<Data.Entities.Schedule>.Sort.Ascending(sortColumn);

                var retorno = await _scheduleRepository
                    .LoadDataTableAsync(model.Search.Value, sortBy, model.Start, model.Length, conditions, columns);

                var totalrecordsFiltered = !string.IsNullOrEmpty(model.Search.Value)
                    ? (int)await _scheduleRepository.CountSearchDataTableAsync(model.Search.Value, conditions, columns)
                    : totalRecords;

                response.Data = _mapper.Map<List<ScheduleListViewModel>>(retorno);
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
        /// DETALHES DO QUIZ
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


                var entity = await _scheduleRepository.FindByIdAsync(id).ConfigureAwait(false);

                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.QuizNotFound));

                return Ok(Utilities.ReturnSuccess(data: _mapper.Map<ScheduleListViewModel>(entity)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// REGISTRAR UM NOVO AGENDAMENTO
        /// </summary>
        /// <remarks>
        ///OBJ DE ENVIO
        ///
        ///        POST
        ///        {
        ///        "date": 0,
        ///        "place": "string",
        ///        "description": "string",
        ///        "familyId": "string",
        ///        "typeSubject": " Visita do TTS",
        ///        "id": "string"
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
        public async Task<IActionResult> Register([FromBody] ScheduleRegisterViewModel model)
        {

            try
            {
                

                var isInvalidState = ModelState.ValidModelStateOnlyFields(nameof(model.Date), nameof(model.Description), nameof(model.FamilyId), nameof(model.Place));
                if (isInvalidState != null)
                    return BadRequest(isInvalidState);


                var dateToSchedule = Utilities.TimeStampToDateTime(model.Date);
                if (dateToSchedule < DateTime.Now)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.DateInvalidToSchedule));

                var family = await _familyRepository.FindByIdAsync(model.FamilyId).ConfigureAwait(false);

                if (family == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));

                var schedule = await _scheduleRepository.CountAsync(x => x.FamilyId == model.FamilyId && x.TypeSubject == TypeSubject.ReuniaoPGM);

                /*Checa se as etapas anteriores foram cumplidas*/
                if (schedule < 3)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.StageInvalidToSchedule));

                var scheduleEntity = _mapper.Map<Schedule>(model);
                scheduleEntity.HolderCpf = family.Holder.Cpf;
                scheduleEntity.HolderName = family.Holder.Name;
                scheduleEntity.HolderNumber = family.Holder.Number;
                scheduleEntity.TypeSubject = model.TypeSubject;



                scheduleEntity.TypeScheduleStatus = TypeScheduleStatus.AguardandoConfirmacao;


                var scheduleId = await _scheduleRepository.CreateAsync(scheduleEntity).ConfigureAwait(false);



                var scheduleHistoryEntity = _mapper.Map<ScheduleHistory>(model);
                scheduleHistoryEntity.HolderCpf = family.Holder.Cpf;
                scheduleHistoryEntity.HolderName = family.Holder.Name;
                scheduleHistoryEntity.HolderNumber = family.Holder.Number;
                scheduleHistoryEntity.TypeSubject = model.TypeSubject;
                scheduleHistoryEntity.ScheduleId = scheduleId;
                await _scheduleHistoryRepository.CreateAsync(scheduleHistoryEntity).ConfigureAwait(false);
                if (model.TypeSubject == TypeSubject.ReuniaoPGM)
                {
                    scheduleHistoryEntity._id = new ObjectId();
                    scheduleHistoryEntity.TypeSubject = TypeSubject.EscolhaDoImovel;
                    await _scheduleHistoryRepository.CreateAsync(scheduleHistoryEntity).ConfigureAwait(false);

                    scheduleHistoryEntity._id = new ObjectId();
                    scheduleHistoryEntity.TypeSubject = TypeSubject.Mudanca;
                    await _scheduleHistoryRepository.CreateAsync(scheduleHistoryEntity).ConfigureAwait(false);

                    scheduleHistoryEntity._id = new ObjectId();
                    scheduleHistoryEntity.TypeSubject = TypeSubject.AcompanhamentoPosMudança;
                    await _scheduleHistoryRepository.CreateAsync(scheduleHistoryEntity).ConfigureAwait(false);
                }
                var dataBody = Util.GetTemplateVariables();
                if (model.TypeSubject == TypeSubject.ReuniaoPGM)
                {
                    //_notificationSendedRepository
                    dataBody.Add("{{ title }}", "Cadastro de Agendamento");
                    dataBody.Add("{{ message }}", $"<p>Caro(a) {family.Holder.Name.GetFirstName()}</p>" +
                                                $"<p>Foi cadastrado um agendamento para o horário {dateToSchedule.ToString("dd/MM/yyyy")}</p>"
                                                );
                }
                else
                {
                    dataBody.Add("{{ title }}", $"Olá {family.Holder.Name.GetFirstName()}!");
                    dataBody.Add("{{ message }}", $"<p>Sua agenda {model.TypeSubject.GetEnumMemberValue()} foi marcada</p>" +
                                                $"<p>Dia { Utilities.TimeStampToDateTime(scheduleEntity.Date).ToString("dd/MM/yyyy")}, horário {Utilities.TimeStampToDateTime(scheduleEntity.Date).ToString("HH:mm")} , endereço {model.Place}</p>" +
                                                $"Aguardamos você!"
                                                );
                }
                var body = _senderMailService.GerateBody("custom", dataBody);

                var unused = Task.Run(async () =>
                {
                    await _senderMailService.SendMessageEmailAsync(Startup.ApplicationName, family.Holder.Email, body, "Cadastro de Agendamento").ConfigureAwait(false);
                });


                /*ENVIA NOTIFICAÇÃO*/

                var title = "Novo agendamento";
                var content = "Você tem um novo agendamento";

                var listNotification = new List<Notification>();
               
                    listNotification.Add(new Notification()
                    {
                        For = ForType.Family,
                        FamilyId = model.FamilyId,
                        Title = title,
                        Description = content
                    });
                

                await _notificationRepository.CreateAsync(listNotification);

                dynamic payloadPush = Util.GetPayloadPush();
                dynamic settingPush = Util.GetSettingsPush();

                await _senderNotificationService.SendPushAsync(title, content, family.DeviceId, data: payloadPush, settings: settingPush, priority: 10);


                return Ok(Utilities.ReturnSuccess(data: "Registrado com sucesso!"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Question, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível cadastrar/atualizar novo questionário", "", "", "", "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }
        /// <summary>
        /// REGISTRAR UM NOVO QUESTIONÁRIO
        /// </summary>
        /// <remarks>
        ///OBJ DE ENVIO
        ///
        ///        POST
        ///        {
        ///          "date": 0,
        ///          "place": "string",
        ///          "description": "string",
        ///          "familyId": "string",
        ///          "typeSubject": " Visita do TTS",
        ///          "id": "string"
        ///        }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("RegisterResettlement")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> RegisterResettlement([FromBody] ScheduleRegisterViewModel model)
        {

            try
            {

                var isInvalidState = ModelState.ValidModelStateOnlyFields(nameof(model.Date), nameof(model.Description), nameof(model.FamilyId), nameof(model.Place));
                if (isInvalidState != null)
                    return BadRequest(isInvalidState);


                var dateToSchedule = Utilities.TimeStampToDateTime(model.Date);
                if (dateToSchedule < DateTime.Now)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.DateInvalidToSchedule));

                var family = await _familyRepository.FindByIdAsync(model.FamilyId).ConfigureAwait(false);
                if (family == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));


                var scheduleEntity = _mapper.Map<Schedule>(model);
                scheduleEntity.HolderCpf = family.Holder.Cpf;
                scheduleEntity.HolderName = family.Holder.Name;
                scheduleEntity.HolderNumber = family.Holder.Number;
                scheduleEntity.TypeSubject = TypeSubject.ReuniaoPGM;
                scheduleEntity.TypeScheduleStatus = TypeScheduleStatus.AguardandoConfirmacao;
                await _scheduleRepository.CreateAsync(scheduleEntity).ConfigureAwait(false);


                var dataBody = Util.GetTemplateVariables();
                dataBody.Add("{{ title }}", "Cadastro de Agendamento");
                dataBody.Add("{{ message }}", $"<p>Caro(a) {family.Holder.Name.GetFirstName()}</p>" +
                                            $"<p>Foi cadastrado um agendamento para o horário {dateToSchedule.ToString("dd/MM/yyyy")}</p>"
                                            );

                var body = _senderMailService.GerateBody("custom", dataBody);

                var unused = Task.Run(async () =>
                {
                    await _senderMailService.SendMessageEmailAsync(Startup.ApplicationName, family.Holder.Email, body, "Cadastro de Agendamento").ConfigureAwait(false);
                });
                return Ok(Utilities.ReturnSuccess(data: "Registrado com sucesso!"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Question, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível cadastrar/atualizar novo questionário", "", "", "", "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }



        /// <summary>
        /// RETORNA OS  DETALHES DA LINHA DO TEMPO DA FAMÍLIA - ESCOLHA DO IMÓVEL
        /// </summary>
        /// <response code = "200" > Returns success</response>
        /// <response code = "400" > Custom Error</response>
        /// <response code = "401" > Unauthorize Error</response>
        /// <response code = "500" > Exception Error</response>
        /// <returns></returns>
        [HttpGet("DetailTimeLineProcessChooseProperty/{familyId}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DetailTimeLineProcessChooseProperty([FromRoute] string familyId)
        {

            /*TODO VERIFICAR REGRA SE NECESSARIO AGENDAMENTO DE ESCOLHA DE IMOVEL*/
            try
            {
                var vwScheduleDetailTimeLineChoosePropertyViewModel = new ScheduleDetailTimeLineChoosePropertyViewModel();
                var family = await _familyRepository.FindByIdAsync(familyId).ConfigureAwait(false);
                if (family == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));

                vwScheduleDetailTimeLineChoosePropertyViewModel = _mapper.Map<ScheduleDetailTimeLineChoosePropertyViewModel>(family);

                // var schedule = await _scheduleRepository.FindOneByAsync(x => x.FamilyId == familyId && x.TypeSubject == TypeSubject.EscolhaDoImovel);
                // if (schedule == null)
                //     return BadRequest(Utilities.ReturnErro(DefaultMessages.ScheduleNotFound));

                var interest = await _propertiesInterestRepository.FindByAsync(x => x.FamilyId == familyId).ConfigureAwait(false) as List<PropertiesInterest>;
                if (interest.Count() > 0)
                {
                    var residencialPropertyInterest = await _residencialPropertyRepository.FindIn("_id", interest.Select(x => ObjectId.Parse(x.ResidencialPropertyId.ToString())).ToList()) as List<ResidencialProperty>;
                    vwScheduleDetailTimeLineChoosePropertyViewModel.InterestResidencialProperty = _mapper.Map<List<ResidencialPropertyViewModel>>(residencialPropertyInterest);

                    var scheduleHistory = _scheduleHistoryRepository.FindBy(x => x.FamilyId == familyId).OrderBy(x => x.TypeSubject).ThenByDescending(x => x.Created).ToList();
                    vwScheduleDetailTimeLineChoosePropertyViewModel.Schedules = _mapper.Map<List<ScheduleHistoryViewModel>>(scheduleHistory);
                }
                vwScheduleDetailTimeLineChoosePropertyViewModel.InterestResidencialProperty = vwScheduleDetailTimeLineChoosePropertyViewModel.InterestResidencialProperty.OrderByDescending(x => x.Created).ToList();
                vwScheduleDetailTimeLineChoosePropertyViewModel.Schedules = vwScheduleDetailTimeLineChoosePropertyViewModel.Schedules.OrderByDescending(x => x.Created).ToList();

                var listQuizByFamily = await _quizFamilyRepository.FindByAsync(x => x.FamilyId == familyId, Builders<QuizFamily>.Sort.Descending(nameof(QuizFamily.Created))) as List<QuizFamily>;
                var listQuiz = await _quizRepository.FindIn("_id", listQuizByFamily.Select(x => ObjectId.Parse(x.QuizId.ToString())).ToList()) as List<Quiz>;

                for (int i = 0; i < listQuizByFamily.Count(); i++)
                {
                    if (listQuiz.Find(x => x._id == ObjectId.Parse(listQuizByFamily[i].QuizId)).TypeQuiz == TypeQuiz.Quiz)
                    {
                        vwScheduleDetailTimeLineChoosePropertyViewModel.DetailQuiz.Add(new ScheduleQuizDetailTimeLinePGMViewModel()
                        {
                            Created = listQuizByFamily[i].Created,
                            Title = listQuiz.Find(x => x._id == ObjectId.Parse(listQuizByFamily[i].QuizId)).Title,
                            Date = listQuizByFamily[i].Created.Value.TimeStampToDateTime().ToString("dd/MM/yyyy"),
                            HasAnswered = listQuizByFamily[i].TypeStatus == TypeStatus.NaoRespondido ? "Não respondido" : "Respondido",
                            QuizId = listQuizByFamily[i].QuizId,
                            QuizFamilyId = listQuizByFamily[i]._id.ToString()
                        });
                    }
                    else
                    {
                        vwScheduleDetailTimeLineChoosePropertyViewModel.DetailEnquete.Add(new ScheduleQuizDetailTimeLinePGMViewModel()
                        {
                            Created = listQuizByFamily[i].Created,
                            Title = listQuiz.Find(x => x._id == ObjectId.Parse(listQuizByFamily[i].QuizId)).Title,
                            Date = listQuizByFamily[i].Created.Value.TimeStampToDateTime().ToString("dd/MM/yyyy"),
                            HasAnswered = listQuizByFamily[i].TypeStatus == TypeStatus.NaoRespondido ? "Não respondido" : "Respondido",
                            QuizId = listQuizByFamily[i].QuizId,
                            QuizFamilyId = listQuizByFamily[i]._id.ToString()
                        });
                    }
                }

                var courseFamily = await _courseFamilyRepository.FindByAsync(x => x.FamilyId == familyId).ConfigureAwait(false) as List<CourseFamily>; ;
                var course = await _courseRepository.FindIn("_id", courseFamily.Select(x => ObjectId.Parse(x.CourseId.ToString())).ToList()) as List<Course>;
                for (int i = 0; i < courseFamily.Count(); i++)
                {
                    vwScheduleDetailTimeLineChoosePropertyViewModel.Courses.Add(new ScheduleCourseViewModel()
                    {
                        Created = courseFamily[i].Created,
                        Title = course.Find(x => x._id == ObjectId.Parse(courseFamily[i].CourseId)).Title,
                        StartDate = course.Find(x => x._id == ObjectId.Parse(courseFamily[i].CourseId)).StartDate,
                        EndDate = course.Find(x => x._id == ObjectId.Parse(courseFamily[i].CourseId)).EndDate,
                        TypeStatusCourse = courseFamily[i].TypeStatusCourse
                    });
                }

                vwScheduleDetailTimeLineChoosePropertyViewModel.Courses = vwScheduleDetailTimeLineChoosePropertyViewModel.Courses.OrderByDescending(x => x.Created).ToList();
                vwScheduleDetailTimeLineChoosePropertyViewModel.DetailQuiz = vwScheduleDetailTimeLineChoosePropertyViewModel.DetailQuiz.OrderByDescending(x => x.Created).ToList();
                vwScheduleDetailTimeLineChoosePropertyViewModel.DetailEnquete = vwScheduleDetailTimeLineChoosePropertyViewModel.DetailEnquete.OrderByDescending(x => x.Created).ToList();

                return Ok(Utilities.ReturnSuccess(data: vwScheduleDetailTimeLineChoosePropertyViewModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// RETORNA OS  DETALHES DA LINHA DO TEMPO DA FAMÍLIA - ESCOLHA DO IMÓVEL 1 e 2
        /// </summary>
        /// <response code = "200" > Returns success</response>
        /// <response code = "400" > Custom Error</response>
        /// <response code = "401" > Unauthorize Error</response>
        /// <response code = "500" > Exception Error</response>
        /// <returns></returns>
        [HttpGet("DetailTimeLineProcessChoosePropertyOneAndTwo/{familyId}/{typeSubject}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DetailTimeLineProcessChoosePropertyOneAndTwo([FromRoute] string familyId, [FromRoute] TypeSubject typeSubject)
        {
            try
            {
                typeSubject = TypeSubject.Mudanca;
                List<int> enumsValid = new List<int> { (int)TypeSubject.Mudanca, (int)TypeSubject.AcompanhamentoPosMudança };
                if (!enumsValid.Contains((int)typeSubject))
                    return BadRequest(Utilities.ReturnErro("Tipo de Assunto inválido! Deve-se utilizar( Mudança e Acompanhamento pós mudança)"));

                var _vwcheduleDetailTimeLineProcessChoosePropertyOneAndTwoViewModel = new ScheduleDetailTimeLineProcessChoosePropertyOneAndTwoViewModel();
                var family = await _familyRepository.FindByIdAsync(familyId).ConfigureAwait(false);
                if (family == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));

                _vwcheduleDetailTimeLineProcessChoosePropertyOneAndTwoViewModel = _mapper.Map<ScheduleDetailTimeLineProcessChoosePropertyOneAndTwoViewModel>(family);

                var scheduleHistory = _scheduleHistoryRepository.FindBy(x => x.FamilyId == familyId && x.TypeSubject == typeSubject).OrderBy(x => x.TypeSubject).ThenBy(x => x.Created).ToList();
                _vwcheduleDetailTimeLineProcessChoosePropertyOneAndTwoViewModel.Schedules = _mapper.Map<List<ScheduleHistoryViewModel>>(scheduleHistory);

                var listQuizByFamily = await _quizFamilyRepository.FindByAsync(x => x.FamilyId == familyId, Builders<QuizFamily>.Sort.Descending(nameof(QuizFamily.Created))) as List<QuizFamily>;
                var listQuiz = await _quizRepository.FindIn("_id", listQuizByFamily.Select(x => ObjectId.Parse(x.QuizId.ToString())).ToList()) as List<Quiz>;

                for (int i = 0; i < listQuizByFamily.Count(); i++)
                {
                    if (listQuiz.Find(x => x._id == ObjectId.Parse(listQuizByFamily[i].QuizId)).TypeQuiz == TypeQuiz.Quiz)
                    {
                        _vwcheduleDetailTimeLineProcessChoosePropertyOneAndTwoViewModel.DetailQuiz.Add(new ScheduleQuizDetailTimeLinePGMViewModel()
                        {
                            Title = listQuiz.Find(x => x._id == ObjectId.Parse(listQuizByFamily[i].QuizId)).Title,
                            Date = listQuizByFamily[i].Created.Value.TimeStampToDateTime().ToString("dd/MM/yyyy"),
                            HasAnswered = listQuizByFamily[i].TypeStatus == TypeStatus.NaoRespondido ? "Não respondido" : "Respondido",
                            QuizId = listQuizByFamily[i].QuizId,
                            QuizFamilyId = listQuizByFamily[i]._id.ToString()
                        });
                    }
                    else
                    {
                        _vwcheduleDetailTimeLineProcessChoosePropertyOneAndTwoViewModel.DetailEnquete.Add(new ScheduleQuizDetailTimeLinePGMViewModel()
                        {
                            Title = listQuiz.Find(x => x._id == ObjectId.Parse(listQuizByFamily[i].QuizId)).Title,
                            Date = listQuizByFamily[i].Created.Value.TimeStampToDateTime().ToString("dd/MM/yyyy"),
                            HasAnswered = listQuizByFamily[i].TypeStatus == TypeStatus.NaoRespondido ? "Não respondido" : "Respondido",
                            QuizId = listQuizByFamily[i].QuizId,
                            QuizFamilyId = listQuizByFamily[i]._id.ToString()
                        });
                    }
                }

                var courseFamily = await _courseFamilyRepository.FindByAsync(x => x.FamilyId == familyId).ConfigureAwait(false) as List<CourseFamily>; ;
                var course = await _courseRepository.FindIn("_id", courseFamily.Select(x => ObjectId.Parse(x.CourseId.ToString())).ToList()) as List<Course>;
                for (int i = 0; i < courseFamily.Count(); i++)
                {
                    _vwcheduleDetailTimeLineProcessChoosePropertyOneAndTwoViewModel.Courses.Add(new ScheduleCourseViewModel()
                    {
                        Title = course.Find(x => x._id == ObjectId.Parse(courseFamily[i].CourseId)).Title,
                        StartDate = course.Find(x => x._id == ObjectId.Parse(courseFamily[i].CourseId)).StartDate,
                        EndDate = course.Find(x => x._id == ObjectId.Parse(courseFamily[i].CourseId)).EndDate,
                        TypeStatusCourse = courseFamily[i].TypeStatusCourse
                    });
                }
                return Ok(Utilities.ReturnSuccess(data: _vwcheduleDetailTimeLineProcessChoosePropertyOneAndTwoViewModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }



        /// <summary>
        /// RETORNA OS  DETALHES DA LINHA DO TEMPO DA FAMÍLIA - REUNIÃO PGM
        /// </summary>
        /// <response code = "200" > Returns success</response>
        /// <response code = "400" > Custom Error</response>
        /// <response code = "401" > Unauthorize Error</response>
        /// <response code = "500" > Exception Error</response>
        /// <returns></returns>
        [HttpGet("DetailTimeLineProcessReunionPGM/{familyId}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DetailTimeLineProcessReunionPGM([FromRoute] string familyId)
        {
            try
            {
                var family = await _familyRepository.FindByIdAsync(familyId).ConfigureAwait(false);
                if (family == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));
                var schedule = await _scheduleRepository.FindOneByAsync(x => x.FamilyId == familyId && x.TypeSubject == TypeSubject.ReuniaoPGM);
                if (schedule == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ScheduleNotFound));

                var listQuizByFamily = await _quizFamilyRepository.FindByAsync(x => x.FamilyId == familyId, Builders<QuizFamily>.Sort.Descending(nameof(QuizFamily.Created))) as List<QuizFamily>;
                var listQuiz = await _quizRepository.FindIn("_id", listQuizByFamily.Select(x => ObjectId.Parse(x.QuizId.ToString())).ToList()) as List<Quiz>;


                var vwTimeLine = _mapper.Map<ScheduleDetailTimeLinePGMViewModel>(schedule);
                for (int i = 0; i < listQuizByFamily.Count(); i++)
                {
                    if (listQuiz.Find(x => x._id == ObjectId.Parse(listQuizByFamily[i].QuizId)).TypeQuiz == TypeQuiz.Quiz)
                    {
                        vwTimeLine.DetailQuiz.Add(new ScheduleQuizDetailTimeLinePGMViewModel()
                        {
                            Title = listQuiz.Find(x => x._id == ObjectId.Parse(listQuizByFamily[i].QuizId)).Title,
                            Date = listQuizByFamily[i].Created.Value.TimeStampToDateTime().ToString("dd/MM/yyyy"),
                            HasAnswered = listQuizByFamily[i].TypeStatus == TypeStatus.NaoRespondido ? "Não respondido" : "Respondido",
                            QuizId = listQuizByFamily[i].QuizId,
                            QuizFamilyId = listQuizByFamily[i]._id.ToString()
                        });
                    }
                    else
                    {
                        vwTimeLine.DetailEnquete.Add(new ScheduleQuizDetailTimeLinePGMViewModel()
                        {
                            Title = listQuiz.Find(x => x._id == ObjectId.Parse(listQuizByFamily[i].QuizId)).Title,
                            Date = listQuizByFamily[i].Created.Value.TimeStampToDateTime().ToString("dd/MM/yyyy"),
                            HasAnswered = listQuizByFamily[i].TypeStatus == TypeStatus.NaoRespondido ? "Não respondido" : "Respondido",
                            QuizId = listQuizByFamily[i].QuizId,
                            QuizFamilyId = listQuizByFamily[i]._id.ToString()
                        });
                    }
                }

                var courseFamily = await _courseFamilyRepository.FindByAsync(x => x.FamilyId == familyId).ConfigureAwait(false) as List<CourseFamily>; ;
                var course = await _courseRepository.FindIn("_id", courseFamily.Select(x => ObjectId.Parse(x.CourseId.ToString())).ToList()) as List<Course>;
                for (int i = 0; i < courseFamily.Count(); i++)
                {
                    vwTimeLine.Courses.Add(new ScheduleCourseViewModel()
                    {
                        Title = course.Find(x => x._id == ObjectId.Parse(courseFamily[i].CourseId)).Title,
                        StartDate = course.Find(x => x._id == ObjectId.Parse(courseFamily[i].CourseId)).StartDate,
                        EndDate = course.Find(x => x._id == ObjectId.Parse(courseFamily[i].CourseId)).EndDate,
                        TypeStatusCourse = courseFamily[i].TypeStatusCourse
                    });
                }
                return Ok(Utilities.ReturnSuccess(data: vwTimeLine));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// MUDA A SITUAÇÃO DA ETAPA - Aguardando confirmação = 0 - Confirmado = 1 - Aguardando reagendamento = 2 - Reagendado = 3 - Finalizado = 4
        /// </summary>
        /// <remarks>
        ///OBJ DE ENVIO
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("ChangeStatus")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ChangeStatus([FromBody] ScheduleChangeStatusViewModel model)
        {

            try
            {

                var isInvalidState = ModelState.ValidModelStateOnlyFields(nameof(model.Date), nameof(model.Description), nameof(model.FamilyId), nameof(model.Place));
                if (isInvalidState != null)
                    return BadRequest(isInvalidState);


                var dateToSchedule = Utilities.TimeStampToDateTime(model.Date);
                if (dateToSchedule < DateTime.Now)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.DateInvalidToSchedule));

                var family = await _familyRepository.FindByIdAsync(model.FamilyId).ConfigureAwait(false);
                if (family == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));


                var scheduleEntity = await _scheduleRepository.FindByIdAsync(model.Id).ConfigureAwait(false); ;
                scheduleEntity.Date = model.Date;
                scheduleEntity.TypeScheduleStatus = model.TypeScheduleStatus;
                scheduleEntity.Description = model.Description;
                scheduleEntity.Place = model.Place;

                if (model.TypeScheduleStatus == TypeScheduleStatus.Reagendado)
                    scheduleEntity.TypeScheduleStatus = TypeScheduleStatus.AguardandoConfirmacao;
                await _scheduleRepository.UpdateAsync(scheduleEntity).ConfigureAwait(false);

                var scheduleHistoryList = await _scheduleHistoryRepository.FindByAsync(x => x.TypeSubject == scheduleEntity.TypeSubject && x.FamilyId == model.FamilyId).ConfigureAwait(false) as List<ScheduleHistory>;
                var scheduleHistoryEntity = _mapper.Map<ScheduleHistory>(model);
                scheduleHistoryEntity.HolderCpf = family.Holder.Cpf;
                scheduleHistoryEntity.HolderName = family.Holder.Name;
                scheduleHistoryEntity.HolderNumber = family.Holder.Number;
                scheduleHistoryEntity.TypeSubject = scheduleEntity.TypeSubject;
                scheduleHistoryEntity.TypeScheduleStatus = model.TypeScheduleStatus;
                scheduleHistoryEntity.ScheduleId = scheduleEntity._id.ToString();
                scheduleHistoryEntity.ParentId = scheduleHistoryList?.LastOrDefault()?._id.ToString();
                scheduleHistoryEntity._id = new ObjectId();
                await _scheduleHistoryRepository.CreateAsync(scheduleHistoryEntity).ConfigureAwait(false);


                if (model.TypeScheduleStatus == TypeScheduleStatus.AguardandoReagendamento)
                {
                    
                    var title = "Solicitação de reagendamento";
                    var content = $"A famíla de número { family.Holder.Number } solicitou um reagendamento";

                    var listNotification = new List<Notification>();     

                    listNotification.Add(new Notification()
                    {
                        For = ForType.TTS,
                        FamilyId = null,
                        Title = title,
                        Description = content
                           
                    });                  

                    await _notificationRepository.CreateAsync(listNotification);

                    dynamic payloadPush = Util.GetPayloadPush();
                    dynamic settingPush = Util.GetSettingsPush();

                    await _senderNotificationService.SendPushAsync(title, content, null, data: payloadPush, settings: settingPush, priority: 10);
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
        /// MUDA A SITUAÇÃO DA ETAPA DOS PROCESSOS DE REASSENTAMENTO  - ReuniaoPGM = 2 - EscolhaDoImovel = 4 - Mudanca = 7 - AcompanhamentoPosMudança = 8
        /// </summary>
        /// <remarks>
        ///OBJ DE ENVIO
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("ChangeSubject")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ChangeSubject([FromBody] ScheduleChangeSubjectViewModel model)
        {

            try
            {
                //Reunião PGM -  ReuniaoPGM = 2,
                //Escolha do imóvel -  EscolhaDoImovel = 7,
                //Mudança -    Mudanca = 4,
                //Acompanhamento pós-mudança -  AcompanhamentoPosMudança = 8
                List<int> enumsValid = new List<int> { (int)TypeSubject.ReuniaoPGM, (int)TypeSubject.EscolhaDoImovel, (int)TypeSubject.Mudanca, (int)TypeSubject.AcompanhamentoPosMudança };
                if (!enumsValid.Contains((int)model.TypeSubject))
                    return BadRequest(Utilities.ReturnErro("Tipo de Assunto inválido! Deve-se utilizar(Reunião Pgm - Escolha do Imóvel - Mudança - Acompanhamento pós mudança)"));


                var isInvalidState = ModelState.ValidModelStateOnlyFields(nameof(model.Date), nameof(model.Description), nameof(model.FamilyId), nameof(model.Place));
                if (isInvalidState != null)
                    return BadRequest(isInvalidState);


                var dateToSchedule = Utilities.TimeStampToDateTime(model.Date);
                if (dateToSchedule < DateTime.Today)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.DateInvalidToSchedule));

                var family = await _familyRepository.FindByIdAsync(model.FamilyId).ConfigureAwait(false);
                if (family == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));


                var scheduleEntity = await _scheduleRepository.FindByIdAsync(model.Id).ConfigureAwait(false);

                if (scheduleEntity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ScheduleNotFound));

                if (scheduleEntity.TypeScheduleStatus != TypeScheduleStatus.Finalizado)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ChangeSubject));

                scheduleEntity.Date = model.Date;
                scheduleEntity.Description = model.Description;
                scheduleEntity.Place = model.Place;
                scheduleEntity.TypeSubject = model.TypeSubject;
                scheduleEntity.TypeScheduleStatus = TypeScheduleStatus.AguardandoConfirmacao;
                await _scheduleRepository.UpdateAsync(scheduleEntity).ConfigureAwait(false);


                var scheduleHistoryList = await _scheduleHistoryRepository.FindByAsync(x => x.TypeSubject == scheduleEntity.TypeSubject && x.FamilyId == model.FamilyId).ConfigureAwait(false) as List<ScheduleHistory>;
                if (scheduleHistoryList.Exists(x => x.TypeSubject == TypeSubject.EscolhaDoImovel || x.TypeSubject == TypeSubject.Mudanca || x.TypeSubject == TypeSubject.AcompanhamentoPosMudança))
                {
                    //var c = scheduleHistoryList.Where(x => x.TypeSubject == scheduleEntity.TypeSubject);
                    await _scheduleHistoryRepository.UpdateOneAsync(scheduleHistoryList.FirstOrDefault()).ConfigureAwait(false);
                }
                else
                {
                    model.Id = null;
                    var scheduleHistoryEntity = _mapper.Map<ScheduleHistory>(model);
                    scheduleHistoryEntity.HolderCpf = family.Holder.Cpf;
                    scheduleHistoryEntity.HolderName = family.Holder.Name;
                    scheduleHistoryEntity.HolderNumber = family.Holder.Number;
                    scheduleHistoryEntity.TypeScheduleStatus = scheduleEntity.TypeScheduleStatus;
                    scheduleHistoryEntity.ScheduleId = scheduleEntity._id.ToString();
                    await _scheduleHistoryRepository.CreateAsync(scheduleHistoryEntity).ConfigureAwait(false);
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


        public async Task<IActionResult> Export([FromForm] DtParameters model, [FromForm] string number, [FromForm] string name, [FromForm] string cpf, [FromForm] long? startDate, [FromForm] long? endDate, [FromForm] string place, [FromForm] string description, [FromForm] TypeScheduleStatus? status, [FromForm] TypeSubject? type)
        {
            var response = new DtResult<ScheduleExportViewModel>();
            try
            {
                var builder = Builders<Data.Entities.Schedule>.Filter;
                var conditions = new List<FilterDefinition<Data.Entities.Schedule>>();

                conditions.Add(builder.Where(x => x.Created != null && x.Disabled == null));

                if (!string.IsNullOrEmpty(number))
                    conditions.Add(builder.Where(x => x.HolderNumber == number));
                if (!string.IsNullOrEmpty(name))
                    conditions.Add(builder.Where(x => x.HolderName.ToUpper().Contains(name.ToUpper())));
                if (!string.IsNullOrEmpty(cpf))
                    conditions.Add(builder.Where(x => x.HolderCpf == cpf.OnlyNumbers()));
                if (startDate.HasValue)
                    conditions.Add(builder.Where(x => x.Date >= startDate));
                if (endDate.HasValue)
                    conditions.Add(builder.Where(x => x.Date <= endDate));
                if (!string.IsNullOrEmpty(place))
                    conditions.Add(builder.Where(x => x.Place.ToUpper().Contains(place.ToUpper())));
                if (!string.IsNullOrEmpty(description))
                    conditions.Add(builder.Where(x => x.Description.ToUpper().Contains(description.ToUpper())));

                if (status != null)
                    conditions.Add(builder.Where(x => x.TypeScheduleStatus == status));

                if (type != null)
                    conditions.Add(builder.Where(x => x.TypeSubject == type));


                var columns = model.Columns.Where(x => x.Searchable && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToArray();

                var sortColumn = !string.IsNullOrEmpty(model.SortOrder) ? model.SortOrder.UppercaseFirst() : model.Columns.FirstOrDefault(x => x.Orderable)?.Name ?? model.Columns.FirstOrDefault()?.Name;
                var totalRecords = (int)await _scheduleRepository.GetCollectionAsync().CountDocumentsAsync(builder.And(conditions));

                var sortBy = model.Order[0].Dir.ToString().ToUpper().Equals("DESC")
                    ? Builders<Data.Entities.Schedule>.Sort.Descending(sortColumn)
                    : Builders<Data.Entities.Schedule>.Sort.Ascending(sortColumn);

                var allData = await _scheduleRepository
                    .LoadDataTableAsync(model.Search.Value, sortBy, model.Start, totalRecords, conditions, columns);

                var condition = builder.And(conditions);
                var fileName = "Agendamentos_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";

                var path = Path.Combine($"{Directory.GetCurrentDirectory()}\\", @"ExportFiles");
                if (Directory.Exists(path) == false)
                    Directory.CreateDirectory(path);

                var fullPathFile = Path.Combine(path, fileName);
                var listViewModel = _mapper.Map<List<ScheduleExportViewModel>>(allData);
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
        /// EXPORTAR DADOS MAPA DE DESLOCAMENTO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("ExportMap")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]


        public async Task<IActionResult> ExportMap([FromForm] DtParameters model, [FromForm] string number)
        {
            
            try
            {
                var builder = Builders<Schedule>.Filter;
                var conditions = new List<FilterDefinition<Schedule>>();

                conditions.Add(builder.Where(x => x.Created != null && x.Disabled == null && x.TypeSubject == TypeSubject.Mudanca));

                if (!string.IsNullOrEmpty(number))
                    conditions.Add(builder.Where(x => x.HolderNumber == number));

                var columns = model.Columns.Where(x => x.Searchable && string.IsNullOrEmpty(x.Name) == false).Select(x => x.Name).ToArray();

                var sortColumn = model.SortOrder;
                var totalRecords = (int)await _scheduleRepository.GetCollectionAsync().CountDocumentsAsync(builder.And(conditions));

                var sortBy = model.Order[0].Dir.ToString().ToUpper().Equals("DESC")
                   ? Builders<Schedule>.Sort.Descending(sortColumn)
                   : Builders<Schedule>.Sort.Ascending(sortColumn);

                var retorno = await _scheduleRepository
                 .LoadDataTableAsync(model.Search.Value, sortBy, 0, 0, conditions, columns) as List<Schedule>;
                               

                var response = _mapper.Map<List<ScheduleMapExportViewModel>>(retorno);


                for (int i = 0; i < retorno.Count(); i++)
                {
                    var item = retorno[i];

                    var family = await _familyRepository.FindByIdAsync(item.FamilyId).ConfigureAwait(false);

                    var infoOrigin = await _utilService.GetInfoFromZipCode(family.Address.CEP).ConfigureAwait(false);
                    if (infoOrigin == null)
                        return BadRequest(Utilities.ReturnErro("Cep não encontrado"));

                    var residencialOrigin = Utilities.GetInfoFromAdressLocation(infoOrigin.StreetAddress + " " + infoOrigin.Complement + " " + infoOrigin.Neighborhood + " " + infoOrigin.CityName + " " + infoOrigin.StateUf);
                    if (residencialOrigin.Erro == true)
                        return BadRequest(Utilities.ReturnErro(DefaultMessages.LocationNotFound));


                    var propertyInterest = await _propertiesInterestRepository.FindByAsync(x => x.FamilyId == item.FamilyId).ConfigureAwait(false);

                    if (propertyInterest == null)
                        return BadRequest(Utilities.ReturnErro(DefaultMessages.PropertyNotChosen));

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

                    response[i].AddressPropertyDistanceMeters = distanceM.ToString("F");
                    response[i].AddressPropertyDistanceKilometers = distanceK.ToString("0.##");
                    response[i].AddressPropertyOrigin = residencialOrigin.FormatedAddress;
                    response[i].AddressPropertyDestination = destination.FormatedAddress;

                }


                var fileName = "Mapa de deslocamento.xlsx";
                var path = Path.Combine($"{Directory.GetCurrentDirectory()}/Content", @"ExportFiles");
                if (Directory.Exists(path) == false)
                    Directory.CreateDirectory(path);

                var fullPathFile = Path.Combine(path, fileName);

                Utilities.ExportToExcel(response, path, "Mapa", fileName.Split('.')[0]);
                if (System.IO.File.Exists(fullPathFile) == false)
                    return BadRequest(Utilities.ReturnErro("Ocorreu um erro fazer download do arquivo"));

                var fileBytes = System.IO.File.ReadAllBytes(fullPathFile);
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);


            }
            catch (Exception ex)
            {
                return BadRequest(Utilities.ReturnErro(ex.Message));
            }
        }
    }
}