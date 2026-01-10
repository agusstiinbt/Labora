var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var apiService = builder.AddProject<Projects.Labora_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.Labora_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

builder.AddProject<Projects.Labora_Server_Users>("labora-server-users");

builder.AddProject<Projects.Labora_Server_Authentication>("labora-server-authentication");

builder.AddProject<Projects.Profesionals_API>("profesionals-api");

builder.Build().Run();
