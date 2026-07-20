using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace IkProjesi.Helpers;

public class SwaggerAuthFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        foreach (ApiDescription api in context.ApiDescriptions)
        {
            ControllerActionDescriptor? descriptor = api.ActionDescriptor as ControllerActionDescriptor;
            if (descriptor == null)
            {
                continue;
            }

            bool allowAnonymous = descriptor.MethodInfo
                .GetCustomAttributes(true)
                .OfType<AllowAnonymousAttribute>()
                .Any();

            bool methodHasAuthorize = descriptor.MethodInfo
                .GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .Any();

            bool classHasAuthorize = descriptor.ControllerTypeInfo
                .GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .Any();

            if (allowAnonymous)
            {
                continue;
            }

            if (methodHasAuthorize == false && classHasAuthorize == false)
            {
                continue;
            }

            string path = "/" + api.RelativePath;
            if (document.Paths == null || document.Paths.ContainsKey(path) == false)
            {
                continue;
            }

            var pathItem = document.Paths[path];
            if (pathItem.Operations == null)
            {
                continue;
            }

            foreach (var operation in pathItem.Operations)
            {
                bool sameMethod = string.Equals(operation.Key.ToString(), api.HttpMethod, StringComparison.OrdinalIgnoreCase);
                if (sameMethod)
                {
                    operation.Value.Security = new List<OpenApiSecurityRequirement>
                    {
                        new OpenApiSecurityRequirement
                        {
                            { new OpenApiSecuritySchemeReference("Bearer", document), new List<string>() }
                        }
                    };
                }
            }
        }
    }
}
