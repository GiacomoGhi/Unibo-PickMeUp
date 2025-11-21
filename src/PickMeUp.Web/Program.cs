using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PickMeUp.Core.Infrastructure;
using PickMeUp.Core.Services.Email;
using System.Globalization;
using System.Linq;
using PickMeUp.Web;
using PickMeUp.Core.Services.GoogleRoutes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<GoogleSettings>(builder.Configuration.GetSection("Authentication:Google"));

// Pick me up database
builder.Services.AddPickMeUpDatabase(builder.Configuration);

// SERVICES FOR AUTHENTICATION
builder.Services.AddSession();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/";
    options.LogoutPath = "/Auth/Logout";
});

var mvcBuilder = builder.Services.AddMvc()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization(options =>
    {                        // Enable loading SharedResource for ModelLocalizer
        options.DataAnnotationLocalizerProvider = (type, factory) =>
            factory.Create(typeof(SharedResource));
    });

#if DEBUG
mvcBuilder.AddRazorRuntimeCompilation();
#endif

// Register PickMeUp.Core services
builder.Services.AddPickMeUpCoreServices();


var app = builder.Build();

// Setup pick me up core database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var env = services.GetRequiredService<IWebHostEnvironment>();
    await services.SetupDatabaseAsync(env.IsDevelopment());
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    // Https redirection only in production
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Localization support if you want to
app.UseRequestLocalization(SupportedCultures.CultureNames);

app.UseRouting();

// Adding authentication to pipeline
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.MapControllerRoute("default", "{controller=Home}/{action=Landing}");


app.Run();


public static class SupportedCultures
{
    public readonly static string[] CultureNames;
    public readonly static CultureInfo[] Cultures;

    static SupportedCultures()
    {
        CultureNames = new[] { "it-it" };
        Cultures = CultureNames.Select(c => new CultureInfo(c)).ToArray();

        //NB: attenzione nel progetto a settare correttamente <NeutralLanguage>it-IT</NeutralLanguage>
    }
}