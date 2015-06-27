using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using TZCPA.ITTicket.Utils;

namespace RTXTest
{
    class Program
    {
        static void Main(string[] args)
        {
            RtxHelper rtx = new RtxHelper();

            string serializedUsersFileName = @"C:\Work\Source\test\RTXTest\users.json";

           var users = Util.GetObjectFromCache(serializedUsersFileName, () =>
            {
                List<UserInfo> allUsers = new List<UserInfo>();
                rtx.GetAllUsers(userInfo =>
                                {
                                    allUsers.Add(userInfo);
                                    Console.WriteLine(userInfo.Name);
                                    //   var userStr = JsonConvert.SerializeObject(userInfo);
                                    // Console.WriteLine(userStr);
                                });

                return allUsers;
            });
            

       
          //  rtx.SendIM("t1","abcde");
            Console.WriteLine("Done");
            Console.Read();
        }
    }
}
