using Microsoft.AspNetCore.Authentication.Cookies;
using WordStation.WebUI.Services.Abstract;
using WordStation.WebUI.Services.Concrete;
using WordStation.WebUI.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.ConfigureDataProtection(builder.Environment);

builder.Services.ConfigureCustomApplicationCookie();

builder.Services.AddHttpClient("WordStationApi", client => {
    var apiUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5032"; 
    client.BaseAddress = new Uri(apiUrl);
});

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Word}/{action=Index}/{id?}");

app.Run();