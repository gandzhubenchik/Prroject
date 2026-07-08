namespace Prroject.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // Для простоты храним хэш
        public string Role { get; set; } = "Employee"; // Admin или Employee

        public List<TodoTask> Tasks { get; set; } = new();
    }
}
