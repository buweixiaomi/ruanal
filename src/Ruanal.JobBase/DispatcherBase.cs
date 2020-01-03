using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.Job
{
    public abstract class DispatcherBase : MarshalByRefObject
    {
        public CrossLoger Loger;
        public ParentCaller ParentCaller;
        public Dictionary<string, string> TaskConfig = new Dictionary<string, string>();
        public virtual void Init() { }
        public abstract List<DispatcherItem> GetDispatchs();
        public void GlobalInit(string parajson)
        {
            if (!string.IsNullOrWhiteSpace(parajson))
            {
                var para = RLib.Utils.DataSerialize.DeserializeObject<Dictionary<string, string>>(parajson);
                foreach (var a in para)
                {
                    TaskConfig[a.Key] = a.Value;
                }
            }
        }
        /// <summary>
        /// 返回null，不进行对象回收
        /// </summary>
        /// <returns></returns>
        public override object InitializeLifetimeService()
        {
            return null;
        }

        /// <summary>
        /// 得到任务配置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultv"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        public string GetTaskConfig(string key, string defaultv = null, bool ignoreCase = false)
        {
            var item = ignoreCase ? TaskConfig.FirstOrDefault(x => x.Key.ToLower() == key.ToLower()) :
                TaskConfig.FirstOrDefault(x => x.Key == key);
            if (string.IsNullOrEmpty(item.Value) && defaultv != null)
                return defaultv;
            return item.Value;
        }

        //关闭时调用 释放资源
        public virtual void Dispose()
        { }
    }

    [Serializable]
    public class DispatcherItem
    {
        /// <summary>
        /// 运行唯一标识 当该任务并行执行实例中有该标识，本次分配直接跳过
        /// </summary>
        public string RunKey { get; set; }
        /// <summary>
        /// 别名
        /// </summary>
        public string NickName { get; set; }
        /// <summary>
        /// 运行配置
        /// </summary>
        public string RunArgs { get; set; }
    }
}
