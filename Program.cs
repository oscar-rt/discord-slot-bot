using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ServerConfig;
class Program
{
    // Program entry point
    static void Main(string[] args)
    {
        // Call the Program constructor, followed by the 
        // MainAsync method and wait until it finishes (which should be never).
        new Program().MainAsync().GetAwaiter().GetResult();
    }

    private readonly DiscordSocketClient _client;
    
    // Keep the CommandService and DI container around for use with commands.
    // These two types require you install the Discord.Net.Commands package.
    private readonly CommandService _commands;
    private readonly IServiceProvider _services;

    private Program()
    {
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            // How much logging do you want to see?
            LogLevel = LogSeverity.Info,
            AlwaysDownloadUsers = true,
            // If you or another service needs to do anything with messages
            // (eg. checking Reactions, checking the content of edited/deleted messages),
            // you must set the MessageCacheSize. You may adjust the number as needed.
            //MessageCacheSize = 50,

            // If your platform doesn't have native WebSockets,
            // add Discord.Net.Providers.WS4Net from NuGet,
            // add the `using` at the top, and uncomment this line:
            //WebSocketProvider = WS4NetProvider.Instance
        });
        
        _commands = new CommandService(new CommandServiceConfig
        {
            // Again, log level:
            LogLevel = LogSeverity.Info,
            
            // There's a few more properties you can set,
            // for example, case-insensitive commands.
            CaseSensitiveCommands = false,
        });
        
        // Subscribe the logging handler to both the client and the CommandService.
        _client.Log += Log;
        _commands.Log += Log;
        
        // Setup your DI container.
        _services = ConfigureServices();
        
    }
    
    // If any services require the client, or the CommandService, or something else you keep on hand,
    // pass them as parameters into this method as needed.
    // If this method is getting pretty long, you can seperate it out into another file using partials.
    private static IServiceProvider ConfigureServices()
    {
        var map = new ServiceCollection(); 
        // Repeat this for all the service classes
        // and other dependencies that your commands might need.

        map.AddSingleton<IDataService, JsonDataStore>();
        // When all your required services are in the collection, build the container.
        // Tip: There's an overload taking in a 'validateScopes' bool to make sure
        // you haven't made any mistakes in your dependency graph.
        return map.BuildServiceProvider();
    }

    // Example of a logging handler. This can be re-used by addons
    // that ask for a Func<LogMessage, Task>.
    private static Task Log(LogMessage message)
    {
        switch (message.Severity)
        {
            case LogSeverity.Critical:
            case LogSeverity.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            case LogSeverity.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case LogSeverity.Info:
                Console.ForegroundColor = ConsoleColor.White;
                break;
            case LogSeverity.Verbose:
            case LogSeverity.Debug:
                Console.ForegroundColor = ConsoleColor.DarkGray;
                break;
        }
        Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message} {message.Exception}");
        Console.ResetColor();
        
        // If you get an error saying 'CompletedTask' doesn't exist,
        // your project is targeting .NET 4.5.2 or lower. You'll need
        // to adjust your project's target framework to 4.6 or higher
        // (instructions for this are easily Googled).
        // If you *need* to run on .NET 4.5 for compat/other reasons,
        // the alternative is to 'return Task.Delay(0);' instead.
        return Task.CompletedTask;
    }

    private async Task MainAsync()
    {
        // Centralize the logic for commands into a separate method.
        await InitCommands();

        DotNetEnv.Env.Load();
        // Login and connect.
        await _client.LoginAsync(TokenType.Bot,
            // < DO NOT HARDCODE YOUR TOKEN >
             DotNetEnv.Env.GetString("DISCORD_TOKEN"));
        await _client.StartAsync();

        // Wait infinitely so your bot actually stays connected.
        await Task.Delay(Timeout.Infinite);
    }

    private async Task InitCommands()
    {
        // Either search the program and add all Module classes that can be found.
        // Module classes MUST be marked 'public' or they will be ignored.
        // You also need to pass your 'IServiceProvider' instance now,
        // so make sure that's done before you get here.
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        // Or add Modules manually if you prefer to be a little more explicit:

        // TODO:  Deal with modules here
        //await _commands.AddModuleAsync<SomeModule>(_services);
        // Note that the first one is 'Modules' (plural) and the second is 'Module' (singular).

        // Subscribe a handler to see if a message invokes a command.
        _client.MessageReceived += HandleCommandAsync;
        _client.GuildMembersDownloaded += UpdateBasicServerInfo;
    }
    
    private async Task UpdateBasicServerInfo(SocketGuild guild){
        

        //Get and update server settings for all guilds
        ServerSettings serverSettings = _services.GetService<IDataService>().GetOrCreateServerSettingsAsync($"{guild.Id}");
        
        //Update Server Name - This can never fail due to id
        serverSettings.ServerName = guild.Name;

        //List<SocketUser> allUsers = new List<SocketUser>();

        // TODO: Commit seppukku for writing this abomintation
        //foreach(SocketChannel channel in guild.Channels){
        //    await Log(new LogMessage(LogSeverity.Info, "func - UpdateBasicServerInfo", $"Channel: {channel}", null));
        //    foreach(SocketUser user in channel.Users){
        //        if(!allUsers.Contains(user) && (user.Id != _client.CurrentUser.Id)){
        //            await Log(new LogMessage(LogSeverity.Info, "func - UpdateBasicServerInfo", $"User: {user.Username}", null));
        //            allUsers.Add(user);
        //        }
        //    }
        //}

        //Update all users info based on id except for money
        if(serverSettings.Users == null){
            serverSettings.Users = new System.Collections.Generic.Dictionary<string, User>();
            foreach(SocketGuildUser user in guild.Users){
                if(user.Id != _client.CurrentUser.Id){
                    User guildUser = new User();
                    guildUser.money = 0.0f;
                    guildUser.userName = user.Username;
                    guildUser.userTag = user.Discriminator;
                    serverSettings.Users.Add($"{user.Id}",guildUser);
                }
            }
        }
        else
        {
            foreach(SocketGuildUser user in guild.Users){
                User guildUser = new User();
                bool userExists = serverSettings.Users.TryGetValue($"{user.Id}", out guildUser);
                if(userExists){
                    guildUser.userName = user.Username;
                    guildUser.userTag = user.Discriminator;
                    serverSettings.Users[$"{user.Id}"] = guildUser;
                }
                else{
                    if(user.Id != _client.CurrentUser.Id){
                        guildUser = new User();
                        guildUser.money = 0.0f;
                        guildUser.userName = user.Username;
                        guildUser.userTag = user.Discriminator;
                        serverSettings.Users.Add($"{user.Id}",guildUser);
                    }
                }
            }
        }

        _services.GetService<IDataService>().UpdateServerSettingsAsync($"{guild.Id}", serverSettings);

        

    }
    private async Task HandleCommandAsync(SocketMessage arg)
    {
        // Bail out if it's a System Message.
        var msg = arg as SocketUserMessage;
        if (msg == null) return;

        // We don't want the bot to respond to itself or other bots or webhooks.
        if (msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot || msg.Author.IsWebhook) return;
        
        // Create a number to track where the prefix ends and the command begins
        int pos = 0;
        // Replace the '!' with whatever character
        // you want to prefix your commands with.
        // Uncomment the second half if you also want
        // commands to be invoked by mentioning the bot instead.
        if (msg.HasCharPrefix('!', ref pos) /* || msg.HasMentionPrefix(_client.CurrentUser, ref pos) */)
        {
            
            // Create a Command Context.
            var context = new SocketCommandContext(_client, msg);
            
            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully).
            var result = await _commands.ExecuteAsync(context, pos, _services);
            //await msg.Channel.SendMessageAsync($"Id: <@{msg.Author.Id}> Username: {msg.Author.Username}  DiscriminatorValue: {msg.Author.DiscriminatorValue}");
            // Uncomment the following lines if you want the bot
            // to send a message if it failed.
            // This does not catch errors from commands with 'RunMode.Async',
            // subscribe a handler for '_commands.CommandExecuted' to see those.
            //if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
            //    await msg.Channel.SendMessageAsync(result.ErrorReason);
            
            if(msg.Content.Contains("!update")){
                foreach(SocketGuild guild in _client.Guilds){
                    await UpdateBasicServerInfo(guild);
                }
            }

        }   
    }
}