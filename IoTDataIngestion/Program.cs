using Microsoft.EntityFrameworkCore;
using IoTDataIngestion;

// Create a web application builder
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
// Configure Entity Framework with SQLite database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=sensorreadings.db"));

// Add MVC controllers
builder.Services.AddControllers();

// Register the MQTT background service
builder.Services.AddHostedService<MqttService>();

// Register the data retention service
builder.Services.AddHostedService<DataRetentionService>();

// Add CORS policy to allow requests from Blazor development server
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorWasm", policy =>
    {
        policy.WithOrigins("https://localhost:3000", "http://localhost:3000", "http://localhost:5045")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Build the web application
var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Enable CORS
app.UseCors("AllowBlazorWasm");

// Enable HTTPS redirection
app.UseHttpsRedirection();

// Add controllers middleware to handle API requests
app.MapControllers();

// Apply database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
}

// Default endpoint for health check
app.MapGet("/", () => "IoT Data Ingestion Microservice");

// Run the application
app.Run();