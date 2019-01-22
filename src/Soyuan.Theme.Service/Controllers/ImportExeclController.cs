using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Soyuan.Theme.Business;
using Soyuan.Theme.Contracts;
using Soyuan.Theme.Contracts.DB;
using Soyuan.Theme.Core.Helper;
using Soyuan.Theme.Service.Services.Application;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Soyuan.Theme.Service.Controllers
{
    [Produces("application/json")]
    [Route("api/Export")]
    public class ImportExeclController : Controller
    {
        private readonly IApplicationService applicationService;
        private readonly UploadLogic logic = new UploadLogic();
        private readonly string downloadPath = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "download" + Path.DirectorySeparatorChar;

        public ImportExeclController(IApplicationService applicationService)
        {
            this.applicationService = applicationService;
            if (!Directory.Exists(downloadPath))
            {
                Directory.CreateDirectory(downloadPath);
            }
        }
        /// <summary>
        /// 旅检人脸表格导出EXECL
        /// </summary>
        /// <param name="esid"></param>
        /// <returns></returns>
        [HttpGet(nameof(ExportData))]
        public async Task<IActionResult> ExportData(string ids)//
        {
            var result = new ResultContract<ResultContract<string>>() { Code = 0, Msg = "导出成功" };

            try
            {
                DataTable tblDatas = new DataTable("Datas");
                DataColumn dc = null;
                dc = tblDatas.Columns.Add("序号", Type.GetType("System.Int32"));
                dc.AutoIncrement = true;//自动增加
                dc.AutoIncrementSeed = 1;//起始为1
                dc.AutoIncrementStep = 1;//步长为1
                dc.AllowDBNull = false;//

                dc = tblDatas.Columns.Add("来源", Type.GetType("System.String"));
                dc = tblDatas.Columns.Add("所属系统", Type.GetType("System.String"));
                dc = tblDatas.Columns.Add("时间", Type.GetType("System.String"));
                dc = tblDatas.Columns.Add("图片", Type.GetType("System.String"));
                List<string> idList = ids.Split(",").ToList();
                foreach (var id in idList)
                {
                    var returnResult = logic.GetTagsInfo(id);
                    EsContract contract = SerializeHelper.deserializeToObject<EsContract>(returnResult.Data.ToString());
                    DataRow newRow;
                    newRow = tblDatas.NewRow();
                    newRow["来源"] = contract.orgName;
                    ApplicationContract appContract = new ApplicationContract();
                    appContract.Id = Guid.Parse(contract.appId);
                    var applist = applicationService.GetApplication(appContract);
                    newRow["所属系统"] = applist.Count > 0 ? applist[0].AppName : "";
                    DateTime time = (DateTime)contract.uploadTime;
                    newRow["时间"] = time.ToString("yyyy-MM-dd hh:mm:ss");
                    newRow["图片"] = contract.url;
                    tblDatas.Rows.Add(newRow);
                }

                MemoryStream ms = ExportHelper.RenderDataTableToExcel(tblDatas) as MemoryStream;

                return File(ms.ToArray(), "application/vnd.ms-excel", "ExportImage.xls");

            }
            catch (Exception e)
            {
                LogHelper.logError("查询ES失败：" + e.StackTrace);
                result.Code = -1;
                result.Msg = e.StackTrace;
            }
            return null;
        }

        /// <summary>
        /// 批量下载
        /// </summary>
        [HttpGet(nameof(Batchdownload))]
        public async Task<IActionResult> Batchdownload(string urls)
        {
            var uploadFilePath = AppDomain.CurrentDomain.BaseDirectory + "BatchDownload";
            //判断文件夹是否存在
            if (!Directory.Exists(uploadFilePath))
            {
                Directory.CreateDirectory(uploadFilePath);
            }
            List<string> urlList = urls.Split(",").ToList();
            foreach (var url in urlList)
            {
                string filename = "";
                DownFile(url, uploadFilePath, ref filename);
            }
            string zipPath = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "Downloads.zip";
            if (System.IO.File.Exists(zipPath))
            {
                System.IO.File.Delete(zipPath);
            }
            SharpZipHelper.GetInstance().Zip(zipPath, uploadFilePath);
            FileStream ms = new FileStream(zipPath, FileMode.Open);
            return File(ms, "application/octet-stream", "Downloads.zip");
        }



        /// <summary>
        /// 下载单个文件
        /// </summary>
        /// <param name="imgurl"></param>
        /// <returns></returns>
        [HttpGet(nameof(DownloadImg))]
        public async Task<IActionResult> DownloadImg(string imgurl)
        {
            string filename = Path.GetFileName(imgurl);
            string mimeType = MimeHelper.GetMimeMapping(imgurl);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(imgurl);
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            Stream responseStream = response.GetResponseStream();
            return File(responseStream, mimeType, filename);
        }

        [HttpPost(nameof(GetFileLength))]
        public async Task<ResultContract<long>> GetFileLength(string name)
        {
            var result = new ResultContract<long>() { Code = 0, Msg = "获取成功" };
            try
            {
                long fileLength = AliyunOSSHepler.GetInstance().GetFileLength(name);
                result.Data = fileLength;
            }
            catch (Exception e)
            {
                result.Msg = "获取失败";
                result.Code = -1;
            }

            return result;
        }

        /// <summary>
        /// 断点下载到服务器然后在推送到页面
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet(nameof(BreakpointDownload))]
        public async Task<IActionResult> BreakpointDownload(string name)
        {
            //先获取文件大小
            var newName = Guid.NewGuid() + "-" + name;
            long fileLength = AliyunOSSHepler.GetInstance().GetFileLength(name);
            string mimeType = MimeHelper.GetMimeMapping(name);
            int partSize = 1024 * 1024 * 10;
            var partCount = AliyunOSSHepler.GetInstance().CalPartCount(fileLength, partSize);
            for (var i = 0; i < partCount; i++)
            {
                var startPos = partSize * i;
                var endPos = partSize * i + (partSize < (fileLength - startPos) ? partSize : (fileLength - startPos)) - 1;
                AliyunOSSHepler.GetInstance().Download(downloadPath, startPos, endPos, name, newName);
            }
            var file = new byte[fileLength];
            using (var fileStream = new FileStream(downloadPath + newName, FileMode.Open))
            {
                fileStream.Read(file, 0, file.Length);
            }

            System.IO.File.Delete(downloadPath + newName);
            return File(file, mimeType, name);
        }

        /// <summary>
        /// 通过其实位置和结束位置返回文件
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost(nameof(GetBreakpoint))]
        public async Task<ResultContract<byte[]>> GetBreakpoint(string name, long startPos, long endPos)
        {
            var result = new ResultContract<byte[]>() { Code = 0, Msg = "获取成功" };
            //先获取文件大小
            try
            {
                var newName = Guid.NewGuid() + "-" + name;
                long fileLength = AliyunOSSHepler.GetInstance().GetFileLength(name);
                string mimeType = MimeHelper.GetMimeMapping(name);
                AliyunOSSHepler.GetInstance().Download(downloadPath, startPos, endPos, name, newName);
                var file = new byte[endPos - startPos];
                using (var fileStream = new FileStream(downloadPath + newName, FileMode.Open))
                {
                    fileStream.Read(file, 0, file.Length);
                }

                System.IO.File.Delete(downloadPath + newName);

                result.Data = file;
            }
            catch (Exception e)
            {
                result.Msg = "获取失败";
                result.Code = -1;
            }

            return result;
        }


        /// <summary>
        /// 下载文件到本地
        /// </summary>
        /// <param name="uRLAddress"></param>
        /// <param name="localPath"></param>
        /// <param name="filename"></param>
        public void DownFile(string uRLAddress, string localPath, ref string filename)
        {
            filename = Path.GetFileName(uRLAddress);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uRLAddress);
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            Stream responseStream = response.GetResponseStream();
            Stream stream = new FileStream(localPath + Path.DirectorySeparatorChar + filename, FileMode.Create);
            byte[] bArr = new byte[1024];
            int size = responseStream.Read(bArr, 0, bArr.Length);
            while (size > 0)
            {
                stream.Write(bArr, 0, size);
                size = responseStream.Read(bArr, 0, bArr.Length);
            }
            stream.Close();
            responseStream.Close();
        }
    }
}
