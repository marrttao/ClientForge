using ClientForge.Features.User.Models;
namespace ClientForge.Features.Project.Models;

public enum ProjectStatus
{
    NotStarted,   
    InProgress,   
    Completed, 
    OnHold        
}

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; } 
    public ProjectStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    
    // 1. Исправление: Сделали Nullable (?)
    public DateTime? FinishedAt { get; set; }
    
    // 2. Исправление: Добавили связь с Клиентом (Пункт 4 из ТЗ)
    public Guid ClientId { get; set; }
    public User.Models.User Client { get; set; } // Навигационное свойство для EF Core

    // 3. Исправление: Коллекция задач вместо одного объекта
    public ICollection<TaskModel> Tasks { get; set; } = new List<TaskModel>();
    
    public override string ToString()
    {
        return $"{Name} — {Status}";
    }
}