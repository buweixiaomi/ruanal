using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.Core
{
    public class DomainAssembleLoader
    {
        public static void ConfigLoad(AppDomain appDomain)
        {
            appDomain.AssemblyLoad -= AppDomain_AssemblyLoad;
            appDomain.AssemblyResolve -= AppDomain_AssemblyResolve;

            appDomain.AssemblyLoad += AppDomain_AssemblyLoad;
            appDomain.AssemblyResolve += AppDomain_AssemblyResolve;
        }

        private static System.Reflection.Assembly AppDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Console.WriteLine("【Resolve】" + args.Name);
            var assname = args.Name.Split(',')[0].Trim();
            var domain = (AppDomain)sender;
            if (!string.IsNullOrEmpty(Core.ConfigConst.JobBin))
            {
                var path = System.IO.Path.Combine(domain.BaseDirectory, Core.ConfigConst.JobBin);
                var file1 = System.IO.Path.Combine(path, assname + ".dll");
                var file2 = System.IO.Path.Combine(path, assname + ".exe");
                if (System.IO.File.Exists(file1))
                {
                    var as1 = System.Reflection.Assembly.LoadFrom(file1);
                    return as1;
                }
                if (System.IO.File.Exists(file2))
                {
                    var as2 = System.Reflection.Assembly.LoadFrom(file2);
                    return as2;
                }
            }
            var assembly = System.Reflection.Assembly.Load(assname);
            return assembly;
        }

        private static void AppDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            Console.WriteLine("【load】" + args.LoadedAssembly.Location);
        }
    }
}
