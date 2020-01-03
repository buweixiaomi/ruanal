using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace Ruanal.ManageApi
{
    public class ApiClient
    {
        public class ApiResult
        {
            public string ResponseContent { get; set; }
            public bool IsSuccess { get { return Code > 0; } }
            public int Code { get; set; }
            public JToken Data { get; set; }
            public string Msg { get; set; }

            public void CheckIsSuccess()
            {
                if (IsSuccess)
                    return;
                throw new Exception(this.Msg);
            }
        }
        private string Url;
        private string UserName;
        private string Password;
        System.Text.Encoding Encoding = Encoding.UTF8;
        public ApiClient(string url, string username, string password)
        {
            this.Url = url;
            this.UserName = username;
            this.Password = password;
        }

        private Task<ApiResult> Invoke(string apiname, Dictionary<string, string> para, object jsonbody)
        {
            string jsoncontent = null;
            if (jsonbody != null)
            {
                jsoncontent = JsonConvert.SerializeObject(jsonbody);
            }
            var url = this.Url.TrimEnd('/') + "/" + apiname.TrimStart('/');
            byte[] content = new byte[0];
            string contenttype = "";
            if (string.IsNullOrWhiteSpace(jsoncontent))
            {
                contenttype = "application/x-www-form-urlencoded";
                content = Encoding.GetBytes(WebUtil.BuildQuery(para));
                Console.WriteLine(WebUtil.BuildQuery(para));
            }
            else
            {
                contenttype = "application/json";
                if (para != null && para.Count > 0)
                {
                    url += "?" + WebUtil.BuildQuery(para);
                }
                content = Encoding.GetBytes(jsoncontent);
            }
            Dictionary<string, string> head = new Dictionary<string, string>();
            head.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.GetBytes(UserName + ":" + Password)));
            var task = Task.Run(() =>
            {
                try
                {

                    string v = WebUtil.DoPost(url, content, contenttype, Encoding, head);
                    var jtoken = JToken.Parse(v);
                    return new ApiResult()
                    {
                        Code = jtoken["code"].Value<int>(),
                        Data = jtoken["data"],
                        Msg = jtoken["msg"].Value<string>(),
                        ResponseContent = v
                    };
                }
                catch (Exception ex)
                {
                    return new ApiResult()
                    {
                        Code = -1,
                        ResponseContent = ex.Message,
                        Data = null,
                        Msg = ex.Message
                    };
                }
            });
            return task;
        }


        /// <summary>取得节点列表</summary>
        /// <returns></returns>
        public List<ApiEntity.Node> NodeList()
        {
            ApiResult result = Invoke("/ManageApi/NodeList", null, null).Result;
            result.CheckIsSuccess();
            return result.Data.ToObject<List<ApiEntity.Node>>();
        }

        /// <summary>重启节点</summary>
        /// <param name="type">方式:0命令方式(默认推荐) 1:Tcp通知方式 </param>
        /// <param name="nodeid"></param>
        /// <returns></returns>
        public int NodeRestart(int type = 0, int nodeid = 0)
        {
            ApiResult result = Invoke("/ManageApi/NodeRestart", new Dictionary<string, string>() {
                {"type",type.ToString() },
                {"nodeid",nodeid.ToString() }
            }, null).Result;
            result.CheckIsSuccess();
            return result.Data.Value<int>();
        }

        /// <summary>取得任务列表</summary>
        /// <returns></returns>
        public List<ApiEntity.Task> TaskList()
        {
            ApiResult result = Invoke("/ManageApi/TaskList", null, null).Result;
            result.CheckIsSuccess();
            return result.Data.ToObject<List<ApiEntity.Task>>();
        }


        /// <summary>添加任务</summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public int TaskAdd(ApiEntity.Task task)
        {
            ApiResult result = Invoke("/ManageApi/TaskAdd", null, task).Result;
            result.CheckIsSuccess();
            return result.Data.Value<int>();
        }
        /// <summary>修改任务</summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public int TaskEdit(ApiEntity.Task task)
        {
            ApiResult result = Invoke("/ManageApi/TaskEdit", null, task).Result;
            result.CheckIsSuccess();
            return result.Data.Value<int>();
        }

        /// <summary>删除任务</summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public int TaskDelete(int taskid)
        {
            ApiResult result = Invoke("/ManageApi/TaskDelete", new Dictionary<string, string>() {
                {"taskid",taskid.ToString() }
            }, null).Result;
            result.CheckIsSuccess();
            return result.Data.Value<int>();
        }

        /// <summary>启动任务</summary>
        /// <param name="taskid">任务id</param>
        /// <returns></returns>
        public bool TaskStart(int taskid)
        {
            ApiResult result = Invoke("/ManageApi/TaskStart", new Dictionary<string, string>() {
                {"taskid",taskid.ToString() }
            }, null).Result;
            result.CheckIsSuccess();
            return result.Data.Value<int>() > 0;
        }


        /// <summary>停止任务（停止任务会自动卸载）</summary>
        /// <param name="taskid">任务id</param>
        /// <returns></returns>
        public bool TaskStop(int taskid)
        {
            ApiResult result = Invoke("/ManageApi/TaskStop", new Dictionary<string, string>() {
                {"taskid",taskid.ToString() }
            }, null).Result;
            result.CheckIsSuccess();
            return result.Data.Value<int>() > 0;
        }


        /// <summary>卸载任务（可选操作，停止任务会自动卸载）</summary>
        /// <param name="taskid">任务id</param>
        /// <returns></returns>
        public bool TaskUnInstall(int taskid)
        {
            ApiResult result = Invoke("/ManageApi/TaskUnInstall", new Dictionary<string, string>() {
                {"taskid",taskid.ToString() }
            }, null).Result;
            result.CheckIsSuccess();
            return result.Data.Value<int>() > 0;
        }

        /// <summary>设置任务版本</summary>
        /// <param name="taskversion">任务信息</param>
        /// <returns></returns>
        public bool TaskSetVersion(ApiEntity.TaskVersion taskversion)
        {
            ApiResult result = Invoke("/ManageApi/TaskSetVersion", null, taskversion).Result;
            result.CheckIsSuccess();
            return result.Data.Value<int>() > 0;
        }

        /// <summary>任务运行日志 最新的前n=50条</summary>
        /// <returns></returns>
        public List<ApiEntity.TaskRunLog> TaskRunLog(int taskid = 0)
        {
            ApiResult result = Invoke("/ManageApi/TaskRunLog", new Dictionary<string, string>() {
                {"taskid",taskid.ToString() }
            }, null).Result;
            result.CheckIsSuccess();
            return result.Data.ToObject<List<ApiEntity.TaskRunLog>>();
        }

    }
}
