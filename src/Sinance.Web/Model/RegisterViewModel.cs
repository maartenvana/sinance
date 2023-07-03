using System.ComponentModel.DataAnnotations;

namespace Sinance.Web.Model
{
    public class RegisterViewModel
    {
        /// <summary>
        /// Confirmation of the password
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// Password to use to register the user
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// Username to register with
        /// </summary>
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }
    }
}