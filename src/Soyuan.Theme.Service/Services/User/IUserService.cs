using Soyuan.Theme.Contracts.DB;
using Soyuan.Theme.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Soyuan.Theme.Service.Services.User
{
    public interface IUserService
    {
        /// <summary>
        /// 检查用户
        /// </summary>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        UserContract CheckUser(string account, string password);

        /// <summary>
        /// 增加用户
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        UserContract AddUser(UserContract user);

        /// <summary>
        /// 根据id获取用户
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        UserEntity GetUser(Guid? userId);

        /// <summary>
        /// 更改用户
        /// </summary>
        /// <param name="userEntity"></param>
        void Update(UserEntity userEntity);
    }
}
