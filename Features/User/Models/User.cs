using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
}