using ChatApp.Data;
using ChatApp.Models;
using ChatApp.Supporters.CustomIdentityProvider;
using ChatApp.Hubs;
using ChatApp.Services;
using ChatApp.Supporters.DataGenerator;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// Add dbcontext
builder.Services.AddDbContext<ChatAppImplementationContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("localDB"));
});

//Add custom identity
builder.Services.AddIdentity<User, Role>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    
}).AddDefaultTokenProviders();
builder.Services.AddTransient<IUserStore<User>, UserStore>();
builder.Services.AddTransient<IRoleStore<Role>, RoleStore>();
builder.Services.AddTransient<IChatService, ChatService>();

builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

    options.LoginPath = "/Account/Login";
    options.SlidingExpiration = true;
});



var app = builder.Build();

/*
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ChatAppImplementationContext>();
    context.Database.EnsureCreated();

    var adminPassword = builder.Configuration.GetValue<string>("AdminAccount:password");
    var adminName = builder.Configuration.GetValue<string>("AdminAccount:username");

    await ApplicationDataGenerator.Initialize(services, adminName, adminPassword);

}    */

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.MapHub<ChatHub>("/chatHub");

app.Run();
