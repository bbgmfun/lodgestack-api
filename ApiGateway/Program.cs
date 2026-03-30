using System.Text.Json;
using System.Text.Json.Nodes;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

var ocelotConfigPath = Path.Combine(builder.Environment.ContentRootPath, "ocelot.json");
var stayApiBaseUrl = builder.Configuration["StayApi:BaseUrl"] ?? Environment.GetEnvironmentVariable("STAYAPI_BASE_URL");

if (!string.IsNullOrWhiteSpace(stayApiBaseUrl) &&
    Uri.TryCreate(stayApiBaseUrl, UriKind.Absolute, out var stayApiUri))
{
    var runtimeConfigPath = Path.Combine(builder.Environment.ContentRootPath, "ocelot.runtime.json");
    var configRoot = JsonNode.Parse(File.ReadAllText(ocelotConfigPath))!.AsObject();
    var routes = configRoot["Routes"]?.AsArray() ?? [];

    foreach (var routeNode in routes)
    {
        if (routeNode is not JsonObject route)
            continue;

        route["DownstreamScheme"] = stayApiUri.Scheme;

        var downstreamHosts = route["DownstreamHostAndPorts"]?.AsArray();
        if (downstreamHosts is null || downstreamHosts.Count == 0 || downstreamHosts[0] is not JsonObject hostPort)
            continue;

        hostPort["Host"] = stayApiUri.Host;
        hostPort["Port"] = stayApiUri.IsDefaultPort
            ? stayApiUri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? 443 : 80
            : stayApiUri.Port;
    }

    File.WriteAllText(runtimeConfigPath, configRoot.ToJsonString(new JsonSerializerOptions
    {
        WriteIndented = true
    }));

    builder.Configuration.AddJsonFile(runtimeConfigPath, optional: false, reloadOnChange: false);
}
else
{
    builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
}

builder.Services.AddOcelot();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors();
app.MapGet("/", () => Results.Ok(new { status = "API Gateway is running" }));

await app.UseOcelot();

app.Run();
