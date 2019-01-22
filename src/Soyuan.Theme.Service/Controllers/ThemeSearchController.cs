using Microsoft.AspNetCore.Mvc;
using Soyuan.Theme.Business;
using Soyuan.Theme.Contracts;
using Soyuan.Theme.Core.Helper;
using Soyuan.Theme.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Soyuan.Theme.Service.Controllers
{
    [Produces("application/json")]
    [Route("api/Theme")]
    public class ThemeSearchController : Controller
    {
        private readonly UploadLogic logic = new UploadLogic();

        /// <summary>
        /// 查询指定
        /// </summary>
        /// <param name="esid"></param>
        /// <returns></returns>
        [HttpGet(nameof(GetEsById))]
        public async Task<ResultContract<object>> GetEsById(string fileId)
        {
            var result = new ResultContract<object>() { Code = 0, Msg = "查询成功" };
            try
            {
                result = logic.GetTagsInfo(fileId);

            }
            catch (Exception e)
            {
                LogHelper.logError("查询ES失败：" + e.StackTrace);
                result.Code = -1;
                result.Msg = e.StackTrace;
            }
            return result;

        }

        /// <summary>
        /// 根据条件查询ES
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        [HttpPost(nameof(GetTagsByCondition))]
        public async Task<ResultContract<object>> GetTagsByCondition([FromBody]ThemeSearchContract contract)
        {
            var result = new ResultContract<object>() { Code = 0, Msg = "查询成功" };
            try
            {
                result = logic.GetTagsByCondition(contract);
            }
            catch (Exception e)
            {
                LogHelper.logError("条件查询ES失败：" + e.ToString());
                result.Code = -1;
                result.Msg = e.Message;
            }

            return result;
        }


        /// <summary>
        /// 分组查询总数
        /// </summary>
        /// <returns></returns>
        [HttpGet(nameof(GetTasByGroup))]
        public async Task<ResultContract<object>> GetTasByGroup()
        {
            var result = new ResultContract<object>() { Code = 0, Msg = "查询成功" };
            try
            {
                result = logic.GetTasByGroup();
            }
            catch (Exception e)
            {
                LogHelper.logError("查询全部ES失败：" + e.StackTrace);
                result.Code = -1;
                result.Msg = e.StackTrace;
            }

            return result;
        }
    }
}
