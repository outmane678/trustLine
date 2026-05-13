using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AnonymousComplaintsAPI.Configurations
{
    public class AddFileUploadParamOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.MethodInfo.Name == "CreateProfileWithUser") // Replace with your actual method name
            {
                foreach (var param in operation.Parameters)
                {
                    if (param.Name == "avatar")
                    {
                        param.In = ParameterLocation.Header;
                        param.Schema = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "binary"
                        };
                    }
                }
            }
        }
    }
}
