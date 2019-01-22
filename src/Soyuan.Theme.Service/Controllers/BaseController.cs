using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;

namespace Soyuan.Theme.Service.Controllers
{


    [EnableCors("AllowAllOrigin")]//跨域
    [WebApiExceptionFilterAttribute]
    public class BaseController : Controller
    {

        
    }
}