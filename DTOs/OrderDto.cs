using System.ComponentModel.DataAnnotations;

namespace EcommerceAdminAPI.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<OrderDetailDto> OrderDetails { get; set; } = new();
    }

    public class CreateOrderDto
    {
        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        [Required]
        public List<CreateOrderDetailDto> OrderDetails { get; set; } = new();
    }

    public class UpdateOrderDto
    {
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty;
    }

    public class OrderDetailDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class CreateOrderDetailDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }
    }
}