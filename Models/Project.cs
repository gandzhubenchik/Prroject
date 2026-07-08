using Prroject.Models;
using System.ComponentModel.DataAnnotations;

public class Project
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Название проекта обязательно")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    // Связь один-ко-многим: в проекте может быть много задач
    public List<TodoTask> Tasks { get; set; } = new();
}
