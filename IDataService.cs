using System;
using ServerConfig;

public interface IDataService{

    public bool UpdateServerSettingsAsync(String serverID, ServerSettings serversettings);

    public ServerSettings GetOrCreateServerSettingsAsync(String serverID);
}