using EcommerceSolutionEFCoreMVC.Data;
using EcommerceSolutionEFCoreMVC.Models.Entities;
using EcommerceSolutionEFCoreMVC.Services.Interfaces;
using EcommerceSolutionEFCoreMVC.Models.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<EcommerceDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.SignIn.RequireConfirmedEmail = true;
    })
    .AddRoles<IdentityRole>()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<EcommerceDbContext>();

builder.Services.Configure<IdentityOptions>(options => 
{
    options.SignIn.RequireConfirmedAccount = true;
    options.SignIn.RequireConfirmedEmail = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
});

/*builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "AspNet.Cookies";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
        options.SlidingExpiration = true;
    });*/

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddControllersWithViews();

builder.Services.ConfigureApplicationCookie(options
    => options.LoginPath = "/Identity/Account/Login");

builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

builder.Services.AddScoped<ISeedUserRoleInitial, SeedUserRoleInitial>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
//builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddTransient<IEmailService, EmailService>();

var cultureInfo = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

await CreateUserProfile(app);

app.UseAuthentication();
app.UseAuthorization();

/*app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");*/

app.MapControllerRoute(
    name: "Admin",
    pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "testemail",
    pattern: "{controller=TestEmail}/{action=SendTestEmail}");


app.Run();

async Task CreateUserProfile(WebApplication app)
{
    var scopedFactory = app.Services.GetService<IServiceScopeFactory>();
    using (var scope = scopedFactory.CreateScope())
    {
        var service = scope.ServiceProvider.GetService<ISeedUserRoleInitial>();
        await service.SeedRolesAsync();
        await service.SeedUsersAsync();
    }
}
