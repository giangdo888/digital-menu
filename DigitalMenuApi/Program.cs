using DigitalMenuApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add application services
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
var app = builder.Build();

// Configure the HTTP request pipeline
app.ConfigureApplicationPipeline();

app.Run();
