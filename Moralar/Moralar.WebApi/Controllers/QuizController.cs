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

    public class QuizController : Controller
    {

        private readonly IMapper _mapper;
        private readonly IQuestionRepository _questionRepository;
        private readonly IQuizRepository _quizRepository;
        private readonly IQuestionDescriptionRepository _questionDescriptionRepository;
        private readonly IQuizFamilyRepository _quizFamilyRepository;
        private readonly IFamilyRepository _familyRepository;
        private readonly IUtilService _utilService;

        public QuizController(IMapper mapper, IQuestionRepository questionRepository, IQuizRepository quizRepository, IQuestionDescriptionRepository questionDescriptionRepository, IQuizFamilyRepository quizFamilyRepository, IFamilyRepository familyRepository, IUtilService utilService)
        {
            _mapper = mapper;
            _questionRepository = questionRepository;
            _quizRepository = quizRepository;
            _questionDescriptionRepository = questionDescriptionRepository;
            _quizFamilyRepository = quizFamilyRepository;
            _familyRepository = familyRepository;
            _utilService = utilService;
        }

        /// <summary>
        /// BLOQUEAR / DESBLOQUEAR QUIZ
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

                var entity = await _quizRepository.FindByIdAsync(model.TargetId);

                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.QuizNotFound));
                entity.DataBlocked = model.Block ? DateTimeOffset.Now.ToUnixTimeSeconds() : (long?)null;

                await _quizRepository.UpdateAsync(entity);
                var typeAction = model.Block == true ? TypeAction.Block : TypeAction.UnBlock;
                await _utilService.RegisterLogAction(LocalAction.Familia, typeAction, TypeResposible.UserAdminstratorGestor, $"Bloqueio de família {Request.GetUserName()}", Request.GetUserId(), Request.GetUserName().Value, model.TargetId);
                return Ok(Utilities.ReturnSuccess(model.Block ? "Bloqueado com sucesso" : "Desbloqueado com sucesso"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível bloquer Família", Request.GetUserId(), Request.GetUserName().Value, model.TargetId, "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }


        /// <summary>
        /// LISTA TODOS OS QUESTIONÁRIOS
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("GetByName")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<IActionResult> GetByName(string nameQuiz)
        {
            try
            {
                var conditions = new List<FilterDefinition<Data.Entities.Quiz>>();
                var builder = Builders<Data.Entities.Quiz>.Filter;
                conditions.Add(builder.Where(x => x.Created != null && x._id != null));
                if (!string.IsNullOrEmpty(nameQuiz))
                    conditions.Add(builder.Where(x => x.Title.ToUpper().Contains(nameQuiz.ToUpper())));
                var condition = builder.And(conditions);


                var entityQuiz = await _quizRepository.GetCollectionAsync().FindSync(condition, new FindOptions<Data.Entities.Quiz>() { }).ToListAsync();
                if (entityQuiz == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.QuizNotFound));

                return Ok(Utilities.ReturnSuccess(data: _mapper.Map<List<QuizViewModel>>(entityQuiz)));
            }
            catch (Exception ex)
            {
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
        [AllowAnonymous]
        public async Task<IActionResult> Export([FromForm] string nameQuiz)
        {



            var response = new DtResult<QuizExportViewModel>();
            try
            {

                var conditions = new List<FilterDefinition<Data.Entities.Quiz>>();
                var builder = Builders<Data.Entities.Quiz>.Filter;
                conditions.Add(builder.Where(x => x.Created != null && x._id != null));
                if (!string.IsNullOrEmpty(nameQuiz))
                    conditions.Add(builder.Where(x => x.Title.ToUpper().Contains(nameQuiz.ToUpper())));
                var condition = builder.And(conditions);
                var fileName = "Questionario_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";
                var allData = await _quizRepository.GetCollectionAsync().FindSync(condition, new FindOptions<Data.Entities.Quiz>() { }).ToListAsync();

                var path = Path.Combine($"{Directory.GetCurrentDirectory()}\\", @"ExportFiles");
                if (Directory.Exists(path) == false)
                    Directory.CreateDirectory(path);

                var fullPathFile = Path.Combine(path, fileName);
                var listViewModel = _mapper.Map<List<QuizExportViewModel>>(allData);
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
        /// LISTAGEM DOS QUESTINÁRIOS DATATABLE
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
        public async Task<IActionResult> LoadData([FromForm] DtParameters model, [FromForm] string title)
        {
            var response = new DtResult<QuizViewModel>();
            try
            {
                var builder = Builders<Data.Entities.Quiz>.Filter;
                var conditions = new List<FilterDefinition<Data.Entities.Quiz>>();

                conditions.Add(builder.Where(x => x.Created != null && x.Disabled == null));
                if (!string.IsNullOrEmpty(title))
                    conditions.Add(builder.Where(x => x.Title == title));


                var columns = model.Columns.Where(x => x.Searchable && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToArray();

                var sortColumn = !string.IsNullOrEmpty(model.SortOrder) ? model.SortOrder.UppercaseFirst() : model.Columns.FirstOrDefault(x => x.Orderable)?.Name ?? model.Columns.FirstOrDefault()?.Name;
                var totalRecords = (int)await _quizRepository.GetCollectionAsync().CountDocumentsAsync(builder.And(conditions));

                var sortBy = model.Order[0].Dir.ToString().ToUpper().Equals("DESC")
                    ? Builders<Data.Entities.Quiz>.Sort.Descending(sortColumn)
                    : Builders<Data.Entities.Quiz>.Sort.Ascending(sortColumn);

                var retorno = await _quizRepository
                    .LoadDataTableAsync(model.Search.Value, sortBy, model.Start, model.Length, conditions, columns);

                var totalrecordsFiltered = !string.IsNullOrEmpty(model.Search.Value)
                    ? (int)await _quizRepository.CountSearchDataTableAsync(model.Search.Value, conditions, columns)
                    : totalRecords;

                response.Data = _mapper.Map<List<QuizViewModel>>(retorno);
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
        [AllowAnonymous]
        public async Task<IActionResult> Detail([FromRoute] string id)
        {
            try
            {
                if (ObjectId.TryParse(id, out var unused) == false)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredencials));

                var userId = Request.GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredencials));

                var entity = await _quizRepository.FindByIdAsync(id).ConfigureAwait(false);

                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.QuizNotFound));

                return Ok(Utilities.ReturnSuccess(data: _mapper.Map<QuizViewModel>(entity)));
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
        ///          "questionRegister": {
        ///            "question": [
        ///              {
        ///                "nameQuestion": "string",
        ///                "typeResponse": "Texto",
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
        public async Task<IActionResult> Register([FromBody] QuizViewModel model)
        {

            try
            {
                //var typeRegister = string.IsNullOrEmpty(model.Id) ? TypeAction.Register : TypeAction.Change;
                //var message = (typeRegister == TypeAction.Register) ? $"Cadastro de novo Questinário {Request.GetUserName()}" : $"Atualização do Questinário {Request.GetUserName()}";
                var ignoreValidation = new List<string>();
                var isInvalidState = ModelState.ValidModelState(ignoreValidation.ToArray());
                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                var quizEntity = _mapper.Map<Quiz>(model);
                var quizEntityId = await _quizRepository.CreateAsync(quizEntity).ConfigureAwait(false);

                List<string> itensAdded = new List<string>();
                foreach (var item in model.QuestionRegister.Question)
                {
                    var entity = _mapper.Map<Question>(item);
                    entity.QuizId = quizEntityId;
                    var itemAdded = await _questionRepository.CreateAsync(entity).ConfigureAwait(false);

                    foreach (var itemDescription in item.Description)
                    {
                        var description = new QuestionDescription()
                        {
                            Description = itemDescription.Description,
                            QuestionId = itemAdded
                        };
                        await _questionDescriptionRepository.CreateAsync(description).ConfigureAwait(false);
                    }
                    itensAdded.Add(itemAdded);
                }
                //await _utilService.RegisterLogAction(LocalAction.Question, typeRegister, TypeResposible.UserAdminstratorGestor, message, Request.GetUserId(), Request.GetUserName().Value, string.Join(";", itensAdded.ToArray()), "");

                return Ok(Utilities.ReturnSuccess(data: "Registrado com sucesso!"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Question, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível cadastrar/atualizar novo questionário", "", "", "", "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// APLICA UM QUESTIONÁRIO A UMA DETERMINADA FAMÍLIA OU A TODAS FAMÍLIA
        /// </summary>
        /// <remarks>
        ///OBJ DE ENVIO
        ///
        /// 
        ///         POST - TODAS FAMÍLIAS
        ///         {
        ///          "quizId": "string",
        ///          "allFamily": true // SE DEIXAR TRUE VAI SER ADICIONÁRIO A TODAS FAMÍLIAS
        ///         }
        ///         POST - IDENTIFICANDO AS FAMÍLIAS
        ///         {
        ///          "quizId": "string",
        ///          "allFamily": false, // PRECISA IDENTIFICAR AS FAMÍLIAS ESCOLHIDAS PARA RESPONDER O QUESTIONÁRIO
        ///          "familyId": [
        ///            "string"
        ///          ]
        ///         }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("RegisterQuizToFamily")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterQuizToFamily([FromBody] QuizFamilyViewModel model)
        {

            try
            {
                var existQuiz = await _quizRepository.CheckByAsync(x => x._id == ObjectId.Parse(model.QuizId)).ConfigureAwait(false);
                if (existQuiz == false)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.QuizNotFound));

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
                    var entityQuizFamily = new QuizFamily()
                    {
                        Created = DateTimeOffset.Now.ToUnixTimeSeconds(),
                        FamilyId = item._id.ToString(),
                        QuizId = model.QuizId,
                        HolderName = item.Holder.Name,
                        HolderCpf = item.Holder.Cpf,
                        HolderNumber = item.Holder.Number,
                        TypeStatusQuiz = TypeStatusQuiz.NaoRespondido
                    };
                    if (await _quizFamilyRepository.CheckByAsync(x => x.QuizId == model.QuizId && x.FamilyId == item._id.ToString()) == false)
                        await _quizFamilyRepository.CreateAsync(entityQuizFamily).ConfigureAwait(false);
                }
                ///TODO
                /// ENVIAR MSG PARA O APP DO CLIENTE


                //await _utilService.RegisterLogAction(LocalAction.Question, typeRegister, TypeResposible.UserAdminstratorGestor, message, Request.GetUserId(), Request.GetUserName().Value, string.Join(";", itensAdded.ToArray()), "");



                return Ok(Utilities.ReturnSuccess(data: "Registrado com sucesso!"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Question, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível cadastrar/atualizar novo questionário", "", "", "", "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }
        /// <summary>
        /// LISTA OS QUESTIONÁRIOS DISPONÍVEIS PARA FAMÍLIA
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("GetQuizByFamily")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetQuizByFamily()
        {
            try
            {
                var userId = Request.GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredencials));

                var quizFamilies = await _quizFamilyRepository.FindByAsync(x => x.FamilyId == userId).ConfigureAwait(false);


                var entityFamily = await _quizRepository.FindIn("_id", quizFamilies.Select(x => ObjectId.Parse(x.QuizId)).ToList()) as List<Quiz>;
                if (quizFamilies == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.QuizNotFound));

                return Ok(Utilities.ReturnSuccess(data: _mapper.Map<List<QuizViewModel>>(entityFamily)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }
        /// <summary>
        /// LISTA OS QUESTIONÁRIOS DISPONÍVEIS PARA FAMÍLIA
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("QuizAvailable")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<IActionResult> QuizAvailable(string number, string name, string cpf)
        {
            try
            {
                var builder = Builders<Data.Entities.QuizFamily>.Filter;
                var conditions = new List<FilterDefinition<Data.Entities.QuizFamily>>();

                conditions.Add(builder.Where(x => x.Created != null));
                if (!string.IsNullOrEmpty(number))
                    conditions.Add(builder.Where(x => x.HolderNumber.ToUpper() == number.ToUpper()));
                if (!string.IsNullOrEmpty(name))
                    conditions.Add(builder.Where(x => x.HolderName.ToUpper().Contains(name.ToUpper())));
                if (!string.IsNullOrEmpty(cpf))
                    conditions.Add(builder.Where(x => x.HolderCpf == cpf));


                var condition = builder.And(conditions);
                var entity = await _quizFamilyRepository.GetCollectionAsync().FindSync(condition, new FindOptions<Data.Entities.QuizFamily>() { }).ToListAsync();
                if (entity.Count() == 0)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.QuizNotFound));

                return Ok(Utilities.ReturnSuccess(data: _mapper.Map<List<QuizFamilyListViewModel>>(entity)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }


    }

}