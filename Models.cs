#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InventoryHub
{
    // ==================== ENUMERATIONS ====================
    public enum ProductType
    {
        Electronics,
        Clothing,
        Furniture,
        Perishable,
        Stationery,
        Other
    }

    public enum StockStatus
    {
        InStock,
        LowStock,
        OutOfStock
    }

    public enum OrderStatus
    {
        Pending,
        Processing,
        Shipped,
        Completed,
        Cancelled
    }

    // ==================== INTERFACES ====================
    public interface IIdentifiable
    {
        int Id { get; }
    }

    public interface IStockable
    {
        void AddStock(int quantity);
        bool RemoveStock(int quantity);
        StockStatus GetStockStatus();
    }

    public interface IDisplayable
    {
        string GetDisplayInfo();
    }

    // ==================== DELEGATES AND EVENTS ====================
    public delegate void StockAlertHandler(Product product, string message);
    public delegate void OrderStatusChangedHandler(Order order, OrderStatus oldStatus, OrderStatus newStatus);

    // ==================== ID GENERATOR HELPER ====================
    public static class IdGenerator
    {
        private static int _nextProductId = 1;
        private static int _nextSupplierId = 1;
        private static int _nextOrderId = 1000;
        private static int _nextCustomerId = 1;
        private static readonly object _lock = new object();

        public static int GetNextProductId()
        {
            lock (_lock) { return _nextProductId++; }
        }

        public static int GetNextSupplierId()
        {
            lock (_lock) { return _nextSupplierId++; }
        }

        public static int GetNextOrderId()
        {
            lock (_lock) { return _nextOrderId++; }
        }

        public static int GetNextCustomerId()
        {
            lock (_lock) { return _nextCustomerId++; }
        }

        public static void Reset()
        {
            lock (_lock)
            {
                _nextProductId = 1;
                _nextSupplierId = 1;
                _nextOrderId = 1000;
                _nextCustomerId = 1;
            }
        }
    }

    // ==================== ABSTRACT BASE CLASS ====================
    [BsonDiscriminator(RootClass = true)]
    [BsonKnownTypes(typeof(ElectronicsProduct), typeof(ClothingProduct), typeof(FurnitureProduct),
                    typeof(PerishableProduct), typeof(StationeryProduct), typeof(OtherProduct))]
    public abstract class Product : IIdentifiable, IStockable, IDisplayable
    {
        private int _quantity;
        private decimal _price;

        public static event StockAlertHandler OnStockAlert;

        protected static void RaiseStockAlert(Product product, string message)
        {
            OnStockAlert?.Invoke(product, message);
        }

        [BsonId]
        public int Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("price")]
        public decimal Price
        {
            get => _price;
            set
            {
                if (value < 0) throw new ArgumentException("Price cannot be negative");
                _price = value;
            }
        }

        [BsonElement("quantity")]
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (value < 0) throw new ArgumentException("Quantity cannot be negative");
                _quantity = value;
                CheckStockAlert();
            }
        }

        [BsonElement("threshold")]
        public int Threshold { get; set; }

        [BsonElement("supplierId")]
        public int SupplierId { get; set; }

        [BsonElement("supplierName")]
        public string SupplierName { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("lastModified")]
        public DateTime LastModified { get; set; }

        [BsonElement("category")]
        public abstract ProductType Category { get; }

        protected Product()
        {
            CreatedAt = DateTime.Now;
            LastModified = DateTime.Now;
            SupplierName = "Not Assigned";
        }

        protected Product(string name, decimal price, int quantity, int threshold) : this()
        {
            Id = IdGenerator.GetNextProductId();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Price = price;
            Quantity = quantity;
            Threshold = threshold;
        }

        ~Product() { }

        public virtual void AddStock(int quantity)
        {
            if (quantity <= 0) throw new ArgumentException("Quantity must be positive");
            Quantity += quantity;
            LastModified = DateTime.Now;
        }

        public virtual bool RemoveStock(int quantity)
        {
            if (quantity <= 0) throw new ArgumentException("Quantity must be positive");
            if (Quantity < quantity) return false;
            Quantity -= quantity;
            LastModified = DateTime.Now;
            return true;
        }

        public virtual StockStatus GetStockStatus()
        {
            if (Quantity == 0) return StockStatus.OutOfStock;
            if (Quantity <= Threshold) return StockStatus.LowStock;
            return StockStatus.InStock;
        }

        protected virtual void CheckStockAlert()
        {
            var status = GetStockStatus();
            if (status == StockStatus.LowStock)
                RaiseStockAlert(this, $"Low stock alert: {Name} has only {Quantity} units left!");
            else if (status == StockStatus.OutOfStock)
                RaiseStockAlert(this, $"OUT OF STOCK: {Name} needs immediate restock!");
        }

        public abstract string GetDisplayInfo();

        public override string ToString()
        {
            return $"[{Id}] {Name} - {Category} - PKR {Price:N0} - Stock: {Quantity}";
        }
    }

    // ==================== DERIVED CLASSES ====================
    public class ElectronicsProduct : Product
    {
        [BsonElement("warrantyMonths")]
        public int WarrantyMonths { get; set; }
        public override ProductType Category => ProductType.Electronics;
        public ElectronicsProduct() : base() { }
        public ElectronicsProduct(string name, decimal price, int quantity, int threshold, int warrantyMonths)
            : base(name, price, quantity, threshold) => WarrantyMonths = warrantyMonths;
        public override string GetDisplayInfo() => $"{base.ToString()} | Warranty: {WarrantyMonths} months";
    }

    public class ClothingProduct : Product
    {
        [BsonElement("size")] public string Size { get; set; }
        [BsonElement("material")] public string Material { get; set; }
        public override ProductType Category => ProductType.Clothing;
        public ClothingProduct() : base() { }
        public ClothingProduct(string name, decimal price, int quantity, int threshold, string size, string material)
            : base(name, price, quantity, threshold) { Size = size; Material = material; }
        public override string GetDisplayInfo() => $"{base.ToString()} | Size: {Size} | Material: {Material}";
    }

    public class FurnitureProduct : Product
    {
        [BsonElement("furnitureType")] public string FurnitureType { get; set; }
        [BsonElement("materialType")] public string MaterialType { get; set; }
        [BsonElement("weightKg")] public decimal WeightKg { get; set; }
        [BsonElement("seaterCapacity")] public int? SeaterCapacity { get; set; }
        [BsonElement("bedSize")] public string BedSize { get; set; }
        public override ProductType Category => ProductType.Furniture;
        public FurnitureProduct() : base() { }
        public FurnitureProduct(string name, decimal price, int quantity, int threshold, string materialType, decimal weightKg)
            : base(name, price, quantity, threshold) { MaterialType = materialType; WeightKg = weightKg; }
        public override string GetDisplayInfo() => $"{base.ToString()} | Type: {FurnitureType} | Material: {MaterialType} | Weight: {WeightKg}kg";
        public decimal CalculateShippingCost() => WeightKg * 50;
    }

    public class PerishableProduct : Product
    {
        [BsonElement("expiryDate")]
        public DateTime ExpiryDate { get; set; }

        public override ProductType Category => ProductType.Perishable;

        public PerishableProduct() : base() { }

        public PerishableProduct(string name, decimal price, int quantity, int threshold, DateTime expiryDate)
            : base(name, price, quantity, threshold) => ExpiryDate = expiryDate.Date;

        public override string GetDisplayInfo() => $"{base.ToString()} | Expires: {ExpiryDate:yyyy-MM-dd}";

        public bool IsExpired() => ExpiryDate != DateTime.MinValue && DateTime.Today > ExpiryDate.Date;

        public int DaysUntilExpiry() => ExpiryDate == DateTime.MinValue ? int.MaxValue : (ExpiryDate.Date - DateTime.Today).Days;

        public override StockStatus GetStockStatus()
        {
            if (IsExpired()) return StockStatus.OutOfStock;
            return base.GetStockStatus();
        }

        protected override void CheckStockAlert()
        {
            if (IsExpired())
            {
                RaiseStockAlert(this, $"EXPIRED: {Name} has passed its expiry date ({ExpiryDate:yyyy-MM-dd}) and cannot be sold!");
                return;
            }

            var status = base.GetStockStatus();
            if (status == StockStatus.LowStock)
                RaiseStockAlert(this, $"Low stock alert: {Name} has only {Quantity} units left!");
            else if (status == StockStatus.OutOfStock)
                RaiseStockAlert(this, $"OUT OF STOCK: {Name} needs immediate restock!");
        }
    }

    public class StationeryProduct : Product
    {
        [BsonElement("pageCount")] public int PageCount { get; set; }
        public override ProductType Category => ProductType.Stationery;
        public StationeryProduct() : base() { }
        public StationeryProduct(string name, decimal price, int quantity, int threshold, int pageCount)
            : base(name, price, quantity, threshold) => PageCount = pageCount;
        public override string GetDisplayInfo() => $"{base.ToString()} | Pages: {PageCount}";
    }

    public class OtherProduct : Product
    {
        [BsonElement("description")] public string Description { get; set; }
        public override ProductType Category => ProductType.Other;
        public OtherProduct() : base() { }
        public OtherProduct(string name, decimal price, int quantity, int threshold, string description)
            : base(name, price, quantity, threshold) => Description = description;
        public override string GetDisplayInfo() => $"{base.ToString()} | {Description}";
    }

    // ==================== SUPPLIER CLASS ====================
    public class Supplier : IIdentifiable
    {
        [BsonId] public int Id { get; set; }
        [BsonElement("name")] public string Name { get; set; }
        [BsonElement("contact")] public string Contact { get; set; }
        [BsonElement("email")] public string Email { get; set; }
        [BsonElement("city")] public string City { get; set; }
        [BsonElement("registeredAt")] public DateTime RegisteredAt { get; set; }
        public Supplier() => RegisteredAt = DateTime.Now;
        public Supplier(string name, string contact, string email, string city) : this()
        { Id = IdGenerator.GetNextSupplierId(); Name = name; Contact = contact; Email = email; City = city; }
        public override string ToString() => $"[{Id}] {Name} - {Contact} - {Email} - {City}";
    }

    // ==================== CUSTOMER CLASS ====================
    public class Customer
    {
        [BsonId] public int Id { get; set; }
        [BsonElement("name")] public string Name { get; set; }
        [BsonElement("email")] public string Email { get; set; }
        [BsonElement("password")] public string Password { get; set; }
        [BsonElement("phone")] public string Phone { get; set; }
        [BsonElement("address")] public string Address { get; set; }
        [BsonElement("registeredAt")] public DateTime RegisteredAt { get; set; }
        [BsonElement("lastLogin")] public DateTime? LastLogin { get; set; }
        public Customer() => RegisteredAt = DateTime.Now;
        public Customer(string name, string email, string password, string phone, string address) : this()
        { Id = IdGenerator.GetNextCustomerId(); Name = name; Email = email; Password = password; Phone = phone; Address = address; }
    }

    // ==================== ORDER AND ORDERITEM CLASSES ====================
    public class OrderItem
    {
        [BsonElement("productId")] public int ProductId { get; set; }
        [BsonElement("productName")] public string ProductName { get; set; }
        [BsonElement("unitPrice")] public decimal UnitPrice { get; set; }
        [BsonElement("quantity")] public int Quantity { get; set; }
        [BsonIgnore] public decimal SubTotal => UnitPrice * Quantity;
    }

    public class Order : IIdentifiable
    {
        private OrderStatus _status;
        public event OrderStatusChangedHandler OnStatusChanged;

        [BsonId] public int OrderId { get; set; }
        [BsonElement("customerName")] public string CustomerName { get; set; }
        [BsonElement("customerId")] public int CustomerId { get; set; }
        [BsonElement("customerEmail")] public string CustomerEmail { get; set; }
        [BsonElement("orderDate")] public DateTime OrderDate { get; set; }
        [BsonElement("status")]
        public OrderStatus Status
        {
            get => _status;
            set { var old = _status; _status = value; OnStatusChanged?.Invoke(this, old, value); }
        }
        [BsonIgnore] public decimal Total => Items?.Sum(i => i.SubTotal) ?? 0;
        [BsonElement("items")] public List<OrderItem> Items { get; set; }
        [BsonIgnore] public int Id => OrderId;

        public Order() { OrderDate = DateTime.Now; Status = OrderStatus.Pending; Items = new List<OrderItem>(); }
        public Order(string customerName) : this() { OrderId = IdGenerator.GetNextOrderId(); CustomerName = customerName; }

        public void AddItem(Product product, int quantity)
        {
            var existing = Items.FirstOrDefault(i => i.ProductId == product.Id);
            if (existing != null) existing.Quantity += quantity;
            else Items.Add(new OrderItem { ProductId = product.Id, ProductName = product.Name, UnitPrice = product.Price, Quantity = quantity });
        }

        public void RemoveItem(int productId) => Items.RemoveAll(i => i.ProductId == productId);
        public void Complete() => Status = OrderStatus.Completed;
        public void Cancel() => Status = OrderStatus.Cancelled;
        public override string ToString() => $"Order #{OrderId} - {CustomerName} - {OrderDate:yyyy-MM-dd} - {Status} - Total: PKR {Total:N0}";
    }

    // ==================== SHOPPING CART CLASS ====================
    public class ShoppingCart
    {
        public string CustomerName { get; set; }
        public List<OrderItem> Items { get; private set; } = new List<OrderItem>();
        public decimal Total => Items.Sum(i => i.SubTotal);
        public int ItemCount => Items.Sum(i => i.Quantity);

        public void AddItem(Product product, int quantity)
        {
            var existing = Items.FirstOrDefault(i => i.ProductId == product.Id);
            if (existing != null) existing.Quantity += quantity;
            else Items.Add(new OrderItem { ProductId = product.Id, ProductName = product.Name, UnitPrice = product.Price, Quantity = quantity });
        }

        public void RemoveItem(int productId) => Items.RemoveAll(i => i.ProductId == productId);
        public void Clear() { Items.Clear(); CustomerName = null; }
    }

    // ==================== PERSISTENT CART (MONGODB) ====================
    public class CartItem
    {
        [BsonElement("productId")] public int ProductId { get; set; }
        [BsonElement("productName")] public string ProductName { get; set; }
        [BsonElement("unitPrice")] public decimal UnitPrice { get; set; }
        [BsonElement("quantity")] public int Quantity { get; set; }
        [BsonIgnore] public decimal SubTotal => UnitPrice * Quantity;
    }

    public class CustomerCart
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("customerId")] public int CustomerId { get; set; }
        [BsonElement("items")] public List<CartItem> Items { get; set; } = new List<CartItem>();
        [BsonElement("lastUpdated")] public DateTime LastUpdated { get; set; } = DateTime.Now;

        public static CustomerCart FromShoppingCart(int customerId, ShoppingCart cart)
        {
            return new CustomerCart
            {
                CustomerId = customerId,
                Items = cart.Items.Select(i => new CartItem
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity
                }).ToList(),
                LastUpdated = DateTime.Now
            };
        }

        public ShoppingCart ToShoppingCart(string customerName = null)
        {
            var cart = new ShoppingCart { CustomerName = customerName };
            foreach (var item in Items)
            {
                cart.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity
                });
            }
            return cart;
        }
    }

    // ==================== REPORT MODELS ====================
    public class StockReport
    {
        public int TotalProducts { get; set; }
        public decimal TotalStockValue { get; set; }
        public int TotalItemsInStock { get; set; }
        public Dictionary<ProductType, int> ProductsByCategory { get; set; }
        public List<Product> Products { get; set; }
    }

    public class LowStockReport
    {
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public List<Product> LowStockProducts { get; set; }
        public List<Product> OutOfStockProducts { get; set; }
        public List<Product> UrgentRestockNeeded { get; set; }
    }

    public class SalesReport
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public double AverageOrderValue { get; set; }
        public decimal TodaySales { get; set; }
        public decimal ThisWeekSales { get; set; }
        public decimal ThisMonthSales { get; set; }
        public List<Order> RecentOrders { get; set; }
    }

    public class RevenueSummary
    {
        public decimal DailyRevenue { get; set; }
        public decimal WeeklyRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal YearlyRevenue { get; set; }
        public decimal LifetimeRevenue { get; set; }
        public List<TopProduct> TopProducts { get; set; }
        public Dictionary<ProductType, decimal> RevenueByCategory { get; set; }
    }

    public class TopProduct
    {
        public int Rank { get; set; }
        public string Name { get; set; }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class DatabaseStats
    {
        public string DatabaseName { get; set; }
        public string ConnectionStatus { get; set; }
        public long ProductCount { get; set; }
        public long SupplierCount { get; set; }
        public long OrderCount { get; set; }
        public long CustomerCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    // ==================== AUTHENTICATION CLASSES ====================
    public class AdminAuthenticator
    {
        private const string ADMIN_USERNAME = "admin";
        private const string ADMIN_PASSWORD = "1234";
        public bool Authenticate(string username, string password) => username == ADMIN_USERNAME && password == ADMIN_PASSWORD;
    }

    public class CustomerAuthenticator
    {
        public Customer Authenticate(string email, string password, List<Customer> customers)
            => customers.FirstOrDefault(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && c.Password == password);
        public bool Register(Customer customer, List<Customer> customers)
        {
            if (customers.Any(c => c.Email.Equals(customer.Email, StringComparison.OrdinalIgnoreCase))) return false;
            customers.Add(customer); return true;
        }
    }
}