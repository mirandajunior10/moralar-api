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
        [HttpGet("QuizAvailable")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<IActionResult> QuizAvailable(string number, string name, string cpf, TypeStatus status)
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

                conditions.Add(builder.Where(x => x.TypeStatus == status));

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