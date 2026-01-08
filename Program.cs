using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Включим поддержку Pages/ с файлами *.cshtml + *.cshtml.cs 
builder.Services.AddRazorPages();


// подключаем БД с логированием
builder.Services.AddDbContext<RazorPagesMovie.Models.ArtMarketDbContext>(options => {
    options.UseNpgsql(builder.Configuration.GetConnectionString("Server=localhost;Database=art_market_db;Username=postgres;Password=1234; Persist Security Info=True"));
    // отладка - вывод логов в консоль
    options.LogTo(Console.WriteLine, LogLevel.Information); // Печатать логи в консоль
    options.EnableSensitiveDataLogging(); // Чтобы видеть значения параметров (например, само имя пользователя в INSERT)
});

// авторизация сделана по примеру https://metanit.com/sharp/aspnet6/13.7.php
// включаем сервис аутонтификации на основе куки
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {   options.LoginPath = "/login";
        options.AccessDeniedPath = "/accessdenied";
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication(); // включаем посредников аутентификации и авторизации
app.UseAuthorization();

app.MapStaticAssets(); //оптимизируем(сжимаем) статические файлы в wwwroot
app.MapRazorPages().WithStaticAssets(); // подключаем маршруты папки Pages/


app.Run();
