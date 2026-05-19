using AppGrpc;
using AppGrpc.Services;

var builder = WebApplication.CreateBuilder(args);

// Use package DotNetEnv config to load env file at start of app
DotNetEnv.Env.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

// Add services to the container.
builder.Services.ServicesRegister(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<AccountGrpcEndpoint>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
