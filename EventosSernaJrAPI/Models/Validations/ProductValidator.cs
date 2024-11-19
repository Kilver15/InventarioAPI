using FluentValidation;

namespace EventosSernaJrAPI.Models.Validations
{
    public class ProductValidator : AbstractValidator<Product>
    {
        public ProductValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("El nombre del producto es requerido");
            RuleFor(x => x.Name).MaximumLength(25).WithMessage("El nombre del producto no puede ser mayor a 25 caracteres");
            RuleFor(x => x.Name).MinimumLength(2).WithMessage("El nombre del producto no puede ser menor a 2 caracteres");
            RuleFor(x => x.Name).Matches("^[a-zA-Z0-9 ]*$").WithMessage("El nombre del producto solo puede contener letras y numeros");
        }
    }
}
