using System.ComponentModel.DataAnnotations;

namespace Api.DTOs
{
    public class UpdateUserRequest
    {
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? ContactNumber { get; set; }

        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [MinLength(6)]
        public string? NewPassword { get; set; }

        public int RoleId { get; set; }
    }

    public class UpdateUsermanRequest
    {
        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;

        [MinLength(6)]
        public string? NewPassword { get; set; }

        public int UserRoleId { get; set; }
    }

    public class CreateUsermanRequest : UpdateUsermanRequest { }
    public class CreateUserRequest : UpdateUserRequest { }
}