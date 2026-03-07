using DigitalMenuApi.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add application services
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
var app = builder.Build();

// Configure the HTTP request pipeline
app.ConfigureApplicationPipeline();


// Automatically apply EF Core Migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DigitalMenuApi.Data.ApplicationDbContext>();
    // This connects to the DB (local or AWS) and runs any pending migrations
    dbContext.Database.Migrate();
}

app.Run();
