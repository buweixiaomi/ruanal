﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Ruanal.WebDomain
{
    public class EnumHelper
    {

        public static string GetEnumDesc(Enum tv)
        {
            var v = GetEnumAttr<DescriptionAttribute>(tv);
            if (v == null)
                return "";
            return v.Description;
        }

        public static T GetEnumAttr<T>(Enum tv) where T : Attribute
        {
            object[] vs = null;
            foreach (var a in tv.GetType().GetFields())
            {
                if (a.Name != tv.ToString())
                    continue;
                vs = a.GetCustomAttributes(typeof(T), false);
            }
            if (vs == null || vs.Length == 0)
                return default(T);
            return (vs[0] as T);
        }
    }
}
