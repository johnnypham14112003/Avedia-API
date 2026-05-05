using App;
using App.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Use package DotNetEnv config to load env file at start of app
DotNetEnv.Env.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

// Add services to the container.
builder.Services.ServicesRegister(builder.Configuration);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Console notes
Console.WriteLine("For develop environment, make sure .env is in the same folder as Program.cs before running.");
Console.WriteLine("For tracking whether this app is running from which location. Make sure .env is at path: " + Directory.GetCurrentDirectory());

var app = builder.Build();

// Wrap whole app to catch error
app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//---------------------------

// Require add secure header, must use https
//app.UseHsts();

// Check whether request is http or https (change http -> https)
//app.UseHttpsRedirection();

//---------------------------

// Return static file like img,css soon for not running its heavy logic
//app.UseStaticFiles();

//---------------------------

// Read URL to identify which method
//app.UseRouting();

//---------------------------

// Check permission to request API from different domains
app.UseCors("AVECORS");
app.UseRateLimiter(); // Limit request from an IP/user in a time

//---------------------------

// Check who is login
app.UseAuthentication();

// Check does user have permission to do an action
app.UseAuthorization();

//---------------------------

// Execute logic in controllers
app.MapControllers().RequireRateLimiting("FiftyOne");

app.Run();