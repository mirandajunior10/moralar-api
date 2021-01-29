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
using Moralar.Repository.Interface;
using System;
using System.Collections.Generic;
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

    public class FamilyController : Controller
    {

        private readonly IMapper _mapper;
        private readonly IFamilyRepository _familyRepository;
        private readonly IUtilService _utilService;
        private readonly ISenderMailService _senderMailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FamilyController(IMapper mapper, IFamilyRepository familyRepository, IUtilService utilService, ISenderMailService senderMailService, IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _familyRepository = familyRepository;
            _utilService = utilService;
            _senderMailService = senderMailService;
            _httpContextAccessor = httpContextAccessor;
        }


        /// <summary>
        /// BLOQUEAR / DESBLOQUEAR AGENTE - ADMIN
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

                var entity = await _familyRepository.FindByIdAsync(model.TargetId);

                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound));
                entity.DataBlocked = model.Block ? DateTimeOffset.Now.ToUnixTimeSeconds() : (long?)null;
                entity.Reason = model.Block ? model.Reason : null;

                await _familyRepository.UpdateAsync(entity);
                var typeAction = model.Block == true ? TypeAction.Block : TypeAction.UnBlock;
                await _utilService.RegisterLogAction(LocalAction.Familia, typeAction, TypeResposible.UserAdminstratorGestor, $"Bloqueio de família {entity.Holder.Name}", Request.GetUserId(), Request.GetUserName().Value, model.TargetId);
                return Ok(Utilities.ReturnSuccess(model.Block ? "Bloqueado com sucesso" : "Desbloqueado com sucesso"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível bloquer Família", Request.GetUserId(), Request.GetUserName().Value, model.TargetId, "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }
        /// <summary>
        /// LISTAGEM DOS CLIENTES
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
        public async Task<IActionResult> LoadData([FromForm] DtParameters model, [FromForm] string number, string nameHolder, string cpf)
        {
            var response = new DtResult<FamilyHolderListViewModel>();
            try
            {
                var builder = Builders<Data.Entities.Family>.Filter;
                var conditions = new List<FilterDefinition<Data.Entities.Family>>();

                conditions.Add(builder.Where(x => x.Created != null));
                if (!string.IsNullOrEmpty(number))
                    conditions.Add(builder.Where(x => x.Holder.Number == number ));
                if (!string.IsNullOrEmpty(nameHolder))
                    conditions.Add(builder.Where(x => x.Holder.Name.ToUpper().Contains(nameHolder.ToUpper())));
                if (!string.IsNullOrEmpty(cpf))
                    conditions.Add(builder.Where(x => x.Holder.Cpf == cpf));

                var columns = model.Columns.Where(x => x.Searchable && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToArray();

                var sortColumn = !string.IsNullOrEmpty(model.SortOrder) ? model.SortOrder.UppercaseFirst() : model.Columns.FirstOrDefault(x => x.Orderable)?.Name ?? model.Columns.FirstOrDefault()?.Name;
                var totalRecords = (int)await _familyRepository.GetCollectionAsync().CountDocumentsAsync(builder.And(conditions));

                var sortBy = model.Order[0].Dir.ToString().ToUpper().Equals("DESC")
                    ? Builders<Data.Entities.Family>.Sort.Descending(sortColumn)
                    : Builders<Data.Entities.Family>.Sort.Ascending(sortColumn);

                var retorno = await _familyRepository
                    .LoadDataTableAsync(model.Search.Value, sortBy, model.Start, model.Length, conditions, columns);

                var totalrecordsFiltered = !string.IsNullOrEmpty(model.Search.Value)
                    ? (int)await _familyRepository.CountSearchDataTableAsync(model.Search.Value, conditions, columns)
                    : totalRecords;

                response.Data = _mapper.Map<List<FamilyHolderListViewModel>>(retorno);
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
        /// LISTA TODAS PROPRIEDADE
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpGet("GetAll")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                //    if (ObjectId.TryParse(id, out var unused) == false)
                //        return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredencials));

                //    var userId = Request.GetUserId();

                //    if (string.IsNullOrEmpty(userId))
                //        return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidCredencials));

                var entity = await _familyRepository.FindAllAsync().ConfigureAwait(false);

                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.ResidencialPropertyNotFound));

                return Ok(Utilities.ReturnSuccess(data: _mapper.Map<List<FamilyHolderListViewModel>>(entity)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

        /// <summary>
        /// REGISTRAR FAMÍLIA
        /// </summary>
        /// <remarks>
        ///OBJ DE ENVIO
        ///         POST
        ///         "holder": {
        ///          "number": "",
        ///          "name": "name",
        ///          "cpf": "cpf",
        ///          "birthday": 1611776217,
        ///          "genre":  Enum,
        ///          "email": "email",
        ///          "phone": "phone",
        ///          "scholarity": Enum
        ///        },
        ///        "spouse": {
        ///          "name": "name",
        ///          "birthday": 1611776217,
        ///          "genre": Enum,
        ///          "scholarity": Enum
        ///        },
        ///        "members": [
        ///          {
        ///            "name": "name",
        ///            "birthday":  1611776217,
        ///            "genre":  Enum,
        ///            "kinShip": 1,
        ///            "scholarity": Enum
        ///          }, {
        ///            "name": "name2",
        ///            "birthday":  1611776217,
        ///            "genre":  Enum,
        ///            "kinShip": Enum,
        ///            "scholarity": Enum
        ///          }
        ///        ],
        ///        "financial": {
        ///          "familyIncome": decimal,
        ///          "propertyValueForDemolished": decimal,
        ///          "maximumPurchase": decimal,
        ///          "incrementValue": decimal
        ///        },
        ///        "priorization": {
        ///          "workFront": bool,
        ///          "permanentDisabled": bool,
        ///          "elderlyOverEighty": bool,
        ///          "yearsInSextyAndSeventyNine": bool,
        ///          "womanServedByProtectiveMeasure": bool,
        ///          "femaleBreadWinner": bool,
        ///          "singleParent": bool,
        ///          "familyWithMoreThanFivePeople": bool,
        ///          "childUnderEighteen": bool,
        ///          "headOfHouseholdWithoutIncome": bool,
        ///          "benefitOfContinuedProvision": bool,
        ///          "familyPurse": bool,
        ///          "involuntaryCohabitation": bool,
        ///          "familyIncomeOfUpTwoMinimumWages": bool
        ///        },
        ///        "password": "",
        ///        "isFirstAcess": bool,
        ///        "providerId": "string"
        /// }
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
        public async Task<IActionResult> Register([FromBody] FamilyCompleteViewModel model)
        {
            //var claim = Util.SetRole(TypeProfile.Profile);
            //var typeAction = string.IsNullOrEmpty(model.Id) ? TypeAction.Register : TypeAction.Change;
            try
            {
                if (model.Holder.Email.ValidEmail() == false)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.EmailInvalid));

                if (await _familyRepository.CheckByAsync(x => x.Holder.Cpf == model.Holder.Cpf).ConfigureAwait(false))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.CpfInUse));

                if (await _familyRepository.CheckByAsync(x => x.Holder.Email == model.Holder.Email).ConfigureAwait(false))
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.EmailInUse));

                var entity = _mapper.Map<Data.Entities.Family>(model);

                var newPassword = Utilities.RandomInt(8);


                var entityId = await _familyRepository.CreateAsync(entity).ConfigureAwait(false) ;
                //await _creditCardRepository.FindByAsync(x => x.CustomerId == userId) as List<CreditCard>;
                await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Cadastro de nova família {entity.Holder.Name}", Request.GetUserId(), Request.GetUserName().Value, entityId, "");

                var dataBody = Util.GetTemplateVariables();
                dataBody.Add("{{ title }}", "Lembrete de senha");
                dataBody.Add("{{ message }}", $"<p>Caro(a) {model.Holder.Name.GetFirstName()}</p>" +
                                            $"<p>Segue sua senha de acesso ao {Startup.ApplicationName}</p>" +
                                            //$"<p><b>Login</b> : {profile.Login}</p>" +
                                            $"<p><b>Senha</b> :{newPassword}</p>"
                                            );

                var body = _senderMailService.GerateBody("custom", dataBody);

                var unused = Task.Run(async () =>
                {
                    await _senderMailService.SendMessageEmailAsync(Startup.ApplicationName, model.Holder.Email, body, "Lembrete de senha").ConfigureAwait(false);
                });
                return Ok(Utilities.ReturnSuccess(data: "Registrado com sucesso!"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível cadastrar nova Família", "", "", "", "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }

        [HttpPost("Edit")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<IActionResult> Edit([FromBody] FamilyEditViewModel model)
        {
            try
            {
                var entityFamily = await _familyRepository.FindByIdAsync(model.Id).ConfigureAwait(false);
                if (entityFamily == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));

                var validOnly = _httpContextAccessor.GetFieldsFromBody();

                var p = entityFamily.SetIfDifferent(model.Holder, validOnly);
                var t = entityFamily.SetIfDifferent(model.Members, validOnly);
                var fast = model.SetIfDifferent(entityFamily.Members, validOnly);
                var entity = _mapper.Map<Data.Entities.Family>(model);

                var entityId = await _familyRepository.UpdateAsync(entity).ConfigureAwait(false);
                await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Change, TypeResposible.UserAdminstratorGestor, $"Update de nova família {entity.Holder.Name}", "", "", model.Id);//Request.GetUserName().Value, Request.GetUserId()

                return Ok(Utilities.ReturnSuccess(data: "Registrado com sucesso!"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Não foi possível cadastrar nova Família", "", "", "", "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }
        /// <summary>
        /// DELETAR MEMBRO DA FAMÍLIA
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///         POST
        ///           {
        ///             "familyId": "6011d02a4c7c9e71c25df866", Id da família
        ///             "indexMember": 1 // Indíce da lista(a contagem sempre começa do zero)
        ///            }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("DeleteMember")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteMember([FromBody] FamilyDeleteMember model)
        {
            try
            {
                var entityFamily = await _familyRepository.FindByIdAsync(model.FamilyId).ConfigureAwait(false);
                if (entityFamily == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));
                if (entityFamily.Members.FindIndex(x => x.Name == model.Name) < 0)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.MemberNotFound));

                entityFamily.Members.RemoveAt(entityFamily.Members.FindIndex(x => x.Name == model.Name));
                await _familyRepository.UpdateOneAsync(entityFamily).ConfigureAwait(false);

                await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Delete, TypeResposible.UserAdminstratorGestor, $"Remover membro de nova família {model.Name}", Request.GetUserId(), Request.GetUserName().Value, entityFamily._id.ToString());

                return Ok(Utilities.ReturnSuccess(data: "Atualizado com sucesso!"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Delete, TypeResposible.UserAdminstratorGestor, $"Não foi possível remover membro de nova família", Request.GetUserId(), Request.GetUserName().Value, model.FamilyId, "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }
        /// <summary>
        /// REGISTRA MEMBRO DA FAMÍLIA
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///         POST
        ///         {
        ///           "familyId": "6011d02a4c7c9e71c25df866",
        ///            "members": [
        ///              {
        ///                      "Name" : "name",
        ///                      "Birthday" : 1611776217,
        ///                      "Genre" : 0,
        ///                      "Scholarity" : 0,
        ///                      "KinShip" : 0
        ///              }
        ///            ]
        ///         }  
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("RegisterMember")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterMember([FromBody] FamilyRegisterMember model)
        {
            //var claim = Util.SetRole(TypeProfile.Profile);
            //var typeAction = string.IsNullOrEmpty(model.Id) ? TypeAction.Register : TypeAction.Change;
            try
            {
                if (model.Members.Count() == 0)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.MemberNotFound));


                var entityFamily = await _familyRepository.FindByIdAsync(model.FamilyId).ConfigureAwait(false);
                if (entityFamily == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FamilyNotFound));

                var existMemberWithSameName = entityFamily.Members.Find(x => model.Members.Exists(c => c.Name == x.Name));
                if (existMemberWithSameName != null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.MemberInUse));

                var namesMembers = string.Join(";", model.Members.Select(x => x.Name).ToArray());

                entityFamily.Members.AddRange(_mapper.Map<List<FamilyMember>>(model.Members));
                await _familyRepository.UpdateOneAsync(entityFamily).ConfigureAwait(false);

                await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Register, TypeResposible.UserAdminstratorGestor, $"Inclusão de membro(s) de nova família {namesMembers}", Request.GetUserId(), Request.GetUserName().Value,  entityFamily._id.ToString());//Request.GetUserName().Value Request.GetUserId()

                return Ok(Utilities.ReturnSuccess(data: "Atualizado com sucesso!"));
            }
            catch (Exception ex)
            {
                await _utilService.RegisterLogAction(LocalAction.Familia, TypeAction.Delete, TypeResposible.UserAdminstratorGestor, $"Não foi possível Inclusão de membro(s) de nova família", Request.GetUserId(), Request.GetUserName().Value, model.FamilyId, "", ex);
                return BadRequest(ex.ReturnErro());
            }
        }
        /// <summary>
        /// LOGIN
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        /// 
        ///         POST
        ///             {
        ///              "login":"string", //optional
        ///              "password":"string", //optional
        ///              "providerId":"string", //optional
        ///              "typeProvider":0 //Enum (optional)
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("Token")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Token([FromBody] LoginFamilyViewModel model)
        {
            var claims = new List<Claim>();
            var claim = Util.SetRole(TypeProfile.Profile);
            var claimUserName = Request.GetUserName();
            claims.Add(claim);

            try
            {
                if (claimUserName != null)
                    claims.Add(claimUserName);

                model.TrimStringProperties();

                //if (string.IsNullOrEmpty(model.RefreshToken) == false)
                //    return TokenProviderMiddleware.RefreshToken(model.RefreshToken, false, claims.ToArray());

                Data.Entities.Family entity;
                //if (model.TypeProvider != TypeProvider.Password)
                //{
                //    entity = await _familyRepository.FindOneByAsync(x => x.ProviderId == model.ProviderId).ConfigureAwait(false);

                //    if (entity == null)
                //        return BadRequest(Utilities.ReturnErro(DefaultMessages.ProfileNotFound, new { IsRegister = true }));
                //}
                //else
                //{
                model.TrimStringProperties();
                var isInvalidState = ModelState.ValidModelStateOnlyFields(nameof(model.HolderCpf), nameof(model.HolderBirthday));

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                entity = await _familyRepository.FindOneByAsync(x => x.Holder.Cpf == model.HolderCpf && x.Holder.Birthday == model.HolderBirthday).ConfigureAwait(false);

                if (entity == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidLogin));

                //}
                claims.Add(new Claim("UserName", entity.Holder.Name));
                //if (entity.DataBlocked != null)
                //    return BadRequest(Utilities.ReturnErro($"Usuário bloqueado : {entity.Reason}"));

                return Ok(Utilities.ReturnSuccess(data: TokenProviderMiddleware.GenerateToken(entity._id.ToString(), false, claims.ToArray())));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }

    }
}