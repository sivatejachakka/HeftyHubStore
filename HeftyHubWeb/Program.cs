using HeftyHub.DataAccess.Data;
using HeftyHub.DataAccess.Repository;
using HeftyHub.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using HeftyHub.Utility;
using Microsoft.AspNetCore.Identity.UI.Services;
using Stripe;
using HeftyHub.DataAccess.DbInitializer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

// on register it will take you to confirmation page
//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();

// to add role to the identity in our project
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
// to make use of ConfigureApplicationCookie, we should add it after Adding Identity
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

// configure facebook registration
builder.Services.AddAuthentication().AddFacebook(option =>
{
    option.AppId = "439735382477147";
    option.AppSecret = "b6b51a48ced87211b9420ea5b756e9fe";
});

// configuring the application to add session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// As Identity is implemented in Razor pages
builder.Services.AddRazorPages();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailSender, EmailSender>();

//for adding pending migrations and create an admin user by default
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
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

// when we are using stripe, one thing we have to configure is the API Key which is a secret key.
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// also have to add session in our request pipeline and with this the application is configured now to use session
app.UseSession();

//for adding pending migrations and create an admin user by default
SeedDatabase();

// As Identity is implemented in Razor pages
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();

//for adding pending migrations and create an admin user by default
void SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        dbInitializer.Initialize();
    }
}