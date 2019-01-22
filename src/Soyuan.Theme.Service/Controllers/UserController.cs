using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Soyuan.Theme.Contracts;
using Soyuan.Theme.Contracts.DB;
using Soyuan.Theme.Core.Helper;
using Soyuan.Theme.Service.Services.User;

namespace Soyuan.Theme.Service.Controllers
{
    [Produces("application/json")]
    [Route("api/User")]
    public class UserController : Controller
    {
        private readonly IUserService userService;

        public UserController(IUserService userService)
        {
            this.userService = userService;
        }

        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost(nameof(AddUser))]
        public async Task<ResultContract<UserContract>> AddUser([FromBody]UserContract user)
        {
            try
            {
                var result = userService.AddUser(user);
                return new ResultContract<UserContract>() { Code = 0, Msg = "", Data = result };
            }
            catch (Exception)
            {
                return new ResultContract<UserContract>() { Code = -1, Msg = "服务异常" };
            }
        }

        /// <summary>
        /// 获取用户
        /// </summary>
        /// <returns></returns>
        [HttpPost(nameof(GetUser))]
        public async Task<ResultContract<UserContract>> GetUser(Guid userId)
        {
            try
            {
                var result = userService.GetUser(userId);
                return new ResultContract<UserContract>()
                {
                    Code = 0,
                    Msg = "",
                    Data = new UserContract()
                    {
                        UserId = result.UserId,
                        UserName = result.UserName,
                        OrganizationId = result.OrganizationId,
                    }
                };
            }
            catch (Exception)
            {
                return new ResultContract<UserContract>() { Code = -1, Msg = "" };
            }

        }


    }
}