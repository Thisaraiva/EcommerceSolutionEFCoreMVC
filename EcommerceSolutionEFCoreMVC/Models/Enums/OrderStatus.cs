using System.ComponentModel.DataAnnotations;

namespace EcommerceSolutionEFCoreMVC.Models.Enums
{
    public enum OrderStatus : int
    {
        [Display(Name = "Created")]
        Created = 0,

        [Display(Name = "Approved")]
        Approved = 1,

        [Display(Name = "Processed")]
        Processed = 2,

        [Display(Name = "Invoiced")]
        Invoiced = 3,

        [Display(Name = "Shipped")]
        Shipped = 4,

        [Display(Name = "Delivered")]
        Delivered = 5,

        [Display(Name = "Canceled")]
        Canceled = 6,

        [Display(Name = "Returned")]
        Returned = 7,

        [Display(Name = "Pending Payment")]
        PendingPayment = 8,

        [Display(Name = "Awaiting Fulfillment")]
        AwaitingFulfillment = 9,

        [Display(Name = "Awaiting Pickup")]
        AwaitingPickup = 10,

        [Display(Name = "Awaiting Shipment")]
        AwaitingShipment = 11,

        [Display(Name = "Partially Shipped")]
        PartiallyShipped = 12,
    }
}