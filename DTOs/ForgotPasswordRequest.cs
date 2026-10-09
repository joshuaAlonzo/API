using System.ComponentModel.DataAnnotations;

namespace Api.DTOs
{
    public class ForgotPasswordRequest
    {
        [Required]
        public string EmailOrPhone { get; set; } = string.Empty;
    }
}