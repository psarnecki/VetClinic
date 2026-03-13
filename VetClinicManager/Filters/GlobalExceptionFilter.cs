using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VetClinicManager.Filters;

public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger, IWebHostEnvironment env)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _env = env ?? throw new ArgumentNullException(nameof(env));
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(
            context.Exception,
            "Unhandled exception on {Method} {Path}",
            context.HttpContext.Request.Method,
            context.HttpContext.Request.Path
        );

        if (_env.IsDevelopment())
            return;

        context.Result = new RedirectToActionResult("Error", "Home", null);
        context.ExceptionHandled = true;
    }
}