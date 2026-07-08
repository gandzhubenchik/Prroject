using Prroject.Models;

namespace Prroject.Data
{
    public static class DataSeeder
    {
        public static void Seed(AppDbContext context)
        {
            context.Database.EnsureCreated();

            // 1. Добавляем проекты, если их еще нет
            if (!context.Projects.Any())
            {
                context.Projects.AddRange(
                    new Project { Name = "Ребрендинг сайта", Description = "Обновление дизайна и переход на .NET 8" },
                    new Project { Name = "Мобильное приложение", Description = "Разработка личного кабинета клиента для iOS/Android" },
                    new Project { Name = "Внутренняя ERP-система", Description = "Автоматизация отчетности и кадрового учета" }
                );
                context.SaveChanges();
            }

            // 2. Добавляем расширенный список пользователей (Админы, Тимлиды, Разработчики)
            if (!context.Users.Any())
            {
                context.Users.AddRange(
                    // Администрация и менеджмент
                    new User { Username = "boss_semen", PasswordHash = "admin123", Role = "Admin" },
                    new User { Username = "pm_valery", PasswordHash = "admin123", Role = "Admin" },

                    // Команда разработки (Исполнители)
                    new User { Username = "dev_alex", PasswordHash = "emp123", Role = "Employee" },
                    new User { Username = "dev_marina", PasswordHash = "emp123", Role = "Employee" },
                    new User { Username = "qa_dmitry", PasswordHash = "emp123", Role = "Employee" },
                    new User { Username = "qa_elena", PasswordHash = "emp123", Role = "Employee" },
                    new User { Username = "design_igor", PasswordHash = "emp123", Role = "Employee" },
                    new User { Username = "dev_artyom", PasswordHash = "emp123", Role = "Employee" }
                );
                context.SaveChanges();
            }
        }
    }

}
