using System.ComponentModel.DataAnnotations;

namespace RawIdentity.Models.ViewModels
{
    public class CreateRoleViewModel
    {
        [Required]
        public string Name { get; set; }
    }
}