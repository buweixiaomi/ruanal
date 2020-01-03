using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ruanal.Job
{
    public class DispatchProxy : DispatcherBase
    {
        public const string xxx = "SSS";
        public static bool IsNoLink(string fullenterdll, string enterclass)
        {
            if (enterclass.LastIndexOf('@') > 0)
            {
                return true;
            }
            return false;
        }

        Type realType;
        MethodInfo realMethodGetDispatchs;
        object realInstance;

        //init完成后调用
        public void InitProxy(string enterdll, string enterclass)
        {
            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, enterdll);
            if (!System.IO.File.Exists(path))
                throw new Exception("无法找到文件：" + enterdll);
            var ass = Assembly.LoadFile(path);
            var methodName = "GetDispatchs";
            if (enterclass.LastIndexOf('@') > 0)
            {
                var x = enterclass.LastIndexOf('@');
                methodName = enterclass.Substring(x + 1);
                enterclass = enterclass.Substring(0, x);
            }
            if (string.IsNullOrEmpty(methodName))
            {
                methodName = "GetDispatchs";
            }
            realType = ass.GetType(enterclass);
            if (realType == null)
                throw new Exception("找不到类名为：" + enterclass);
            realMethodGetDispatchs = realType.GetMethods().FirstOrDefault(x => x.Name == methodName);
            if (realMethodGetDispatchs == null)
                throw new Exception("找不到方法：" + methodName);
            realInstance = Activator.CreateInstance(realType);
            ProxyConfig();
        }



        private void ProxyConfig()
        {
            if (realInstance != null && realType != null)
            {
                Dictionary<string, string> refx = null;
                var f1 = realType.GetField("TaskConfig", BindingFlags.Public | BindingFlags.Instance);
                if (f1 != null && f1.IsPublic && f1.FieldType == typeof(Dictionary<string, string>))
                {
                    refx = (Dictionary<string, string>)f1.GetValue(realInstance);
                    if (refx == null)
                    {
                        refx = new Dictionary<string, string>();
                        f1.SetValue(realInstance, refx);
                    }
                }
                else
                {

                    var p1 = realType.GetProperty("TaskConfig", BindingFlags.Public | BindingFlags.Instance);
                    if (p1 != null && p1.PropertyType == typeof(Dictionary<string, string>))
                    {
                        refx = (Dictionary<string, string>)p1.GetValue(realInstance, null);
                        if (refx == null)
                        {
                            refx = new Dictionary<string, string>();
                            p1.SetValue(realInstance, refx, null);
                        }
                    }
                }
                if (refx != null)
                {
                    foreach (var k in TaskConfig)
                        refx[k.Key] = k.Value;
                }
            }
        }


        public override List<DispatcherItem> GetDispatchs()
        {
            List<object> para = new List<object>();
            var pl = realMethodGetDispatchs.GetParameters().Length;
            for (var k = 0; k < pl; k++)
            {
                para.Add(null);
            }
            var objs = realMethodGetDispatchs.Invoke(realInstance, para.ToArray());
            var json = RLib.Utils.DataSerialize.SerializeJson(objs);
            var s = RLib.Utils.DataSerialize.DeserializeObject<List<DispatcherItem>>(json);
            return s;
        }

    }
}
