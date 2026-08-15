using System;

namespace Api.DTOs
{
    public class UserResponse
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ContactNumber { get; set; }
        public string Username { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UsermanResponse : UserResponse
    {
        public string UserName { get => Username; set => Username = value; }
        public int UserRoleId { get => RoleId; set => RoleId = value; }
    }
}