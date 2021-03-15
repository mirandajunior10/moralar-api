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
using System.ComponentModel;
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
    [DisplayName("QuestionController - Comandos para manipulação das fasdfas")]

    public class QuestionController : Controller
    {

        private readonly IMapper _mapper;
        private readonly IQuestionRepository _questionRepository;
        private readonly IQuizRepository _quizRepository;
        private readonly IQuestionDescriptionRepository _questionDescriptionRepository;
        private readonly IQuizFamilyRepository _quizFamilyRepository;
        private readonly IFamilyRepository _familyRepository;
        private readonly IUtilService _utilService;
        [DisplayName("QuestionController - Comandos para manipulação das questões")]
        public QuestionController(IMapper mapper, IQuestionRepository questionRepository, IQuizRepository quizRepository, IQuestionDescriptionRepository questionDescriptionRepository, IQuizFamilyRepository quizFamilyRepository, IFamilyRepository familyRepository, IUtilService utilService)
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
        /// BLOQUEAR / DESBLOQUEAR QUESTÃO
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
        public async Task<IActionResult> BlockUnBlockQuestion([FromBody] BlockViewModel model)
        {
            try
            {
                model.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelStateOnlyFields(nameof(model.TargetId));

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                var entity = await _questionRepository.FindByIdAsync(model.TargetId);

                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.QuestionNotFound));
                entity.DataBlocked = model.Block ? DateTimeOffset.Now.ToUnixTimeSeconds() : (long?)null;

                await _questionRepository.UpdateAsync(entity);
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
        /// INSERIR NOVA QUESTÃO
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///POST
        ///{
        ///  "id": "601955ee7d765d74ec11a33a",// ID DO QUIZ
        ///  "questionRegister": {
        ///    "question": [
        ///      {
        ///        "nameQuestion": "teste nova questão",
        ///        "typeResponse": 1,
        ///        "description": [
        ///          {
        ///            "description": "Idade 5 anos"
        ///          },
        ///          {
        ///            "description": "Idade 6 anos"
        ///          },
        ///        ]
        ///      }
        ///    ]
        ///  }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("RegisterNewQuestion")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> RegisterNewQuestion([FromBody] QuizNewQuestionViewModel model)
        {

            try
            {
                var typeRegister = TypeAction.Register;
                var message = $"Registro de nova questão {Request.GetUserName()}";

                var ignoreValidation = new List<string>();
                var isInvalidState = ModelState.ValidModelState(ignoreValidation.ToArray());
                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                var entityQuestion = await _quizRepository.FindByIdAsync(model.Id).ConfigureAwait(false);
                if (entityQuestion == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.QuizNotFound));

                List<string> itensAdded = new List<string>();
                foreach (var item in model.QuestionRegister.Question)
                {
                    var entity = _mapper.Map<Question>(item);
                    entity.QuizId = model.Id;
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
                //var entityAddNewQuestion = _mapper.Map<Question>(model.Question);
                //entityAddNewQuestion.NameTitle = model.NameTitle;







                //List<string> itensAdded = new List<string>();
                //foreach (var item in model.Question)
                //{

                //    var entity = _mapper.Map<Question>(item);
                //    item.NameTitle = model.NameTitle;
                //    var itemAdded = await _questionRepository.UpdateOneAsync(entity).ConfigureAwait(false);

                //    foreach (var itemDescription in item.Description)
                //    {
                //        var description = new QuestionDescription()
                //        {
                //            Description = itemDescription.Description,
                //            QuestionId = itemAdded
                //        };
                //        await _questionDescriptionRepository.CreateAsync(description).ConfigureAwait(false);
                //    }
                //    itensAdded.Add(itemAdded);
                //}
                //await _utilService.RegisterLogAction(LocalAction.Question, typeRegister, TypeResposible.UserAdminstratorGestor, message, Request.GetUserId(), Request.GetUserName().Value, string.Join(";", itensAdded.ToArray()), "");
                return Ok(Utilities.ReturnSuccess(data: "Registrado com sucesso!"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Question, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível cadastrar/atualizar novo questionário", "", "", "", "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }

        [HttpGet("GetQuestionByIdToResponse")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<IActionResult> GetQuestionByIdToResponse(string id)
        {
            try
            {
                //    if (ObjectId.TryParse(id, out var unused) == false)
                //        return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredencials));

                //    var userId = Request.GetUserId();

                //    if (string.IsNullOrEmpty(userId))
                //        return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredencials));

                var entityQuestion = await _questionRepository.FindOneByAsync(x => x._id == ObjectId.Parse(id) && x.Disabled == null).ConfigureAwait(false) ;
                if (entityQuestion == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.QuestionNotFound));

                var entityDescription = await _questionDescriptionRepository.FindByAsync(x => x.QuestionId == id).ConfigureAwait(false) as List<QuestionDescription>;
                if (entityDescription.Count() == 0)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.DescriptionNotFound));

                var listQuestion = _mapper.Map<QuestionViewModel>(entityQuestion);
                listQuestion.Description = _mapper.Map<List<QuestionDescriptionViewModel>>(entityDescription);
             
                return Ok(Utilities.ReturnSuccess(data: listQuestion));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        [HttpPost("Delete")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Delete([FromBody] string id)
        {

            try
            {
                var typeRegister = TypeAction.Delete;
                var message = $"Deletada a questão {Request.GetUserName()}";

                var ignoreValidation = new List<string>();
                var isInvalidState = ModelState.ValidModelState(ignoreValidation.ToArray());
                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                var entityQuestion = await _questionRepository.FindByIdAsync(id).ConfigureAwait(false);
                if (entityQuestion == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.QuestionNotFound));

                var deleteQuestion = await _questionRepository.DisableOneAsync(id).ConfigureAwait(false);


                await _utilService.RegisterLogAction(LocalAction.Question, typeRegister, TypeResposible.UserAdminstratorGestor, message, Request.GetUserId(), Request.GetUserName().Value, id, "");
                return Ok(Utilities.ReturnSuccess(data: "Deletado com sucesso!"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Question, TypeAction.Delete, TypeResposible.UserAdminstratorGestor, $"Não foi possível deletar a questão", "", "", "", "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }


        /// <summary>
        /// CADASTRO | UPDATE DE UMA QUESTÃO
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         POST
        ///         {
        ///             "id": "601955ee7d765d74ec11a33a",
        ///             "title": "TEste alteração",
        ///             "question": 
        ///             [
        ///              {
        ///                "nameQuestion": "testeeee atualização",
        ///                "typeResponse": 1,
        ///                "description": 
        ///                [
        ///                     {
        ///                       "description": "aaaaaaaa",
        ///                       "id":"601955ee7d765d74ec11a33c"// para atualizar a descrição
        ///                     },{
        ///                       "description": "bbbbb" // para incluir, deve-se mandar sem o id
        ///                     },{
        ///                       "description": "cccccc"// para incluir, deve-se mandar sem o id
        ///                     }
        ///                 ],
        ///               "id":"601955ee7d765d74ec11a33b" //id da Questão
        ///              }
        ///            ]
        ///         }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("Update")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<IActionResult> Update([FromBody] QuizUpdateViewModel model)
        {

            try
            {
                //var typeRegister = string.IsNullOrEmpty(model.Id) ? TypeAction.Register : TypeAction.Change;
                //var message = (typeRegister == TypeAction.Register) ? $"Cadastro de novo Questinário {Request.GetUserName()}" : $"Atualização do Questinário {Request.GetUserName()}";
                var ignoreValidation = new List<string>();
                var isInvalidState = ModelState.ValidModelState(ignoreValidation.ToArray());
                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                var quizEntityId = await _quizRepository.FindByIdAsync(model.Id).ConfigureAwait(false);
                if (quizEntityId == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.QuizNotFound));

                var quizToModify = _mapper.Map<Quiz>(model);
                await _quizRepository.UpdateOneAsync(quizToModify).ConfigureAwait(false);


                var questionEntity = await _questionRepository.FindByAsync(x => x.QuizId == model.Id).ConfigureAwait(false) as List<Question>;
                if (questionEntity.Count() == 0)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.QuestionNotFound));


                foreach (var item in model.QuestionRegister.Question)
                {
                    var entity = _mapper.Map<Question>(item);
                    var findToUpdateOrIncludeQuestion = questionEntity.Find(x => x._id == ObjectId.Parse(item.Id));
                    if (findToUpdateOrIncludeQuestion != null)
                    {
                        entity.QuizId = model.Id;
                        await _questionRepository.UpdateAsync(entity).ConfigureAwait(false);

                        var questionDescription = await _questionDescriptionRepository.FindByAsync(x => x.QuestionId == entity._id.ToString()) as List<QuestionDescription>;
                        foreach (var itemDescription in item.Description)
                        {
                            if (!string.IsNullOrEmpty(itemDescription.Id))
                            {
                                var findToUpdateOrIncludeQuestionDescription = questionDescription.Find(x => x._id == ObjectId.Parse(itemDescription.Id));
                                if (findToUpdateOrIncludeQuestionDescription != null)
                                {
                                    var update = _mapper.Map<QuestionDescription>(itemDescription);
                                    update.QuestionId = findToUpdateOrIncludeQuestionDescription.QuestionId;
                                    await _questionDescriptionRepository.UpdateOneAsync(update).ConfigureAwait(false);
                                }
                            }
                            else
                            {
                                var create = _mapper.Map<QuestionDescription>(itemDescription);
                                create.QuestionId = entity._id.ToString();
                                await _questionDescriptionRepository.CreateAsync(create).ConfigureAwait(false);
                            }
                        }
                    }
                    else
                    {
                        await _questionRepository.CreateAsync(entity).ConfigureAwait(false);
                    }
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
    }
}