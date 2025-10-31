using BLL.Service.Payments.Interface;
using DAL.Models.Enum;

namespace BLL.Service.Payments
{
    public class PaymentGatewayFactory : IPaymentGatewayFactory
    {
        private readonly IEnumerable<IPaymentGatewayProcessor> _processors;

        public PaymentGatewayFactory(IEnumerable<IPaymentGatewayProcessor> processors)
        {
            _processors = processors;
        }

        public IPaymentGatewayProcessor Get(PaymentGateway gateway)
        {
            var processor = _processors.FirstOrDefault(p => p.Gateway == gateway);
            if (processor == null)
                throw new ArgumentOutOfRangeException(
                    nameof(gateway),
                    "Unsupported payment gateway"
                );
            return processor;
        }
    }
}
