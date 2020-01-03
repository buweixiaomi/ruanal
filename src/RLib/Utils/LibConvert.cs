using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace RLib.Utils
{
    public static class Converter
    {

        /// <summary>返回有关指定对象是否为 System.TypeCode.DBNull 类型的指示。</summary>
        /// <param name="Obj">一个对象</param>
        /// <returns></returns>
        public static bool IsDbNull(object Obj)
        {
            return Convert.IsDBNull(Obj);
        }

        /// <summary>日期型转整型</summary>
        /// <param name="localtime">请求时区为本地或未指定</param>
        /// <returns>秒数</returns>
        public static long LocalToTimeStamp(System.DateTime localtime)
        {
            TimeSpan tp = localtime.ToUniversalTime() - new System.DateTime(1970, 1, 1);
            return (long)tp.TotalSeconds;
        }

        /// <summary>日期型转整型</summary>
        /// <param name="time">请求时区为utc</param>
        /// <returns>秒数</returns>
        public static long UTCToTimeStamp(System.DateTime utctime)
        {
            TimeSpan tp = utctime - new System.DateTime(1970, 1, 1);
            return (long)tp.TotalSeconds;
        }

        /// <summary>整型转日期型</summary>
        /// <param name="Seconds"></param>
        /// <returns>返回为本地时间</returns>
        public static DateTime TimeStampToLocalTime(long Seconds)
        {
            var oldtime = new System.DateTime(1970, 1, 1);
            var localtime = oldtime.AddSeconds(Seconds).ToLocalTime();
            return localtime;
        }


        /// <summary>整型转日期型</summary>
        /// <param name="Seconds"></param>
        /// <returns>返回为utc时区</returns>
        public static DateTime TimeStampToUTCTime(long Seconds)
        {
            var oldtime = new System.DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var localtime = oldtime.AddSeconds(Seconds);
            return localtime;
        }

        public static DateTime? TryToDateTime(string str)
        {
            try
            {
                DateTime dt = new DateTime();
                if (DateTime.TryParse(str, out dt))
                {
                    return dt;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>string类型转换为short类型
        /// </summary>
        public static short StrToShort(string aStr)
        {
            short iRet = 0;
            short.TryParse(aStr, out iRet);
            return iRet;
        }

        /// <summary>string类型转换为int类型
        /// </summary>
        public static int StrToInt(string aStr)
        {
            int iRet = 0;
            int.TryParse(aStr, out iRet);
            return iRet;
        }

        /// <summary>string类型转换为Tnt64类型
        /// </summary>
        public static Int64 StrToInt64(string aStr)
        {
            Int64 iRet = 0;
            Int64.TryParse(aStr, out iRet);
            return iRet;
        }

        /// <summary>string类型转换为double类型
        /// </summary>
        public static double StrToDouble(string aStr)
        {
            double dRet = 0;
            double.TryParse(aStr, out dRet);
            if (double.IsNaN(dRet)) return 0;
            return dRet;
        }

        /// <summary>string类型转换为float类型
        /// </summary>
        public static float StrToFloat(string aStr)
        {
            float fRet = 0;
            float.TryParse(aStr, out fRet);
            return fRet;
        }

        /// <summary>string类型转换为decimal类型
        /// </summary>
        public static decimal StrToDecimal(string aStr)
        {
            decimal dRet = 0;
            decimal.TryParse(aStr, out dRet);
            return dRet;
        }

        /// <summary>string类型转换为日期类型,转换失败返回DateTime.Now
        /// </summary>
        public static DateTime StrToDateTime(string aStr)
        {
            DateTime dRet = DateTime.Now;
            if (DateTime.TryParse(aStr, out dRet))
                return dRet;
            else
                return DateTime.Now;
        }
        /// <summary>string类型转换为BOOL类型,转换失败返回False
        /// </summary>
        public static Boolean StrToBoolean(string aStr)
        {
            Boolean result;
            if (!Boolean.TryParse(aStr, out result))
            {
                return false;
            }
            return result;
        }


        /// <summary>string类型转换为日期类型
        /// </summary>
        /// <param name="aStr">要转换的字符串</param>
        /// <param name="aDefault">如果转换失败的默认值</param>
        /// <returns></returns>
        public static DateTime StrToDateTime(string aStr, DateTime aDefault)
        {
            DateTime dRet = aDefault;
            if (DateTime.TryParse(aStr, out dRet))
                return dRet;
            else
                return aDefault;
        }

        /// <summary>int类型转换为bool类型(1为TRUE，其余为FALSE)
        /// </summary>
        public static bool IntToBool(int aInt)
        {
            return (aInt == 1);
        }

        /// <summary>bool类型转换为int类型(TRUE为1，FALSE为0)
        /// </summary>
        public static int BoolToInt(bool aBool)
        {
            if (aBool)
                return 1;
            else
                return 0;
        }

        /// <summary>object类型转换为short类型
        /// </summary>
        public static short ObjToShort(object obj)
        {
            if (obj == null) return 0;
            short iRet = 0;
            short.TryParse(obj.ToString(), out iRet);
            return iRet;
        }

        /// <summary>object类型转换为int类型
        /// </summary>
        public static int ObjToInt(object obj)
        {
            if (obj == null) return 0;
            if (obj.GetType() == typeof(bool))
                return ((bool)obj) ? 1 : 0;
            int iRet = 0;
            int.TryParse(obj.ToString(), out iRet);
            return iRet;
        }

        /// <summary>object类型转换为Int64类型
        /// </summary>
        public static Int64 ObjToInt64(object obj)
        {
            if (obj == null) return 0;
            Int64 iRet = 0;
            Int64.TryParse(obj.ToString(), out iRet);
            return iRet;
        }

        /// <summary>object类型转换为double类型
        /// </summary>
        public static double ObjToDouble(object obj)
        {
            if (obj == null) return 0;
            double dRet = 0;
            double.TryParse(obj.ToString(), out dRet);
            if (double.IsNaN(dRet)) return 0;
            return dRet;
        }

        /// <summary>object类型转换为decimal类型
        /// </summary>
        public static decimal ObjToDecimal(object obj)
        {
            if (obj == null) return 0;
            decimal dRet = 0;
            decimal.TryParse(obj.ToString(), out dRet);
            return dRet;
        }

        /// <summary>object类型转换为float类型
        /// </summary>
        public static float ObjToFloat(object obj)
        {
            if (obj == null) return 0;
            float dRet = 0;
            float.TryParse(obj.ToString(), out dRet);
            return dRet;
        }


        /// <summary>object类型转换为datetime类型
        /// </summary>
        public static DateTime ObjToDateTime(object obj)
        {
            DateTime dRet = new DateTime();
            if (obj == null) return dRet;

            try
            {
                return Convert.ToDateTime(obj);
            }
            catch
            {
                return dRet;
            }
        }

        /// <summary>object类型转换为bool类型，直接强制转换(bool)obj
        /// </summary>
        public static bool ObjToBool(object obj)
        {
            if (obj == null) return false;
            if (obj is bool)
            {
                return (bool)obj;
            }
            if (obj.ToString() == "true") return true;
            if (ObjToInt(obj) == 1)
                return true;
            else
                return false;
        }

        /// <summary>object类型转换为string类型
        /// </summary>
        public static string NullToStr(object obj)
        {
            if (obj == null) return "";
            if (Convert.IsDBNull(obj)) return "";
            return obj.ToString();
        }
        /// <summary>相当于NullToStr
        /// </summary>
        public static string ObjToStr(object obj)
        {
            return NullToStr(obj);
        }

        /// <summary>将参数转为最大长度的str</summary>
        public static string ToMaxLengthStr(object obj,int maxLength)
        {
            var str = NullToStr(obj);
            if (str.Length > maxLength)
                return str.Substring(0, maxLength);
            return str;
        }

        /// <summary>对象转换成字节数组,自动判断isDbNull,返回null
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns>失败返回null</returns>
        public static byte[] ObjToBytes(object obj)
        {
            if (obj == null || Convert.IsDBNull(obj))
            {
                return null;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(ms, obj);
                return ms.GetBuffer();
            }
        }

        /// <summary>
        /// 字节数组转换成对象,自动判断isDbNull,返回null
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static object BytesToObj(byte[] data)
        {
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            using (MemoryStream rems = new MemoryStream(data))
            {
                rems.Position = 0;
                return formatter.Deserialize(rems);
            }
        }



        /// <summary>字符串转换成字节数组(采用UTF8)</summary>
        /// <param name="Str">字符串</param>
        /// <returns></returns>
        public static byte[] StrToBytes(string Str)
        {
            byte[] result = new byte[Encoding.UTF8.GetByteCount(Str)];
            result = Encoding.UTF8.GetBytes(Str);
            return result;
        }

        /// <summary>字节数组转换成字符串(采用UTF8)（过期函数，建议采用ObjToBytesToStr）</summary>
        /// <param name="bytes">字节数组，DataSet返回的数据可以直接使用,如:(byte[])Ds.Tables[0].Rows[0]["f_fromsql"]</param>
        /// <returns></returns>
        public static string BytesToStr(byte[] bytes)
        {
            if (bytes == null)
            {
                return "";
            }
            else if (bytes.Length == 0)
            {
                return "";
            }
            else
            {
                return System.Text.Encoding.UTF8.GetString(bytes).Replace("\0", string.Empty);
            }
        }


        /// <summary>填冲c字符n次
        /// </summary>
        public static string DupeString(string tmp, int count)
        {
            string Tmp = "";
            for (int i = 1; i <= count; i++)
            {
                Tmp = Tmp + tmp;
            }
            return Tmp;
        }

        /// <summary>byte数组转16进制字符串，一个字节两个字母
        /// </summary>
        public static string BytesToByteStr(IEnumerable<byte> bs)
        {
            if (bs == null)
                return "";
            StringBuilder sbtobs = new StringBuilder();
            foreach (var a in bs)
            {
                string tss = a.ToString("x2");
                sbtobs.Append(tss);
            }
            return sbtobs.ToString();
        }
    }
}
