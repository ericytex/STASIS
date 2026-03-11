using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using STASIS.Data;
using STASIS.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<StasisDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.SignIn.RequireConfirmedAccount = true; 
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<StasisDbContext>();

// Email Configuration
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EmailSender>();

builder.Services.AddScoped<ISampleService, SampleService>();
builder.Services.AddScoped<IStorageService, StorageService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<ILabSetupService, LabSetupService>();
builder.Services.AddScoped<IShipmentService, ShipmentService>();

builder.Services.AddRazorPages()
    .AddMvcOptions(options =>
    {
        options.Filters.Add<STASIS.Services.PasswordChangeFilter>();
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
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
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Seed Admin User
// Requires AdminSeedPassword in configuration (user secrets, env var, or appsettings).
// Set via: dotnet user-secrets set "AdminSeedPassword" "YourSecurePassword"
using (var scope = app.Services.CreateScope())
{
    var adminPassword = app.Configuration["AdminSeedPassword"];
    if (string.IsNullOrEmpty(adminPassword))
    {
        var startupLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        startupLogger.LogWarning(
            "AdminSeedPassword is not configured. The admin account will not be seeded. " +
            "Set it with: dotnet user-secrets set \"AdminSeedPassword\" \"YourSecurePassword\"");
    }
    else
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var adminEmail = "admin@stasis.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded && await roleManager.RoleExistsAsync("Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}

app.Run();
