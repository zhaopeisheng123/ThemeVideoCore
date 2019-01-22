using Quartz;
using Soyuan.Theme.Contracts;
using Soyuan.Theme.Core.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Soyuan.Theme.Business
{
    public class MyJobLogic : IJob //创建IJob的实现类，并实现Excute方法。
    {
        private readonly string KafkaTopic = ConfigHelper.ReadConfigByName("KafkaUploadTopic");

        public Task Execute(IJobExecutionContext context)
        {
            return Task.Run(() =>
            {
                this.BigVideo();
            });
        }

        public void BigVideo()
        {
            try
            {
                AliyunOSSHepler oss = new AliyunOSSHepler();
                //获取文件所在路径
                string workDir = AppDomain.CurrentDomain.BaseDirectory + ConfigHelper.ReadConfigByName("UploadFilePath");
                DirectoryInfo folder = new DirectoryInfo(workDir);
                //回写数据库的上云路径
                string osspath = "";
                //本地全部上云
                FileInfo[] list = folder.GetFiles("*");
                LogHelper.logError("获取大文件路径：" + workDir);
                LogHelper.logError("获取大文件数量：" + list.Length.ToString());
                for (int i = 0; i < list.Length; i++)
                {
                    if (!CommonDictionary.GetInstance().KafkaIsOnline) continue;
                    bool returnType = oss.UploadFiles(workDir, list[i].Name, ref osspath);
                    LogHelper.logError("上云路径：" + osspath);
                    if (returnType)
                    {
                        string fileid = list[i].Name.Split(".")[0];
                        UploadContract contract = new UploadContract();
                        contract.FileId = Guid.Parse(fileid);
                        contract.IsUpload = true;
                        contract.Url = osspath;
                        contract.NewFileName = list[i].Name;
                        KafKaContract kafka = new KafKaContract();
                        kafka.MsgId = fileid;
                        kafka.MsgCode = KafkaMsgCodeEnum.Update;
                        kafka.Msg = SerializeHelper.serializeToString(contract);
                        KafKaLogic.GetInstance().Push(kafka, KafkaTopic);
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.logError("上云异常:" + e.Message);
            }
        }
    }
}
