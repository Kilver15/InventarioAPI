using FluentValidation;

namespace EventosSernaJrAPI.Models.Validations
{
    public class CategoryValidator : AbstractValidator<Category>
    {
        public CategoryValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("El nombre de la categoria es requerido");
            RuleFor(x => x.Name).MaximumLength(25).WithMessage("El nombre de la categoria no puede ser mayor a 25 caracteres");
            RuleFor(x => x.Name).MinimumLength(3).WithMessage("El nombre de la categoria no puede ser menor a 3 caracteres");
            RuleFor(x => x.Name).Matches("^[a-zA-Z0-9 ]*$").WithMessage("El nombre de la categoria solo puede contener letras y numeros");
        }
    }
}
