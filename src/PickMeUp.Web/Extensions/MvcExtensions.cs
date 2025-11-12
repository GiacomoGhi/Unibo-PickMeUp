using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Globalization;

namespace PickMeUp.Web.Extensions;

public static class MvcExtensions
{
    /// <summary>
    /// Gets the current request's culture for localization (e.g., "it-IT", "en-US")
    /// </summary>
    public static CultureInfo CurrentCulture(this RazorPage page)
    {
        var rqf = page.Context.Features.Get<IRequestCultureFeature>();
        return rqf?.RequestCulture.Culture ?? CultureInfo.CurrentCulture;
    }
}
