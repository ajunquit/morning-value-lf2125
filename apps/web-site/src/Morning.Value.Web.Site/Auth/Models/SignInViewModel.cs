using System.ComponentModel.DataAnnotations;

namespace Morning.Value.Web.Site.Auth.Models
{
    public class SignInViewModel
    {
        [Required, EmailAddress, Display(Name = "Correo")]
        public string Email { get; set; } = "";

        [Required, MinLength(6), DataType(DataType.Password), Display(Name = "Contraseña")]
        public string Password { get; set; } = "";
    }
}
