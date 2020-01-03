using Ruanal.WebDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Ruanal.Web.Controllers
{
    public class AccountController : ManageBaseController
    {
        //
        // GET: /Account/

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(string loginname, string loginpwd)
        {
            try
            {
                ViewBag.loginname = loginname;
                if (string.IsNullOrEmpty(loginname))
                {
                    throw new Exception("请输入登录名");
                }
                Ruanal.WebDomain.BLL.ManagerBll managerbll = new Ruanal.WebDomain.BLL.ManagerBll();
                var model = managerbll.LoginIn(loginname, loginpwd);
                if (model == null)
                {
                    throw new Exception("登录失败！");
                }
                LoginTokenModel tokenmodel = new LoginTokenModel()
                {
                    Id = model.ManagerId,
                    Name = model.Name,
                    SubName = model.SubName ?? ""
                };
                string name = Utils.SerializeObject(tokenmodel);
                FormsAuthentication.SetAuthCookie(name, false, "/");

                return RedirectToAction("index", "home");
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;
                return View();
            }
        }

        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public ActionResult Profile()
        {
            Ruanal.WebDomain.BLL.ManagerBll bll = new Ruanal.WebDomain.BLL.ManagerBll();
            var model = bll.GetManagerDetail(Token.Id);
            ViewBag.manager = model;
            return View();
        }


        [HttpPost]
        public ActionResult Profile(string oldpwd, string newpwd, string newpwd2)
        {
            oldpwd = (oldpwd ?? "").Trim();
            newpwd = (newpwd ?? "").Trim();
            newpwd2 = (newpwd2 ?? "").Trim();
            if (newpwd != newpwd2)
            {
                ViewBag.msg = "两次密码不一致！";
                return View();
            }
            Ruanal.WebDomain.BLL.ManagerBll bll = new Ruanal.WebDomain.BLL.ManagerBll();
            try
            {
                bool isok = bll.ChangePwd(Token.Id, oldpwd, newpwd);
                ViewBag.msg = "修改成功";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;
                return View();
            }
        }
    }
}
