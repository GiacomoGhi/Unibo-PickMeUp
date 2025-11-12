using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace PickMeUp.Web.Infrastructure;

public class ModelStateToTempDataAttribute : ActionFilterAttribute
{
    /// <summary>
    /// The TempData key used to store serialized ModelState data.
    /// </summary>
    public static string MODELSTATE_KEY = "ModelStateModel";

    /// <summary>
    /// Called after the action executes. Handles ModelState preservation/restoration.
    /// </summary>
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        OnActionExecutedModelstates(context);
        base.OnActionExecuted(context);
    }

    /// <summary>
    /// Deserializes a JSON string back into a ModelStateDictionary.
    /// </summary>
    public static ModelStateDictionary DeserialiseModelState(string serialisedErrorList)
    {
        var errorList = JsonConvert.DeserializeObject<List<ModelStateTransferValue>>(serialisedErrorList) ?? [];
        var modelState = new ModelStateDictionary();

        foreach (var item in errorList)
        {
            if (item.RawValue is Newtonsoft.Json.Linq.JContainer jContainer)
            {
                modelState.SetModelValue(item.Key, jContainer.Values<string>().ToArray(), item.AttemptedValue);
            }
            else
            {
                modelState.SetModelValue(item.Key, item.RawValue, item.AttemptedValue);
            }

            foreach (var error in item.ErrorMessages)
            {
                modelState.AddModelError(item.Key, error);
            }
        }

        return modelState;
    }

    /// <summary>
    /// Handles ModelState preservation and restoration based on the action result type.
    /// </summary>
    private static void OnActionExecutedModelstates(ActionExecutedContext context)
    {
        if (context.Result is ViewResult && context.Controller is Controller controller)
        {
            // SE SONO IN UNA VIEW PROVO A RICARICARE IL MODELSTATE SE PREPARATO DALLA RICHIESTA PRECEDENTE
            if (controller.TempData[MODELSTATE_KEY] is string serialisedModelState)
            {
                context.ModelState.Merge(DeserialiseModelState(serialisedModelState));
            }
        }
        else if (context.Result is RedirectResult or RedirectToRouteResult or RedirectToActionResult)
        {
            // SE SONO IN UN REDIRECT PRESERVO GLI ERRORI NEL MODELSTATE
            if (!context.ModelState.IsValid && context.Controller is Controller controllerRedirect)
            {
                controllerRedirect.TempData[MODELSTATE_KEY] = SerialiseModelState(context.ModelState);
            }
        }
    }

    /// <summary>
    /// Serializes a ModelStateDictionary to a JSON string for storage in TempData.
    /// </summary>
    private static string SerialiseModelState(ModelStateDictionary modelState)
    {
        var errorList = modelState
            .Select(x => new ModelStateTransferValue
            {
                Key = x.Key,
                AttemptedValue = x.Value?.AttemptedValue,
                RawValue = x.Value?.RawValue,
                ErrorMessages = x.Value?.Errors.Select(err => err.ErrorMessage).ToArray() ?? [],
            }).ToList();

        return JsonConvert.SerializeObject(errorList);
    }

    public class ModelStateTransferValue
    {
        /// <summary>
        /// The model property key (e.g., "Email", "Password").
        /// </summary>
        public string Key { get; set; } = default!;
        
        /// <summary>
        /// The attempted value submitted by the user.
        /// </summary>
        public string? AttemptedValue { get; set; }
        
        /// <summary>
        /// The raw value from model binding.
        /// </summary>
        public object? RawValue { get; set; }
        
        /// <summary>
        /// Validation error messages associated with this field.
        /// </summary>
        public string[] ErrorMessages { get; set; } = [];
    }
}

public static class ModelStateExtensions
{
    /// <summary>
    /// Checks if a specific field has a value stored in the preserved ModelState.
    /// </summary>
    public static bool HasToRestoreValue(this ITempDataDictionary tempData, string name)
    {
        if (!tempData.TryGetValue(ModelStateToTempDataAttribute.MODELSTATE_KEY, out object? value)
            || value is not string serialised)
        {
            return false;
        }

        var modelstateToRestore = ModelStateToTempDataAttribute.DeserialiseModelState(serialised);
        return modelstateToRestore.ContainsKey(name);
    }

    /// <summary>
    /// Attempts to restore the attempted value for a specific field from the preserved ModelState.
    /// </summary>
    public static object? TryRestoreValue(this ITempDataDictionary tempData, string name)
    {
        if (!tempData.TryGetValue(ModelStateToTempDataAttribute.MODELSTATE_KEY, out object? value)
            || value is not string serialised
            || !ModelStateToTempDataAttribute.DeserialiseModelState(serialised).TryGetValue(name, out var entry))
        {
            return null;
        }
        
        return entry.AttemptedValue;
    }
}

public static class ModelStateRazorPageExtensions
{
    /// <summary>
    /// Gets the preserved ModelState as a JSON string in camelCase format.
    /// Used for client-side validation or error display.
    /// </summary>
    public static string GetModelStateDictionaryToJson(this Microsoft.AspNetCore.Mvc.Razor.RazorPageBase page)
    {
        if (!page.TempData.TryGetValue(ModelStateToTempDataAttribute.MODELSTATE_KEY, out object? value)
            || value is not string serialised)
        {
            return JsonSerializerHelper.ToJsonCamelCase(new ModelStateDictionary());
        }

        var modelstate = ModelStateToTempDataAttribute.DeserialiseModelState(serialised);
        return JsonSerializerHelper.ToJsonCamelCase(modelstate);
    }
}
