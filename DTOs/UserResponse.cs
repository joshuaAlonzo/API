using System;
using System.Text.Json.Serialization;

namespace Api.DTOs
{
    public class UserResponse
    {
        [JsonPropertyName("userId")]
        public int UserId { get; set; }
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = string.Empty;
        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = string.Empty;
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        [JsonPropertyName("contactNumber")]
        public string? ContactNumber { get; set; }
        [JsonPropertyName("region")]
        public string? Region { get; set; }
        [JsonPropertyName("province")]
        public string? Province { get; set; }
        [JsonPropertyName("cityMunicipality")]
        public string? CityMunicipality { get; set; }
        [JsonPropertyName("barangay")]
        public string? Barangay { get; set; }
        [JsonPropertyName("street")]
        public string? Street { get; set; }
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
        [JsonPropertyName("roleId")]
        public int RoleId { get; set; }
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }

    public class UsermanResponse : UserResponse
    {
        public string UserName { get => Username; set => Username = value; }
        public int UserRoleId { get => RoleId; set => RoleId = value; }
    }
}