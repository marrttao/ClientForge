using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClientForge.Features.User.Models;

namespace ClientForge.Features.Project.Models;

public class TaskModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = "";

    [Required]
    public string Description { get; set; } = "";

    [Required]
    public DateTime DueDate { get; set; }

    [Required]
    public Guid ProjectId { get; set; }

    [ForeignKey(nameof(ProjectId))]
    public Project Project { get; set; } = null!;

    [Required]
    public Guid WorkerId { get; set; }

    [ForeignKey(nameof(WorkerId))]
    public User.Models.User Worker { get; set; } = null!;

    public TaskStatus Status { get; set; }

    public string? SubmissionResult { get; set; }

    public string? ReviewComment { get; set; }
}

