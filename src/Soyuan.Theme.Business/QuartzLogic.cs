using Quartz;
using Quartz.Impl;
using Soyuan.Theme.Core.Helper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Soyuan.Theme.Business
{
    public class QuartzLogic
    {
        public async Task<string[]> StartFileUploadOss()
        {
            try
            {
                ISchedulerFactory factory = new StdSchedulerFactory();
                //1、通过调度工厂获得调度器
                IScheduler _scheduler = await factory.GetScheduler();
                //2、开启调度器
                await _scheduler.Start();
                //3、创建一个触发器
                //生产环境放开
                DateTimeOffset startTime = DateBuilder.NextGivenSecondDate(DateTime.Now, 1);
                var Osstrigger = TriggerBuilder.Create().StartAt(startTime).WithCronSchedule(ConfigHelper.ReadConfigByName("QuartzTime"))
                                .Build();
                var Failuretrigger = TriggerBuilder.Create().StartAt(startTime).WithCronSchedule(ConfigHelper.ReadConfigByName("QuartzTime"))
                                .Build();


                //4、创建任务
                var OssDetail = JobBuilder.Create<MyJobLogic>()
                                .WithIdentity("job", "group")
                                .Build();
                var FailurejobDetail = JobBuilder.Create<FailureUploadLogic>()
                               .WithIdentity("job1", "group")
                               .Build();

                //5、将触发器和任务器绑定到调度器中
                await _scheduler.ScheduleJob(OssDetail, Osstrigger);
                await _scheduler.ScheduleJob(FailurejobDetail, Failuretrigger);
                return await Task.FromResult(new string[] { "value1", "value2" });

            }
            catch (Exception e)
            {
                LogHelper.logError("定时启动异常：" + e.Message);
                return null;
            }
        }


        /// <summary>
        /// 指定时间执行任务
        /// </summary>
        /// <typeparam name="T">任务类，必须实现IJob接口</typeparam>
        /// <param name="cronExpression">cron表达式，即指定时间点的表达式</param>
        public void ExecuteByCron<T>(string cronExpression) where T : IJob
        {
            //创建一个标准调度器工厂
            ISchedulerFactory factory = new StdSchedulerFactory();
            //通过从标准调度器工厂获得一个调度器，用来启动任务
            IScheduler scheduler = factory.GetScheduler().Result;
            //调度器的线程开始执行，用以触发Trigger
            scheduler.Start();

            //使用组别、名称创建一个工作明细，此处为所需要执行的任务
            IJobDetail detail1 = JobBuilder.Create<T>().Build();
            ITrigger trigger1 = (ICronTrigger)TriggerBuilder.Create().WithCronSchedule(cronExpression).Build();
            scheduler.ScheduleJob(detail1, trigger1);
        }
    }
}
