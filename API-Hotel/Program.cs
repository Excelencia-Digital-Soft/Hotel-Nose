using hotel.Extensions;
using hotel.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure services using extension methods
builder.Services.AddCorsConfiguration();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddIdentityConfiguration();
builder.Services.AddApiVersioningConfiguration();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddDatabaseServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddBackgroundServices();

var app = builder.Build();


// Configure the HTTP request pipeline
app.UseApplicationPipeline();

app.Run();

