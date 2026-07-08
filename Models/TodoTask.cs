namespace Prroject.Models
{
    using System.ComponentModel.DataAnnotations;
    public class TodoTask
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название обязательно")]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public string Priority { get; set; } = "Medium"; // Low, Medium, High

        [Required]
        public string Status { get; set; } = "New"; // New, InProgress, Completed, Requested

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Срок выполнения обязателен")]
        [DataType(DataType.Date)]
        [FutureDate(ErrorMessage = "Срок не может быть раньше сегодняшнего дня")]
        public DateTime DueDate { get; set; }

        public int? UserId { get; set; }
        public User? User { get; set; }
        public int? ProjectId { get; set; }
        public Project? Project { get; set; }
        public string? AttachedFilePath { get; set; }

    }
}
