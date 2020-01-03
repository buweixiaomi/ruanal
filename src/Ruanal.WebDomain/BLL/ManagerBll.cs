using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruanal.WebDomain.BLL
{
    public class ManagerBll
    {
        DAL.ManagerDal managerdal = new DAL.ManagerDal();
        public PageModel<Model.Manager> GetManagerPage(int pno, int pagesize, string keywords)
        {
            using (var dbconn = Pub.GetConn())
            {
                int totalcount = 0;
                var model = managerdal.GetManagerPage(dbconn, pno, pagesize, keywords, out totalcount);
                return new PageModel<Model.Manager>() { List = model, PageNo = pno, PageSize = pagesize, TotalCount = totalcount };
            }
        }

        public static void InitDefault()
        {
            try
            {
                DAL.ManagerDal managerdal = new DAL.ManagerDal();
                using (var dbconn = Pub.GetConn())
                {
                    var items = managerdal.GetManagerMiniTop(dbconn, 2);
                    if (items.Count == 0)
                    {
                        managerdal.AddManager(dbconn, new Model.Manager()
                        {
                            AllowLogin = 1,
                            CreateTime = DateTime.Now,
                            LastLoginTime = null,
                            LoginName = "run",
                            LoginPwd = "",
                            ManagerId = 0,
                            Name = "张三",
                            Remark = "",
                            State = 0,
                            SubName = "zhangthree",
                            UpdateTime = null

                        });
                    }
                }
            }
            catch (Exception ex) { }
        }

        public List<Model.Manager> GetManagerTop(int topcount)
        {
            using (var dbconn = Pub.GetConn())
            {
                var model = managerdal.GetManagerMiniTop(dbconn, topcount);
                return model;
            }
        }


        public Model.Manager GetManagerDetail(int managerid)
        {
            using (var dbconn = Pub.GetConn())
            {
                var model = managerdal.GetManagerDetail(dbconn, managerid);
                if (model == null)
                    throw new MException(MExceptionCode.NotExist, "用户不存在！");
                return model;
            }
        }

        public bool DeleteManager(int managerid)
        {
            using (var dbconn = Pub.GetConn())
            {
                dbconn.BeginTransaction();
                try
                {
                    var model = managerdal.DeleteManager(dbconn, managerid);
                    //添加操作日志
                    new OperationLogBll().AddLog(new Model.OperationLog
                    {
                        Module = "员工管理",
                        OperationName = Utils.CurrUserName(),
                        OperationContent = "删除" + managerid + "号的员工",
                        OperationTitle = "删除信息",
                        Createtime = DateTime.Now
                    });
                    dbconn.Commit();
                    return model > 0;
                }
                catch (Exception ex)
                {
                    dbconn.Rollback();
                    throw ex;
                }

            }
        }


        public Model.Manager AddManager(WebDomain.Model.Manager model)
        {
            if (string.IsNullOrEmpty(model.Name))
            {
                throw new MException(MExceptionCode.BusinessError, "名称不能为空！");
            }

            using (var dbconn = Pub.GetConn())
            {
                if (!string.IsNullOrEmpty(model.LoginName))
                {
                    if (managerdal.ExistLoginName(dbconn, model.LoginName, 0) > 0)
                        throw new MException(MExceptionCode.BusinessError, "登录名已存在！");
                }
                dbconn.BeginTransaction();
                try
                {
                    model.LoginPwd = RLib.Utils.Security.MakeMD5((model.LoginPwd ?? "000000").Trim());
                    model = managerdal.AddManager(dbconn, model);

                    dbconn.Commit();
                    //添加操作日志
                    new OperationLogBll().AddLog(new WebDomain.Model.OperationLog
                    {
                        Module = "员工管理",
                        OperationName = Utils.CurrUserName(),
                        OperationContent = "新增" + model.LoginName + "的信息",
                        OperationTitle = "新增信息",
                        Createtime = DateTime.Now
                    });
                    return model;
                }
                catch (Exception ex)
                {
                    dbconn.Rollback();
                    throw ex;
                }
            }
        }



        public bool UpdateManager(WebDomain.Model.Manager model)
        {
            if (string.IsNullOrEmpty(model.Name))
            {
                throw new MException(MExceptionCode.BusinessError, "名称不能为空！");
            }
            using (var dbconn = Pub.GetConn())
            {
                if (!string.IsNullOrEmpty(model.LoginName))
                {
                    if (managerdal.ExistLoginName(dbconn, model.LoginName, model.ManagerId) > 0)
                        throw new MException(MExceptionCode.BusinessError, "登录名已存在！");
                }
                dbconn.BeginTransaction();
                try
                {

                    int r = managerdal.UpdateManager(dbconn, model);
                    if (r <= 0)
                        throw new MException(MExceptionCode.BusinessError, "更新失败");
                    dbconn.Commit();
                    //添加操作日志               
                    new OperationLogBll().AddLog(new WebDomain.Model.OperationLog
                    {
                        Module = "员工管理",
                        OperationName = Utils.CurrUserName(),
                        OperationContent = "修改" + model.LoginName + "的信息",
                        OperationTitle = "修改信息",
                        Createtime = DateTime.Now
                    });
                    return true;
                }
                catch (Exception ex)
                {
                    dbconn.Rollback();
                    throw ex;
                }
            }
        }

        public bool ResetPwd(int managerid)
        {
            string newpwd = RLib.Utils.Security.MakeMD5("000000");
            using (var dbconn = Pub.GetConn())
            {
                if (managerdal.UpdateManagerPwd(dbconn, managerid, newpwd) > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public Model.Manager LoginIn(string loginname, string loginpwd)
        {
            if (string.IsNullOrEmpty(loginname))
                throw new MException(MExceptionCode.BusinessError, "用户名不能为空！");
            using (var dbconn = Pub.GetConn())
            {
                var models = managerdal.GetByLoginName(dbconn, loginname);
                if (models.Count == 0)
                    throw new MException(MExceptionCode.BusinessError, "用户名不存在！");
                if (models.Count > 1)
                    throw new MException(MExceptionCode.BusinessError, "用户名有重名，请联系管理员！");
                var model = models[0];
                if (model.State == 1)
                {
                    throw new MException(MExceptionCode.BusinessError, "用户已冻结！");
                }
                if (model.AllowLogin == 0)
                {
                    throw new MException(MExceptionCode.BusinessError, "不允许登录！");
                }
                loginpwd = (loginpwd ?? "");
                if (model.LoginPwd != (string.IsNullOrEmpty(loginpwd) ? "" : RLib.Utils.Security.MakeMD5(loginpwd)))
                {
                    throw new MException(MExceptionCode.BusinessError, "密码不正确！");
                }
                return model;
            }
        }


        public bool ChangePwd(int p, string oldpwd, string newpwd)
        {
            if (string.IsNullOrWhiteSpace(oldpwd))
            {
                throw new MException(MExceptionCode.BusinessError, "原密码不能为空！");
            }
            if (string.IsNullOrWhiteSpace(newpwd))
            {
                throw new MException(MExceptionCode.BusinessError, "新密码不能为空！");
            }
            string md5newpwd = RLib.Utils.Security.MakeMD5(newpwd.Trim());
            string md5oldpwd = RLib.Utils.Security.MakeMD5(oldpwd.Trim());
            using (var dbconn = Pub.GetConn())
            {
                var model = managerdal.GetManagerDetail(dbconn, p);
                if (model.LoginPwd != md5oldpwd)
                {
                    throw new MException(MExceptionCode.BusinessError, "原密码不正确！");
                }
                if (managerdal.UpdateManagerPwd(dbconn, p, md5newpwd) > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
