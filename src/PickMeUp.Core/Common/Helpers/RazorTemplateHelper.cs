using RazorLight;
using System;
using System.Threading.Tasks;

namespace PickMeUp.Core.Common.Helpers;

/// <summary>
/// Helper for rendering Razor templates to HTML strings.
/// </summary>
public static class RazorTemplateHelper
{
    /// <summary>
    /// Renders a Razor template to HTML string.
    /// </summary>
    /// <typeparam name="TModel">The model type for the template.</typeparam>
    /// <param name="razorEngine">The RazorLight engine instance.</param>
    /// <param name="templateName">The name of the template file (without .cshtml extension).</param>
    /// <param name="model">The model to pass to the template.</param>
    /// <returns>The rendered HTML string.</returns>
    public static async Task<string> RenderTemplateAsync<TModel>(
        IRazorLightEngine razorEngine,
        string templateName,
        TModel model)
    {
        try
        {
            return await razorEngine.CompileRenderAsync($"{templateName}.cshtml", model);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to render template '{templateName}': {ex.Message}", ex);
        }
    }
}
