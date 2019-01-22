using Quartz;
using Soyuan.Theme.Contracts;
using Soyuan.Theme.Contracts.DB;
using Soyuan.Theme.Core.Helper;
using Soyuan.Theme.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Soyuan.Theme.Business
{
    public class FailureUploadLogic : IJob //创建IJob的实现类，并实现Excute方法。
    {
        private readonly ThemeDBContext _dbContext = new ThemeDBContext();
        private readonly string KafkaTopic = ConfigHelper.ReadConfigByName("KafkaUploadTopic");
        private readonly string imgPath = AppDomain.CurrentDomain.BaseDirectory + ConfigHelper.ReadConfigByName("ImgsPath");

        public Task Execute(IJobExecutionContext context)
        {
            return Task.Run(() =>
            {
                this.FailureVideo();
            });
        }

        public void FailureVideo()
        {
            try
            {
                //获取文件所在路径
                string workDir = AppDomain.CurrentDomain.BaseDirectory + ConfigHelper.ReadConfigByName("FailurePath");
                if (System.IO.Directory.Exists(workDir))
                {
                    var list = this._dbContext.FileDB.FindAll<FileDbContract>();
                    LogHelper.logError("读取失败json");
                    foreach (var entity in list)
                    {
                        if (!CommonDictionary.GetInstance().KafkaIsOnline) continue;
                        if (!string.IsNullOrEmpty(entity.Id))
                        {
                            LogHelper.logInfo("读取json文件：" + entity.Id);
                            DirectoryInfo folder = new DirectoryInfo(workDir);
                            //回写数据库的上云路径
                            string osspath = "";
                            //本地全部上云
                            FileInfo[] fileList = folder.GetFiles(entity.Id + ".*");
                            for (int i = 0; i < fileList.Length; i++)
                            {
                                UploadContract contract = SerializeHelper.deserializeToObject<UploadContract>(entity.data);
                                bool imgupload = true;
                                string imgurl = "";

                                if (contract.IsUpload == true)
                                {
                                    KafKaContract kafka = new KafKaContract();
                                    kafka.MsgId = entity.Id;
                                    kafka.MsgCode = KafkaMsgCodeEnum.Add;
                                    kafka.Msg = SerializeHelper.serializeToString(contract);
                                    KafKaLogic.GetInstance().Push(kafka, KafkaTopic);
                                    continue;
                                }

                                if (CommonDictionary.GetInstance().VideoType.Count(d => d.ToLower() == contract.FileType.ToLower()) > 0)
                                {
                                    imgurl = AliyunOSSHepler.GetInstance().GetPicFromVideo(workDir + fileList[i].Name, imgPath + Path.DirectorySeparatorChar + contract.FileId + ".jpg", "1");
                                    imgupload = AliyunOSSHepler.GetInstance().UploadFiles(imgPath, contract.FileId + ".jpg", ref imgurl);
                                }

                                if (imgupload)
                                {
                                    bool returnType = AliyunOSSHepler.GetInstance().UploadFiles(workDir, fileList[i].Name, ref osspath);
                                    
                                    if (returnType)
                                    {
                                        LogHelper.logInfo("上云封面地址:" + imgurl + "_______上云视频路径：" + osspath);
                                        try
                                        {
                                            contract.IsUpload = true;
                                            contract.Url = osspath;
                                            contract.ThumbnailUrl = imgurl;
                                            entity.data = SerializeHelper.serializeToString(contract);
                                            this._dbContext.FileDB.Save<FileDbContract>(contract.FileId.ToString(), entity);
                                            KafKaContract kafka = new KafKaContract();
                                            kafka.MsgId = entity.Id;
                                            kafka.MsgCode = KafkaMsgCodeEnum.Add;
                                            kafka.Msg = SerializeHelper.serializeToString(contract);
                                            KafKaLogic.GetInstance().Push(kafka, KafkaTopic);
                                        }
                                        catch (Exception e)
                                        {
                                            LogHelper.logError("解析json数据推送kafka异常：文件名-" + fileList[i].Name + "__" + e.ToString());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    LogHelper.logError("失败任务无数据,本次不执行");
                }
            }
            catch (Exception e)
            {
                LogHelper.logError("失败文件推送异常:" + e.Message + "__" + e.ToString());
            }
        }
    }
}
