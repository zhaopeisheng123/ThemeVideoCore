using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Soyuan.Theme.Core.Helper
{
    public class FileHelper
    {
        /// <summary>
        /// 计算文件大小函数(保留两位小数),Size为字节大小
        /// </summary>
        /// <param name="Size">初始文件大小</param>
        /// <returns></returns>       
        public static string CountSize(long Size)
        {
            string m_strSize = "";
            long FactSize = 0;
            FactSize = Size;
            if (FactSize < 1024.00)
                m_strSize = FactSize.ToString("F2") + " Byte";
            else if (FactSize >= 1024.00 && FactSize < 1048576)
                m_strSize = (FactSize / 1024.00).ToString("F2") + " K";
            else if (FactSize >= 1048576 && FactSize < 1073741824)
                m_strSize = (FactSize / 1024.00 / 1024.00).ToString("F2") + " M";
            else if (FactSize >= 1073741824)
                m_strSize = (FactSize / 1024.00 / 1024.00 / 1024.00).ToString("F2") + " G";
            return m_strSize;
        }

        /// <summary>
        /// 獲取文件大小
        /// </summary>
        /// <param name="Size">初始文件大小</param>
        /// <returns></returns>       
        public static double CountSizeM(long Size)
        {
            return (Size / 1024.00 / 1024.00);
        }

        public static void SavaFile(string fileLocalFullName, Stream file, ref double fileSize)
        {
            //保存文件到本地
            using (FileStream fileStream = new FileStream(fileLocalFullName, FileMode.Create, FileAccess.Write))
            {
                int fsLen = (int)file.Length;
                byte[] fbyte = new byte[fsLen];
                file.Read(fbyte, 0, fsLen);
                fileStream.Write(fbyte, 0, fsLen);
                fileSize = CountSizeM(fbyte.LongLength);
            }
        }

        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static double GetSizeM(string path)
        {
            var fileInfo = new System.IO.FileInfo(path);
            return CountSizeM(fileInfo.Length);
        }
    }
}
