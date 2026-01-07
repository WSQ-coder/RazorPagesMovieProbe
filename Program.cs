using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
//builder.Services.AddDbContext<RazorPagesMovie.Models.ArtMarketDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Server=localhost;Database=art_market_db;Username=postgres;Password=1234; Persist Security Info=True"))); ;

builder.Services.AddDbContext<RazorPagesMovie.Models.ArtMarketDbContext>(options => {
    options.UseNpgsql(builder.Configuration.GetConnectionString("Server=localhost;Database=art_market_db;Username=postgres;Password=1234; Persist Security Info=True"));
    // отладка - вывод логов в консоль
    options.LogTo(Console.WriteLine, LogLevel.Information); // Печатать логи в консоль
    options.EnableSensitiveDataLogging(); // Чтобы видеть значения параметров (например, само имя пользователя в INSERT)
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
// Устанавливаем стартовую страницу
//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapRazorPages();
//    endpoints.MapGet("/", async context =>
//    {context.Response.Redirect("/Index");});
//});
app.Run();
