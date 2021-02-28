using System;
using System.Collections.Generic;
using SlotMachineBackend;

namespace ServerConfig
{
    public class User{
        public String userName { get; set; }
        public String userTag { get; set; }
        public float credits { get; set; }
    }
    
    public class ServerSettings{        
        public String OwnerId;
        public List<String> AdminIds;
        public String ServerName { get; set; }
        public bool AllowAllChannels { get; set; }
        public List<String> AllowedChannelIds;
        public Dictionary<String, User> Users;

        public SlotMachine TheMachine{ get; set; }

        public ServerSettings(){
            OwnerId = DotNetEnv.Env.GetString("OWNER_ID");
            AdminIds = new List<string>();
            ServerName = "?";
            AllowAllChannels = false;
            AllowedChannelIds = new List<string>();
            Users = new Dictionary<string, User>();
            TheMachine = new SlotMachine(9);
        }

        public override string ToString()
        {
            String adminIds = ""; foreach(String str in AdminIds){adminIds += str + ", ";}
            String allowedChannelIds = ""; foreach(String str in AllowedChannelIds){allowedChannelIds += str + ", ";}
            String users = ""; foreach(User usr in Users.Values){users += usr.userName + " C: " + usr.credits + ", " ;}

            return $@"
            OwnerId: {OwnerId}
            AdminIds: {adminIds}
            Server Name: {ServerName}
            Allow All Channels: {AllowAllChannels}
            Allowed Channel Ids: {allowedChannelIds}
            Users: {users}
            ";
        }
    }

}