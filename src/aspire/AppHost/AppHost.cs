var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var username = builder.AddParameter("username", secret: true, value: "admin");
var password = builder.AddParameter("password", secret: true, value: "admin");

var keycloak = builder.AddKeycloak("keycloak", 8080, username, password)
    .WithDataVolume("keycloak")
    .WithRealmImport("./Realms")
    .WithBindMount("./keycloak-themes/my-company-theme", "/opt/keycloak/themes/my-company-theme");

var postgresPassword = builder.AddParameter("postgres-password", secret: true, value: "1111");
var postgres = builder.AddPostgres("postgres", password: postgresPassword, port: 5436)
    .WithDataVolume();

var aspiredb = postgres.AddDatabase("aspiredb");

var apiService = builder.AddProject<Projects.AspireAppTemplate_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithReference(aspiredb)
    .WaitFor(aspiredb)
    .WithEnvironment("Keycloak__BaseUrl", "http://keycloak:8080")
    .WithEnvironment("Keycloak__Realm", "WeatherShop")
    .WithEnvironment("Keycloak__ClientId", "admin-cli")
    .WithEnvironment("Keycloak__ClientSecret", "")
    .WithEnvironment("Keycloak__AdminUsername", username)
    .WithEnvironment("Keycloak__AdminPassword", password);

builder.AddProject<Projects.AspireAppTemplate_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)    
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithReference(keycloak)
    .WaitFor(keycloak);

await builder.Build().RunAsync();
