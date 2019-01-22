using System;
using System.Security.Cryptography;
using System.Text;

namespace Soyuan.Theme.Core.Security
{
    public class SecurityHelper
    {
        /// <summary>
        /// SHA256加密
        /// </summary>
        /// <param name="inputStr">加密的字符串</param>
        /// <param name="code">长度</param>
        /// <returns></returns>
        public static string GetSha256Hash(string inputStr, int code)
        {
            var hash = string.Empty;
            // Send a sample text to hash.  
            var bytes = Encoding.UTF8.GetBytes(inputStr);
            if (bytes.Length > 8)
            {
                using (var sha256 = SHA256.Create())
                {
                    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(inputStr), 0, 8);
                    // Get the hashed string.  
                    hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
                }
            }
            else
            {
                using (var md5 = MD5.Create())
                {
                    var result = md5.ComputeHash(Encoding.ASCII.GetBytes(inputStr));
                    var strResult = BitConverter.ToString(result);
                    hash = strResult.Replace("-", "");
                }
            }
            if (code == 16)
            {
                return hash.Substring(8, 16);
            }
            else
            {
                return hash.Substring(0, 32);
            }
        }

        /// <summary>
        /// Md5
        /// </summary>
        /// <param name="inputStr"></param>
        /// <returns></returns>
        public static string Str2Md5(string inputStr)
        {
            using (var md5 = MD5.Create())
            {
                byte[] data = Encoding.ASCII.GetBytes(inputStr);
                byte[] md5Data = md5.ComputeHash(data);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < md5Data.Length; i++)
                {
                    sb.Append(md5Data[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
