
using ClientForge.Features.User.Models; // Не забудь подключить пространство имен User
namespace ClientForge.Features.Project.Models;
public enum TaskStatus
{
    New, InProgress, UnderReview, NeedsRework, Completed
}

public class TaskModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime DueDate { get; set; }
    
    // ИСПРАВЛЕНИЕ 1: Меняем int на Guid, так как Project.Id это Guid
    public Guid ProjectId { get; set; } 
    // ДОБАВЛЕНО: Навигационное свойство на сам проект
    public Project Project { get; set; } 

    // ИСПРАВЛЕНИЕ 2: Меняем int на Guid, так как User.Id это Guid
    public Guid WorkerId { get; set; } 
    // ДОБАВЛЕНО: Навигационное свойство на работника (User)
    public User.Models.User Worker { get; set; } 

    public TaskStatus Status { get; set; } = TaskStatus.New;
    public string? SubmissionResult { get; set; }
    public string? ReviewComment { get; set; }
}