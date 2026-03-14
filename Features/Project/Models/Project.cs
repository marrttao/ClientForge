using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClientForge.Features.User.Models;

namespace ClientForge.Features.Project.Models;

public class Project
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = "";

    [Required]
    public string Description { get; set; } = "";

    public ProjectStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? FinishedAt { get; set; }

    [Required]
    public Guid ClientId { get; set; }

    [ForeignKey(nameof(ClientId))]
    public User.Models.User Client { get; set; } = null!;

    public ICollection<TaskModel> Tasks { get; set; } = new List<TaskModel>();
}

