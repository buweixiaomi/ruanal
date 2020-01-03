using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Dapper;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Ruanal.WebDomain
{
    public class Utils
    {
        /// <summary>
        /// 0 sqlserver 1mysql 
        /// </summary>
        public static int DataBaseType = 1;

        public static void AddOperateLog(string Module, string OperationTitle, string OperationContent, string UserName = null)
        {
            if (UserName == null)
                UserName = Utils.CurrUserName();
            new BLL.OperationLogBll().AddLog(new WebDomain.Model.OperationLog
            {
                Module = Module,
                OperationName = UserName,
                OperationContent = OperationContent,
                OperationTitle = OperationTitle
            });
        }


        private static string ToRightTagName(string ortag)
        {
            ortag = (ortag ?? "").Trim();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ortag.Length; i++)
            {
                sb.Append(ortag.Substring(i, 1).Trim());
            }
            ortag = sb.ToString();
            ortag = ortag.Replace(" ", "").Replace(",", "").Replace("[", "").Replace("]", "").Replace(";", "");
            if (string.IsNullOrEmpty(ortag))
                return "";
            ortag = "[" + ortag + "]";
            return ortag;
        }

        public static string[] SplitTags(string ortags)
        {
            ortags = ortags ?? "";
            List<string> tags = new List<string>();
            for (int i = 0; i < ortags.Length; )
            {
                if (ortags[i] != '[')
                {
                    break;
                }
                int end = ortags.IndexOf(']', i);
                if (end < i)
                    break;
                tags.Add(ortags.Substring(i + 1, end - i - 1));
                i = end + 1;
            }
            return tags.ToArray();
        }

        public static string CombineTags(IEnumerable<string> ortags)
        {
            List<string> resulttag = new List<string>();
            if (ortags != null)
            {
                foreach (var a in ortags)
                {
                    resulttag.Add(ToRightTagName(a));
                }
            }
            return string.Join("", resulttag.Distinct());
        }

        public static string GetPrivateConfigName(string projectCodeName, int cusprojectid, string orconfigname)
        {
            return string.Format("{0}_{1}", GetPrivateProjectName(projectCodeName, cusprojectid), orconfigname);
        }


        public static string GetPrivateProjectName(string projectCodeName, int serverprojectid)
        {
            return string.Format("Private_{0}_{1}", serverprojectid, projectCodeName);
        }
        public static string DirToUrl(string dirpath)
        {
            return dirpath.Replace("\\", "/");


        }

        public static string DirPathGetDowloadUrl(string dirpath)
        {
            string durl = string.Format("/api/config/downloadfile?filename={0}", System.Web.HttpUtility.UrlEncode(dirpath));
            return durl;
        }
        public static string SerializeObject(object obj)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            return json;
        }

        public static T DeSerialize<T>(string jsonobj)
        {
            var json = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonobj);
            return json;
        }

        public static string GetGroupKey()
        {
            string groupkey = DateTime.Now.ToString("yyMMddHHmmssfff");
            return groupkey;
        }


        public static string CurrUserName()
        {
            try
            {
                if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.User != null)
                {
                    var model = GetTokenModel(System.Web.HttpContext.Current.User.Identity.Name);
                    if (model == null)
                        return "";
                    return model.Name;
                }
                return "";
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public static LoginTokenModel GetTokenModel(string tokenname)
        {
            try
            {
                var model = Utils.DeSerialize<LoginTokenModel>(tokenname);
                return model;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static LoginTokenModel GetTokenModel()
        {
            try
            {
                if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.User != null)
                {
                    if (!System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
                        return null;
                    var model = Utils.DeSerialize<LoginTokenModel>(System.Web.HttpContext.Current.User.Identity.Name);
                    return model;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string URLDecode(string encodestring)
        {
            var v = System.Web.HttpUtility.UrlDecode(encodestring ?? "");
            return v;
        }


        public static int CurrUserId()
        {
            var token = GetTokenModel();
            if (token == null)
                return 0;
            return token.Id;
        }
        /// <summary>
        /// 解压缩一个 zip 文件。
        /// </summary>
        /// <param name="zipedFile">The ziped file.</param>
        /// <param name="strDirectory">The STR directory.</param>
        /// <param name="password">zip 文件的密码。</param>
        /// <param name="overWrite">是否覆盖已存在的文件。</param>
        public static void UnZip(string zipedFile, string strDirectory, string password, bool overWrite)
        {

            if (strDirectory == "")
                strDirectory = Directory.GetCurrentDirectory();
            if (!strDirectory.EndsWith("\\"))
                strDirectory = strDirectory + "\\";
            if (!Directory.Exists(strDirectory))
                Directory.CreateDirectory(strDirectory);

            using (ZipInputStream s = new ZipInputStream(File.OpenRead(zipedFile)))
            {
                s.Password = password;
                ZipEntry theEntry;

                while ((theEntry = s.GetNextEntry()) != null)
                {
                    string directoryName = "";
                    string pathToZip = "";
                    pathToZip = theEntry.Name;

                    if (pathToZip != "")
                        directoryName = Path.GetDirectoryName(pathToZip) + "\\";

                    string fileName = Path.GetFileName(pathToZip);


                    if (!Directory.Exists(strDirectory + directoryName))
                        Directory.CreateDirectory(strDirectory + directoryName);

                    if (fileName != "")
                    {
                        if ((File.Exists(strDirectory + directoryName + fileName) && overWrite) || (!File.Exists(strDirectory + directoryName + fileName)))
                        {
                            using (FileStream streamWriter = File.Create(strDirectory + directoryName + fileName))
                            {
                                int size = 2048;
                                byte[] data = new byte[2048];
                                while (true)
                                {
                                    size = s.Read(data, 0, data.Length);

                                    if (size > 0)
                                        streamWriter.Write(data, 0, size);
                                    else
                                        break;
                                }
                                streamWriter.Close();
                            }
                        }
                    }
                }

                s.Close();
            }
        }

        public static int DeleteFileOrDir(string path)
        {
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
                return 1;
            }
            if (System.IO.Directory.Exists(path))
            {
                System.IO.Directory.Delete(path, true);
                return 1;
            }
            return 0;
        }
    }

}
