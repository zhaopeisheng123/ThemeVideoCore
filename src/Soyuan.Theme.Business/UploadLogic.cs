using Newtonsoft.Json;
using Soyuan.Theme.Contracts;
using Soyuan.Theme.Contracts.DB;
using Soyuan.Theme.Core.Helper;
using Soyuan.Theme.Domain;
using Soyuan.Theme.Domain.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Soyuan.Theme.Business
{
    public class UploadLogic
    {
        private readonly ThemeDBContext _dbContext = new ThemeDBContext();
        private string FailureWorkDir = AppDomain.CurrentDomain.BaseDirectory + ConfigHelper.ReadConfigByName("FailurePath");
        private string UploadFileWorkDir = AppDomain.CurrentDomain.BaseDirectory + ConfigHelper.ReadConfigByName("UploadFilePath");
        private string ESIndex = ConfigHelper.ReadConfigByName("ESIndex");
        private string ESType = ConfigHelper.ReadConfigByName("ESType");
        private string ESAddress = "http://" + ConfigHelper.ReadConfigByName("ESIp") + ":" + ConfigHelper.ReadConfigByName("ESPort");
        /// <summary>
        /// 订阅总署文件上传消息
        /// </summary>
        /// <param name="kafkaData"></param>
        public void PullUploadMsg(KafKaContract kafkaData)
        {
            string KafkaMsgTopic = ConfigHelper.ReadConfigByName("KafkaMsgTopic");
            LogHelper.logInfo("插入文件：" + kafkaData.Msg);
            var result = false;
            try
            {
                switch (kafkaData.MsgCode)
                {
                    case KafkaMsgCodeEnum.AddList:
                        var insertListData = SerializeHelper.deserializeToObject<List<UploadContract>>(kafkaData.Msg);
                        result = InsertESList(insertListData);
                        break;
                    case KafkaMsgCodeEnum.Add:
                        var insertData = SerializeHelper.deserializeToObject<UploadContract>(kafkaData.Msg);
                        result = InsertES(insertData);
                        break;
                    case KafkaMsgCodeEnum.Update:
                        var updateData = SerializeHelper.deserializeToObject<UploadContract>(kafkaData.Msg);
                        result = UpdateES(updateData);
                        break;
                    default:
                        //msgCode错误
                        break;
                }
            }
            catch (Exception e)
            {
                result = false;
            }

            if (result) //入库成功
            {
                if (kafkaData.MsgCode == KafkaMsgCodeEnum.AddList)
                {
                    kafkaData.MsgCode = KafkaMsgCodeEnum.InsertListSuccess;
                }
                else
                {
                    kafkaData.MsgCode = KafkaMsgCodeEnum.InsertSuccess;
                }
            }
            else //入库失败
            {
                kafkaData.MsgCode = KafkaMsgCodeEnum.InsertError;
            }

            KafKaLogic.GetInstance().Push(kafkaData, KafkaMsgTopic);
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="kafkaData"></param>
        public void PullMsg(KafKaContract kafkaData)
        {
            LogHelper.logInfo("删除文件：" + kafkaData.Msg);
            switch (kafkaData.MsgCode)
            {
                case KafkaMsgCodeEnum.InsertError:
                    //文件上传失败（文件不删除）
                    break;
                case KafkaMsgCodeEnum.InsertSuccess:
                    DeleteFile(SerializeHelper.deserializeToObject<UploadContract>(kafkaData.Msg));
                    break;
                case KafkaMsgCodeEnum.Update:
                    DeleteFile(SerializeHelper.deserializeToObject<UploadContract>(kafkaData.Msg));
                    break;
                case KafkaMsgCodeEnum.InsertListSuccess:
                    var msgList = SerializeHelper.deserializeToObject<List<UploadContract>>(kafkaData.Msg);
                    foreach (var item in msgList)
                    {
                        DeleteFile(item);
                    }
                    break;
                default:
                    //msgCode错误
                    break;
            }
        }

        private void DeleteFile(UploadContract fileContract)
        {
            if (!fileContract.IsUpload) return;
            //文件上传成功 删除文件
            if (fileContract.IsFailure) //如果是上传失败的文件
            {
                System.IO.File.Delete(FailureWorkDir + Path.DirectorySeparatorChar + fileContract.NewFileName);
            }
            else
            {
                System.IO.File.Delete(UploadFileWorkDir + Path.DirectorySeparatorChar + fileContract.NewFileName);
            }

            //删除json
            _dbContext.FileDB.Delete<FileDbContract>(fileContract.FileId.ToString());
        }

        /// <summary>
        /// ES新增数据
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        public bool InsertES(UploadContract contract)
        {
            try
            {

                var data = new EsContract()
                {
                    fileId = contract.FileId,
                    userId = contract.UserId,
                    orgId = contract.OrgId,
                    appId = contract.AppId,
                    orgName = contract.OrgName,
                    fileName = contract.FileName,
                    thumbnailUrl = contract.ThumbnailUrl,
                    newFileName = contract.NewFileName,
                    upload = contract.IsUpload,
                    url = contract.Url,
                    tags = contract.Tags,
                    uploadTime = contract.UploadTime,
                    insertTime = DateTime.Now,
                    displayName = contract.DisplayName,
                    fileType = contract.FileType,
                    groupId = contract.GroupId,
                    remark = contract.Remark,
                    order = contract.Order,
                    themeType = contract.ThemeType
                };
                LogHelper.logInfo("单个添加:" + SerializeHelper.serializeToString(data));
                var result = HttpHelper.HttpPost(SerializeHelper.serializeToString(data), ESAddress + "/document/addTaginfo");
                ResultContract<object> res = SerializeHelper.deserializeToObject<ResultContract<object>>(result);
                if (res.Code == 200)
                {
                    return true;
                }
                return false;

            }
            catch (Exception e)
            {
                LogHelper.logInfo("单个添加异常：" + e.ToString());
                return false;
            }
        }

        /// <summary>
        /// ES批量新增数据
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        public bool InsertESList(List<UploadContract> contractlist)
        {
            try
            {
                List<EsContract> list = new List<EsContract>();
                foreach (var contract in contractlist)
                {
                    EsContract data = new EsContract
                    {
                        fileId = contract.FileId,
                        userId = contract.UserId,
                        orgId = contract.OrgId,
                        appId = contract.AppId,
                        orgName = contract.OrgName,
                        fileName = contract.FileName,
                        newFileName = contract.NewFileName,
                        upload = contract.IsUpload,
                        url = contract.Url,
                        thumbnailUrl = contract.ThumbnailUrl,
                        tags = contract.Tags,
                        uploadTime = contract.UploadTime,
                        insertTime = DateTime.Now,
                        displayName = contract.DisplayName,
                        fileType = contract.FileType,
                        groupId = contract.GroupId,
                        remark = contract.Remark,
                        order = contract.Order,
                        themeType = contract.ThemeType
                    };
                    list.Add(data);
                }
                LogHelper.logInfo("批量添加:" + SerializeHelper.serializeToString(list));
                var result = HttpHelper.HttpPost(SerializeHelper.serializeToString(list), ESAddress + "/document/addTagsinfo?index=themevideo&type=tagsinfo");
                ResultContract<object> res = SerializeHelper.deserializeToObject<ResultContract<object>>(result);
                if (res.Code == 200)
                {
                    return true;
                }
                return false;

            }
            catch (Exception e)
            {
                LogHelper.logInfo("ES批量新增数据异常：" + e.ToString());
                return false;
            }
        }


        /// <summary>
        /// ES修改
        /// </summary>
        /// <param name="contract"></param>
        /// <param name="esid"></param>
        /// <returns></returns>
        public bool UpdateES(UploadContract contract)
        {
            try
            {
                var data = new EsContract()
                {
                    fileId = contract.FileId,
                    userId = contract.UserId,
                    orgId = contract.OrgId,
                    appId = contract.AppId,
                    orgName = contract.OrgName,
                    fileName = contract.FileName,
                    newFileName = contract.NewFileName,
                    upload = contract.IsUpload,
                    url = contract.Url,
                    thumbnailUrl = contract.ThumbnailUrl,
                    tags = contract.Tags,
                    uploadTime = contract.UploadTime,
                    insertTime = contract.InsertTime,
                    displayName = contract.DisplayName,
                    fileType = contract.FileType,
                    groupId = contract.GroupId,
                    remark = contract.Remark,
                    order = contract.Order,
                    themeType = contract.ThemeType
                };
                var result = HttpHelper.HttpPut(SerializeHelper.serializeToString(data), ESAddress + "/document/updateTaginfo");
                ResultContract<object> res = SerializeHelper.deserializeToObject<ResultContract<object>>(result);
                if (res.Code == 200)
                {
                    return true;
                }
                return false;

            }
            catch (Exception e)
            {
                LogHelper.logInfo("ES修改异常：" + e.ToString());
                return false;
            }

        }

        /// <summary>
        /// 获取单个标签
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ResultContract<object> GetTagsInfo(string id)
        {
            var result = HttpHelper.httpGet(ESAddress + "/document/getTagById?id=" + id);
            ResultContract<object> returnResource = JsonConvert.DeserializeObject<ResultContract<object>>(result);
            if (returnResource.Code == 200)
            {
                returnResource.Code = 0;
            }
            return returnResource;
        }

        /// <summary>
        /// 根据条件查询
        /// </summary>
        /// <returns></returns>
        public ResultContract<object> GetTagsByCondition(ThemeSearchContract contract)
        {
            var data = new Dictionary<string, object>();
            data.Add("index", ESIndex);
            data.Add("type", ESType);

            //排序
            var sortmaps = new Dictionary<string, object>();
            sortmaps.Add("uploadTime", "DESC");
            data.Add("sortMaps", sortmaps);

            //范围条件
            var rangeMaps = new List<Dictionary<string, object>>();
            var rangeMap = new Dictionary<string, object>();
            if (!string.IsNullOrWhiteSpace(contract.StartTime))
            {
                rangeMap.Add("from", contract.StartTime);
            }
            if (!string.IsNullOrWhiteSpace(contract.EndTime))
            {
                rangeMap.Add("to", contract.EndTime);
            }
            if (rangeMap.Count > 0)
            {
                rangeMap.Add("field", "uploadTime");

            }
            rangeMaps.Add(rangeMap);
            data.Add("rangeMaps", rangeMaps);
            //精确查询条件
            var mustQuery = new Dictionary<string, object>();
            var mustnotQuery = new Dictionary<string, object>();
            var shouldQuery = new Dictionary<string, object>();
            if (!string.IsNullOrWhiteSpace(contract.FileType))
            {
                List<string> FileTypeList = contract.FileType.Split(",").ToList();
                List<string> QueryList = new List<string>();
                foreach (var item in FileTypeList)
                {
                    QueryList.Add(item.ToLower());
                    QueryList.Add(item.ToUpper());
                }
                string[] fileTypeArray = QueryList.ToArray();
                shouldQuery.Add("fileType", fileTypeArray);
                data.Add("inShouldQuery", shouldQuery);
            }
            if (!string.IsNullOrWhiteSpace(contract.NotFileType))
            {
                List<string> notFileTypeList = contract.NotFileType.Split(",").ToList();
                List<string> notQueryList = new List<string>();
                foreach (var item in notFileTypeList)
                {
                    notQueryList.Add(item.ToLower());
                    notQueryList.Add(item.ToUpper());
                }
                string[] notfileTypeArray = notQueryList.ToArray();
                mustnotQuery.Add("fileType", notfileTypeArray);
                data.Add("inMustnotQuery", mustnotQuery);
            }
            if (!string.IsNullOrWhiteSpace(contract.OrgId))
            {
                mustQuery.Add("orgId", contract.OrgId);
            }
            if (!string.IsNullOrWhiteSpace(contract.AppId))
            {
                mustQuery.Add("appId", contract.AppId);
            }
            if (!string.IsNullOrWhiteSpace(contract.Tags))
            {
                mustQuery.Add("tags", contract.Tags);
            }
            if (!string.IsNullOrWhiteSpace(contract.ThemeType))
            {
                mustQuery.Add("themeType", contract.ThemeType);
            }
            if (!string.IsNullOrWhiteSpace(contract.GroupId))
            {
                mustQuery.Add("groupId", contract.GroupId);
            }
            if (!string.IsNullOrWhiteSpace(contract.IsUpload))
            {
                mustQuery.Add("IsUpload", bool.Parse(contract.IsUpload));
            }
            if (!string.IsNullOrWhiteSpace(contract.RetrievalText))
            {
                var retrieval = new Dictionary<string, object>();
                var query = new string[] { contract.RetrievalText };
                retrieval.Add("query", query);
                var fields = new string[] { "fileId", "userId", "orgId", "appId", "groupId", "orgName", "fileName", "newFileName", "upload", "url", "tags", "fileType", "displayName", "remark", "thumbnailUrl" };
                retrieval.Add("fields", fields);
                data.Add("inFullSearch", retrieval);
            }

            data.Add("mustQuery", mustQuery);
            data.Add("from", contract.From ?? 0);
            data.Add("size", contract.Size ?? 1000);
            LogHelper.logInfo("查询条件：" + SerializeHelper.serializeToString(data));
            var result = HttpHelper.HttpPost(SerializeHelper.serializeToString(data), ESAddress + "/document/getTagsByCondition");
            ResultContract<object> resultcontract = JsonConvert.DeserializeObject<ResultContract<object>>(result);
            if (resultcontract.Code == 200)
            {
                resultcontract.Code = 0;
            }
            return resultcontract;
        }


        public ResultContract<object> GetTasByGroup()
        {

            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("groupType", "fileType");
            data.Add("index", ESIndex);
            data.Add("type", ESType);
            var result = HttpHelper.HttpPost(SerializeHelper.serializeToString(data), ESAddress + "/document/getTagsByGroup");
            ResultContract<object> returnResource = JsonConvert.DeserializeObject<ResultContract<object>>(result);
            if (returnResource.Code == 200)
            {
                returnResource.Code = 0;
            }
            return returnResource;
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public ResultContract<string> DeleteFile(string fileId)
        {
            var result = new ResultContract<string>() { Code = 0, Msg = "删除成功" };
            try
            {
                //先删除本地数据库
                var url = ESAddress + "/document/delete/" + fileId;
                var data = HttpHelper.HttpDelete(url);
                ResultContract<object> returnResource = JsonConvert.DeserializeObject<ResultContract<object>>(data);
                if (returnResource.Code == 200)
                {
                    //删除云端
                    AliyunOSSHepler.GetInstance().DeleteObject(fileId);
                }
                else
                {
                    result.Code = -1;
                    result.Msg = returnResource.Msg;
                }
            }
            catch (Exception e)
            {
                LogHelper.logError("删除文件失败！" + e.StackTrace);
                result.Code = -1;
                result.Msg = "删除失败";
                result.Data = e.StackTrace;
            }


            return result;
        }

    }
}
