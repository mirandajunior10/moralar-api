using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using Moralar.Data.Entities;
using Moralar.Domain;
using Moralar.Domain.Services.Interface;
using Moralar.Domain.ViewModels;
using Moralar.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UtilityFramework.Application.Core;
using UtilityFramework.Application.Core.JwtMiddleware;
using UtilityFramework.Application.Core.ViewModels;
using UtilityFramework.Services.Iugu.Core.Interface;
using UtilityFramework.Services.Iugu.Core.Models;

namespace Moralar.WebApi.Controllers
{
    [EnableCors("AllowAllOrigin")]
    [Route("api/v1/[controller]")]
    [Authorize(ActiveAuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CreditCardController : Controller
    {
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly bool _isSandBox;
        private readonly IIuguCustomerServices _iuguCustomerServices;
        private readonly IIuguPaymentMethodService _iuguPaymentMethodService;
        private readonly IMapper _mapper;
        private readonly IProfileRepository _profileUserBusiness;
        private readonly IUtilService _utilService;

        public CreditCardController(ICreditCardRepository creditCardRepository,
            IIuguPaymentMethodService iuguPaymentMethodService, IProfileRepository profileUserBusiness,
            IIuguCustomerServices iuguCustomerServices, IMapper mapper, IUtilService utilService)
        {
            _creditCardRepository = creditCardRepository;
            _iuguPaymentMethodService = iuguPaymentMethodService;
            _profileUserBusiness = profileUserBusiness;
            _iuguCustomerServices = iuguCustomerServices;
            _mapper = mapper;
            _utilService = utilService;
            _isSandBox = Utilities.GetConfigurationRoot().GetSection("IUGU:SandBox").Get<bool>();
        }

        /// <summary>
        ///     LISTAR CARTÕES DE CRÉDITO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Get()
        {
            var response = new List<CreditCardViewModel>();
            try
            {
                var userId = Request.GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredencials));

                var profileEntity = await _profileUserBusiness.FindByIdAsync(userId);

                if (profileEntity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound, responseList: true));

                var accountKey = _isSandBox ? profileEntity.AccountKeyDev : profileEntity.AccountKey;

                var listCreditCard =
                    await _creditCardRepository.FindByAsync(x => x.ProfileId == userId) as List<CreditCard>;

                if (string.IsNullOrEmpty(accountKey))
                    return Ok(Utilities.ReturnSuccess(data: response));

                var listIuguCards = await _iuguPaymentMethodService.ListarCredCardsAsync(accountKey) as List<IuguCreditCard>;

                if (listIuguCards != null)
                {

                    for (var i = 0; i < listIuguCards.Count; i++)
                    {
                        var cardIugu = listIuguCards[i];

                        if (cardIugu == null)
                            continue;

                        var card = listCreditCard.Find(x => x.TokenCard == cardIugu.Id);
                        var map = _mapper.Map<CreditCardViewModel>(cardIugu);

                        if (card == null || map == null)
                            continue;

                        map.Id = card._id.ToString();
                        map.Flag = _utilService.GetFlag(cardIugu.Data.Brand?.ToLower());

                        response.Add(map);
                    }
                }
                return Ok(Utilities.ReturnSuccess(data: response));

            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(responseList: true));
            }
        }

        /// <summary>
        /// DETALHES DO CARTÃO
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Details([FromRoute] string id)
        {
            try
            {
                if (ObjectId.TryParse(id, out var unused) == false)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidIdentifier));

                var userId = Request.GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredencials));

                var profileEntity = await _profileUserBusiness.FindByIdAsync(userId);

                if (profileEntity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound));

                var accountKey = _isSandBox ? profileEntity.AccountKeyDev : profileEntity.AccountKey;

                var cardEntity = await _creditCardRepository.FindByIdAsync(id).ConfigureAwait(false);

                if (cardEntity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.CreditCardNotFound));

                var creditCardIugu = await _iuguPaymentMethodService.BuscarCredCardsAsync(accountKey, cardEntity.TokenCard);

                if (creditCardIugu == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.CreditCardNotFoundIugu));

                var cardViewModel = _mapper.Map<CreditCardViewModel>(creditCardIugu);

                if (cardViewModel == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.CreditCardNotFound));

                cardViewModel.Flag = _utilService.GetFlag(creditCardIugu.Data.Brand?.ToLower());
                cardViewModel.Id = cardEntity._id.ToString();

                return Ok(Utilities.ReturnSuccess(data: cardViewModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// REGISTRAR CARTÃO DE CREDITO
        /// </summary>
        /// <remarks>
        ///     OBJ DE ENVIO
        /// 
        ///     POST api/v1/CreditCard
        ///     {
        ///         "name": "string",
        ///         "number": "string", // #### #### #### ####
        ///         "expMonth": 0, // FORMAT MM
        ///         "expYear": 0, // FORMAT AAAA
        ///         "cvv": "string" // MINLENGTH 3 / MAX 4
        ///     }
        /// 
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
        public async Task<IActionResult> Register([FromBody] CreditCardViewModel model)
        {
            try
            {
                var userId = Request.GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound));

                model.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelState();

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                var now = DateTime.Now;

                if (model.Number?.Length < 14)
                    return BadRequest(Utilities.ReturnErro("Número de cartão inválido."));
                if (model.ExpMonth > 12 || model.ExpMonth < 1)
                    return BadRequest(Utilities.ReturnErro("Mês de vencimento inválido."));
                if (model.ExpYear < now.Year)
                    return BadRequest(Utilities.ReturnErro("Ano de vencimento inválido."));
                if (model.Cvv?.Length < 3 || model.Cvv?.Length > 4)
                    return BadRequest(Utilities.ReturnErro("Código de segurança inválido."));

                model.Name = model.Name.ToUpper();

                var entity = await _profileUserBusiness.FindByIdAsync(userId).ConfigureAwait(false);

                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound));

                if (_isSandBox == false && string.IsNullOrEmpty(entity.AccountKey) ||
                    _isSandBox && string.IsNullOrEmpty(entity.AccountKeyDev))
                {

                    var iuguCustomerResponse = await _iuguCustomerServices.SaveClientAsync(new IuguCustomerCreated
                    {
                        Email = entity.Email,
                        Name = entity.FullName
                    });

                    if (iuguCustomerResponse.HasError)
                        return BadRequest(Utilities.ReturnErro(iuguCustomerResponse.MessageError));

                    if (_isSandBox)
                        entity.AccountKeyDev = iuguCustomerResponse.Id;
                    else
                        entity.AccountKey = iuguCustomerResponse.Id;

                    entity = _profileUserBusiness.Update(entity);
                }

                var tokenCardIugu = _iuguPaymentMethodService.SaveCreditCard(new IuguPaymentMethodToken
                {
                    Method = "credit_card",
                    Test = _isSandBox.ToString().ToLower(),
                    Data = new IuguDataPaymentMethodToken
                    {
                        FirstName = model.Name.GetFirstName(),
                        LastName = model.Name.GetLastName(),
                        Number = model.Number,
                        Month = $"{model.ExpMonth:00}",
                        VerificationValue = model.Cvv,
                        Year = $"{model.ExpYear:0000}"
                    }
                });

                var accountKey = _isSandBox ? entity.AccountKeyDev : entity.AccountKey;

                if (string.IsNullOrEmpty(tokenCardIugu.MessageError) == false)
                    return BadRequest(Utilities.ReturnErro(tokenCardIugu.MessageError));

                var iuguCreditCardResponse = _iuguPaymentMethodService.LinkCreditCardClient(
                    new IuguCustomerPaymentMethod
                    {
                        CustomerId = accountKey,
                        Description = $"Meu {entity.CreditCards.Count + 1} Cartão de credito",
                        SetAsDefault = (!entity.CreditCards.Any()).ToString(),
                        Token = tokenCardIugu.Id
                    }, accountKey);

                if (iuguCreditCardResponse.HasError)
                    return BadRequest(Utilities.ReturnErro(iuguCreditCardResponse.MessageError));

                var creditCardId = _creditCardRepository.Create(new CreditCard
                {
                    ProfileId = userId,
                    TokenCard = iuguCreditCardResponse.Id
                });

                entity.CreditCards.Add(creditCardId);

                await _profileUserBusiness.UpdateAsync(entity);

                return Ok(Utilities.ReturnSuccess("Registrado com sucesso"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }


        /// <summary>
        /// REMOVER CARTÃO DE CRÉDITO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            try
            {
                if (ObjectId.TryParse(id, out var unused) == false)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidIdentifier));

                var userId = Request.GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredencials));

                var profileEntity = await _profileUserBusiness.FindByIdAsync(userId);

                if (profileEntity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound));

                var accountKey = _isSandBox ? profileEntity.AccountKeyDev : profileEntity.AccountKey;

                var creditCardLoad = await _creditCardRepository.FindByIdAsync(id).ConfigureAwait(false);

                if (creditCardLoad == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.CreditCardNotFound));

                await _iuguPaymentMethodService.RemoverCredCardAsync(accountKey, creditCardLoad.TokenCard);

                _creditCardRepository.DeleteOne(id);

                profileEntity.CreditCards.Remove(id);
                await _profileUserBusiness.UpdateAsync(profileEntity);

                return Ok(Utilities.ReturnSuccess(DefaultMessages.Deleted));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }
    }
}