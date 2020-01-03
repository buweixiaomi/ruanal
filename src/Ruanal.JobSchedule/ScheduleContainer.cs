using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.ServiceSchedule
{
    public class ScheduleContainer
    {
        public Quartz.IScheduler Scheduler = null;
        List<JobContext> Jobs = new List<JobContext>();
        private object joblocker = new object();
        public ScheduleContainer()
        {
            var ssf = new StdSchedulerFactory();
            Scheduler = ssf.GetScheduler();
            Scheduler.Start();
            Scheduler.Context.Add("Container", this);
        }

        public bool StartJob(string jobName, IJobExecutor handler, string cron)
        {
            lock (joblocker)
            {
                var existjob = Jobs.Exists(x => x.JobName == jobName);
                if (existjob)
                    return false;
                //string InnerJobId = Guid.NewGuid().ToString();
                JobDetailImpl jobdetail = new JobDetailImpl(jobName, typeof(RuanalInnerJob));
                jobdetail.Description = "内部调度任务";
                Quartz.ITrigger triger = null;
                var isrunonce = cron.ToLower() == "runonce";
                if (isrunonce)
                {
                    var ttriger = new Quartz.Impl.Triggers.SimpleTriggerImpl("trigger_" + jobName);
                    ttriger.RepeatCount = 0;
                    ttriger.RepeatInterval = TimeSpan.FromSeconds(1);
                    triger = ttriger;
                }
                else
                {
                    var ttriger = new Quartz.Impl.Triggers.CronTriggerImpl("trigger_" + jobName);
                    ttriger.CronExpressionString = cron;
                    triger = ttriger;
                }
                JobContext jobitem = new JobContext()
                {
                    //  InnerJobId = InnerJobId,
                    IsRunOnce = isrunonce,
                    JobName = jobName,
                    JobDetail = jobdetail,
                    LastRunTime = null,
                    OnInvoke = handler
                };
                jobitem.Triggers.Add(triger);
                Jobs.Add(jobitem);
                Scheduler.ScheduleJob(jobdetail, triger);
                return true;
            }
        }

        public bool StopJob(string jobName)
        {
            lock (joblocker)
            {
                var jobcontent = Jobs.FirstOrDefault(x => x.JobName == jobName);
                if (jobcontent == null)
                    return false;
                jobcontent.OnInvoke = null;
                foreach (var a in jobcontent.Triggers)
                {
                    try { Scheduler.PauseTrigger(a.Key); }// 停止触发器 
                    catch { }
                    try { Scheduler.UnscheduleJob(a.Key); }// 停止触发器 
                    catch { }
                }
                try { Scheduler.DeleteJob(jobcontent.JobDetail.Key); }// 删除任务
                catch { }
                Jobs.Remove(jobcontent);
                return true;
            }
        }

        /// <summary>
        /// 调用任务方法
        /// </summary>
        /// <param name="jobId"></param>
        public void CallExecute(string jobName)
        {
            lock (joblocker)
            {
                var jobcontent = Jobs.FirstOrDefault(x => x.JobName == jobName);
                if (jobcontent == null)
                    return;
                jobcontent.LastRunTime = DateTime.Now;
                if (jobcontent.OnInvoke == null)
                    return;
                jobcontent.OnInvoke.Execute(jobcontent);
            }
        }

        public List<JobContext> GetAllJobs()
        {
            lock (joblocker)
            {
                return Jobs.ToList();
            }
        }

        public JobContext GetJobContext(string jobName)
        {
            lock (joblocker)
            {
                var jobcontent = Jobs.FirstOrDefault(x => x.JobName == jobName);
                return jobcontent;
            }
        }

        public void Dispose()
        {
            Scheduler.PauseAll();
            Scheduler.Shutdown(false);
        }
    }
}
