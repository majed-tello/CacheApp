using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CacheApp.Cache;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CacheAttribute : Attribute, IAsyncActionFilter
{
    private readonly int _expirationInMinutes;
    private readonly CacheTechnologies _cacheTechnology;

    public CacheAttribute(int expirationInMinutes, CacheTechnologies cacheTechnology = CacheTechnologies.InMemory)
    {
        _expirationInMinutes = expirationInMinutes;
        _cacheTechnology = cacheTechnology;
    }
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var cacheServiceResolcer = context.HttpContext.RequestServices.GetService<CacheServiceResolver>()!;
        ICacheService cacheService = cacheServiceResolcer(_cacheTechnology);

        var cacheKey = context.HttpContext.Request.Path.Value!;

        var cachedValue = await cacheService.GetStringAsync(cacheKey);

        if (!string.IsNullOrWhiteSpace(cachedValue))
        {
            context.Result = new ContentResult
            {
                StatusCode = 200,
                ContentType = "application/json",
                Content = cachedValue
            };

            return;
        }
        var response = await next();

        if (response.Result is OkObjectResult okObjectResult)
            await cacheService.SetAsync(cacheKey, okObjectResult.Value, TimeSpan.FromMinutes(_expirationInMinutes));
    }
}