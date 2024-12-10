using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProiectMPA.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Configuration;
using ProiectMPA.Hubs;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<ProiectMPAContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'ProiectMPAContext' not found.")));

builder.Services.AddDbContext<IdentityContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddSignalR();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    // Configurare blocare utilizator după încercări eșuate
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.AllowedForNewUsers = true;

    // Configurare politici pentru parola
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // Alte opțiuni legate de autentificare
    options.SignIn.RequireConfirmedAccount = true;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<IdentityContext>();
    //.AddEntityFrameworkStores<ProiectMPAContext>()
    //.AddDefaultTokenProviders();

//builder.Services.AddAuthorization(opts =>
//{
//    opts.AddPolicy("OnlyAdmins", policy =>
//    {
//        policy.RequireClaim("Department", "IT");
//    });
//});

//builder.Services.AddAuthorization(opts =>
//{
//    opts.AddPolicy("SalesManager", policy => {
//        policy.RequireRole("Manager");
//        policy.RequireClaim("Department", "Sales");
//    });
//});

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

app.UseAuthentication();
app.UseAuthorization();


app.MapRazorPages();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.MapHub<ChatHub>("/Chat");

app.Run();
