using System.ComponentModel.DataAnnotations;

namespace Sinance.Web.Model
{
    /// <summary>
    /// Login confirmation model for external logins
    /// </summary>
    public class ExternalLoginConfirmationViewModel
    {
        /// <summary>
        /// Username associated with the login
        /// </summary>
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }
    }

    /// <summary>
    /// Model for loggin in
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Password associated with the user
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// Remember the user after turning off the browser
        /// </summary>
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        /// <summary>
        /// Username to login with
        /// </summary>
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }
    }

    /// <summary>
    /// User manage model
    /// </summary>
    public class ManageUserViewModel
    {
        /// <summary>
        /// Confirmation for the new password
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// New password for the user
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        /// <summary>
        /// Old password of the user
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }
    }

    /// <summary>
    /// Model for registering as a user
    /// </summary>
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