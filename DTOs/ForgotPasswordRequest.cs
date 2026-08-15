using System.ComponentModel.DataAnnotations;

namespace Api.DTOs
{
    public class ForgotPasswordRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;
    }
}