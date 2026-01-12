using Microsoft.AspNetCore.Authentication.Cookies;
using WordStation.WebUI.Services.Abstract;
using WordStation.WebUI.Services.Concrete;
using WordStation.WebUI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

builder.Services.ConfigureDataProtection(builder.Environment);

// Authentication - Cookie Based (Extension Metodu)
builder.Services.ConfigureCustomApplicationCookie();

// HttpClient Configuration
builder.Services.AddHttpClient("WordStationApi", client => {
    // API URL - appsettings.json'dan okunabilir, fallback veriyoruz
    var apiUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000/api/"; 
    client.BaseAddress = new Uri(apiUrl);
});

// Service Registrations
builder.Services.AddScoped<IAuthApiService, AuthApiService>();
builder.Services.AddScoped<IWordApiService, WordApiService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// app.UseSession(); // Session middleware (KALDIRILDI)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();