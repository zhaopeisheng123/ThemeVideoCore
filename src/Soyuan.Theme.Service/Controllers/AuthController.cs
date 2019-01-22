using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Soyuan.Theme.Contracts;
using Soyuan.Theme.Core.JWT;
using Soyuan.Theme.Service.Services.User;
using Soyuan.Theme.Service.Services.Application;

namespace Soyuan.Theme.Service.Controllers
{
    [Produces("application/json")]
    [Route("api/Auth")]
    public class AuthController : Controller
    {
        private readonly IUserService userService;

        private readonly IApplicationService applicationService;

        public AuthController(IUserService userService, IApplicationService applicationService)
        {
            this.userService = userService;
            this.applicationService = applicationService;
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="loginParam"></param>
        /// <returns></returns>
        [HttpPost(nameof(Login))]
        public async Task<ResultContract<LoginResultContract>> Login([FromBody]LoginParamContract loginParam)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(loginParam.Account))
                {
                    return new ResultContract<LoginResultContract> { Code = -1, Msg = "请输入账号" };
                }
                if (string.IsNullOrWhiteSpace(loginParam.Password))
                {
                    return new ResultContract<LoginResultContract> { Code = -1, Msg = "请输入密码" };
                }
                //用户验证
                var user = userService.CheckUser(loginParam.Account, loginParam.Password);
                if (user == null)
                {
                    return new ResultContract<LoginResultContract> { Code = -1, Msg = "账号或密码不正确" };
                }

                //平台验证
                var application = applicationService.GetApplicationByID(loginParam.AppId);

                if (application == null)
                {
                    return new ResultContract<LoginResultContract> { Code = -1, Msg = "平台未注册" };
                }

                //更改用户登录状态
                var userEntity = userService.GetUser(user.UserId);
                userEntity.IsLogin = true;
                userService.Update(userEntity);

                //生成token
                var model = new TokenDataModel
                {
                    UserId = user.UserId,
                    Account = user.UserAccount,
                    AppName = application.AppName,
                    FromSystem = ""
                };
                var token = JWT.GenerateToken(model, application.AppSecret);
                var result = new LoginResultContract()
                {
                    Token = token,
                    User = new Contracts.DB.UserContract()
                    {
                        UserId = user.UserId,
                        UserName = user.UserName,
                        OrganizationId = user.OrganizationId
                    }
                };
                return new Contracts.ResultContract<LoginResultContract>() { Code = 0, Msg = "", Data = result };
            }
            catch (Exception)
            {
                return new Contracts.ResultContract<LoginResultContract>() { Code = -1, Msg = "服务异常" }; ;
            }
        }

        /// <summary>
        /// 注销登录
        /// </summary>
        /// <returns></returns>
        [HttpPost(nameof(Logout))]
        public async Task<ResultContract<string>> Logout()
        {
            string userIdString = User.Claims.FirstOrDefault(i => i.Type == "UserId")?.Value;

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return new ResultContract<string>() { Code = -1, Msg = "注销失败" };
            }
            var user = userService.GetUser(userId);
            if (user==null)
            {
                return new ResultContract<string>() { Code = -1, Msg = "注销失败" };
            }

            user.IsLogin = false;

            userService.Update(user);

            return new ResultContract<string>() { Code = 0, Msg = "注销成功" }; 
        }

    }
}