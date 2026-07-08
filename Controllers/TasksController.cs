using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prroject.Data;
using Prroject.Filters;
using Prroject.Models;

[Authorize]
[ServiceFilter(typeof(LogActionFilter))] 
public class TasksController : Controller
{
    private readonly AppDbContext _context;

    public class TaskFilterViewModel
    {
        public List<TodoTask> Tasks { get; set; } = new();
        public SelectList? UsersList { get; set; }
        public int? SelectedUserId { get; set; }
        public string? SelectedStatus { get; set; }
    }

    public TasksController(AppDbContext context) => _context = context;

    public async Task<IActionResult> Index(int? userId, string status)
    {
        var username = User.Identity?.Name;
        var userRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

        var query = _context.Tasks.Include(t => t.User).Include(t => t.Project).AsQueryable();

        if (userRole == "Employee")
        {
            // Сотрудник видит только свои задачи или новые задачи без исполнителя
            query = query.Where(t => t.User!.Username == username || (t.Status == "New" && t.UserId == null));
        }
        else if (userRole == "Admin")
        {
            // Фильтрация для Администратора
            if (userId.HasValue) query = query.Where(t => t.UserId == userId);
            if (!string.IsNullOrEmpty(status)) query = query.Where(t => t.Status == status);
        }

        var model = new TaskFilterViewModel
        {
            Tasks = await query.ToListAsync(),
            UsersList = new SelectList(await _context.Users.Where(u => u.Role == "Employee").ToListAsync(), "Id", "Username"),
            SelectedUserId = userId,
            SelectedStatus = status
        };
        // Считаем процент выполненных задач для прогресс-бара
        int totalTasksCount = model.Tasks.Count;
        int completedTasksCount = model.Tasks.Count(t => t.Status == "Completed");

        // Защита от деления на ноль, если задач еще нет
        int completionPercentage = totalTasksCount > 0
            ? (int)Math.Round((double)completedTasksCount / totalTasksCount * 100)
            : 0;

        ViewBag.CompletionPercentage = completionPercentage;

        return View(model);
    }


    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        // Загружаем сотрудников и проекты для выпадающих списков
        ViewBag.Users = new SelectList(await _context.Users.Where(u => u.Role == "Employee").ToListAsync(), "Id", "Username");
        ViewBag.Projects = new SelectList(await _context.Projects.ToListAsync(), "Id", "Name");
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TodoTask task)
    {
        // Валидация существования пользователя
        if (task.UserId.HasValue && !_context.Users.Any(u => u.Id == task.UserId))
        {
            ModelState.AddModelError("UserId", "Указанный сотрудник не существует.");
        }

        // Валидация существования проекта
        if (task.ProjectId.HasValue && !_context.Projects.Any(p => p.Id == task.ProjectId))
        {
            ModelState.AddModelError("ProjectId", "Указанный проект не существует.");
        }

        if (ModelState.IsValid)
        {
            if (task.UserId.HasValue)
            {
                task.Status = "InProgress";
            }

            _context.Add(task);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Users = new SelectList(await _context.Users.Where(u => u.Role == "Employee").ToListAsync(), "Id", "Username");
        ViewBag.Projects = new SelectList(await _context.Projects.ToListAsync(), "Id", "Name");
        return View(task);
    }


    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, string newStatus, IFormFile? uploadedFile)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        if (task.Status == "Completed" && newStatus == "InProgress")
        {
            TempData["Error"] = "Нельзя перевести завершенную задачу обратно в работу.";
            return RedirectToAction(nameof(Index));
        }
        if (newStatus == "Completed" && task.DueDate.Date < DateTime.Today)
        {
            TempData["Error"] = $"Вы не можете сдать задачу '{task.Title}', так как срок выполнения упущен ({task.DueDate.ToShortDateString()})!";
            return RedirectToAction(nameof(Index));
        }
        // Логика загрузки файла, если сотрудник сдает задачу и прикрепил файл
        if (newStatus == "Completed" && uploadedFile != null && uploadedFile.Length > 0)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(uploadedFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await uploadedFile.CopyToAsync(fileStream);
            }

            task.AttachedFilePath = uniqueFileName;
        }

        task.Status = newStatus;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // Метод для скачивания файла (Доступен только админу)
    [Authorize(Roles = "Admin")]
    public IActionResult DownloadFile(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return NotFound();

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("Файл не найден на сервере.");
        }

        // Возвращаем файл пользователю. Оригинальное имя вырезаем из нашего уникального GUID_Имя
        var originalFileName = fileName.Substring(fileName.IndexOf('_') + 1);
        var fileBytes = System.IO.File.ReadAllBytes(filePath);

        return File(fileBytes, "application/octet-stream", originalFileName);
    }

    [HttpPost]
    public async Task<IActionResult> ClaimTask(int id)
    {
        var task = await _context.Tasks.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id);
        var username = User.Identity?.Name;
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (task != null && user != null && task.UserId == null)
        {
            task.Status = "Requested"; // Заявка на выполнение
            task.UserId = user.Id;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
