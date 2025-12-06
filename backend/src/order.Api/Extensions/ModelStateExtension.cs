
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Order.Api.Extensions;


public static class ModelStateExtension
{
    public static List<string> GetErrors(this ModelStateDictionary modelState)
    {
        List<string> result = new List<string>();

        foreach (var item in modelState.Values)
            result.AddRange(item.Errors.Select(error => error.ErrorMessage));

        return result;
    }
}