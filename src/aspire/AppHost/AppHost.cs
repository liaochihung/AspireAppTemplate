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
    .WithReference(cache)
    .WaitFor(cache)
    .WithEnvironment("Keycloak__BaseUrl", "http://keycloak:8080")
    .WithEnvironment("Keycloak__Realm", "AspireApp")
    .WithEnvironment("Keycloak__ClientId", "admin-cli")
    .WithEnvironment("Keycloak__ClientSecret", "")
    .WithEnvironment("Keycloak__AdminUsername", username)
    .WithEnvironment("Keycloak__AdminPassword", password);

var minioUser = builder.AddParameter("minio-user", secret: true, value: "minioadmin");
var minioPass = builder.AddParameter("minio-pass", secret: true, value: "minioadmin");

var minio = builder.AddContainer("minio", "minio/minio")
    .WithArgs("server", "/data", "--console-address", ":9001")
    .WithHttpEndpoint(port: 9000, targetPort: 9000, name: "api")
    .WithHttpEndpoint(port: 9001, targetPort: 9001, name: "ui")
    .WithEnvironment("MINIO_ROOT_USER", minioUser)
    .WithEnvironment("MINIO_ROOT_PASSWORD", minioPass)
    .WithVolume("minio-data", "/data");

apiService.WithEnvironment("MinIO__Endpoint", minio.GetEndpoint("api"))
          .WithEnvironment("MinIO__AccessKey", minioUser)
          .WithEnvironment("MinIO__SecretKey", minioPass);

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
