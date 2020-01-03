﻿using RLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruanal.WebDomain.DAL
{
    public class ManagerDal
    {

        #region Manger相关

        public Model.Manager GetManagerDetail(RLib.DB.DbConn dbconn, int managerid)
        {
            string sql = "select * from manager where managerid=@managerid;";
            var model = dbconn.Query<Model.Manager>(sql, new { managerid = managerid }).FirstOrDefault();
            return model;
        }

        public List<Model.Manager> GetByLoginName(RLib.DB.DbConn dbconn, string loginname)
        {
            string sql = "select * from manager where loginname=@loginname and  state<>-1;";
            var model = dbconn.Query<Model.Manager>(sql, new { loginname = loginname });
            return model.ToList();
        }

        public List<Model.Manager> GetManagerPage(RLib.DB.DbConn dbconn, int pno, int pagesize, string keywords, out int totalcount)
        {
            string sql = @"select * from (select row_number() over (order by m.managerid asc) as rownum, * from manager m  where state<>-1 and 
                                    name like ('%'+@keywords+'%')
                                    and 
                                    subname like ('%'+@keywords+'%')
                                    and 
                                    loginname like ('%'+@keywords+'%'))
                    A where A.rownum>@startindex and A.rownum<=@startindex+@pagesize; ";
            string countsql = @"select count(1) from manager where state<>-1 and 
                                    name like ('%'+@keywords+'%')
                                    and 
                                    subname like ('%'+@keywords+'%')
                                    and 
                                    loginname like ('%'+@keywords+'%')";
            var models = dbconn.Query<Model.Manager>(sql, new
            {
                keywords = keywords ?? "",
                startindex = (pno - 1) * pagesize,
                pagesize = pagesize
            });
            totalcount = dbconn.ExecuteScalar<int>(countsql, new
            {
                keywords = keywords ?? ""
            });
            return models;
        }


        public List<Model.Manager> GetManagerMiniTop(RLib.DB.DbConn dbconn, int topcount)
        {
            string sql = @"select top " + Math.Max(0, topcount) + " managerid,name,subname from manager where state<>-1 ;";
            var models = dbconn.Query<Model.Manager>(sql, new
            {
                topcount = topcount
            });
            return models;
        }

        public Model.Manager AddManager(RLib.DB.DbConn dbconn, Model.Manager model)
        {
            string sql = "insert into manager(name,subname,loginname,loginpwd,allowlogin,state,createtime,remark)" +
                "values(@name,@subname,@loginname,@loginpwd,@allowlogin,@state,getdate(),@remark);";
            dbconn.ExecuteSql(sql, new
            {
                name = model.Name,
                subname = model.SubName ?? "",
                loginname = model.LoginName ?? "",
                loginpwd = model.LoginPwd ?? "",
                allowlogin = model.AllowLogin,
                state = model.State,
                remark = model.Remark ?? ""
            });
            model.ManagerId = dbconn.GetIdentity();
            return model;
        }

        public int UpdateManager(RLib.DB.DbConn dbconn, Model.Manager model)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("UPDATE  manager set ");
            sb.Append("name = @name,");
            sb.Append("subName = @subname,");
            sb.Append("loginName = @loginname,");
            //  sb.Append("loginPwd = @loginpwd,");
            sb.Append("allowLogin = @allowlogin,");
            sb.Append("state = @state,");
            sb.Append("updateTime = getdate(),");
            sb.Append("remark = @remark ");
            sb.Append(" WHERE managerId = @managerid;");

            int rows = dbconn.ExecuteSql(sb.ToString(), new
            {
                managerid = model.ManagerId,

                name = model.Name,
                subname = model.SubName ?? "",
                loginname = model.LoginName ?? "",
                loginpwd = model.LoginPwd ?? "",
                allowlogin = model.AllowLogin,
                state = model.State,
                remark = model.Remark ?? ""
            });
            return rows;
        }

        public int DeleteManager(RLib.DB.DbConn dbconn, int managerid)
        {
            string sql = "update manager set state=-1 where managerid=@managerid;";
            int rows = dbconn.ExecuteSql(sql, new { managerid = managerid });
            return rows;
        }

        public int ExistLoginName(RLib.DB.DbConn dbconn, string loginname, int except_managerid)
        {
            string sql = "select count(1) from manager where loginname=@loginname and state<>-1 and managerid<>@managerid;";
            int count = dbconn.ExecuteScalar<int>(sql, new { loginname = loginname, managerid = except_managerid });
            return count;
        }

        public int UpdateManagerPwd(RLib.DB.DbConn dbconn, int managerid, string newpwd)
        {
            string sql = "update manager set loginpwd=@loginpwd where managerid=@managerid;";
            int rows = dbconn.ExecuteSql(sql, new { managerid = managerid, loginpwd = newpwd ?? "" });
            return rows;
        }

        public bool ExistPermissionKey(RLib.DB.DbConn dbconn, int managerid, string permissionkey)
        {
            string sql = "SELECT count(1) FROM managertaglink ml " +
                            "join tagpermission tp on ml.usertagId=tp.usertagId " +
                            "where ml.managerId=@managerid and tp.permissionKey=@permissionkey;";
            int r = dbconn.ExecuteScalar<int>(sql, new { managerid = managerid, permissionkey = permissionkey });
            return r > 0;
        }

        public List<string> ManagerKeys(RLib.DB.DbConn dbconn, int managerid)
        {
            string sql = "SELECT distinct tp.permissionKey FROM managertaglink ml " +
                            "join tagpermission tp on ml.usertagId=tp.usertagId " +
                            "where ml.managerId=@managerid;";
            List<string> r = dbconn.Query<string>(sql, new { managerid = managerid });
            return r;
        }

        public List<string> TagPermission(RLib.DB.DbConn dbconn, int usertagid)
        {
            string sql = "select permissionKey from tagpermission where usertagid=@usertagid;";
            List<string> r = dbconn.Query<string>(sql, new { usertagid = usertagid });
            return r;
        }


        public void SetTagPermission(RLib.DB.DbConn dbconn, int usertagid, List<string> keys)
        {
            DeleteTagPermission(dbconn, usertagid);
            string sqlinsert = "insert into   tagpermission(usertagid,permissionKey) values(@usertagid,@key);";
            foreach (var k in keys)
            {
                if (string.IsNullOrEmpty(k))
                    continue;
                dbconn.ExecuteSql(sqlinsert, new { usertagid = usertagid, key = k });
            }
        }

        public int DeleteTagPermission(RLib.DB.DbConn dbconn, int usertagid)
        {
            string sql = "delete from   tagpermission where usertagid=@usertagid;";
            return dbconn.ExecuteSql(sql, new { usertagid = usertagid });
        }

        public List<Model.Manager> GetTagUsers(RLib.DB.DbConn dbconn, int usertagid)
        {
            string sql = "select m.* from managertaglink ml join manager m on m.managerid=ml.managerid where m.state<>-1 and ml.usertagid=@usertagid;";
            List<Model.Manager> r = dbconn.Query<Model.Manager>(sql, new { usertagid = usertagid });
            return r;
        }

        #endregion end Manager相关 

    }
}
