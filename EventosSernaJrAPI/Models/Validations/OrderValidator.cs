using FluentValidation;

namespace EventosSernaJrAPI.Models.Validations
{
    public class OrderValidator: AbstractValidator<Order>
    {
        public OrderValidator()
        {
            RuleFor(x => x.CustomerName).NotEmpty().WithMessage("El nombre del cliente es requerido");
            RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("El número de teléfono es requerido");
            RuleFor(x => x.Address).NotEmpty().WithMessage("La dirección es requerida");
            RuleFor(x => x.OrderDate).NotEmpty().WithMessage("La fecha de la orden es requerida");
            RuleFor(x => x.DeliveryTime).NotEmpty().WithMessage("La hora de entrega es requerida");
            RuleFor(x => x.PickupDateTime).NotEmpty().WithMessage("La fecha de recogida es requerida");
            RuleFor(x => x.PhoneNumber).Matches(@"^\d{10}$").WithMessage("El número de teléfono debe tener 10 dígitos");
        }
    }
}
