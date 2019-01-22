using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Soyuan.Theme.Core.JWT
{
    public class JWT
    {
        /// <summary>
        /// 对用户信息加密得到token
        /// </summary>
        /// <param name="model">用户信息模型</param>
        /// <param name="secret">密码</param>
        /// <returns>token字符串</returns>
        public static string GenerateToken(TokenDataModel model, string secret)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var authTime = DateTime.Now;
            var expiresAt = authTime.AddDays(3);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("Account",model.Account),
                    //new Claim("OrganizationId",model.OrganizationId==null?"":model.OrganizationId.ToString()),
                    //new Claim("DepartmentId",model.DepartmentId==null?"":model.DepartmentId.ToString()),
                    new Claim("UserId",model.UserId.ToString()),
                    new Claim("FromSystm",model.FromSystem),
                    new Claim("AppName",model.AppName),
                    new Claim("Jti",Guid.NewGuid().ToString())
                }),
                Expires = expiresAt,
                Audience = model.AppName,
                NotBefore = DateTime.Now,
                Issuer = "ThemeVideo",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return tokenString;
        }

    }
}
