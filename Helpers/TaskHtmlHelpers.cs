namespace Prroject.Helpers
{
    using Microsoft.AspNetCore.Html;
    using Microsoft.AspNetCore.Mvc.Rendering;

    public static class TaskHtmlHelpers
    {
        public static IHtmlContent PriorityBadge(this IHtmlHelper htmlHelper, string priority)
        {
            string colorClass = priority.ToLower() switch
            {
                "high" => "bg-danger",
                "medium" => "bg-warning text-dark",
                "low" => "bg-success",
                _ => "bg-secondary"
            };

            return new HtmlString($"<span class='badge {colorClass}'>{priority}</span>");
        }

        public static string RowHighlight(this IHtmlHelper htmlHelper, DateTime dueDate, string status)
        {
            if (status != "Completed" && dueDate.Date < DateTime.Today)
            {
                return "table-danger fw-bold"; // Выделение просроченных задач
            }
            return string.Empty;
        }
    }

}
