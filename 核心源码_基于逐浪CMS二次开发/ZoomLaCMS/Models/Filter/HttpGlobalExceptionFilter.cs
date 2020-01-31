using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZoomLa.Components;

namespace ZoomLaCMS.Filter
{
    public class HttpGlobalExceptionFilter : IExceptionFilter
    {
        //private ILog log = LogManager.GetLogger(Startup.repository.Name, typeof(HttpGlobalExceptionFilter));
        public void OnException(ExceptionContext context)
        {
            //log.Error(context.Exception);
            CMSLog.L(context.Exception.Message,"exception");
            throw new Exception(context.Exception.Message);
        }
    }
}
