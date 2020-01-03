using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.BLL
{
    public class CmdHelper
    {
        private static CmdHelper _ins = new CmdHelper();
        public static CmdHelper Instance
        {
            get
            {
                return _ins;
            }
        }

        DAL.TaskDal taskdal = new DAL.TaskDal();
        DAL.CmdDal cmddal = new DAL.CmdDal();
        DAL.NodeDal nodedal = new DAL.NodeDal();
        DAL.DispatchDal dispatchdal = new DAL.DispatchDal();
        private CmdHelper() { }

        public bool StartTask(int taskId, List<int> nodeids)
        {
            var taskandbinds = TaskBll.ToRightTaskNodes(taskId, nodeids, true);
            if (taskandbinds.Item2.Count() == 0)
                throw new MException("没有节点，无法启动！");
            if (taskandbinds.Item1.CurrVersionId == 0)
            {
                throw new MException("请上传任务版本文件并设置当前运行版本！");
            }
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                dbconn.BeginTransaction();
                try
                {
                    var cmdmodel = new Model.Cmd()
                    {
                        CmdArgs = RLib.Utils.DataSerialize.SerializeJson(new Ruanal.Core.CmdTaskArg() { TaskId = taskId }),
                        CmdState = 0,
                        CmdType = Ruanal.Core.ConfigConst.CmdType_StartTask,
                        NodeId = 0
                    };
                    List<string> clientIds = new List<string>();
                    foreach (var a in taskandbinds.Item2)
                    {
                        cmdmodel.NodeId = a.NodeId;
                        cmddal.AddCmd(dbconn, cmdmodel);
                        taskdal.UpdateBindLocalState(dbconn, a.BindId, 1);

                        clientIds.Add(a.Node.ClientId);
                    }
                    dbconn.Commit();
                    NotifyNewCmd(clientIds);
                    return true;
                }
                catch (Exception ex)
                {
                    dbconn.Rollback();
                    throw ex;
                }
            }
        }
        public bool StopTask(int taskId, List<int> nodeids)
        {
            var taskandbinds = TaskBll.ToRightTaskNodes(taskId, nodeids, false);
            if (taskandbinds.Item2.Count() == 0)
                throw new MException("没有节点，无法停止！");
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                dbconn.BeginTransaction();
                try
                {
                    var cmdmodel = new Model.Cmd()
                    {
                        CmdArgs = RLib.Utils.DataSerialize.SerializeJson(new Ruanal.Core.CmdTaskArg() { TaskId = taskId }),
                        CmdState = 0,
                        CmdType = Ruanal.Core.ConfigConst.CmdType_StopTask,
                        NodeId = 0
                    };
                    List<string> clientIds = new List<string>();
                    foreach (var a in taskandbinds.Item2)
                    {
                        cmdmodel.NodeId = a.NodeId;
                        cmddal.AddCmd(dbconn, cmdmodel);
                        taskdal.UpdateBindLocalState(dbconn, a.BindId, 0);

                        clientIds.Add(a.Node.ClientId);
                    }
                    dbconn.Commit();
                    NotifyNewCmd(clientIds);
                    return true;
                }
                catch (Exception ex)
                {
                    dbconn.Rollback();
                    throw ex;
                }
            }
        }
        public bool DeleteTask(int taskId, List<int> nodeids)
        {
            var taskandbinds = TaskBll.ToRightTaskNodes(taskId, nodeids, false);
            if (taskandbinds.Item2.Count() == 0)
                throw new MException("没有节点，无法卸载！");
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                dbconn.BeginTransaction();
                try
                {
                    var cmdmodel = new Model.Cmd()
                    {
                        CmdArgs = RLib.Utils.DataSerialize.SerializeJson(new Ruanal.Core.CmdTaskArg() { TaskId = taskId }),
                        CmdState = 0,
                        CmdType = Ruanal.Core.ConfigConst.CmdType_DeleteTask,
                        NodeId = 0
                    };
                    List<string> clientIds = new List<string>();
                    foreach (var a in taskandbinds.Item2)
                    {
                        cmdmodel.NodeId = a.NodeId;
                        cmddal.AddCmd(dbconn, cmdmodel);
                        clientIds.Add(a.Node.ClientId);
                        taskdal.UpdateBindLocalState(dbconn, a.BindId, 0);
                    }
                    dbconn.Commit();
                    NotifyNewCmd(clientIds);
                    return true;
                }
                catch (Exception ex)
                {
                    dbconn.Rollback();
                    throw ex;
                }
            }
        }

        public void NotifyNewCmd(List<string> clientIds)
        {
            if (clientIds == null) clientIds = new List<string>();
            try { Ruanal.Core.Notify.NotifyHelper.Notify(Ruanal.Core.ConfigConst.NotifyTopic_NewCmd, string.Join("|", clientIds)); }
            catch (Exception ex) { }
        }
        public void NotifyNewDispatch(string taskId)
        {
            try { Ruanal.Core.Notify.NotifyHelper.Notify(Ruanal.Core.ConfigConst.NotifyTopic_NewDispatch, taskId); }
            catch (Exception ex) { }
        }

        public bool UpdateConfigTask(int nodeId)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var nodemodel = nodedal.Detail(dbconn, nodeId);
                if (nodemodel == null)
                    return false;
                var cmdmodel = new Model.Cmd()
                {
                    CmdArgs = "",
                    CmdState = 0,
                    CmdType = Ruanal.Core.ConfigConst.CmdType_ConfigUpdate,
                    NodeId = nodeId
                };
                cmddal.AddCmd(dbconn, cmdmodel);
                NotifyNewCmd(new List<string>() { nodemodel.ClientId });
                return true;
            }
        }

        public bool StopDispatch(int dispatchid)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                dbconn.BeginTransaction();
                try
                {
                    var dismodel = dispatchdal.GetDetail(dbconn, dispatchid);
                    if (dismodel.NodeId <= 0)
                    {
                        throw new MException("任务未分配到节点，停止执行失败！");
                    }
                    var cmdmodel = new Model.Cmd()
                    {
                        CmdState = 0,
                        CmdType = Ruanal.Core.ConfigConst.CmdType_StopDispatchJob,
                        NodeId = dismodel.NodeId,
                        CmdArgs = RLib.Utils.DataSerialize.SerializeJson(
                           new Ruanal.Core.CmdDispatchArg()
                           {
                               TaskId = dismodel.TaskId,
                               DispatchId = dismodel.DispatchId,
                               InvokeId = dismodel.InvokeId
                           })
                    };
                    cmddal.AddCmd(dbconn, cmdmodel);
                    dbconn.Commit();
                    var nodemodel = nodedal.Detail(dbconn, dismodel.NodeId);
                    if (nodemodel != null)
                    {
                        NotifyNewCmd(new List<string>() { nodemodel.ClientId });
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    dbconn.Rollback();
                    throw ex;
                }
            }
        }

        public bool RestartNode(int nodeId)
        {
            using (RLib.DB.DbConn dbconn = Pub.GetConn())
            {
                var nodemodel = nodedal.Detail(dbconn, nodeId);
                if (nodemodel == null)
                    return false;
                var cmdmodel = new Model.Cmd()
                {
                    CmdArgs = "",
                    CmdState = 0,
                    CmdType = Ruanal.Core.ConfigConst.CmdType_RestartNode,
                    NodeId = nodeId
                };
                cmddal.AddCmd(dbconn, cmdmodel);
                NotifyNewCmd(new List<string>() { nodemodel.ClientId });
                return true;
            }
        }
    }
}
