using DAL.Models.Enum;

namespace BLL.Service.Payments.Interface
{
    public interface IPaymentGatewayFactory
    {
        IPaymentGatewayProcessor Get(PaymentGateway gateway);
    }
}
