using System.ComponentModel.DataAnnotations;

namespace Sinance.Web.Model
{
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
}