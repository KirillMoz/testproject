using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using testproject.Data;
using testproject.Models;

var builder = WebApplication.CreateBuilder(args);

// Добавьте DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Настройка Identity (УБЕРИТЕ угловые скобки и исправьте синтаксис)
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Добавление MVC
builder.Services.AddControllersWithViews();

// Добавьте поддержку Razor Pages (нужно для Identity)
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Добавьте UseAuthentication перед UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "register",
    pattern: "Register/{action=Index}",
    defaults: new { controller = "Register" });

app.MapControllerRoute(
    name: "profile",
    pattern: "Profile/{action=Index}/{id?}",
    defaults: new { controller = "Profile" });

app.MapControllerRoute(
    name: "friends",
    pattern: "Friends/{action=Index}",
    defaults: new { controller = "Friends" });

app.MapControllerRoute(
    name: "messages",
    pattern: "Messages/{action=Index}/{id?}",
    defaults: new { controller = "Messages" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Добавьте маршрут для Razor Pages
app.MapRazorPages();

app.Run();