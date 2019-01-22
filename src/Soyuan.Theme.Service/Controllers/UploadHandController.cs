using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Soyuan.Theme.Contracts;
using Soyuan.Theme.Service.Services.UploadHand;
using Soyuan.Theme.Business;
using System.Net.Http;
using System.IO;
using Soyuan.Theme.Core.Helper;
using Microsoft.AspNetCore.Cors;
using Soyuan.Theme.Contracts.DB;
using Soyuan.Theme.Contracts.Enum;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Soyuan.Theme.Service.Controllers
{
    [Produces("application/json")]
    [Route("api/UploadHand")]
    public class UploadHandController : Controller
    {
        private readonly UploadLogic logic = new UploadLogic();
        private readonly IUploadHandService _uploadHandService;
        private readonly string KafkaTopic = ConfigHelper.ReadConfigByName("KafkaUploadTopic");
        private readonly string FailurePath = AppDomain.CurrentDomain.BaseDirectory + ConfigHelper.ReadConfigByName("FailurePath");
        private readonly string uploadFilePath = AppDomain.CurrentDomain.BaseDirectory + ConfigHelper.ReadConfigByName("UploadFilePath");

        public UploadHandController(IUploadHandService uploadHandService)
        {
            _uploadHandService = uploadHandService;
            //判断失败文件夹是否存在
            if (!Directory.Exists(FailurePath))
            {
                Directory.CreateDirectory(FailurePath);
            }

            if (!Directory.Exists(uploadFilePath))
            {
                Directory.CreateDirectory(uploadFilePath);
            }

        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost(nameof(UploadFile))]
        public async Task<ResultContract<string>> UploadFile(BaseUploadContract data)
        {
            var result = new ResultContract<string>() { Code = 0, Msg = "上传成功" };
            try
            {
                //验证参数
                var orgInfo = new OrganizationContract();
                this.CheckParameter(data, ref result, ref orgInfo);
                if (result.Code == -1) return result;

                List<UploadContract> fileList = new List<UploadContract>();
                var files = Request.Form.Files;
                var groupId = Guid.NewGuid().ToString();
                int order = 1;
                foreach (var file in files)
                {
                    var fileModel = new UploadContract()
                    {
                        UserId = data.UserId,
                        OrgId = data.OrgId,
                        AppId = data.AppId,
                        Tags = data.Tags,
                        UploadTime = DateTime.Now,
                        DisplayName = data.DisplayName,
                        Remark = data.Remark,
                        OrgName = orgInfo.OrganizationName,
                        Order = order,
                        ThemeType = data.ThemeType
                    };

                    //分组id 
                    fileModel.GroupId = groupId;
                    var fileData = new MultipartFormDataContent();

                    //判断文件夹是否存在
                    if (!Directory.Exists(uploadFilePath))
                    {
                        Directory.CreateDirectory(uploadFilePath);
                    }
                    var imgPath = AppDomain.CurrentDomain.BaseDirectory + ConfigHelper.ReadConfigByName("ImgsPath");
                    if (!Directory.Exists(imgPath))
                    {
                        Directory.CreateDirectory(imgPath);
                    }

                    fileModel.FileName = file.FileName;
                    var lastName = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1, (file.FileName.Length - file.FileName.LastIndexOf(".") - 1)); //扩展名
                    fileModel.FileId = Guid.NewGuid();
                    fileModel.NewFileName = fileModel.FileId + "." + lastName;
                    var fileLocalFullName = uploadFilePath + Path.DirectorySeparatorChar + fileModel.NewFileName;
                    var stream = file.OpenReadStream();
                    //文件保存到本地
                    double fileSize = 0;
                    //保存文件
                    FileHelper.SavaFile(fileLocalFullName, stream, ref fileSize);
                    //文件上传成功 修改model
                    fileModel.IsUpload = false;
                    fileModel.Url = fileLocalFullName;
                    fileModel.FileType = lastName;
                    //组织名称。后期加上
                    //判断文件大小
                    double uploadLimitSize = 0;
                    double.TryParse(ConfigHelper.ReadConfigByName("UploadLimitSizeM"), out uploadLimitSize);

                    //如果是视频截取封面
                    if (CommonDictionary.GetInstance().VideoType.Count(d => d.ToLower() == lastName.ToLower()) > 0)
                    {

                        var thumbnailPath = AliyunOSSHepler.GetInstance().GetPicFromVideo(fileLocalFullName, imgPath + Path.DirectorySeparatorChar + fileModel.FileId + ".jpg", "1");
                        var thumbnailUrl = "";
                        var isUpload = AliyunOSSHepler.GetInstance().UploadFiles(imgPath, fileModel.FileId + ".jpg", ref
                                  thumbnailUrl, true);

                        fileModel.ThumbnailUrl = thumbnailPath;

                        if (isUpload && !string.IsNullOrWhiteSpace(thumbnailUrl))
                        {
                            fileModel.ThumbnailUrl = thumbnailUrl;
                        }
                        else
                        {
                            fileModel.IsFailure = true;
                            this._uploadHandService.SavaTagsFile(fileModel);
                            //把文件 移动到错误文件 文件夹
                            System.IO.File.Move(uploadFilePath + Path.DirectorySeparatorChar + fileModel.NewFileName, FailurePath + Path.DirectorySeparatorChar + fileModel.NewFileName);
                            result.Code = -1;
                            result.Msg = "截图失败";
                            return result;
                        }
                    }

                    //如果断网把标签和文件都存到本地
                    if (!CommonDictionary.GetInstance().KafkaIsOnline)
                    {
                        this._uploadHandService.SavaTagsFile(fileModel);
                    }
                    else
                    {
                        //如果文件大小比预设大小 小 直接上云
                        if (fileSize < uploadLimitSize)
                        {
                            var url = "";
                            fileModel.IsUpload = AliyunOSSHepler.GetInstance().UploadFiles(uploadFilePath, fileModel.NewFileName, ref
                                   url, false);

                            //如果上传失败  标签存本地
                            if (!fileModel.IsUpload)
                            {
                                fileModel.IsFailure = true;
                                this._uploadHandService.SavaTagsFile(fileModel);
                                //把文件 移动到错误文件 文件夹
                                System.IO.File.Move(uploadFilePath + Path.DirectorySeparatorChar + fileModel.NewFileName, FailurePath + Path.DirectorySeparatorChar + fileModel.NewFileName);
                            }
                            else
                            {
                                fileModel.Url = url;
                            }
                        }

                        fileList.Add(fileModel);
                        order++;
                    }
                }

                ////把标签 推送到总署
                KafKaContract kafkaModel = new KafKaContract();
                if (fileList.Count > 1)  //批量添加
                {
                    kafkaModel.MsgCode = KafkaMsgCodeEnum.AddList;
                    kafkaModel.Msg = SerializeHelper.serializeToString(fileList);
                    KafKaLogic.GetInstance().Push(kafkaModel, KafkaTopic);

                }
                else if (fileList.Count() == 1) //单个文件添加
                {
                    kafkaModel.MsgCode = KafkaMsgCodeEnum.Add;
                    fileList[0].GroupId = Guid.NewGuid().ToString();
                    kafkaModel.Msg = SerializeHelper.serializeToString(fileList[0]);
                    KafKaLogic.GetInstance().Push(kafkaModel, KafkaTopic);
                }


            }
            catch (Exception e)
            {
                LogHelper.logError("上传文件失败：" + e.StackTrace);
                result.Code = -1;
                result.Msg = e.Message;
            }

            return result;
        }

        /// <summary>
        /// 断点续传  通过文件名上传
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost(nameof(UploadFileByName))]
        public async Task<ResultContract<string>> UploadFileByName(BaseUploadContract data)
        {
            var result = new ResultContract<string>() { Code = 0, Msg = "上传成功" };
            try
            {
                //验证参数
                var orgInfo = new OrganizationContract();
                this.CheckParameter(data, ref result, ref orgInfo);
                if (result.Code == -1) return result;

                var files = Request.Form.Files;
                var groupId = Guid.NewGuid().ToString();
                int order = 1;

                //文件保存到本地
                double fileSize = 0;

                var fullName = uploadFilePath + Path.DirectorySeparatorChar + data.Name;
                if (!System.IO.File.Exists(fullName))
                {
                    result.Code = -1;
                    result.Msg = "文件不存在";
                    return result;
                }

                fileSize = FileHelper.GetSizeM(fullName);

                var fileModel = new UploadContract()
                {
                    UserId = data.UserId,
                    OrgId = data.OrgId,
                    AppId = data.AppId,
                    Tags = data.Tags,
                    UploadTime = DateTime.Now,
                    DisplayName = data.DisplayName,
                    Remark = data.Remark,
                    OrgName = orgInfo.OrganizationName,
                    Order = order,
                    ThemeType = data.ThemeType
                };

                //分组id 
                fileModel.GroupId = groupId;
                var fileData = new MultipartFormDataContent();

                //判断文件夹是否存在
                if (!Directory.Exists(uploadFilePath))
                {
                    Directory.CreateDirectory(uploadFilePath);
                }
                var imgPath = AppDomain.CurrentDomain.BaseDirectory + ConfigHelper.ReadConfigByName("ImgsPath");
                if (!Directory.Exists(imgPath))
                {
                    Directory.CreateDirectory(imgPath);
                }

                //文件不需要从命名
                fileModel.NewFileName = fileModel.FileName = data.Name;
                var lastName = data.Name.Substring(data.Name.LastIndexOf(".") + 1, (data.Name.Length - data.Name.LastIndexOf(".") - 1)); //扩展名
                fileModel.FileId = Guid.NewGuid();
                var fileLocalFullName = uploadFilePath + Path.DirectorySeparatorChar + fileModel.NewFileName;

                //文件上传成功 修改model
                fileModel.IsUpload = false;
                fileModel.Url = fileLocalFullName;
                fileModel.FileType = lastName;
                //组织名称。后期加上
                //判断文件大小
                double uploadLimitSize = 0;
                double.TryParse(ConfigHelper.ReadConfigByName("UploadLimitSizeM"), out uploadLimitSize);

                //如果是视频截取封面
                if (CommonDictionary.GetInstance().VideoType.Count(d => d.ToLower() == lastName.ToLower()) > 0)
                {

                    var thumbnailPath = AliyunOSSHepler.GetInstance().GetPicFromVideo(fileLocalFullName, imgPath + Path.DirectorySeparatorChar + fileModel.FileId + ".jpg", "1");
                    var thumbnailUrl = "";
                    var isUpload = AliyunOSSHepler.GetInstance().UploadFiles(imgPath, fileModel.FileId + ".jpg", ref
                              thumbnailUrl, true);

                    fileModel.ThumbnailUrl = thumbnailPath;

                    if (isUpload && !string.IsNullOrWhiteSpace(thumbnailUrl))
                    {
                        fileModel.ThumbnailUrl = thumbnailUrl;
                    }
                    else
                    {
                        fileModel.IsFailure = true;
                        this._uploadHandService.SavaTagsFile(fileModel);
                        //把文件 移动到错误文件 文件夹
                        System.IO.File.Move(uploadFilePath + Path.DirectorySeparatorChar + fileModel.NewFileName, FailurePath + Path.DirectorySeparatorChar + fileModel.NewFileName);
                        result.Code = -1;
                        result.Msg = "截图失败";
                        return result;
                    }
                }

                //如果断网把标签和文件都存到本地
                if (!CommonDictionary.GetInstance().KafkaIsOnline)
                {
                    this._uploadHandService.SavaTagsFile(fileModel);
                }
                else
                {
                    //如果文件大小比预设大小 小 直接上云
                    if (fileSize < uploadLimitSize)
                    {
                        var url = "";
                        fileModel.IsUpload = AliyunOSSHepler.GetInstance().UploadFiles(uploadFilePath, fileModel.NewFileName, ref
                               url, false);

                        //如果上传失败  标签存本地
                        if (!fileModel.IsUpload)
                        {
                            fileModel.IsFailure = true;
                            this._uploadHandService.SavaTagsFile(fileModel);
                            //把文件 移动到错误文件 文件夹
                            System.IO.File.Move(uploadFilePath + Path.DirectorySeparatorChar + fileModel.NewFileName, FailurePath + Path.DirectorySeparatorChar + fileModel.NewFileName);
                        }
                        else
                        {
                            fileModel.Url = url;
                        }
                    }

                    ////把标签 推送到总署
                    KafKaContract kafkaModel = new KafKaContract();
                    fileModel.GroupId = Guid.NewGuid().ToString();
                    kafkaModel.MsgCode = KafkaMsgCodeEnum.Add;
                    kafkaModel.Msg = SerializeHelper.serializeToString(fileModel);
                    KafKaLogic.GetInstance().Push(kafkaModel, KafkaTopic);
                }
            }
            catch (Exception e)
            {
                LogHelper.logError("上传文件失败：" + e.StackTrace);
                result.Code = -1;
                result.Msg = e.Message;
            }

            return result;
        }
 
        /// <summary>
        /// 获取续传点。
        /// </summary>
        /// <param name="md5Name"></param>
        /// <returns></returns>
        [HttpPost(nameof(GetFileResumePoint))]
        public async Task<string> GetFileResumePoint(string md5Name)
        {
            var saveFilePath = uploadFilePath + Path.DirectorySeparatorChar + md5Name;
            if (System.IO.File.Exists(saveFilePath))
            {
                var fs = System.IO.File.OpenWrite(saveFilePath);
                var fsLength = fs.Length.ToString();
                fs.Close();
                return fsLength;
            }

            return "0";
        }

        /// <summary>
        /// 文件续传。
        /// </summary>
        /// <returns></returns>
        [HttpPost(nameof(FileResume))]
        public async Task<ResultContract<string>> FileResume(string md5Name)
        {
            var result = new ResultContract<string>() { Code = 0, Msg = "上传成功" };
            try
            {
                var files = Request.Form.Files[0];
                var fileStream = files.OpenReadStream();
                var saveFilePath = uploadFilePath + Path.DirectorySeparatorChar + md5Name;
                SaveAs(saveFilePath, fileStream);
            }
            catch (Exception e)
            {
                result.Msg = "上传失败";
                result.Data = e.StackTrace;
                return result;
            }

            return result;
        }


        /// <summary>
        /// 给已有文件追加文件流。
        /// </summary>
        /// <param name="saveFilePath"></param>
        /// <param name="stream"></param>
        private void SaveAs(string saveFilePath, System.IO.Stream stream)
        {
            //接收到的字节信息。
            long startPosition = 0;
            long endPosition = 0;
            var contentRange = Request.Form["range"].ToString().Split('-');
            if (contentRange.Length == 2)
            {
                startPosition = long.Parse(contentRange[0]);
                endPosition = long.Parse(contentRange[1]);
            }
            //默认写针位置。
            System.IO.FileStream fs;
            long writeStartPosition = 0;
            if (System.IO.File.Exists(saveFilePath))
            {
                fs = System.IO.File.OpenWrite(saveFilePath);
                writeStartPosition = fs.Length;
            }
            else
            {
                fs = new System.IO.FileStream(saveFilePath, System.IO.FileMode.Create);
            }
            //调整写针位置。
            if (writeStartPosition > endPosition)
            {
                fs.Close();
                return;
            }
            else if (writeStartPosition < startPosition)
            {
                fs.Close();
                return;
            }
            else if (writeStartPosition > startPosition && writeStartPosition < endPosition)
            {
                writeStartPosition = startPosition;
            }
            fs.Seek(writeStartPosition, System.IO.SeekOrigin.Current);
            //向文件追加文件流。
            byte[] nbytes = new byte[512];
            int nReadSize = 0;
            nReadSize = stream.Read(nbytes, 0, 512);
            while (nReadSize > 0)
            {
                fs.Write(nbytes, 0, nReadSize);
                nReadSize = stream.Read(nbytes, 0, 512);
            }

            fs.Close();
        }

        /// <summary>
        /// 验证参数
        /// </summary>
        /// <param name="data"></param>
        /// <param name="result"></param>
        private void CheckParameter(BaseUploadContract data, ref ResultContract<string> result, ref OrganizationContract orgInfo)
        {
            //验证appid
            if (string.IsNullOrWhiteSpace(data.AppId) || !_uploadHandService.CheckAppId(data.AppId))
            {
                result.Code = -1;
                result.Msg = "系统没有注册！";
            }

            //验证组织机构id
            if (data.OrgId == null)
            {
                result.Code = -1;
                result.Msg = "请输入组织机构id！";
            }

            orgInfo = _uploadHandService.CheckOrgInfo(data.OrgId.GetValueOrDefault());
            if (orgInfo == null)
            {
                result.Code = -1;
                result.Msg = "组织机构输入错误！";
            }

            //验证资源类型
            if (data.ThemeType != ThemeTypeEnum.ThemeEvent.GetIntValue() && data.ThemeType != ThemeTypeEnum.ThemeResource.GetIntValue())
            {
                result.Code = -1;
                result.Msg = "资源类型错误！";
            }
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        [HttpDelete(nameof(DeleteFile))]

        public async Task<ResultContract<string>> DeleteFile(string fileId)
        {
            return logic.DeleteFile(fileId);
        }


        [HttpGet(nameof(test))]

        public async Task<ResultContract<string>> test()
        {
            var result = new ResultContract<string>();
            //var root = AppDomain.CurrentDomain.BaseDirectory + ConfigHelper.ReadConfigByName("ImgsPath");
            ////判断文件夹是否存在
            //if (!Directory.Exists(root))
            //{
            //    Directory.CreateDirectory(root);
            //}

            //AliyunOSSHepler.GetInstance().GetPicFromVideo(@"E:\\oceans.mp4", @"E:\\abc111231231231.jpg", "1");
            //AliyunOSSHepler.GetInstance().CatchImg(, "E:\\123456789");
            result.Msg = "123123123";

            System.IO.File.Delete(@"/home/sphs/es/PublishOutput/PublishOutput/UploadFile/1cb4144b-524d-4474-9045-a74b042acbd4.png");
            LogHelper.logInfo("test");
            return result;
        }

        /// <summary>
        /// 大视频调用测试
        /// </summary>
        /// <returns></returns>
        [HttpGet(nameof(test1))]
        public async Task<ResultContract<string>> test1()
        {
            LogHelper.logInfo("大视频调用测试");
            var result = new ResultContract<string>();
            MyJobLogic joblogic = new MyJobLogic();
            joblogic.BigVideo();
            return result;
        }



        /// <summary>
        /// 失败文件上传调用测试
        /// </summary>
        /// <returns></returns>
        [HttpGet(nameof(test2))]
        public async Task<ResultContract<string>> test2()
        {
            var result = new ResultContract<string>();
            FailureUploadLogic failLogic = new FailureUploadLogic();
            failLogic.FailureVideo();
            return result;
        }
    }
}
