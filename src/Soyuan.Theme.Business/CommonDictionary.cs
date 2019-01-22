using Soyuan.Theme.Core.Helper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Soyuan.Theme.Business
{
    public class CommonDictionary
    {
        private static CommonDictionary Instance;

        private CommonDictionary()
        {
        }

        public static CommonDictionary GetInstance()
        {
            // 如果类的实例不存在则创建，否则直接返回
            if (Instance == null)
            {
                Instance = new CommonDictionary();
            }

            return Instance;
        }

        private bool? _kafkaIsOnline = null;
        /// <summary>
        /// 服务器是否在线
        /// </summary>
        public bool KafkaIsOnline
        {
            get
            {
                if (_kafkaIsOnline == null)
                {
                    TestConnection();
                    //轮询判断
                    Task.Run(() =>
                    {
                        while (true)
                        {
                            TestConnection();
                            System.Threading.Thread.Sleep(1000 * 10);
                        }
                    });
                }

                return _kafkaIsOnline.GetValueOrDefault();
            }
            private set
            {
                _kafkaIsOnline = value;
            }
        }

        public List<string> _videoType = null;
        /// <summary>
        /// 要切图的视频类型
        /// </summary>
        public List<string> VideoType
        {
            get
            {
                if (_videoType == null)
                {
                    var videoTypeConf = ConfigHelper.ReadConfigByName("VideoType");
                    var types = videoTypeConf.Split(",");
                    _videoType = new List<string>(types);
                }
                return _videoType;
            }
            private set
            {
                _videoType = value;
            }
        }


        /// <summary>
        /// 检测kafka服务器是否在线
        /// </summary>
        private void TestConnection()
        {
            var KafkaIp = ConfigHelper.ReadConfigByName("KafkaIp");
            var KafkaPort = ConfigHelper.ReadConfigByName("KafkaPort");
            _kafkaIsOnline = HttpHelper.TestConnection(KafkaIp, int.Parse(KafkaPort), 3000);
        }

    }
}
