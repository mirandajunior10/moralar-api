using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Moralar.Data.Entities;
using Moralar.Data.Enum;
using Moralar.Domain;
using Moralar.Domain.Services.Interface;
using Moralar.Domain.ViewModels;
using Moralar.Domain.ViewModels.Family;
using Moralar.Repository.Interface;
using Moralar.WebApi.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
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

    public class FamilyController : Controller
    {

        private readonly IMapper _mapper;
        private readonly IFamilyRepository _familyRepository;
        public FamilyController(IMapper mapper, IFamilyRepository familyRepository)
        {
            _mapper = mapper;
            _familyRepository = familyRepository;
        }


        /// <summary>
        /// DETALHES DA FAMÍLIA
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
        /// REGISTRAR USUÁRIO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("Register")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Register([FromBody] FamilyCompleteViewModel model)
        {
            var claim = Util.SetRole(TypeProfile.Profile);
            try
            {
                //model.TrimStringProperties();
                //var ignoreValidation = new List<string>();
                //if (model.TypeProvider != TypeProvider.Password)
                //{
                //    ignoreValidation.Add(nameof(model.Login));
                //    ignoreValidation.Add(nameof(model.Password));
                //}

                //var isInvalidState = ModelState.ValidModelState(ignoreValidation.ToArray());

                //if (isInvalidState != null)
                //    return BadRequest(isInvalidState);

                //if (model.TypeProvider != TypeProvider.Password)
                //{

                //    if (string.IsNullOrEmpty(model.ProviderId))
                //        return BadRequest(Utilities.ReturnErro(DefaultMessages.EmptyProviderId));
                //    var messageErro = "";
                //    if (await _profileRepository.CheckByAsync(x => x.ProviderId == model.ProviderId))
                //    {
                //        switch (model.TypeProvider)
                //        {
                //            case TypeProvider.Apple:
                //                messageErro = DefaultMessages.AppleIdInUse;
                //                break;
                //            default:
                //                messageErro = DefaultMessages.FacebookInUse;
                //                break;
                //        }
                //        return BadRequest(Utilities.ReturnErro(messageErro));

                //    }

                //    model.Login = model.Email;
                //}

                if (await _familyRepository.CheckByAsync(x => x.HolderCpf == model.HolderCpf).ConfigureAwait(false))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.CpfInUse));

                //if (await _familyRepository.CheckByAsync(x => x.Login == model.).ConfigureAwait(false))
                //    return BadRequest(Utilities.ReturnErro(DefaultMessages.LoginInUse));

                //if (await _profileRepository.CheckByAsync(x => x.Email == model.Email).ConfigureAwait(false))
                //    return BadRequest(Utilities.ReturnErro(DefaultMessages.EmailInUse));

                var entity = _mapper.Map<Data.Entities.Family>(model);

                //entity.Password = model.Password;

                var entityId = await _familyRepository.CreateAsync(entity).ConfigureAwait(false);

                return Ok(Utilities.ReturnSuccess(data: TokenProviderMiddleware.GenerateToken(entityId, false, claim)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

       
    }
}