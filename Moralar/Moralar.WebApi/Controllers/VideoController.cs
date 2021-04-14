using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Moralar.Data.Entities;
using Moralar.Domain;
using Moralar.Domain.ViewModels;
using Moralar.Domain.ViewModels.Video;
using Moralar.Domain.ViewModels.VideoViewed;
using Moralar.Repository.Interface;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UtilityFramework.Application.Core;
using UtilityFramework.Application.Core.JwtMiddleware;
using UtilityFramework.Application.Core.ViewModels;

namespace Moralar.WebApi.Controllers
{
    [EnableCors("AllowAllOrigin")]
    [Route("api/v1/[controller]")]
    [Authorize(ActiveAuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]


    public class VideoController : Controller
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IVideoViewedRepository _videoViewedRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public VideoController(IVideoRepository videoRepository, IVideoViewedRepository videoViewedRepository, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _videoRepository = videoRepository;
            _videoViewedRepository = videoViewedRepository;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }






        ///// <summary>
        ///// METODO DE DETALHES DO ITEM
        ///// </summary>
        ///// <response code="200">Returns success</response>
        ///// <response code="400">Custom Error</response>
        ///// <response code="401">Unauthorize Error</response>
        ///// <response code="500">Exception Error</response>
        ///// <returns></returns>
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
                //var userId = Request.GetUserId();

                //if (string.IsNullOrEmpty(userId))
                //    return BadRequest(Utilities.ReturnErro(nameof(DefaultMessages.InvalidCredentials)));

                var videoEntity = await _videoRepository.FindByIdAsync(id).ConfigureAwait(false);
                if (videoEntity == null)
                    return BadRequest(Utilities.ReturnErro(nameof(DefaultMessages.VideoNotFound)));

                var vwViewModel = _mapper.Map<VideoViewModel>(videoEntity);


                return Ok(Utilities.ReturnSuccess(data: vwViewModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(nameof(DefaultMessages.MessageException)));
            }
        }
        ///// <summary>
        ///// METODO DE DETALHES DO ITEM
        ///// </summary>
        ///// <response code="200">Returns success</response>
        ///// <response code="400">Custom Error</response>
        ///// <response code="401">Unauthorize Error</response>
        ///// <response code="500">Exception Error</response>
        ///// <returns></returns>
        [HttpGet("GetAll")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var userId = Request.GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(Utilities.ReturnErro(nameof(DefaultMessages.InvalidCredentials)));

                var videoEntity = await _videoRepository.FindAllAsync().ConfigureAwait(false);
                if (videoEntity == null)
                    return BadRequest(Utilities.ReturnErro(nameof(DefaultMessages.VideoNotFound)));

                var vieoViewModel = _mapper.Map<List<VideoViewModel>>(videoEntity);

                return Ok(Utilities.ReturnSuccess(data: vieoViewModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(nameof(DefaultMessages.MessageException)));
            }
        }


