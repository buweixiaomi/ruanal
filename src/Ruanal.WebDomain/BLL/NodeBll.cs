using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.BLL
{
    public class NodeBll
    {
        DAL.NodeDal nodedal = new DAL.NodeDal();
        public const double NodeHeartErrorTimeMins = 1;
        public List<Model.Node> GetAllNode()
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var models = nodedal.GetAllNode(dbconn, false);
                return models;
            }
        }

        public Model.Node AddNode(Model.Node model)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                throw new MException("请输入标题！");
            }
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                if (!string.IsNullOrWhiteSpace(model.ClientId))
                {
                    if (nodedal.ExistClientId(dbconn, model.ClientId, 0))
                    {
                        throw new MException("序列号已存在！");
                    }
                }
                model.NodeType = -1;
                model = nodedal.Add(dbconn, model);
                return model;
            }
        }

        public bool UpdateNode(Model.Node model)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                throw new MException("请输入标题！");
            }
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                if (!string.IsNullOrWhiteSpace(model.ClientId))
                {
                    if (nodedal.ExistClientId(dbconn, model.ClientId, model.NodeId))
                    {
                        throw new MException("序列号已存在！");
                    }
                }
                nodedal.Update(dbconn, model);
                BLL.CmdHelper.Instance.UpdateConfigTask(model.NodeId);
                return true;
            }
        }

        public bool DeleteNode(int nodeId)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                dbconn.BeginTransaction();
                try
                {

                    var taskdal = new DAL.TaskDal();
                    var tasks = taskdal.GetNodeTasks(dbconn, nodeId, false);
                    if (tasks.Count > 0)
                    {
                        throw new MException("节点有任务，不能删除节点！");
                    }
                    nodedal.Delete(dbconn, nodeId);
                    dbconn.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    dbconn.Rollback();
                    throw ex;
                }
            }
        }

        public Dictionary<string, List<string>> ViewStatus(int nodeId)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var nodemodel = this.nodedal.Detail(dbconn, nodeId);
                string msg = Ruanal.Core.ConfigConst.TalkNodeTaskStatus + string.Format("{0}", nodemodel.ClientId);
                var result = Ruanal.Core.Notify.NotifyHelper.TalkToAll(msg, 4000, 1);
                Dictionary<string, List<string>> vs = new Dictionary<string, List<string>>();
                DAL.NodeDal nodedal = new DAL.NodeDal();
                foreach (var a in result)
                {
                    var _s = RLib.Utils.StringHelper.SplitToStringList(a, new char[] { '#' });
                    vs[nodeId.ToString()] = _s;
                }
                return vs;
            }
        }
        public Model.Node GetDetail(int nodeId)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var model = nodedal.Detail(dbconn, nodeId);
                return model;
            }
        }

        public Model.Node GetServerByClientId(string clientid)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var model = nodedal.Detail(dbconn, clientid);
                if (model == null)
                    return null;
                return model;
            }
        }

        public bool SetDispatchState(int nodeId, int dispatchState)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                dbconn.BeginTransaction();
                try
                {
                    nodedal.SetDispatchState(dbconn, nodeId, dispatchState);
                    dbconn.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    dbconn.Rollback();
                    throw ex;
                }
            }
        }
    }
}
