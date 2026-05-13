using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;

namespace AnonymousComplaintsAPI.DTOs.Requests
{
    /// <summary>
    /// Custom model binder that prevents automatic trimming of string values
    /// ASP.NET Core trims strings by default during model binding which can cause
    /// validation issues when trailing/leading whitespace is intentional or when
    /// validating minimum length requirements.
    /// </summary>
    public class NoTrimStringModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelName = bindingContext.ModelName;
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;

            // Return the value WITHOUT trimming (this is the key difference from default behavior)
            if (string.IsNullOrEmpty(value))
            {
                bindingContext.Result = ModelBindingResult.Success(value);
                return Task.CompletedTask;
            }

            // Set the result without trimming
            bindingContext.Result = ModelBindingResult.Success(value);
            return Task.CompletedTask;
        }
    }
}
