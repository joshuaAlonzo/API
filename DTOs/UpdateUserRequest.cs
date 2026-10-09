using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Api.DTOs
{
    public class UpdateUserRequest
    {
        [StringLength(100)]
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100)]
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

        [StringLength(100)]
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [MinLength(6)]
        [JsonPropertyName("newPassword")]
        public string? NewPassword { get; set; }

        [JsonPropertyName("roleId")]
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

}