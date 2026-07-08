namespace Prroject.Filters
{
    using Microsoft.AspNetCore.Mvc.Filters;
    using System.Diagnostics;

    public class LogActionFilter : ActionFilterAttribute
    {
        private Stopwatch? _stopwatch;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _stopwatch = Stopwatch.StartNew();
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            _stopwatch?.Stop();
            var user = context.HttpContext.User.Identity?.Name ?? "Anonymous";
            var elapsed = _stopwatch?.ElapsedMilliseconds ?? 0;

            Debug.WriteLine($"User: {user} | Action: {context.ActionDescriptor.DisplayName} | Time: {elapsed}ms");
        }
    }

}
