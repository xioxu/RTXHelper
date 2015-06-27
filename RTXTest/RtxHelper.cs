// -----------------------------------------------------------------------
// <copyright file="RtxHelper.cs" company="TZEPM">
//      Copyright  TZEPM. All rights reserved.
// </copyright>
// <author>??</author>
// <date>2013-08-21</date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json;
using RTXSAPILib;
using RTXTest;

namespace TZCPA.ITTicket.Utils
{
    /// <summary>
    /// 发送Rtx消息
    /// </summary>
    public class RtxHelper
    {
        private static RTXSAPIRootObj RootObj;  //声明一个根对象

        #region 构造函数
        static RtxHelper()
        {
            RootObj = new RTXSAPIRootObj();

            RootObj.ServerIP = RtxServerIp;
            RootObj.ServerPort = RtxSdkApiPort;
        }

        #endregion

        #region 属性 从web.config中取得发Rtx参数
        /// <summary>
        /// Rtx服务器Ip
        /// </summary>
        public static string RtxServerIp
        {
            get { return ConfigurationManager.AppSettings["RtxServerIp"]; }
        }

        /// <summary>
        /// 端口
        /// </summary>
        public static short RtxSdkApiPort
        {
            get { return Convert.ToInt16(ConfigurationManager.AppSettings["RtxSdkApiPort"]); }
        }

        /// <summary>
        /// 系统发送消息账号 
        /// </summary>
        public static string RtxSenderAccount
        {
            get { return ConfigurationManager.AppSettings["RtxSenderAccount"]; }
        }

        /// <summary>
        /// 系统帐户密码
        /// </summary>
        public static string RtxSenderAccountPwd
        {
            get { return ConfigurationManager.AppSettings["RtxSenderAccountPwd"]; }
        }

        /// <summary>
        /// 是否测试
        /// </summary>
        public static bool IsDebug
        {
            get { return Convert.ToBoolean(ConfigurationManager.AppSettings["IsDebug"]); }
        }

        /// <summary>
        /// 是否关闭
        /// </summary>
        public static bool IsClosed
        {
            get { return Convert.ToBoolean(ConfigurationManager.AppSettings["IsClosed"]); }
        }

        //系统调试时收消息地址
        public static string DebugRtx
        {
            get
            {
                return string.IsNullOrEmpty(ConfigurationManager.AppSettings["DebugRtx"])
                           ? string.Empty
                           : ConfigurationManager.AppSettings["DebugRtx"];
            }
        }

        #endregion

        public bool SendNotify(string bstrReceiver, int lDelayTime, string bstrTitle, string bstrMsg)
        {
            
            return SendNotify(bstrReceiver, bstrTitle, lDelayTime, bstrMsg);
        }

