using FluentValidation;

namespace EventosSernaJrAPI.Models.Validations
{
    public class UserValidator : AbstractValidator<User>
    {
        public UserValidator()
        {
            RuleFor(x => x.Username).NotEmpty().WithMessage("El nombre es requerido");
            RuleFor(x => x.Password).NotEmpty().WithMessage("La contraseña es requerida");
            RuleFor(x => x.Password).MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres");
            RuleFor(x => x.Password).MaximumLength(15).WithMessage("La contraseña debe tener máximo 15 caracteres");
            RuleFor(x => x.Password).Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,15}$").WithMessage("La contraseña debe tener al menos una mayúscula, una minúscula y un número");
        }
    }
}
