using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClientForge.Features.Project.Models;
namespace ClientForge.Features.User.Models;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    [MinLength(1)]
    [DataType(DataType.Text)]
    public string Login { get; set; }
    [Required]
    [MaxLength(50)]
    [MinLength(1)]
    [DataType(DataType.Text)]
    public string Name { get; set; }
    [Required]
    [MaxLength(50)]
    [MinLength(1)]
    [DataType(DataType.Text)]
    public string Surname { get; set; }
    [Required]
    [MaxLength(50)]
    [MinLength(1)]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }
    
    public Role Role { get; set; } = Role.guest;
    
    [Required]
    [MaxLength(100)]
    [MinLength(1)]
    public string HashedPassword { get; private set; }
    
    
    // ДОБАВЛЕНО: Связь "Один ко многим". Проекты, где этот User является Клиентом
    public ICollection<Project.Models.Project> Projects { get; set; } = new List<Project.Models.Project>();

    // ДОБАВЛЕНО: Связь "Один ко многим". Задачи, назначенные на этого User (как на работника)
    public ICollection<TaskModel> Tasks { get; set; } = new List<TaskModel>();
    
}