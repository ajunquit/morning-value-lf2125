using System.ComponentModel.DataAnnotations;

namespace Morning.Value.Web.Site.Auth.Models
{
    public class SignUpViewModel
    {
        [Required, StringLength(80), Display(Name = "Nombre")]
        public string Name { get; set; } = "";

        [Required, EmailAddress, Display(Name = "Correo")]
        public string Email { get; set; } = "";

        [Required, MinLength(6), DataType(DataType.Password), Display(Name = "Contraseña")]
        public string Password { get; set; } = "";

        [Required, MinLength(6), DataType(DataType.Password), Display(Name = "Confirmar contraseña")]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; } = "";
    }
}