        /// <summary>
        /// EDITAR UM VÍDEO
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///         POST
        ///             {
        ///               "Thumbnail": "fasdfasqwerqwe",/// IMAGEM DO VÍDEO
        ///               "Title": "", // TÍTULO
        ///               "Duration": "1:32",//DURAÇÃO DO VÍDEO
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("Edit")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Edit([FromBody] VideoViewModel model)
        {
            try
            {
                var validOnly = _httpContextAccessor.GetFieldsFromBody();

                model.TrimStringProperties();

                var now = DateTimeOffset.Now.ToUnixTimeSeconds();

                //register
                var isInvalidState = ModelState.ValidModelState();

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                var video = await _videoRepository.FindByIdAsync(model.Id).ConfigureAwait(false);
                if (video == null)
                    return BadRequest(Utilities.ReturnErro(nameof(DefaultMessages.VideoNotFound)));

                var entity = _mapper.Map<Data.Entities.Video>(model);
                entity.Thumbnail = model.Thumbnail.SetPathImage();
                entity.Title = model.Title;
                entity.Created = video.Created;

                await _videoRepository.UpdateAsync(entity).ConfigureAwait(false);

                return Ok(Utilities.ReturnSuccess(DefaultMessages.Updated));

            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(nameof(DefaultMessages.MessageException)));
            }
        }
        /// <summary>
        /// CADASTRAR UM VÍDEO
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///         POST
        ///             {
        ///               "Thumbnail": "fasdfasqwerqwe",/// IMAGEM DO VÍDEO
        ///               "Title": "", // TÍTULO
        ///               "Duration": "1:32",//DURAÇÃO DO VÍDEO
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("Save")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Save([FromBody] VideoViewModel model)
        {
            try
            {
                var validOnly = _httpContextAccessor.GetFieldsFromBody();

                model.TrimStringProperties();

                var now = DateTimeOffset.Now.ToUnixTimeSeconds();

                //register
                var isInvalidState = ModelState.ValidModelState();

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                var entity = _mapper.Map<Data.Entities.Video>(model);
                entity.Thumbnail = model.Thumbnail.SetPathImage();
                entity.Title = model.Title;
                entity.Created = Utilities.ToTimeStamp(DateTime.Now);
                await _videoRepository.CreateAsync(entity).ConfigureAwait(false);

                return Ok(Utilities.ReturnSuccess(DefaultMessages.Registred));

            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(nameof(DefaultMessages.MessageException)));
            }
        }
        /// <summary>
        /// CADASTRAR UM VÍDEO
        /// </summary>
        /// <remarks>
        /// OBJ DE ENVIO
        ///         POST
        ///             {
        ///               "VideoId" :"",
        ///               "FamilyId" : ""
        ///             }
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("RegisterVideoViewed")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> RegisterVideoViewed([FromBody] VideoViewedViewModel model)
        {
            try
            {
                var validOnly = _httpContextAccessor.GetFieldsFromBody();

                model.TrimStringProperties();

                var now = DateTimeOffset.Now.ToUnixTimeSeconds();

                //register
                var isInvalidState = ModelState.ValidModelState();

                if (isInvalidState != null)
                    return BadRequest(isInvalidState);

                var entity = _mapper.Map<Data.Entities.VideoViewed>(model);

                await _videoViewedRepository.CreateAsync(entity).ConfigureAwait(false);

                return Ok(Utilities.ReturnSuccess(DefaultMessages.Registred));

            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(nameof(DefaultMessages.MessageException)));
            }
        }

        /// <summary>
        /// METODO DE LISTAGEM COM DATATABLE
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
        [AllowAnonymous]
        public async Task<IActionResult> LoadData([FromForm] DtParameters model)
        {
            var response = new DtResult<VideoListViewModel>();
            try
            {
                var builder = Builders<Video>.Filter;
                var conditions = new List<FilterDefinition<Video>>();

                conditions.Add(builder.Where(x => x.Disabled == null && x.DataBlocked == null));

                var columns = model.Columns.Where(x => x.Searchable && !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToArray();

                var sortColumn = model.SortOrder;
                var totalRecords = (int)await _videoRepository.GetCollectionAsync().CountDocumentsAsync(builder.And(conditions));

                var sortBy = model.Order[0].Dir.ToString().ToUpper().Equals("DESC")
                   ? Builders<Video>.Sort.Descending(sortColumn)
                   : Builders<Video>.Sort.Ascending(sortColumn);

                var retorno = await _videoRepository
                 .LoadDataTableAsync(model.Search.Value, sortBy, model.Start, model.Length, conditions, fields: columns);

                var totalrecordsFiltered = !string.IsNullOrEmpty(model.Search.Value)
                   ? (int)await _videoRepository.CountSearchDataTableAsync(model.Search.Value, conditions, columns)
                   : totalRecords;

                var _amoutViewed = _videoViewedRepository.FindAll().GroupBy(x => new { x.VideoId })
                     .Select(gp => new
                     {
                         VideoId = gp.Key.VideoId,
                         Qtd = gp.Count(),
                     }).ToList();
                var listReturn = _mapper.Map<List<VideoListViewModel>>(retorno);

                foreach (var item in listReturn)
                {
                    var amountView = _amoutViewed.Find(x => x.VideoId == item.Id);
                    if (amountView == null)
                        item.AmountViewed = 0;
                    else 
                        item.AmountViewed = amountView.Qtd;
                }


                response.Data = listReturn;
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
        /// METODO DE REMOVER ITEM
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [HttpPost("Delete/{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            try
            {
                if (ObjectId.TryParse(id, out var unused) == false)
                    return BadRequest(Utilities.ReturnErro(nameof(DefaultMessages.InvalidIdentifier)));

                await _videoRepository.DeleteOneAsync(id).ConfigureAwait(false);

                return Ok(Utilities.ReturnSuccess(nameof(DefaultMessages.Deleted)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(nameof(DefaultMessages.MessageException)));
            }
        }
        /// <summary>
        /// BLOQUEIA OU DESBLOQUEIA O VIDEO
        /// </summary>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("BlockUnblock")]
        [Produces("application/json")]
        //[ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> BlockUnblock([FromBody] BlockViewModel model)
        {

            try
            {
                if (ObjectId.TryParse(model.TargetId, out var unused) == false)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidIdentifier));

                var entityVideo = await _videoRepository.FindOneByAsync(x => x._id == ObjectId.Parse(model.TargetId)).ConfigureAwait(false);

                if (entityVideo == null)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.VideoNotFound));

                entityVideo.DataBlocked = model.Block ? DateTimeOffset.Now.ToUnixTimeSeconds() : (long?)null;


                var entityId = await _videoRepository.UpdateOneAsync(entityVideo).ConfigureAwait(false);

                return Ok(Utilities.ReturnSuccess("Registrado com sucesso"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro());
            }
        }
    }
}