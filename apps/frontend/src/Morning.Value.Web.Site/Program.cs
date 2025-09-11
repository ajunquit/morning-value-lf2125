using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Morning.Value.Application;
using Morning.Value.Application.Common.Services;
using Morning.Value.Infrastructure;
using Morning.Value.Web.Site.Auth.Services;
using Morning.Value.Web.Site.Books;
using Morning.Value.Web.Site.Loans;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 1) Cookie Auth
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/auth/signin";
        options.LogoutPath = "/auth/signout";
        options.AccessDeniedPath = "/auth/denied";
        options.Cookie.Name = "mv.auth";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

// 2) Política global: TODO requiere estar autenticado
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.AddPolicy("ReaderOnly", p => p.RequireRole("Reader"));
});

builder.Services.
    AddApplicationServices(builder.Configuration).
    AddInfrastructureServices(builder.Configuration);

builder.Services.AddScoped<ILoanRepository, InMemoryLoanRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<LibraryService>();

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// *** Importante: primero Authentication, luego Authorization
app.UseAuthentication();
app.UseAuthorization();

// Ruta explícita para la raíz "/"
app.MapControllerRoute(
    name: "root",
    pattern: "",
    defaults: new { controller = "Home", action = "Index" }
);

// Ruta por defecto del resto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