        /// <summary>
        /// 使用配置文件中的RTX账户发送RTX消息
        /// </summary>
        /// <param name="bstrReceivers">接受人</param>
        /// <param name="bstrMsg">消息内容</param>
        /// <returns>是否发送成功</returns>
        public bool SendIM(string[] bstrReceivers, string bstrMsg)
        {
            try
            {
                foreach (string account in bstrReceivers)
                {
                    if (string.IsNullOrEmpty(account.Trim()) == false)
                    {
                        SendIM(account, bstrMsg);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 使用配置文件中的RTX账户发送RTX消息
        /// </summary>
        /// <param name="bstrReceivers">接受人</param>
        /// <param name="bstrMsg">消息内容</param>
        /// <returns>是否发送成功</returns>
        public bool SendIM(string bstrReceivers, string bstrMsg)
        {
            return SendIM(RtxSenderAccount, RtxSenderAccountPwd, bstrReceivers, bstrMsg);
        }

        public void GetAllUsers(Action<UserInfo> afterRetrivedUserInfo)
        {
            string serializedDepFileName = @"C:\Work\Source\test\RTXTest\dep.json";
            Department rootDep = Util.GetObjectFromCache(serializedDepFileName, () =>
                                                                           {
                                                                               var rootDepXml = RootObj.DeptManager.GetChildDepts("");
                                                                               rootDep = getXmlDepartments(rootDepXml)[0];
                                                                               rootDep.FullPath = rootDep.Name;
                                                                               rootDep.ChildDepartments = getChildDepartment(rootDep);
                                                                               return rootDep;
                                                                           });

          //  var ch = RootObj.DeptManager.GetChildDepts("天职国际集团\\天职工程");
            getDepUsers(rootDep, afterRetrivedUserInfo);
        }

        private void getDepUsers(Department department, Action<UserInfo> afterRetrivedUserInfo)
        {
            var userXml = RootObj.DeptManager.GetDeptUsers(department.FullPath);
            XDocument.Parse(userXml)
                         .Element("Users")
                         .Descendants("User").Select(x =>
                         {
                             var acct = x.Attribute("Name").Value;
                             var userInfo =  getRtxUserInfo(acct);
                             userInfo.DepartMentFullName = department.FullPath;
                             userInfo.DepartMentName = department.Name;

                             if (afterRetrivedUserInfo != null)
                             {
                                 afterRetrivedUserInfo(userInfo);
                             }

                             return userInfo;
                         }).ToList();

            if (department.ChildDepartments != null && department.ChildDepartments.Count > 0)
            {
                department.ChildDepartments.ForEach(x =>
                                                    {
                                                        getDepUsers(x, afterRetrivedUserInfo);
                                                    });
            }
        }

        private UserInfo getRtxUserInfo(string rtxAccount)
        {
            string pbName = string.Empty;
            string email = string.Empty;
            string mobile = string.Empty;
            string phone = string.Empty;
            int authType = -1;
            int gender = -1;
            RootObj.UserManager.GetUserBasicInfo(rtxAccount, out pbName,out gender,out mobile,out email,out phone,out authType);

            return new UserInfo()
                   {
                       Name = pbName,
                       Email = email,
                       Tel = mobile,
                       RtxAccount = rtxAccount,
                       Gender = (byte)gender
                   };
        }


        private List<Department> getChildDepartment(Department department)
        {
            var childDepXml = RootObj.DeptManager.GetChildDepts(department.FullPath);
            var childDeps = getXmlDepartments(childDepXml);

            if (childDeps != null)
            {
                childDeps.ForEach(x =>
                                  {
                                      x.FullPath = department.FullPath + "\\" + x.Name;
                                      x.ChildDepartments = getChildDepartment(x);
                                  });
            }

            return childDeps;
        }

        private List<Department> getXmlDepartments(string xmlString)
        {
            var doc = XDocument.Parse(xmlString);
            return doc.Element("Departments")
                .Descendants("Department")
                .Select(x => new Department() { Name = x.Attribute("Name").Value }).ToList();
        } 

        protected virtual bool SendIM(string bstrSender, string bstrPwd, string bstrReceivers, string bstrMsg)
        {
            try
            {
               
                if (IsClosed == true)
                {
                    return true;
                }

                if (IsDebug == true)
                {
                    bstrReceivers = DebugRtx;
                }

                RootObj.SendIM(bstrSender, bstrPwd, bstrReceivers, bstrMsg, string.Format("{{{0}}}", Guid.NewGuid()));
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 发送提醒消息，以弹窗方式展现
        /// </summary>
        /// <param name="bstrReceiver">接受人</param>
        /// <param name="bstrTitle">标题</param>
        /// <param name="lDelayTime">弹窗显示时间，单位为ms</param>
        /// <param name="bstrMsg">消息内容</param>
        /// <returns>是否发送成功</returns>
        protected virtual bool SendNotify(string bstrReceiver, string bstrTitle, int lDelayTime, string bstrMsg)
        {
            try
            {
                if (IsClosed == true)
                {
                    return true;
                }

                if (IsDebug == true)
                {
                    bstrReceiver = DebugRtx;
                }
               
                if (string.IsNullOrEmpty(bstrReceiver.Trim()) == false)
                {
                    RootObj.SendNotify(bstrReceiver, bstrTitle, lDelayTime, bstrMsg);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
