using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Api.DTOs
{
    /// <summary>Used for user registration — matches the users table in pharmacy.db.</summary>
    public class RegisterRequest
    {
        [Required]
        [StringLength(100)]
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        [JsonPropertyName("role_id")]
        public int RoleId { get; set; }

        [Required]
        [StringLength(100)]
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [JsonPropertyName("last_name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("contact_number")]
        public string? ContactNumber { get; set; }
    }
}