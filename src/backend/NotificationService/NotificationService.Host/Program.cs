using NotificationService.Host;

var hostBuilder = HostStartup.CreateHostBuilder(args);
var host = hostBuilder.Build();
await host.RunAsync();