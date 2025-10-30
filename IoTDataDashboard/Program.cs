using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using IoTDataDashboard;
using IoTDataDashboard.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient with the backend API base URL
builder.Services.AddScoped(sp => new HttpClient { 
    BaseAddress = new Uri("http://localhost:5000/"),
    Timeout = TimeSpan.FromSeconds(30)
});

// Register the SensorDataService
builder.Services.AddScoped<SensorDataService>();

await builder.Build().RunAsync();