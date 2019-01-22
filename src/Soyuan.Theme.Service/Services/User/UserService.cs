using Soyuan.Theme.Contracts.DB;
using Soyuan.Theme.Core.Helper;
using Soyuan.Theme.Core.Security;
using Soyuan.Theme.Domain;
using Soyuan.Theme.Domain.Entity;
using System;
using System.Linq;

namespace Soyuan.Theme.Service.Services.User
{
    public class UserService : IUserService
    {
        private readonly ThemeDBContext _db;

        public UserService(ThemeDBContext db)
        {
            this._db = db;
        }

        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public UserContract AddUser(UserContract user)
        {
            user.UserId = Guid.NewGuid();
            var entity = user.MapTo<UserEntity>();
            entity.CreateTime = DateTime.Now;
            entity.Secretkey = SecurityHelper.GetSha256Hash(WatchHelper.CreateNo(), 16).ToLower();
            entity.Password = SecurityHelper.GetSha256Hash(DesEncrypt.Encrypt(SecurityHelper.GetSha256Hash(user.Password, 32).ToLower(), entity.Secretkey).ToLower(), 32).ToLower();
            this._db.User.Add(entity);
            this._db.SaveChanges();
            return user;
        }

        /// <summary>
        /// 检查用户
        /// </summary>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public UserContract CheckUser(string account, string password)
        {
            var user = this._db.User.FirstOrDefault(e=>e.UserAccount == account);
            if (user!=null)
            {
                string encryPassword = SecurityHelper.GetSha256Hash(DesEncrypt.Encrypt(SecurityHelper.GetSha256Hash(password, 32).ToLower(), user.Secretkey).ToLower(), 32).ToLower();

                if (encryPassword == user.Password)
                {
                    return user.MapTo<UserContract>();
                }
            }

            return null;
        }

        /// <summary>
        /// 获取用户
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public UserEntity GetUser(Guid? userId)
        {
            return this._db.User.FirstOrDefault(e=>e.UserId == userId);
        }

        /// <summary>
        /// 更改用户
        /// </summary>
        /// <param name="userEntity"></param>
        public void Update(UserEntity userEntity)
        {
            this._db.User.Update(userEntity);
            this._db.SaveChanges();
        }
    }
}
