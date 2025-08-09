using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceAdminAPI.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }
        
        public int OrderId { get; set; }
        
        public int ProductId { get; set; }
        
        public int Quantity { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        
        public virtual Order Order { get; set; } = null!;
        
        public virtual Product Product { get; set; } = null!;
    }
}