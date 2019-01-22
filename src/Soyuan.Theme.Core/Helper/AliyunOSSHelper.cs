using Aliyun.OSS;
using Aliyun.OSS.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Aliyun.OSS.Common;

namespace Soyuan.Theme.Core.Helper
{
    public class AliyunOSSHepler
    {
        private static AliyunOSSHepler AliyunInstance;
        private string m_endPoint, m_keyID, m_keySecret, m_bucket, m_ossPre;
        private int m_multiSize, m_blockSize;

        public AliyunOSSHepler()
        {
            m_endPoint = ConfigHelper.ReadConfigByName("endPoint");
            m_keyID = ConfigHelper.ReadConfigByName("keyID");
            m_keySecret = ConfigHelper.ReadConfigByName("keySecret");
            m_bucket = ConfigHelper.ReadConfigByName("bucket");
            m_ossPre = "http://" + m_bucket + "." + m_endPoint;
            m_blockSize = 200 * 1024;
            m_multiSize = 2 * m_blockSize;
        }

        public static AliyunOSSHepler GetInstance()
        {
            // 如果类的实例不存在则创建，否则直接返回
            if (AliyunInstance == null)
            {
                AliyunInstance = new AliyunOSSHepler();
            }

            return AliyunInstance;
        }

        /// <summary>
        /// 上传文件成功后删除本地资源
        /// </summary>
        /// <param name="workDir">文件存放根目录，直接从配置文件里读取，不在数据库中获取</param>
        /// <param name="paths">文件相对（workDir)路径列表</param>
        /// <returns></returns>
        public bool UploadFiles(string workDir, string paths, ref string osspath, bool isDelete = false)
        {
            string path = workDir.Replace("\\", "/");
            path = path.TrimEnd('/');
            osspath = m_ossPre + "/" + paths;
            if (uploadFile(path, paths))
            {
                try
                {
                    string FilePath = path + "/" + paths;
                    if (File.GetAttributes(FilePath).ToString().IndexOf("ReadOnly") != -1)
                    {
                        File.SetAttributes(FilePath, FileAttributes.Normal); // 如果是只读文件改变文件为正常文件
                    }

                    if (isDelete)
                    {
                        File.Delete(FilePath);//直接删除文件  
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("UploadFiles删除失败：" + ex.Message);
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 文件上传阿里云
        /// </summary>
        /// <param name="workDir"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool uploadFile(string workDir, string path)
        {
            string localPath = workDir + "/" + path;
            try
            {
                var fi = new FileInfo(localPath);
                long fileSize = fi.Length;
                if (fileSize > m_multiSize)
                {
                    return uploadMulti(localPath, path, fileSize);
                }
                else
                {
                    string md5;
                    using (var fs = File.Open(localPath, FileMode.Open, FileAccess.Read))
                    {
                        md5 = OssUtils.ComputeContentMd5(fs, fs.Length);//md5效验
                    }
                    return uploadSingle(localPath, path, md5);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("获取文件信息错误：" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 上传单个文件
        /// </summary>
        /// <param name="localPath"></param>
        /// <param name="path"></param>
        /// <param name="md5"></param>
        /// <returns></returns>
        private bool uploadSingle(string localPath, string path, string md5)
        {
            string endPoint = "http://" + m_endPoint;
            var client = new OssClient(endPoint, m_keyID, m_keySecret);
            try
            {
                var objectMeta = new ObjectMetadata
                {
                    ContentMd5 = md5
                };
                client.PutObject(m_bucket, path, localPath, objectMeta);
            }
            catch (Exception ex)
            {
                Console.WriteLine("上传单个文件失败：" + ex.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 分片上传
        /// </summary>
        /// <param name="localPath"></param>
        /// <param name="path"></param>
        /// <param name="fileSize"></param>
        /// <returns></returns>
        private bool uploadMulti(string localPath, string path, long fileSize)
        {
            string endPoint = "http://" + m_endPoint;
            var client = new OssClient(endPoint, m_keyID, m_keySecret);
            try
            {
                var request = new InitiateMultipartUploadRequest(m_bucket, path);
                var result = client.InitiateMultipartUpload(request);
                string uploadId = result.UploadId;

                int partSize = (int)(fileSize / 9990);
                if (partSize < m_blockSize)
                {
                    partSize = m_blockSize;
                }
                int partCount = (int)(fileSize / partSize);
                var partETags = new List<PartETag>();
                using (var fs = File.Open(localPath, FileMode.Open, FileAccess.Read))
                {
                    for (int i = 0; i < partCount; i++)
                    {
                        long skipBytes = partSize * i;
                        fs.Seek(skipBytes, 0);
                        long size = (i < partCount - 1) ? partSize : (fileSize - skipBytes);
                        var req = new UploadPartRequest(m_bucket, path, uploadId)
                        {
                            InputStream = fs,
                            PartSize = size,
                            PartNumber = i + 1
                        };
                        var res = client.UploadPart(req);
                        partETags.Add(res.PartETag);
                    }
                }

                var completeMultipartUploadRequest = new CompleteMultipartUploadRequest(m_bucket, path, uploadId);
                foreach (var partETag in partETags)
                {
                    completeMultipartUploadRequest.PartETags.Add(partETag);
                }
                client.CompleteMultipartUpload(completeMultipartUploadRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine("分片上传失败：" + ex.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 分块下载
        /// </summary>
        /// <param name="bufferedStream"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <param name="localFilePath"></param>
        /// <param name="bucketName"></param>
        /// <param name="fileKey"></param>
        public void Download(string downloadPath, long startPos, long endPos, String fileKey, string createFileName)
        {

            Stream contentStream = null;
            using (var fileStream = new FileStream(downloadPath + createFileName, FileMode.OpenOrCreate))
            {
                var bufferedStream = new BufferedStream(fileStream);
                try
                {
                    OssClient client = new OssClient(m_endPoint, m_keyID, m_keySecret);
                    var getObjectRequest = new GetObjectRequest(m_bucket, fileKey);
                    getObjectRequest.SetRange(startPos, endPos);
                    var ossObject = client.GetObject(getObjectRequest);
                    byte[] buffer = new byte[1024 * 1024];
                    var bytesRead = 0;
                    bufferedStream.Seek(startPos, SeekOrigin.Begin);
                    contentStream = ossObject.Content;
                    while ((bytesRead = contentStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        bufferedStream.Write(buffer, 0, bytesRead);
                    }
                }
                finally
                {
                    if (contentStream != null)
                    {
                        bufferedStream.Flush();
                        contentStream.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// 计算下载的块数
        /// </summary>
        /// <param name="fileLength"></param>
        /// <param name="partSize"></param>
        /// <returns></returns>
        public int CalPartCount(long fileLength, long partSize)
        {
            var partCount = (int)(fileLength / partSize);
            if (fileLength % partSize != 0)
            {
                partCount++;
            }
            return partCount;
        }

        /// <summary>
        /// 获取文件长度
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public long GetFileLength(string name)
        {
            OssClient client = new OssClient(m_endPoint, m_keyID, m_keySecret);
            var objectMetadata = client.GetObjectMetadata(m_bucket, name);
            var fileLength = objectMetadata.ContentLength;
            return fileLength;
        }

        public string GetPicFromVideo(string VideoName, string PicName, string CutTimeFrame)
        {
            var root = AppDomain.CurrentDomain.BaseDirectory;
            var Arguments = " -i " + VideoName                    //视频路径
                              + " -r 1"                               //提取图片的频率
                              + " -y -f image2 -ss " + CutTimeFrame   //设置开始获取帧的视频时间
                              + @" " + PicName;  //输出的图片文件名，路径前必须有空格

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var psi = new ProcessStartInfo(root + @"cmd\\ffmpeg.exe", @" " + Arguments) { UseShellExecute = false, RedirectStandardInput = true };

                    var proc = Process.Start(psi);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    var psi = new ProcessStartInfo(@"ffmpeg", @" " + Arguments) { UseShellExecute = false, RedirectStandardInput = true };

                    Process.Start(psi);
                }

                System.Threading.Thread.Sleep(1000);
            }
            catch (Exception e)
            {
                LogHelper.logError("截图失败：" + e.Message + "____" + e.ToString());
                throw e;
            }




            //返回视频图片完整路径
            if (System.IO.File.Exists(PicName))
                return PicName;
            return "";
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="fileId"></param>
        public void DeleteObject(string fileId)
        {
            try
            {
                OssClient client = new OssClient(m_endPoint, m_keyID, m_keySecret);
                // 删除文件。
                client.DeleteObject(m_bucket, fileId);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

    }
}
