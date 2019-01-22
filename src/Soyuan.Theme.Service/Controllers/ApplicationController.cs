using Microsoft.AspNetCore.Mvc;
using Soyuan.Theme.Contracts;
using Soyuan.Theme.Contracts.DB;
using Soyuan.Theme.Core.Helper;
using Soyuan.Theme.Service.Services.Application;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soyuan.Theme.Service.Controllers
{
    [Produces("application/json")]
    [Route("api/Application")]
    public class ApplicationController : Controller
    {

        private readonly IApplicationService applicationService;

        public ApplicationController(IApplicationService applicationService)
        {
            this.applicationService = applicationService;
        }

        /// <summary>
        /// 注册应用
        /// </summary>
        /// <param name="registerApplicationData"></param>
        /// <returns></returns>
        [HttpPost(nameof(RegisterApplication))]
        public async Task<ResultContract<Guid?>> RegisterApplication([FromBody]RegisterApplicationData registerApplicationData)
        {
            try
            {
                var result = applicationService.RegisterApplication(registerApplicationData);
                return result;
            }
            catch (Exception e)
            {
                return new Contracts.ResultContract<Guid?>() { Code = -1, Msg = "服务异常" }; ;
            }
        }

        /// <summary>
        /// 根据条件查询所有系统
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        [HttpPost(nameof(GetApplication))]
        public async Task<ResultContract<List<ApplicationContract>>> GetApplication([FromBody] ApplicationContract contract)
        {
            var result = new ResultContract<List<ApplicationContract>>() { Code = 0, Msg = "查询成功" };
            try
            {
                if (contract==null)
                {
                    contract = new ApplicationContract();
                }
                result.Data = applicationService.GetApplication(contract);
            }
            catch (Exception e)
            {
                LogHelper.logError("查询系统失败：" + e.StackTrace);
                result.Code = -1;
                result.Msg = e.StackTrace;
            }
            
            return result;
        }
    }
}