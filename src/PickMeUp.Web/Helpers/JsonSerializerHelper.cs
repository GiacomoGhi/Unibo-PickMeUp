using Newtonsoft.Json;

namespace PickMeUp.Web.Infrastructure;

public static class JsonSerializerHelper
{
    public static string ToJsonCamelCase(object obj)
    {
        return JsonConvert.SerializeObject(obj, new JsonSerializerSettings { ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() });
    }
}
