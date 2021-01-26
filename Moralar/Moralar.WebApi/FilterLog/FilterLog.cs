using AutoMapper;
using Microsoft.AspNetCore.Mvc.Filters;
using Moralar.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Moralar.WebApi.FilterLog
{
    public class FilterLog : Attribute, IAsyncActionFilter
    {
        private readonly IMapper _mapper;
        public FilterLog(IMapper mapper)
        {
            _mapper = mapper;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //transactionLog.DynamicDataBefore = context.ActionArguments.Values.FirstOrDefault().ToString();
            await next();
            if (context.HttpContext.Response.StatusCode == 200)
            {
                TransactionLog transactionLog = new TransactionLog()
                {
                    DynamicDataAfter = context.ActionArguments.Values.FirstOrDefault().ToString(),
                    HttpVerb = context.HttpContext.Request.Method,
                    Route = string.Join("/", context.RouteData.Values),
                    Date = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    Type = "teste",
                    UserId = "601065af923cf89b913aede9"
                };
                var p = context;
                //transactionLog.Date=
                //transactionLog.Type = context.HttpContext.Request.Path.Value.Split('/', 4).FirstOrDefault().;
                //transactionLog.UserId = context.HttpContext.Request.Path.Value.Split('/', 4);
                if (context.HttpContext.Request.Method == "PUTCH")
                {
                    //transactionLog.DynamicDataBefore



                }
            }

            //     public string UserId { get; set; }
            //public long Date { get; set; }
            //public string Route { get; set; }
            //public BsonDocument DynamicDataBefore { get; set; }
            //public BsonDocument DynamicDataAfter { get; set; }


            //[BsonRepresentation(BsonType.Int32)]
            //public TypeTransaction Type { get; set; }
            //[BsonRepresentation(BsonType.Int32)]
            //public TypeHttpVerb HttpVerb { get; set; }
            //public string CollectionName => nameof(TransactionLog);
        }
    }
}
