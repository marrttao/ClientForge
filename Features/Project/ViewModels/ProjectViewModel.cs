using System.ComponentModel.DataAnnotations;
using ClientForge.Features.Project.Models;
using ClientForge.Features.User.Models;
namespace ClientForge.Features.Project.ViewModels;

public class ProjectViewModel
{
        [Required]
        [StringLength(50)]
        public string Surname { get; set; }
        [StringLength(500)]
        public string description { get; set; }
        [Required]
        public ProjectStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? FinishedAt { get; set; }
        [Required]
        public string ClientName { get; set; }
        [Required]
        public string ClientSurname { get; set; }
}