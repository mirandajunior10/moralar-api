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
using Moralar.Domain.ViewModels.Question;
using Moralar.Domain.ViewModels.Quiz;
using Moralar.Domain.ViewModels.Schedule;
using Moralar.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
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

    public class ScheduleController : Controller
    {

        private readonly IMapper _mapper;
        private readonly IFamilyRepository _familyRepository;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IScheduleHistoryRepository _scheduleHistoryRepository;
        private readonly IUtilService _utilService;
        private readonly ISenderMailService _senderMailService;

        public ScheduleController(IMapper mapper, IFamilyRepository familyRepository, IScheduleRepository scheduleRepository, IScheduleHistoryRepository scheduleHistoryRepository, IUtilService utilService, ISenderMailService senderMailService)
        {
            _mapper = mapper;
            _familyRepository = familyRepository;
            _scheduleRepository = scheduleRepository;
            _scheduleHistoryRepository = scheduleHistoryRepository;
            _utilService = utilService;
            _senderMailService = senderMailService;
        }




        /// <summary>
        /// DETALHES DA FAMÍLIA
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("GetHistoryByFamily")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetHistoryByFamily()
        {
            try
            {
                var userId = Request.GetUserId();
                var entity = await _scheduleRepository.FindByAsync(x => x.FamilyId == userId).ConfigureAwait(false);

                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.CourseNotFound));

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
        [AllowAnonymous]
        public async Task<IActionResult> LoadData([FromForm] DtParameters model, [FromForm] string number, [FromForm] string name, [FromForm] string cpf, [FromForm] long? startDate, [FromForm] long? endDate, [FromForm] string place, [FromForm] string description, [FromForm] TypeScheduleStatus ?status, [FromForm] TypeSubject typeSubject)
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

        ///// <summary>
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
        [AllowAnonymous]
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
        /// REGISTRAR UM NOVO QUESTIONÁRIO
        /// </summary>
        /// <remarks>
        ///OBJ DE ENVIO
        ///
        ///        POST
        ///        {
        ///          "title": "string",
        ///           "typeQuiz": 0 = Questionário, 1 = Enquete,
        ///          "questionRegister": {
        ///            "question": [
        ///              {
        ///                "nameQuestion": "string",
        ///                "typeResponse": "Texto", Enum Texto = 0,MultiplaEscolha = 1,EscolhaUnica = 2,ListaSuspensa = 3
        ///                "description": [
        ///                  {
        ///                    "description": "string"
        ///                  }
        ///                ]
        ///              }
        ///            ]
        ///          }
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


                var scheduleEntity = _mapper.Map<Schedule>(model);
                scheduleEntity.HolderCpf = family.Holder.Cpf;
                scheduleEntity.HolderName = family.Holder.Name;
                scheduleEntity.HolderNumber = family.Holder.Number;
                scheduleEntity.TypeSubject = model.TypeSubject;
                scheduleEntity.TypeScheduleStatus = TypeScheduleStatus.AguardandoConfirmacao;


                await _scheduleRepository.CreateAsync(scheduleEntity).ConfigureAwait(false);


                var scheduleHistoryEntity = _mapper.Map<ScheduleHistory>(model);
                await _scheduleHistoryRepository.CreateAsync(scheduleHistoryEntity).ConfigureAwait(false);

                var dataBody = Util.GetTemplateVariables();


                dataBody.Add("{{ title }}", $"Olá {family.Holder.Name.GetFirstName()}!");
                dataBody.Add("{{ message }}", $"<p>Sua agenda { Utilities.ToEnum<TypeSubject>(model.TypeSubject.ToString())} foi marcada</p>" +
                                            $"<p>Dia { Utilities.TimeStampToDateTime(scheduleEntity.Date).ToString("dd/MM/yyyy")}, horário {Utilities.TimeStampToDateTime(scheduleEntity.Date).ToString("HH:mm")} , endereço {{xxx}}</p>" +
                                            $"Aguardamos você!"
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
        /// REGISTRAR UM NOVO QUESTIONÁRIO
        /// </summary>
        /// <remarks>
        ///OBJ DE ENVIO
        ///
        ///        POST
        ///        {
        ///          "title": "string",
        ///           "typeQuiz": 0 = Questionário, 1 = Enquete,
        ///          "questionRegister": {
        ///            "question": [
        ///              {
        ///                "nameQuestion": "string",
        ///                "typeResponse": "Texto", Enum Texto = 0,MultiplaEscolha = 1,EscolhaUnica = 2,ListaSuspensa = 3
        ///                "description": [
        ///                  {
        ///                    "description": "string"
        ///                  }
        ///                ]
        ///              }
        ///            ]
        ///          }
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
        [AllowAnonymous]
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
        /// REGISTRAR UM NOVO QUESTIONÁRIO
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
        [AllowAnonymous]
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

                model.Id = null;
                var scheduleHistoryEntity = _mapper.Map<ScheduleHistory>(model);
                scheduleHistoryEntity.HolderCpf = family.Holder.Cpf;
                scheduleHistoryEntity.HolderName = family.Holder.Name;
                scheduleHistoryEntity.HolderNumber = family.Holder.Number;
                scheduleHistoryEntity.TypeScheduleStatus = model.TypeScheduleStatus;
                scheduleHistoryEntity.ScheduleId = scheduleEntity._id.ToString();
                await _scheduleHistoryRepository.CreateAsync(scheduleHistoryEntity).ConfigureAwait(false);


                return Ok(Utilities.ReturnSuccess(data: "Registrado com sucesso!"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Question, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível cadastrar/atualizar novo questionário", "", "", "", "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }
        ///// <summary>
        ///// MUDAR O ASSUNTO DA
        ///// </summary>
        ///// <remarks>
        /////OBJ DE ENVIO
        ///// </remarks>
        ///// <response code="200">Returns success</response>
        ///// <response code="400">Custom Error</response>
        ///// <response code="401">Unauthorize Error</response>
        ///// <response code="500">Exception Error</response>
        ///// <returns></returns>
        //[HttpPost("ChangeSubject")]
        //[Produces("application/json")]
        //[ProducesResponseType(typeof(ReturnViewModel), 200)]
        //[ProducesResponseType(400)]
        //[ProducesResponseType(401)]
        //[ProducesResponseType(500)]
        //[AllowAnonymous]
        //public async Task<IActionResult> ChangeSubject([FromBody] ScheduleChangeSubjectViewModel model)
        //{

        //    try
        //    {
        //        var isInvalidState = ModelState.ValidModelStateOnlyFields(nameof(model.Date), nameof(model.Description), nameof(model.FamilyId), nameof(model.Place));
        //        if (isInvalidState != null)
        //            return BadRequest(isInvalidState);


        //        var dateToSchedule = Utilities.TimeStampToDateTime(model.Date);
        //        if (dateToSchedule < DateTime.Now)
        //            return BadRequest(Utilities.ReturnErro(DefaultMessages.DateInvalidToSchedule));

        //        var family = await _familyRepository.FindByIdAsync(model.FamilyId).ConfigureAwait(false);
        //        if (family == null)
        //            return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));


        //        var scheduleEntity = _mapper.Map<Schedule>(model);
        //        scheduleEntity.HolderCpf = family.Holder.Cpf;
        //        scheduleEntity.HolderName = family.Holder.Name;
        //        scheduleEntity.HolderNumber = family.Holder.Number;
        //        scheduleEntity.TypeScheduleStatus = model.TypeSubject;
        //        return Ok(Utilities.ReturnSuccess(data: "Registrado com sucesso!"));
        //    }
        //    catch (Exception ex)
        //    {
        //        await _utilService.RegisterLogAction(LocalAction.Question, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível cadastrar/atualizar novo questionário", "", "", "", "", ex);
        //        return BadRequest(ex.ReturnErro());
        //    }
        //}


    }

}