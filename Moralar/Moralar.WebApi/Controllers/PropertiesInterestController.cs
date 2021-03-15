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
using Moralar.Domain.ViewModels.PropertiesInterest;
using Moralar.Domain.ViewModels.Question;
using Moralar.Domain.ViewModels.Quiz;
using Moralar.Repository.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

    public class PropertiesInterestController : Controller
    {

        private readonly IMapper _mapper;
        private readonly IFamilyRepository _familyRepository;
        private readonly IResidencialPropertyRepository _residencialPropertyRepository;
        private readonly IPropertiesInterestRepository _propertiesInterestRepository;
        private readonly IUtilService _utilService;

        public PropertiesInterestController(IMapper mapper, IFamilyRepository familyRepository, IResidencialPropertyRepository residencialPropertyRepository, IPropertiesInterestRepository propertiesInterestRepository, IUtilService utilService)
        {
            _mapper = mapper;
            _familyRepository = familyRepository;
            _residencialPropertyRepository = residencialPropertyRepository;
            _propertiesInterestRepository = propertiesInterestRepository;
            _utilService = utilService;
        }





        ///// <summary>
        ///// LISTAGEM DOS QUESTINÁRIOS DATATABLE
        ///// </summary>
        ///// <response code="200">Returns success</response>
        ///// <response code="400">Custom Error</response>
        ///// <response code="401">Unauthorize Error</response>
        ///// <response code="500">Exception Error</response>
        ///// <returns></returns>
        //[HttpPost("LoadData")]
        //[ProducesResponseType(typeof(ReturnViewModel), 200)]
        //[ProducesResponseType(400)]
        //[ProducesResponseType(401)]
        //[ProducesResponseType(500)]
        ////[ApiExplorerSettings(IgnoreApi = true)]
        //[AllowAnonymous]
        //public async Task<IActionResult> LoadData([FromForm] DtParameters model, [FromForm] string title, [FromForm] TypeQuiz typeQuiz)
        //{
        //    var response = new DtResult<PropertiesInterestViewModel>();
        //    try
        //    {
        //        var builder = Builders<Data.Entities.PropertiesInterest>.Filter;
        //        var conditions = new List<FilterDefinition<Data.Entities.PropertiesInterest>>();

        //        conditions.Add(builder.Where(x => x.Created != null && x.Disabled == null));

        //        var columns = model.Columns.Where(x => x.Searchable && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToArray();

        //        var sortColumn = !string.IsNullOrEmpty(model.SortOrder) ? model.SortOrder.UppercaseFirst() : model.Columns.FirstOrDefault(x => x.Orderable)?.Name ?? model.Columns.FirstOrDefault()?.Name;
        //        var totalRecords = (int)await _quizRepository.GetCollectionAsync().CountDocumentsAsync(builder.And(conditions));

        //        var sortBy = model.Order[0].Dir.ToString().ToUpper().Equals("DESC")
        //            ? Builders<Data.Entities.Quiz>.Sort.Descending(sortColumn)
        //            : Builders<Data.Entities.Quiz>.Sort.Ascending(sortColumn);

        //        var retorno = await _quizRepository
        //            .LoadDataTableAsync(model.Search.Value, sortBy, model.Start, model.Length, conditions, columns);

        //        var totalrecordsFiltered = !string.IsNullOrEmpty(model.Search.Value)
        //            ? (int)await _quizRepository.CountSearchDataTableAsync(model.Search.Value, conditions, columns)
        //            : totalRecords;

        //        response.Data = _mapper.Map<List<QuizViewModel>>(retorno);
        //        response.Draw = model.Draw;
        //        response.RecordsFiltered = totalrecordsFiltered;
        //        response.RecordsTotal = totalRecords;

        //        return Ok(response);

        //    }
        //    catch (Exception ex)
        //    {
        //        response.Erro = true;
        //        response.MessageEx = $"{ex.InnerException} {ex.Message}".Trim();

        //        return Ok(response);
        //    }
        //}

        ///// <summary>
        ///// DETALHES DO QUIZ
        ///// </summary>
        ///// <response code="200">Returns success</response>
        ///// <response code="400">Custom Error</response>
        ///// <response code="401">Unauthorize Error</response>
        ///// <response code="500">Exception Error</response>
        ///// <returns></returns>
        //[HttpGet("Detail/{id}")]
        //[Produces("application/json")]
        //[ProducesResponseType(typeof(ReturnViewModel), 200)]
        //[ProducesResponseType(400)]
        //[ProducesResponseType(401)]
        //[ProducesResponseType(500)]
        //[AllowAnonymous]
        //public async Task<IActionResult> Detail([FromRoute] string id)
        //{
        //    try
        //    {
        //        var entity = await _quizRepository.FindByIdAsync(id).ConfigureAwait(false);
        //        if (entity == null)
        //            return BadRequest(Utilities.ReturnErro(DefaultMessages.QuizNotFound));

        //        var question = await _questionRepository.FindByAsync(x => x.QuizId == entity._id.ToString()).ConfigureAwait(false) as List<Question>;
        //        if (question.Count() == 0)
        //            return BadRequest(Utilities.ReturnErro(DefaultMessages.QuestionNotFound));

        //        var _quizViewModel = new QuizDetailViewModel()
        //        {
        //            Id = entity._id.ToString(),
        //            Title = entity.Title,
        //            TypeQuiz = entity.TypeQuiz
        //        };

        //        var questionDescription = await _questionDescriptionRepository.FindIn("QuestionId", question.Select(x => ObjectId.Parse(x._id.ToString())).ToList()) as List<QuestionDescription>;
        //        for (int i = 0; i < question.Count(); i++)
        //        {
        //            _quizViewModel.QuestionViewModel.Add(_mapper.Map<QuestionViewModel>(question[i]));
        //            foreach (var item in questionDescription.Where(x => x.QuestionId == question[i]._id.ToString()))
        //            {
        //                if (item != null)
        //                    _quizViewModel.QuestionViewModel[i].Description.Add(new QuestionDescriptionViewModel()
        //                    {
        //                        Id = item._id.ToString(),
        //                        Description = item.Description,
        //                        QuestionId = item.QuestionId
        //                    });
        //            }
        //        }
        //        var pc = questionDescription;
        //        return Ok(Utilities.ReturnSuccess(data: _quizViewModel));
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.ReturnErro());
        //    }
        //}

        /// <summary>
        /// REGISTRAR UM INTERESSE AO IMÓVEL
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
        public async Task<IActionResult> Register([FromBody] PropertiesInterestRegisterViewModel model)
        {

            try
            {
                var ignoreValidation = new List<string>();
                var isInvalidState = ModelState.ValidModelState(ignoreValidation.ToArray());
                if (isInvalidState != null)
                    return BadRequest(isInvalidState);


                if (await _propertiesInterestRepository.CheckByAsync(X => X.FamilyId == model.FamilyId && X.ResidelcialPropertyId == model.ResidelcialPropertyId).ConfigureAwait(false))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyInUse));

                var familyEntity = await _familyRepository.FindByIdAsync(model.FamilyId).ConfigureAwait(false);
                if (familyEntity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));

                var residencialEntity = await _residencialPropertyRepository.FindByIdAsync(model.ResidelcialPropertyId).ConfigureAwait(false);
                if (residencialEntity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ResidencialPropertyNotFound));


                var entityProperty = _mapper.Map<PropertiesInterest>(model);
                entityProperty.HolderName = familyEntity.Holder.Name;
                entityProperty.HolderEmail = familyEntity.Holder.Email;
                entityProperty.HolderCpf = familyEntity.Holder.Cpf;
                entityProperty.HolderNumber = familyEntity.Holder.Number;
                entityProperty.Priorization = familyEntity.Priorization;

                await _propertiesInterestRepository.CreateAsync(entityProperty).ConfigureAwait(false);


                return Ok(Utilities.ReturnSuccess(data: "Registrado com sucesso!"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Question, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível cadastrar/atualizar novo questionário", "", "", "", "", ex);
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
        [AllowAnonymous]
        public async Task<IActionResult> LoadData([FromForm] DtParameters model)
        {
            //, [FromForm] string number, [FromForm] string holderName, [FromForm] string holderCpf
            var response = new DtResult<PropertiesInterestViewModel>();
            //
            try
            {
                var builder = Builders<Data.Entities.PropertiesInterest>.Filter;
                var conditions = new List<FilterDefinition<Data.Entities.PropertiesInterest>>();

                conditions.Add(builder.Where(x => x.Created != null));
                //if (!string.IsNullOrEmpty(number))
                //    conditions.Add(builder.Where(x => x.HolderNumber == number));
                //if (!string.IsNullOrEmpty(holderName))
                //    conditions.Add(builder.Where(x => x.HolderName.ToUpper().Contains(holderName.ToUpper())));
                //if (!string.IsNullOrEmpty(holderCpf))
                //    conditions.Add(builder.Where(x => x.HolderCpf == holderCpf.OnlyNumbers()));

                var columns = model.Columns.Where(x => x.Searchable && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToArray();

                //var sortColumn = !string.IsNullOrEmpty(model.SortOrder) ? model.SortOrder.UppercaseFirst() : model.Columns.FirstOrDefault(x => x.Orderable)?.Name ?? model.Columns.FirstOrDefault()?.Name;
                var sortColumn = !string.IsNullOrEmpty(model.SortOrder) ? model.SortOrder.UppercaseFirst() : model.Columns.FirstOrDefault(x => x.Orderable)?.Name ?? model.Columns.FirstOrDefault()?.Name;
                var totalRecords = (int)await _propertiesInterestRepository.GetCollectionAsync().CountDocumentsAsync(builder.And(conditions));

                var sortBy =
                model.Order[0].Dir.ToString().ToUpper().Equals("DESC")
                ? Builders<Data.Entities.PropertiesInterest>.Sort.Descending(sortColumn)
                : Builders<Data.Entities.PropertiesInterest>.Sort.Ascending(sortColumn);

                var pfasdfa =await _propertiesInterestRepository.FindAllAsync().ConfigureAwait(false) as List<PropertiesInterest>;
                var retorno = await _propertiesInterestRepository
                    .LoadDataTableAsync(model.Search.Value, sortBy, model.Start, model.Length, conditions, columns);


                var t = retorno.ToList();
                var propertiesEntity = _mapper.Map<List<PropertiesInterestViewModel>>(retorno);
                for (int i = 0; i < t.Count(); i++)
                {
                    var s = t[i].Priorization;

                    foreach (var p in s.GetType().GetProperties().Where(p => !p.GetGetMethod().GetParameters().Any()))
                    {
                        var g = (PriorityRate)p.GetValue(s, null);
                        if (g.Value == true)
                            propertiesEntity.Find(x => x.Id == t[i]._id.ToString()).PriorityRates.Add(g);
                    }
                }
                


                var totalrecordsFiltered = !string.IsNullOrEmpty(model.Search.Value)
                    ? (int)await _propertiesInterestRepository.CountSearchDataTableAsync(model.Search.Value, conditions, columns)
                    : totalRecords;

                var teste = !string.IsNullOrEmpty(model.SortOrder) ? model.SortOrder.UppercaseFirst() : model.Columns.FirstOrDefault(x => x.Orderable)?.Name ?? model.Columns.FirstOrDefault()?.Name;

                var residencialEntity = await _residencialPropertyRepository.FindIn("_id", retorno.Select(x => ObjectId.Parse(x.ResidelcialPropertyId.ToString())).ToList()) as List<ResidencialProperty>;
                for (int i = 0; i < propertiesEntity.Count(); i++)
                {
                    var objResidencial = residencialEntity.FirstOrDefault(x => x._id == ObjectId.Parse(propertiesEntity[i].ResidelcialPropertyId));
                    if (objResidencial != null)
                        propertiesEntity[i].ResidencialPropertyAdress = _mapper.Map<ResidencialPropertyAdress>(objResidencial.ResidencialPropertyAdress);
                }

                response.Data = propertiesEntity.OrderBy(c => c.PriorityRates.Min(x => x.Rate)).ToList();
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

    }

}