using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace GrassrootsFloodCtrl.Common
{
    public class SSOService
    {
        private TiackSSO.OrgAndUserService sso = new TiackSSO.OrgAndUserService();
        private TiackSSOAuth.SsoAuthService sso1 = new TiackSSOAuth.SsoAuthService();
        const string _serviceCode = "jcfx";
        /// <summary>
        /// 单点登录
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private string GetSign(string time)
        {
            const string servicepwd = "jcfxpwd";
            var sign = FormsAuthentication.HashPasswordForStoringInConfigFile(_serviceCode + servicepwd + time, "md5").ToLower();
            //LogWrite.Info("单点登录", sign);
            return sign;
        }
        /// <summary>
        /// 获取组织列表
        /// </summary>
        /// <returns></returns>
        public string GetSubOrg()
        {
            var time = DateTime.Now.ToString("yyyyMMddHHmmss");
            var sign = GetSign(time);
            //第3个参数是组织代码；0代表ALL
            return sso.getSubOrg(_serviceCode, time, sign, "0", "2", "json");
        }
        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <returns></returns>
        public string GetUserList()
        {
            var time = DateTime.Now.ToString("yyyyMMddHHmmss");
            var sign = GetSign(time);
            //第3个参数是组织代码；0代表ALL
            return sso.getUserList(_serviceCode, time, sign, "0", "2", "json");
        }
        //获取部门
        public string GetOrgInfo(string orgCoding)
        {
            var time = DateTime.Now.ToString("yyyyMMddHHmmss");
            var sign = GetSign(time);
            return sso.getOrgInfo(_serviceCode, time, sign,  orgCoding, "json");
        }
        //获取用的详情
        public string GetUserInfo(string loginName,string orgCoding)
        {
            var time = DateTime.Now.ToString("yyyyMMddHHmmss");
            var sign = GetSign(time);
            return sso.getUserInfo(_serviceCode, time, sign, loginName, orgCoding, "json");
        }
        /// <summary>
        /// 获取票据
        /// </summary>
        /// <param name="ticket"></param>
        /// <returns></returns>
        public string TicketValidation(string ticket)
        {
            var time = DateTime.Now.ToString("yyyyMMddHHmmss");
            var sign = GetSign(time);
            return sso1.ticketValidation(_serviceCode, time, sign, ticket, "json");
        }
    }
}