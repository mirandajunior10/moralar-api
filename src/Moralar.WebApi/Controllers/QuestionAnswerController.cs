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
using Moralar.Domain.ViewModels.QuestionAnswer;
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

    public class QuestionAnswerController : Controller
    {

        private readonly IMapper _mapper;
        private readonly IQuestionRepository _questionRepository;
        private readonly IQuizRepository _quizRepository;
        private readonly IQuestionDescriptionRepository _questionDescriptionRepository;
        private readonly IQuestionAnswerRepository _questionAnswerRepository;
        private readonly IQuizFamilyRepository _quizFamilyRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly IFamilyRepository _familyRepository;
        private readonly IUtilService _utilService;

        public QuestionAnswerController(IMapper mapper, IQuestionRepository questionRepository, IQuizRepository quizRepository, IQuestionDescriptionRepository questionDescriptionRepository, IQuestionAnswerRepository questionAnswerRepository, IQuizFamilyRepository quizFamilyRepository, IProfileRepository profileRepository, IFamilyRepository familyRepository, IUtilService utilService)
        {
            _mapper = mapper;
            _questionRepository = questionRepository;
            _quizRepository = quizRepository;
            _questionDescriptionRepository = questionDescriptionRepository;
            _questionAnswerRepository = questionAnswerRepository;
            _quizFamilyRepository = quizFamilyRepository;
            _profileRepository = profileRepository;
            _familyRepository = familyRepository;
            _utilService = utilService;
        }      

        /// <summary>
        /// BUSCA AS RESPOSTAS
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("GetResponses/{quizId}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetResponses([FromQuery] string quizId)
        {
            try
            {
                List<QuestionAnswerListViewModel> listAnswers = new List<QuestionAnswerListViewModel>();

                var quiz = await _quizRepository.FindByAsync(x => x._id.ToString() == quizId).ConfigureAwait(false) as List<Quiz>;
                var answers = await _questionAnswerRepository.FindAllAsync().ConfigureAwait(false) as List<QuestionAnswer>;
                //var question = await _questionRepository.FindIn("_id", answers.Select(x => ObjectId.Parse(x.Questions.QuestionId)).ToList()) as List<Question>;
                var questionDescription = await _questionDescriptionRepository.FindAllAsync().ConfigureAwait(false) as List<QuestionDescription>;
                for (int i = 0; i < answers.Count(); i++)
                {
                    var questionAnswerListViewModel = new QuestionAnswerListViewModel()
                    {
                        FamilyNumber = answers[i].FamilyNumber,
                        FamilyHolderName = answers[i].FamilyHolderName,
                        FamilyHolderCpf = answers[i].FamilyHolderCpf,
                        //Title = quiz.Find(x => question.Any(c => ObjectId.Parse(c.QuizId) == x._id)).Title,
                        Date = answers[i].Created.Value,
                       // Question = question.Find(x => x._id == ObjectId.Parse(answers[i].Questions.QuestionId)).NameQuestion,
                        QuestionAneswerId = answers[i]._id.ToString(),
                        QuizId = quiz[i]._id.ToString()
                        
                    };
                    //switch (question.Find(x => x._id == ObjectId.Parse(answers[i].Questions.QuestionId)).TypeResponse)
                    //{
                    //    case TypeResponse.MultiplaEscolha:
                    //        {
                    //            foreach (var item in answers[i].Questions.QuestionDescriptionId)
                    //                questionAnswerListViewModel.Answers.Add(questionDescription.Find(x => x._id == ObjectId.Parse(item)).Description);
                    //            break;
                    //        }
                    //    case TypeResponse.ListaSuspensa:
                    //        {
                    //            foreach (var item in answers[i].Questions.QuestionDescriptionId)
                    //                questionAnswerListViewModel.Answers.Add(questionDescription.Find(x => x._id == ObjectId.Parse(item)).Description);
                    //            break;
                    //        }
                    //    case TypeResponse.EscolhaUnica:
                    //        {
                    //            questionAnswerListViewModel.Answers.Add(questionDescription.Find(x => x._id == ObjectId.Parse(answers[i].Questions.QuestionDescriptionId.FirstOrDefault())).Description);
                    //            break;
                    //        }
                    //    case TypeResponse.Texto:
                    //        {
                    //            questionAnswerListViewModel.Answers.Add(answers[i].Questions.AnswerDescription);
                    //            break;
                    //        }
                    //}
                    listAnswers.Add(questionAnswerListViewModel);
                }
                return Ok(Utilities.ReturnSuccess(data: listAnswers));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }
        
        /// <summary>
        /// REGISTRAR UMA RESPOSTA
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///       POST
        ///       {
        ///         "familyId": "string",
        ///         "quizId": "string",
        ///         "questions": {
        ///            "questionDescriptionId": [
        ///               "string"
        ///          ],
        ///          "questionId": "string",
        ///          "answerDescription": "string"
        ///          },
        ///          "responsibleForResponsesId": "string"
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
        public async Task<IActionResult> Register([FromBody] QuestionAnswerRegisterViewModel model)
        {

            try
            {
                var typeRegister = TypeAction.Register;
                var message = $"Registro de nova questão {Request.GetUserName()}";

                var ignoreValidation = new List<string>();
                var isInvalidState = ModelState.ValidModelState(ignoreValidation.ToArray());
                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                var entityQuestion = await _quizRepository.FindByIdAsync(model.QuizId).ConfigureAwait(false);
                if (entityQuestion == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.QuestionNotFound));

                var entityFamily = await _familyRepository.FindByIdAsync(model.FamilyId).ConfigureAwait(false);
                if (entityFamily == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));
                model.FamilyHolderName = entityFamily.Holder.Name;
                model.FamilyHolderCpf = entityFamily.Holder.Cpf;
                model.FamilyNumber = entityFamily.Holder.Number;


                var profileResponsibleForResponse = new Data.Entities.Profile();
                if (string.IsNullOrEmpty(model.ResponsibleForResponsesId) == false)
                {
                    profileResponsibleForResponse = await _profileRepository.FindByIdAsync(model.ResponsibleForResponsesId).ConfigureAwait(false);
                    if (profileResponsibleForResponse != null)
                    {
                        model.ResponsibleForResponsesCpf = profileResponsibleForResponse.Cpf;
                        model.ResponsibleForResponsesName = profileResponsibleForResponse.Name;
                    }
                }

                var entityNewAnswer = _mapper.Map<QuestionAnswer>(model);
                entityNewAnswer.ResponsibleForResponses = model.ResponsibleForResponsesId == null ? Request.GetUserId() : model.ResponsibleForResponsesId;
                await _questionAnswerRepository.CreateAsync(entityNewAnswer);


                /* Atualiza o status do quiz para respondido */
                _quizFamilyRepository.UpdateMultiple(Query<QuizFamily>.Where(x => x.FamilyId == model.FamilyId && x.QuizId == model.QuizId),
                    new UpdateBuilder<QuizFamily>().Set(x => x.TypeStatus, TypeStatus.Respondido), UpdateFlags.Multi);


                return Ok(Utilities.ReturnSuccess(data: "Registrado com sucesso!"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Question, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível cadastrar/atualizar novo questionário", "", "", "", "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }
        /// <summary>
        /// REGISTRAR UMA RESPOSTA
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///       POST
        ///       {
        ///         "questionDescriptionId": [
        ///           "string"
        ///         ],
        ///         "questionId": "string",
        ///         "answerDescription": "string",
        ///         "familyId": "string",
        ///         "responsibleForResponsesId": "string",
        ///         "id": "string"
        ///       }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        //[HttpPost("Register")]
        //[Produces("application/json")]
        //[ProducesResponseType(typeof(ReturnViewModel), 200)]
        //[ProducesResponseType(400)]
        //[ProducesResponseType(401)]
        //[ProducesResponseType(500)]
        //public async Task<IActionResult> Register([FromBody] QuestionAnswerRegisterViewModel model)
        //{

        //    try
        //    {
        //        var typeRegister = TypeAction.Register;
        //        var message = $"Registro de nova questão {Request.GetUserName()}";

        //        var ignoreValidation = new List<string>();
        //        var isInvalidState = ModelState.ValidModelState(ignoreValidation.ToArray());
        //        if (isInvalidState != null)
        //            return BadRequest(isInvalidState);

        //        //var entityQuestion = await _questionRepository.FindByIdAsync(model.QuestionId).ConfigureAwait(false);
        //        //if (entityQuestion == null)
        //        //    return BadRequest(Utilities.ReturnErro(DefaultMessages.QuestionNotFound));

        //        var entityFamily = await _familyRepository.FindByIdAsync(model.FamilyId).ConfigureAwait(false);
        //        if (entityFamily == null)
        //            return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));
        //        model.FamilyHolderName = entityFamily.Holder.Name;
        //        model.FamilyHolderCpf = entityFamily.Holder.Cpf;
        //        model.FamilyNumber = entityFamily.Holder.Number;


        //        //var profileResponsibleForResponse = new Data.Entities.Profile();
        //        //if (string.IsNullOrEmpty(model.ResponsibleForResponsesId) == false)
        //        //{
        //        //    profileResponsibleForResponse = await _profileRepository.FindByIdAsync(model.ResponsibleForResponsesId).ConfigureAwait(false);
        //        //    if (profileResponsibleForResponse != null)
        //        //    {
        //        //        model.ResponsibleForResponsesCpf = profileResponsibleForResponse.Cpf;
        //        //        model.ResponsibleForResponsesName = profileResponsibleForResponse.Name;
        //        //    }
        //        //}

        //        //var entityNewAnswer = _mapper.Map<QuestionAnswer>(model);
        //        //entityNewAnswer.ResponsibleForResponses = model.ResponsibleForResponsesId == null ? Request.GetUserId() : model.ResponsibleForResponsesId;
        //        //await _questionAnswerRepository.CreateAsync(entityNewAnswer);                                             


        //        ///* Atualiza o status do quiz para respondido */
        //        //_quizFamilyRepository.UpdateMultiple(Query<QuizFamily>.Where(x => x.FamilyId == model.FamilyId && x.QuizId == entityQuestion.QuizId), 
        //        //    new UpdateBuilder<QuizFamily>().Set(x => x.TypeStatus, TypeStatus.Respondido), UpdateFlags.Multi);


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