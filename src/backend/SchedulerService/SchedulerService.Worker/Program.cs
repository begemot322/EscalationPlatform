using EscalationPlatform.OpenTelemetry;
using SchedulerService;

const string serviceName = "SchedulerService";
var telemetryResource = TelemetryResourceBuilder.CreateResource(serviceName, out var version);
Console.WriteLine($"Starting {serviceName}:{version}\n");

var builder = Host.CreateApplicationBuilder(args);
builder.AddSchedulerServices(telemetryResource);

var host = builder.Build();
host.Run();