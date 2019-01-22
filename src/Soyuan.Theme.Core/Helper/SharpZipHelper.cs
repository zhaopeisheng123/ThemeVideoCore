using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Soyuan.Theme.Core.Helper
{
    public class SharpZipHelper
    {
        private static SharpZipHelper SharpZipInstance;

        public static SharpZipHelper GetInstance()
        {
            // 如果类的实例不存在则创建，否则直接返回
            if (SharpZipInstance == null)
            {
                SharpZipInstance = new SharpZipHelper();
            }

            return SharpZipInstance;
        }
        public void Zip(string zipPath,string uploadFilePath)
        {
            //压缩文件

            //得到一个压缩文件,流
            FileStream zipFile = new FileStream(zipPath, FileMode.Create);

            //创建一个压缩流,写入压缩流中的内容，自动被压缩
            ZipOutputStream zos = new ZipOutputStream(zipFile);


            //当前目录
            DirectoryInfo di = new DirectoryInfo(uploadFilePath);

            FileInfo[] files = di.GetFiles("*");

            byte[] buffer = new byte[10 * 1024];

            foreach (FileInfo fi in files)
            {
                //第一步，写入压缩的说明
                ZipEntry entry = new ZipEntry(fi.Name);

                entry.Size = fi.Length;

                //保存
                zos.PutNextEntry(entry);

                //第二步，写入压缩的文件内容

                int length = 0;

                Stream input = fi.Open(FileMode.Open);
                while ((length = input.Read(buffer, 0, 10 * 1024)) > 0)
                {
                    zos.Write(buffer, 0, length);
                }

                input.Close();
                fi.Delete();
            }

            zos.Finish();
            zos.Close();

        }
    }
}
