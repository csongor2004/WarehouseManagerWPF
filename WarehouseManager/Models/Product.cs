namespace WarehouseManager.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int StockLevel { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }
    }
}