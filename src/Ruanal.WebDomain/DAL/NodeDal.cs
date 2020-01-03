using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruanal.WebDomain.DAL
{
    public class NodeDal
    {
        public Model.Node Add(RLib.DB.DbConn dbconn, Model.Node model)
        {
            string sql = @"INSERT INTO [dbo].[node]
           ([clientId]
            ,[title]
           ,[nodeConfig]
           ,[nodeType]
           ,[lastHeartTime]
           ,[macs]
           ,[ips]
            ,[remark]
            ,[state]
           ,[createTime])
     VALUES
           (@clientId
            ,@title
           ,@nodeConfig
           ,@nodeType
           ,@lastHeartTime
           ,@macs
           ,@ips
            ,@remark
            ,@state
           ,getdate())";
            dbconn.ExecuteSql(sql, new
            {
                clientId = model.ClientId ?? "",
                title = model.Title ?? "",
                nodeConfig = model.NodeConfig ?? "",
                nodeType = model.NodeType,
                lastHeartTime = model.LastHeartTime,
                macs = model.Macs ?? "",
                ips = model.IPS ?? "",
                state = 0,
                remark = model.Remark ?? ""
            });
            model.NodeId = dbconn.GetIdentity();
            return model;
        }

        public int Update(RLib.DB.DbConn dbconn, Model.Node model)
        {
            string sql = @"UPDATE [dbo].[node]
                     SET [clientId] =@clientId
                   ,title = @title
                  ,[nodeConfig] = @nodeConfig
              --     ,[nodeType] =@nodeType 
                  ,[macs] = @macs
                  ,[ips] = @ips 
                  ,[remark] =@remark
                WHERE nodeId=@nodeId";
            int r = dbconn.ExecuteSql(sql, new
            {
                nodeId = model.NodeId,
                clientId = model.ClientId ?? "",
                nodeConfig = model.NodeConfig ?? "",
                title = model.Title ?? "",
                // nodeType = model.NodeType, 
                macs = model.Macs ?? "",
                ips = model.IPS ?? "",
                state = 0,
                remark = model.Remark ?? ""
            });
            return r;
        }

        public int Delete(RLib.DB.DbConn dbconn, int nodeId)
        {
            string sql = "update [dbo].[node] set state=-1 WHERE nodeId=@nodeId;";
            int r = dbconn.ExecuteSql(sql, new { nodeId = nodeId });
            return r;
        }

        public int UpdateLastHeart(RLib.DB.DbConn dbconn, int nodeId)
        {
            string sql = "update  [dbo].[node] set lastHeartTime=getdate() WHERE nodeId=@nodeId;";
            int r = dbconn.ExecuteSql(sql, new { nodeId = nodeId });
            return r;
        }

        public bool ExistClientId(RLib.DB.DbConn dbconn, string clientId, int currNodeId)
        {
            string sql = "select count(1) from dbo.node where [state]=0 and clientid=@clientId and nodeId<>@nodeId;";
            int r = dbconn.ExecuteScalar<int>(sql, new { clientId = clientId, nodeId = currNodeId });
            return r > 0;
        }

        public Model.Node Detail(RLib.DB.DbConn dbconn, string clientId)
        {
            string sql = "select top 1 * from dbo.node where clientId=@clientId and [state]=0 ;";
            var model = dbconn.Query<Model.Node>(sql, new { clientId = clientId }).FirstOrDefault();
            return model;
        }

        public Model.Node Detail(RLib.DB.DbConn dbconn, int nodeId)
        {
            string sql = "select top 1 * from dbo.node where nodeId=@nodeId;";
            var model = dbconn.Query<Model.Node>(sql, new { nodeId = nodeId }).FirstOrDefault();
            return model;
        }

        public List<Model.Node> GetAllNode(RLib.DB.DbConn dbconn, bool justWorkNode = false)
        {
            string sql = " select * from dbo.node where [state]=0 ";
            if (justWorkNode)
            {
                sql += " and nodeType=0 ";
            }
            sql += " order by nodeType desc,nodeId asc ";
            var models = dbconn.Query<Model.Node>(sql);
            return models;
        }

        public List<Model.Node> GetMainNode(RLib.DB.DbConn dbconn)
        {
            string sql = " select * from dbo.node where [state]=0 and nodeType=1 ";
            var models = dbconn.Query<Model.Node>(sql);
            return models;
        }

        public int SetNodeType(RLib.DB.DbConn dbconn, int nodeId, int nodeType)
        {
            string sql = "update  [dbo].[node] set nodeType=@nodetype WHERE nodeId=@nodeId and nodeType=-1;";
            int r = dbconn.ExecuteSql(sql, new { nodeId = nodeId, nodeType = nodeType });
            return r;
        }

        public int SetDispatchState(RLib.DB.DbConn dbconn, int nodeId, int stopDispathState)
        {
            string sql = "update  [dbo].[node] set stopDispatch=@stopdispatch WHERE nodeId=@nodeId;";
            int r = dbconn.ExecuteSql(sql, new { nodeId = nodeId, stopdispatch = stopDispathState });
            return r;
        }
    }
}
