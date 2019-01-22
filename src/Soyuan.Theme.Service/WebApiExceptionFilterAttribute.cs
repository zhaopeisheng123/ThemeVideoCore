using Microsoft.AspNetCore.Mvc.Filters;
using Soyuan.Theme.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Soyuan.Theme.Service
{
    public class WebApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext actionExecutedContext)
        {
            LogHelper.logError(actionExecutedContext.Exception.StackTrace);
            base.OnException(actionExecutedContext);
        }
    }
}
