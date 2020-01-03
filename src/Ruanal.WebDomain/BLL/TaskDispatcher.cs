using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.BLL
{
    public class TaskDispatcher
    {
        public static readonly TaskDispatcher Instance = new TaskDispatcher();
        DAL.DispatchDal dispatchdal = new DAL.DispatchDal();
        System.Collections.Concurrent.ConcurrentDictionary<int, List<Model.Dispatch>> cacheDises =
            new System.Collections.Concurrent.ConcurrentDictionary<int, List<Model.Dispatch>>();
        object rootlock = new object();


        int cachePage = 2000;

        System.Threading.ManualResetEvent manualReset = new System.Threading.ManualResetEvent(false);
        private TaskDispatcher()
        {

        }

        private void RefreshCache(int taskId)
        {
            if (!cacheDises.ContainsKey(taskId))
            {
                cacheDises.TryAdd(taskId, new List<Model.Dispatch>());
            }
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var dispaths = dispatchdal.GetUnDispatchs(dbconn, taskId, cachePage, false);
                cacheDises.TryUpdate(taskId, dispaths, dispaths);
            }
        }

        bool isRefreshing = false;
        DateTime nextRefreshTIme = DateTime.Now;

        private void NotifyUpdate()
        {
            if (isRefreshing)
                return;
            if (DateTime.Now.CompareTo(nextRefreshTIme) > 0)
            {
                lock (rootlock)
                {
                    if (isRefreshing)
                        return;
                    isRefreshing = true;
                }
                System.Threading.ThreadPool.QueueUserWorkItem(x =>
                {
                    try
                    {
                        nextRefreshTIme = DateTime.Now;
                    }
                    catch (Exception)
                    {
                        nextRefreshTIme = DateTime.Now;
                    }
                    finally
                    {
                        isRefreshing = false;
                    }
                });
            }
        }




        public List<Model.Dispatch> GetDispatch(int maxCount)
        {
            return null;
            //manualReset.w
        }
    }
}
