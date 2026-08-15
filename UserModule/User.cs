using System;

namespace Api.UserModule
{
    /// <summary>
    /// Represents a user entity mapped directly to the `users` table in pharmacy.db:
    /// columns: user_id, first_name, last_name, email, contact_number, username, password_hash, role_id, created_at
    /// </summary>
    public class User : IEquatable<User>
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ContactNumber { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User() { }

        public User(int userId, string firstName, string lastName, string email,
                    string? contactNumber, string username, string passwordHash,
                    int roleId, DateTime createdAt)
        {
            UserId        = userId;
            FirstName     = firstName;
            LastName      = lastName;
            Email         = email;
            ContactNumber = contactNumber;
            Username      = username;
            PasswordHash  = passwordHash;
            RoleId        = roleId;
            CreatedAt     = createdAt;
        }

        public bool Equals(User? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (UserId > 0 || other.UserId > 0) return UserId == other.UserId;
            return string.Equals(Username, other.Username, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj) => Equals(obj as User);

        public override int GetHashCode()
        {
            if (UserId > 0) return UserId.GetHashCode();
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Username);
        }

        public override string ToString() =>
            $"User[UserId={UserId}, Username={Username}, Email={Email}, RoleId={RoleId}]";
    }
}