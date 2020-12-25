using System;
using System.Collections.Generic;

namespace ServerConfig
{
    public class User{
        public String userName { get; set; }
        public String userTag { get; set; }
        public float money { get; set; }
    }
    
    public class ServerSettings{        
        public String OwnerId;
        public List<String> AdminIds;
        public String ServerName { get; set; }
        public bool AllowAllChannels { get; set; }
        public List<String> AllowedChannelIds;
        public Dictionary<String, User> Users;

        public ServerSettings(){
            OwnerId = DotNetEnv.Env.GetString("OWNER_ID");
            AdminIds = new List<string>();
            ServerName = "?";
            AllowAllChannels = false;
            AllowedChannelIds = new List<string>();
            Users = new Dictionary<string, User>();
        }

        public override string ToString()
        {
            String adminIds = ""; foreach(String str in AdminIds){adminIds += str + ", ";}
            String allowedChannelIds = ""; foreach(String str in AllowedChannelIds){allowedChannelIds += str + ", ";}
            String users = ""; foreach(User usr in Users.Values){users += usr.userName + ", ";}

            return $"OwnerId: {OwnerId}\nAdminIds: {adminIds}\nServer Name: {ServerName}\nAllow All Channels: {AllowAllChannels}\nAllowed Channel Ids: {allowedChannelIds}\nUsers: {users}\n";

        }
    }

}