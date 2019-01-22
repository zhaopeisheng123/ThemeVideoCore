using Confluent.Kafka;
using Soyuan.Theme.Contracts;
using Soyuan.Theme.Core.Helper;
using System;
using System.Threading.Tasks;

namespace Soyuan.Theme.Business
{
    public class KafKaLogic
    {
        private static KafKaLogic KafKaInstance;
        private string KafkaUrl = ConfigHelper.ReadConfigByName("KafkaIp") + ":" + ConfigHelper.ReadConfigByName("KafkaPort");
        private string KafkaGroupId = ConfigHelper.ReadConfigByName("KafkaGroupId") + Guid.NewGuid();

        private KafKaLogic()
        {

        }

        public static KafKaLogic GetInstance()
        {
            // 如果类的实例不存在则创建，否则直接返回
            if (KafKaInstance == null)
            {
                KafKaInstance = new KafKaLogic();
            }

            return KafKaInstance;
        }

        /// <summary>
        /// 推送消息
        /// </summary>
        /// <param name="msg"></param>
        public void Push(KafKaContract msg, string kafkaTopic)
        {
            var config = new ProducerConfig { BootstrapServers = KafkaUrl };
            using (var producer = new Producer<Null, string>(config))
            {
                var data = SerializeHelper.serializeToString(msg);
                var dr = producer.ProduceAsync(kafkaTopic, new Message<Null, string> { Value = data }).Result;
            }
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        public void Pull(Action<KafKaContract, string> action)
        {
            var conf = new ConsumerConfig
            {
                GroupId = KafkaGroupId,
                BootstrapServers = KafkaUrl,
                // Note: The AutoOffsetReset property determines the start offset in the event
                // there are not yet any committed offsets for the consumer group for the
                // topic/partitions of interest. By default, offsets are committed
                // automatically, so in this example, consumption will only start from the
                // eariest message in the topic 'my-topic' the first time you run the program.
                AutoOffsetReset = AutoOffsetResetType.Earliest
            };

            Task.Run(() =>
            {
                using (var c = new Consumer<Null, string>(conf))
                {
                    //kafka 入库主题
                    string KafkaUploadTopic = ConfigHelper.ReadConfigByName("KafkaUploadTopic");
                    //kafka 删除文件主题
                    string KafkaMsgTopic = ConfigHelper.ReadConfigByName("KafkaMsgTopic");
                    var confIsAgent = ConfigHelper.ReadConfigByName("IsAgent", "");
                    var IsAgent = false;
                    Boolean.TryParse(confIsAgent, out IsAgent);
                    if (IsAgent)   //下面海关节点订阅删除文件消息
                    {
                        c.Subscribe(new string[] { KafkaMsgTopic });
                    }
                    else //总署海关订阅入库消息
                    {
                        c.Subscribe(new string[] { KafkaUploadTopic, KafkaMsgTopic });
                    }

                    LogHelper.logInfo("kafka Subscribe：" + KafkaUploadTopic + "," + KafkaMsgTopic);

                    //c.Subscribe(new string[] { KafkaUploadTopic, KafkaMsgTopic });
                    bool consuming = true;
                    // The client will automatically recover from non-fatal errors. You typically
                    // don't need to take any action unless an error is marked as fatal.
                    c.OnError += (_, e) =>
                    {
                        consuming = !e.IsFatal;
                        LogHelper.logError("kafka error" + e.Reason);
                    };

                    while (consuming)
                    {
                        try
                        {
                            var cr = c.Consume();
                            var data = SerializeHelper.deserializeToObject<KafKaContract>(cr.Value);
                            action?.Invoke(data, cr.Topic);
                        }
                        catch (ConsumeException e)
                        {
                            LogHelper.logError("kafka error" + e.Error.Reason);
                        }
                    }

                    // Ensure the consumer leaves the group cleanly and final offsets are committed.
                    c.Close();
                }
            });
        }
    }
}
