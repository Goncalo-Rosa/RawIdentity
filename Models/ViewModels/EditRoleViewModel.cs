using System.ComponentModel.DataAnnotations;

namespace RawIdentity.Models.ViewModels
{
    public class EditRoleViewModel
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}