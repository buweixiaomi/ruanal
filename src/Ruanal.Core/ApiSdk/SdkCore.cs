using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Ruanal.Core.ApiSdk
{
    public class SdkCore
    {
        public static ApiResult<T> InvokeApi<T>(string url, object para) where T : new()
        {
            try
            {
                string fullurl = url;
                if (!url.ToLower().StartsWith("http"))
                    fullurl = Config.GetSystemConfig(ConfigConst.ServerUrlKeyName, "").TrimEnd('/') + "/" + url.TrimStart('/');
                Dictionary<string, string> paras = Utils.Utils.GetDicFromObject(para);
                byte[] bs = Utils.HttpHelper.Post(fullurl, paras);
                string resultstring = System.Text.Encoding.UTF8.GetString(bs);

                //RLib.WatchLog.Loger.Log("Api返回信息", url + " " + resultstring);
                var model = RLib.Utils.DataSerialize.DeserializeObject<ApiResult<T>>(resultstring);
                return model;
            }
            catch (Exception ex)
            {
                //RLib.WatchLog.Loger.Error("Api出错", url + " " + ex.Message);
                return new ApiResult<T>() { code = -9999, msg = "调用接口出错！" + ex.Message };
            }
        }


        public static ApiResult<byte[]> Download(string url)
        {
            try
            {
                string fullurl = url;
                if (!url.ToLower().StartsWith("http://"))
                    fullurl = Config.GetSystemConfig(ConfigConst.ServerUrlKeyName, "").TrimEnd('/') + "/" + url.TrimStart('/');
                byte[] bs = Utils.HttpHelper.Post(fullurl, null);
                return new ApiResult<byte[]>() { code = 1, msg = "ok", data = bs };
            }
            catch (Exception ex)
            {
                return new ApiResult<byte[]>() { code = -9999, msg = "调用接口出错！" };
            }
        }
        public static ApiResult<byte[]> Download2(string url)
        {
            try
            {
                string fullurl = url;
                if (!url.ToLower().StartsWith("http://"))
                    fullurl = Config.GetSystemConfig(ConfigConst.ServerUrlKeyName, "").TrimEnd('/') + "/" + url.TrimStart('/');
                byte[] bs = Utils.HttpHelper.Get(fullurl);
                return new ApiResult<byte[]>() { code = 1, msg = "ok", data = bs };
            }
            catch (Exception ex)
            {
                return new ApiResult<byte[]>() { code = -9999, msg = "调用接口出错！" };
            }
        }
    }
}
