using System.ComponentModel.DataAnnotations.Schema;

namespace RetailappPOE.Models.SQLModels
{
    public class CartItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        [ForeignKey(nameof(OrderId))]
        public OrderSQL? Order { get; set; }

        public int ProductId { get; set; }
        [ForeignKey(nameof(ProductId))]
        public ProductSQL? Product { get; set; }

        public int Quantity { get; set; }
    }
}
