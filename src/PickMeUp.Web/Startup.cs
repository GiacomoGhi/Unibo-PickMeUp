using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PickMeUp.Core.Infrastructure;
using PickMeUp.Core.Services.Email;
using PickMeUp.Web.Infrastructure;
using PickMeUp.Web.SignalR;
using PickMeUp.Web.SignalR.Hubs;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PickMeUp.Web;

public class Startup(IConfiguration configuration, IWebHostEnvironment env)
{
    public IConfiguration Configuration { get; } = configuration;

    public IWebHostEnvironment Env { get; set; } = env;

    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
        services.Configure<EmailSettings>(Configuration.GetSection("Email"));

        // Pick me up database
        services.AddPickMeUpDatabase(Configuration);

        // SERVICES FOR AUTHENTICATION
        services.AddSession();
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
        {
            options.LoginPath = "/Login/Login";
            options.LogoutPath = "/Login/Logout";
        });

        var builder = services.AddMvc()
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
            .AddDataAnnotationsLocalization(options =>
            {                        // Enable loading SharedResource for ModelLocalizer
                options.DataAnnotationLocalizerProvider = (type, factory) =>
                    factory.Create(typeof(SharedResource));
            });

#if DEBUG
        builder.AddRazorRuntimeCompilation();
#endif

        services.Configure<RazorViewEngineOptions>(options =>
        {
            options.AreaViewLocationFormats.Clear();
            options.AreaViewLocationFormats.Add("/Areas/{2}/{1}/{0}.cshtml");
            options.AreaViewLocationFormats.Add("/Areas/{2}/Views/{1}/{0}.cshtml");
            options.AreaViewLocationFormats.Add("/Areas/{2}/Views/Shared/{0}.cshtml");
            options.AreaViewLocationFormats.Add("/Views/Shared/{0}.cshtml");

            options.ViewLocationFormats.Clear();
            options.ViewLocationFormats.Add("/Features/{1}/{0}.cshtml");
            options.ViewLocationFormats.Add("/Features/Views/{1}/{0}.cshtml");
            options.ViewLocationFormats.Add("/Features/Views/Shared/{0}.cshtml");
            options.ViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
        });

        // SIGNALR FOR COLLABORATIVE PAGES
        services.AddSignalR();

        // Registration of SignalR events
        services.AddScoped<IPublishDomainEvents, SignalrPublishDomainEvents>();

        // Register PickMeUp.Core services
        services.AddPickMeUpCoreServices();
    }

    public async Task Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Setup pick me up core database
        await app.ApplicationServices.SetupDatabase(env.IsDevelopment());

        // Configure the HTTP request pipeline.
        if (!env.IsDevelopment())
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

        var node_modules = new CompositePhysicalFileProvider(Directory.GetCurrentDirectory(), "node_modules");
        var areas = new CompositePhysicalFileProvider(Directory.GetCurrentDirectory(), "Areas");
        var compositeFp = new CustomCompositeFileProvider(env.WebRootFileProvider, node_modules, areas);
        env.WebRootFileProvider = compositeFp;
        app.UseStaticFiles();

        app.UseEndpoints(endpoints =>
        {
            // ROUTING PER HUB
            endpoints.MapHub<TemplateHub>("/templateHub");

            endpoints.MapAreaControllerRoute("Example", "Example", "Example/{controller=Users}/{action=Index}/{id?}");
            endpoints.MapControllerRoute("default", "{controller=Login}/{action=Login}");
        });
    }
}

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
