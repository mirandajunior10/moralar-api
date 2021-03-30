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

    public class QuizFamilyController : Controller
    {

        private readonly IMapper _mapper;
        private readonly IQuestionRepository _questionRepository;
        private readonly IQuizRepository _quizRepository;
        private readonly IQuestionDescriptionRepository _questionDescriptionRepository;
        private readonly IQuizFamilyRepository _quizFamilyRepository;
        private readonly IFamilyRepository _familyRepository;
        private readonly IUtilService _utilService;

        public QuizFamilyController(IMapper mapper, IQuestionRepository questionRepository, IQuizRepository quizRepository, IQuestionDescriptionRepository questionDescriptionRepository, IQuizFamilyRepository quizFamilyRepository, IFamilyRepository familyRepository, IUtilService utilService)
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
        /// LISTA OS QUESTIONÁRIOS DISPONÍVEIS PARA FAMÍLIA
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("LoadDataQuizAvailable")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> LoadDataQuizAvailable([FromForm] DtParameters model, [FromForm] string number, [FromForm] string name, [FromForm] string cpf, [FromForm] TypeStatus status)
        {
            //
            var response = new DtResult<QuizFamilyListViewModel>();
            try
            {
                var builder = Builders<Data.Entities.QuizFamily>.Filter;
                var conditions = new List<FilterDefinition<Data.Entities.QuizFamily>>();


                conditions.Add(builder.Where(x => x.Created != null && x.Disabled == null));
                if (!string.IsNullOrEmpty(number))
                    conditions.Add(builder.Where(x => x.HolderNumber.ToUpper() == number.ToUpper()));
                if (!string.IsNullOrEmpty(name))
                    conditions.Add(builder.Where(x => x.HolderName.ToUpper().Contains(name.ToUpper())));
                if (!string.IsNullOrEmpty(cpf))
                    conditions.Add(builder.Where(x => x.HolderCpf == cpf));

                conditions.Add(builder.Where(x => x.TypeStatus == status));
                var columns = model.Columns.Where(x => x.Searchable && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToArray();

                var sortColumn = !string.IsNullOrEmpty(model.SortOrder) ? model.SortOrder.UppercaseFirst() : model.Columns.FirstOrDefault(x => x.Orderable)?.Name ?? model.Columns.FirstOrDefault()?.Name;
                var totalRecords = (int)await _quizFamilyRepository.GetCollectionAsync().CountDocumentsAsync(builder.And(conditions));

                var sortBy = model.Order[0].Dir.ToString().ToUpper().Equals("DESC")
                    ? Builders<Data.Entities.QuizFamily>.Sort.Descending(sortColumn)
                    : Builders<Data.Entities.QuizFamily>.Sort.Ascending(sortColumn);

                var retorno = await _quizFamilyRepository
                    .LoadDataTableAsync(model.Search.Value, sortBy, model.Start, model.Length, conditions, columns) as List<QuizFamily>;

                var totalrecordsFiltered = !string.IsNullOrEmpty(model.Search.Value)
                    ? (int)await _quizFamilyRepository.CountSearchDataTableAsync(model.Search.Value, conditions, columns)
                    : totalRecords;
                var listOnlyQuiz = await _quizRepository.FindIn("_id", retorno.Select(x => ObjectId.Parse(x.QuizId.ToString())).ToList()) as List<Quiz>;
                var listQuizFamily = _mapper.Map<List<QuizFamilyListViewModel>>(retorno);
                for (int i = 0; i < listQuizFamily.Count(); i++)
                {
                    listQuizFamily[i].Title = listOnlyQuiz.Find(x => x._id == listOnlyQuiz[i]._id)?.Title;
                    listQuizFamily[i].TypeQuiz = listOnlyQuiz.Find(x => x._id == listOnlyQuiz[i]._id).TypeQuiz;
                }
                response.Data = listQuizFamily.Where(x => x.TypeQuiz == TypeQuiz.Quiz).ToList();
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
        /// DETALHES DO QUIZ DISPONIBILIZADO PARA O MORADOR
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
                var quizFamily = await _quizFamilyRepository.FindByIdAsync(id).ConfigureAwait(false);
                if (quizFamily == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.QuizFamilyNotFound));

                var family = await _familyRepository.FindByIdAsync(quizFamily.FamilyId).ConfigureAwait(false);
                if (family == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));

                var entity = await _quizRepository.FindByIdAsync(quizFamily.QuizId).ConfigureAwait(false);
                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.QuizNotFound));

                var question = await _questionRepository.FindByAsync(x => x.QuizId == entity._id.ToString()).ConfigureAwait(false) as List<Question>;
                if (question.Count() == 0)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.QuestionNotFound));

                var _quizViewModel = new QuizDetailViewModel()
                {
                    Id = entity._id.ToString(),
                    Title = entity.Title,
                    TypeQuiz = entity.TypeQuiz
                };

                var questionDescription = await _questionDescriptionRepository.FindIn("QuestionId", question.Select(x => ObjectId.Parse(x._id.ToString())).ToList()) as List<QuestionDescription>;
                for (int i = 0; i < question.Count(); i++)
                {
                    _quizViewModel.QuestionViewModel.Add(_mapper.Map<QuestionViewModel>(question[i]));
                    foreach (var item in questionDescription.Where(x => x.QuestionId == question[i]._id.ToString()))
                    {
                        if (item != null)
                            _quizViewModel.QuestionViewModel[i].Description.Add(new QuestionDescriptionViewModel()
                            {
                                Id = item._id.ToString(),
                                Description = item.Description,
                                QuestionId = item.QuestionId
                            });
                    }
                }

                List<QuestionAnswerListViewModel> listAnswers = new List<QuestionAnswerListViewModel>();

                //var quiz = await _quizRepository.FindAllAsync().ConfigureAwait(false) as List<Quiz>;
                //var answers = await _questionAnswerRepository.FindAllAsync().ConfigureAwait(false) as List<QuestionAnswer>;
                //var question = await _questionRepository.FindIn("_id", answers.Select(x => ObjectId.Parse(x.QuestionId)).ToList()) as List<Question>;
                //var questionDescription = await _questionDescriptionRepository.FindAllAsync().ConfigureAwait(false) as List<QuestionDescription>;
                //for (int i = 0; i < answers.Count(); i++)
                //{
                //    var questionAnswerListViewModel = new QuestionAnswerListViewModel()
                //    {
                //        FamilyNumber = answers[i].FamilyNumber,
                //        FamilyHolderName = answers[i].FamilyHolderName,
                //        FamilyHolderCpf = answers[i].FamilyHolderCpf,
                //        Title = quiz.Find(x => question.Any(c => ObjectId.Parse(c.QuizId) == x._id)).Title,
                //        Date = answers[i].Created.Value,
                //        Question = question.Find(x => x._id == ObjectId.Parse(answers[i].QuestionId)).NameQuestion
                //    };
                //    switch (question.Find(x => x._id == ObjectId.Parse(answers[i].QuestionId)).TypeResponse)
                //    {
                //        case TypeResponse.MultiplaEscolha:
                //            {
                //                foreach (var item in answers[i].QuestionDescriptionId)
                //                    questionAnswerListViewModel.Answers.Add(questionDescription.Find(x => x._id == ObjectId.Parse(item)).Description);
                //                break;
                //            }
                //        case TypeResponse.ListaSuspensa:
                //            {
                //                foreach (var item in answers[i].QuestionDescriptionId)
                //                    questionAnswerListViewModel.Answers.Add(questionDescription.Find(x => x._id == ObjectId.Parse(item)).Description);
                //                break;
                //            }
                //        case TypeResponse.EscolhaUnica:
                //            {
                //                questionAnswerListViewModel.Answers.Add(questionDescription.Find(x => x._id == ObjectId.Parse(answers[i].QuestionDescriptionId.FirstOrDefault())).Description);
                //                break;
                //            }
                //        case TypeResponse.Texto:
                //            {
                //                questionAnswerListViewModel.Answers.Add(answers[i].AnswerDescription);
                //                break;
                //            }
                //    }
                //    listAnswers.Add(questionAnswerListViewModel);
                //    var objReturn = new
                //    {
                //        quiz = _quizViewModel,
                //        family = family
                //    };
                return Ok(Utilities.ReturnSuccess(data: _quizViewModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

    }

}