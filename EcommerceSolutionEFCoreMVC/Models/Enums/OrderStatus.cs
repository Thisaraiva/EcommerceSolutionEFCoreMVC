using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum OrderStatus : int
    {
        Created = 0,
        Approved = 1,
        Processed = 2,
        Invoiced = 3,
        Shipped = 4,
        Delivered = 5,
        Canceled = 6,
        Returned = 7,
        PendingPayment = 8,      // Pagamento pendente
        AwaitingFulfillment = 9, // Aguardando processamento
        AwaitingPickup = 10,     // Aguardando retirada
        AwaitingShipment = 11,   // Aguardando envio
        PartiallyShipped = 12,   // Parcialmente enviado
    }
}
