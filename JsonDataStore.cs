using System;
using System.IO;
using ServerConfig;
using System.Collections.Generic;
using Newtonsoft.Json;


public class JsonDataStore : IDataService
{
    private readonly object ReadWriteLock = new object();
    private static readonly String serverSettingsFile = ".\\server.config";
    
    public bool UpdateServerSettingsAsync(String serverID, ServerSettings serverSettings)
    {
        bool updated = false;

        lock (ReadWriteLock)
        {
            if(File.Exists(serverSettingsFile)){
                try{
                    String jsonString = File.ReadAllText(serverSettingsFile);
                    Dictionary<String, ServerSettings> serverConfig = JsonConvert.DeserializeObject<Dictionary<string, ServerSettings>>(jsonString);

                    serverConfig[serverID] = serverSettings;
                    string jsonStringOut = JsonConvert.SerializeObject(serverConfig);

                    File.WriteAllText(serverSettingsFile, jsonStringOut);                    
                    return true;
                }
                catch(Exception onWrite){
                    System.Console.WriteLine($"\nException: {onWrite.HResult}");
                    System.Console.WriteLine("Dev Comment: Problem writing server settings to file.");
                    System.Console.WriteLine("Location: JsonDataStore.cs - function UpdateServerSettingsAsync - line 20");
                    System.Console.WriteLine($"Source: {onWrite.Source}");
                    System.Console.WriteLine($"Message: {onWrite.Message}");
                    System.Console.WriteLine($"Stack Trace: {onWrite.StackTrace}");
                    System.Console.WriteLine($"Inner Exception: {onWrite.InnerException}\n");
                }
            }
            else{
                try{
                    Dictionary<String, ServerSettings> serverConfig = new Dictionary<string, ServerSettings>();
                    serverConfig[serverID] = serverSettings;
                    string jsonStringOut = JsonConvert.SerializeObject(serverConfig);
                    File.WriteAllText(serverSettingsFile, jsonStringOut);
                    return true;
                }
                catch(Exception onWrite){
                    System.Console.WriteLine($"\nException: {onWrite.HResult}");
                    System.Console.WriteLine("Dev Comment: Problem writing server settings to file.");
                    System.Console.WriteLine("Location: JsonDataStore.cs - function UpdateServerSettingsAsync - line 41");
                    System.Console.WriteLine($"Source: {onWrite.Source}");
                    System.Console.WriteLine($"Message: {onWrite.Message}");
                    System.Console.WriteLine($"Stack Trace: {onWrite.StackTrace}");
                    System.Console.WriteLine($"Inner Exception: {onWrite.InnerException}\n");
                }
            }
        }

        return updated;
    }
    
    public ServerSettings GetOrCreateServerSettingsAsync(String serverID)
    {
        lock (ReadWriteLock)
        {
            if(File.Exists(serverSettingsFile)){
                try{
                    String jsonString = File.ReadAllText(serverSettingsFile);
                    Dictionary<String, ServerSettings> serverConfig = JsonConvert.DeserializeObject<Dictionary<string, ServerSettings>>(jsonString);
                    
                    ServerSettings serverSettings = new ServerSettings();
                    bool serverIsPresent = serverConfig.TryGetValue(serverID, out serverSettings);
                    if(serverIsPresent){
                        return serverSettings;
                    }
                    else{
                        System.Console.WriteLine("Server is not registered, creating new settings...");
                        return new ServerSettings();
                    }
                }
                catch(Exception onWrite){
                    System.Console.WriteLine($"\nException: {onWrite.HResult}");
                    System.Console.WriteLine("Dev Comment: Problem parsing and serializing config file.");
                    System.Console.WriteLine("Location: JsonDataStore.cs - function GetOrCreateServerSettingsAsync - line 68");
                    System.Console.WriteLine($"Source: {onWrite.Source}");
                    System.Console.WriteLine($"Message: {onWrite.Message}");
                    System.Console.WriteLine($"Stack Trace: {onWrite.StackTrace}");
                    System.Console.WriteLine($"Inner Exception: {onWrite.InnerException}\n");
                    return new ServerSettings();
                }
            }
            else{
                System.Console.WriteLine("No server config exists, creating new settings...");
                return new ServerSettings();
            }
        }
    }
}