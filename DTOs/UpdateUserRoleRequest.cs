using System.ComponentModel.DataAnnotations;

namespace Api.DTOs
{
    public class UpdateUserRoleRequest
    {
        [Required]
        [StringLength(100)]
        public string RoleName { get; set; } = string.Empty;
    }

    public class UpdateUserroleRequest
    {
        [Required]
        [StringLength(100)]
        public string UserRole { get; set; } = string.Empty;
    }

    public class CreateUserroleRequest : UpdateUserroleRequest { }
    public class CreateUserRoleRequest : UpdateUserRoleRequest { }
}