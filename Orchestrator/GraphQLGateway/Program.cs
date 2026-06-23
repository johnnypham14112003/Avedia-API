using GraphQLGateway;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ServicesRegister(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Require add secure header, must use https
app.UseHsts();

// Check whether request is http or https (change http -> https)
app.UseHttpsRedirection();

//---------------------------

// Return static file like img,css soon for not running its heavy logic
//app.UseStaticFiles();

//---------------------------

// Check permission to request API from different domains
app.UseCors("AVECORS");
app.UseRateLimiter(); // Limit request from an IP/user in a time

//---------------------------

app.MapGraphQL();

app.Run();