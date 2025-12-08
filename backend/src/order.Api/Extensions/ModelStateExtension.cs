
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Order.Api.Extensions;


internal static class ModelStateExtension
{
    internal static List<string> GetErrors(this ModelStateDictionary modelState)
    {
        List<string> result = new List<string>();

        foreach (var item in modelState.Values)
            result.AddRange(item.Errors.Select(error => error.ErrorMessage));

        return result;
    }
}