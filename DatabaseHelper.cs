using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryHub
{
    public static class DatabaseHelper
    {
        private static IMongoClient _client;
        private static IMongoDatabase _database;
        private static IMongoCollection<Product> _productsCollection;
        private static IMongoCollection<Supplier> _suppliersCollection;
        private static IMongoCollection<Order> _ordersCollection;
        private static IMongoCollection<Customer> _customersCollection;
        private static IMongoCollection<CustomerCart> _cartsCollection;

        private static readonly string ConnectionString = "Write Connection String";
        private static readonly string DatabaseName = "Get name from appsettings.json";

        public static bool IsConnected { get; private set; } = false;

        static DatabaseHelper()
        {
            Connect();
        }

        private static void Connect()
        {
            try
            {
                _client = new MongoClient(ConnectionString);
                _database = _client.GetDatabase(DatabaseName);
                _database.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait();

                _productsCollection = _database.GetCollection<Product>("Products");
                _suppliersCollection = _database.GetCollection<Supplier>("Suppliers");
                _ordersCollection = _database.GetCollection<Order>("Orders");
                _customersCollection = _database.GetCollection<Customer>("Customers");
                _cartsCollection = _database.GetCollection<CustomerCart>("Carts");

                IsConnected = true;
            }
            catch
            {
                IsConnected = false;
            }
        }

        public static async Task Initialize()
        {
            if (!IsConnected) return;
            try
            {
                await SyncIdGenerators();

                var productCount = await _productsCollection.CountDocumentsAsync(FilterDefinition<Product>.Empty);
                if (productCount == 0)
                {
                    await SeedSampleData();
                }
            }
            catch { }
        }

        private static async Task SyncIdGenerators()
        {
            try
            {
                var products = await _productsCollection.Find(_ => true).SortByDescending(p => p.Id).Limit(1).FirstOrDefaultAsync();
                var suppliers = await _suppliersCollection.Find(_ => true).SortByDescending(s => s.Id).Limit(1).FirstOrDefaultAsync();
                var orders = await _ordersCollection.Find(_ => true).SortByDescending(o => o.OrderId).Limit(1).FirstOrDefaultAsync();
                var customers = await _customersCollection.Find(_ => true).SortByDescending(c => c.Id).Limit(1).FirstOrDefaultAsync();

                int maxProductId = products?.Id ?? 0;
                int maxSupplierId = suppliers?.Id ?? 0;
                int maxOrderId = orders?.OrderId ?? 999;
                int maxCustomerId = customers?.Id ?? 0;

                typeof(IdGenerator).GetField("_nextProductId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                    ?.SetValue(null, maxProductId + 1);
                typeof(IdGenerator).GetField("_nextSupplierId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                    ?.SetValue(null, maxSupplierId + 1);
                typeof(IdGenerator).GetField("_nextOrderId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                    ?.SetValue(null, maxOrderId + 1);
                typeof(IdGenerator).GetField("_nextCustomerId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                    ?.SetValue(null, maxCustomerId + 1);
            }
            catch { }
        }

        private static async Task SeedSampleData()
        {
            try
            {
                IdGenerator.Reset();

                var s1 = new Supplier("Tech Distributors", "+92-300-1234567", "info@techdist.com", "Karachi");
                var s2 = new Supplier("Global Traders", "+92-321-7654321", "sales@global.com", "Lahore");
                var s3 = new Supplier("Premium Goods", "+92-333-9988776", "contact@premium.com", "Islamabad");
                await _suppliersCollection.InsertManyAsync(new[] { s1, s2, s3 });

                var sampleCustomer = new Customer("John Doe", "john@example.com", "1234", "+92-300-1112233", "123 Main Street, Karachi");
                await _customersCollection.InsertOneAsync(sampleCustomer);

                var products = new List<Product>
                {
                    new ElectronicsProduct("Gaming Laptop", 155000, 10, 3, 24) { SupplierId = s1.Id, SupplierName = s1.Name },
                    new ElectronicsProduct("Wireless Mouse", 2500, 45, 10, 12) { SupplierId = s1.Id, SupplierName = s1.Name },
                    new ElectronicsProduct("Mechanical Keyboard", 8500, 30, 5, 12) { SupplierId = s1.Id, SupplierName = s1.Name },
                    new ClothingProduct("Leather Jacket", 12500, 25, 8, "M", "Leather") { SupplierId = s2.Id, SupplierName = s2.Name },
                    new ClothingProduct("Cotton T-Shirt", 1200, 80, 15, "L", "Cotton"),
                    new FurnitureProduct("Office Chair", 18500, 12, 5, "Metal", 120) { SupplierId = s2.Id, SupplierName = s2.Name, FurnitureType = "Chair" },
                    new FurnitureProduct("Study Table", 12500, 8, 3, "Wood", 25) { SupplierId = s3.Id, SupplierName = s3.Name, FurnitureType = "Table" },
                    new PerishableProduct("Fresh Milk", 180, 50, 15, DateTime.Now.AddDays(7)),
                    new PerishableProduct("Yogurt", 120, 40, 10, DateTime.Now.AddDays(5)),
                    new StationeryProduct("A4 Paper Pack", 450, 100, 20, 500)
                };
                await _productsCollection.InsertManyAsync(products);
            }
            catch { }
        }

        // ==================== PRODUCT OPERATIONS ====================
        public static async Task<int> GetNextProductId()
        {
            try
            {
                var lastProduct = await _productsCollection.Find(_ => true)
                    .SortByDescending(p => p.Id)
                    .Limit(1)
                    .FirstOrDefaultAsync();
                return (lastProduct?.Id ?? 0) + 1;
            }
            catch
            {
                return 1;
            }
        }

        public static async Task AddProduct(Product product)
        {
            try
            {
                product.Id = await GetNextProductId();
                await _productsCollection.InsertOneAsync(product);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add product: {ex.Message}");
            }
        }

        public static async Task UpdateProduct(Product product)
        {
            product.LastModified = DateTime.Now;
            var filter = Builders<Product>.Filter.Eq(p => p.Id, product.Id);
            await _productsCollection.ReplaceOneAsync(filter, product);
        }

        public static async Task<bool> RemoveProduct(int id)
        {
            var result = await _productsCollection.DeleteOneAsync(p => p.Id == id);
            return result.DeletedCount > 0;
        }

        public static async Task<Product> GetProductById(int id)
        {
            return await _productsCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public static async Task<List<Product>> GetAllProducts()
        {
            return await _productsCollection.Find(_ => true).ToListAsync();
        }

        public static async Task<List<Product>> GetAvailableProducts()
        {
            return await _productsCollection.Find(p => p.Quantity > 0).ToListAsync();
        }

        public static async Task<List<Product>> GetLowStockProducts()
        {
            var products = await GetAllProducts();
            return products.Where(p => p.GetStockStatus() == StockStatus.LowStock).ToList();
        }

        public static async Task<List<Product>> GetOutOfStockProducts()
        {
            var products = await GetAllProducts();
            return products.Where(p => p.GetStockStatus() == StockStatus.OutOfStock).ToList();
        }

        public static async Task<bool> ProductExists(string name)
        {
            var filter = Builders<Product>.Filter.Regex(p => p.Name, new BsonRegularExpression($"^{name}$", "i"));
            return await _productsCollection.Find(filter).AnyAsync();
        }

        public static async Task<List<Product>> SearchProducts(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return await GetAllProducts();
            keyword = keyword.ToLower();
            var products = await GetAllProducts();
            return products.Where(p => p.Name.ToLower().Contains(keyword) ||
                                       p.Category.ToString().ToLower().Contains(keyword))
                          .OrderBy(p => p.Name).ToList();
        }

        public static async Task<Product> SearchProductByName(string name)
        {
            var filter = Builders<Product>.Filter.Regex(p => p.Name, new BsonRegularExpression(name, "i"));
            return await _productsCollection.Find(filter).FirstOrDefaultAsync();
        }

        public static async Task<List<string>> GetAllProductNames()
        {
            var products = await GetAllProducts();
            return products.Select(p => p.Name).OrderBy(n => n).ToList();
        }

        public static async Task<bool> AddStock(int productId, int quantity)
        {
            var product = await GetProductById(productId);
            if (product == null) return false;
            product.AddStock(quantity);
            await UpdateProduct(product);
            return true;
        }

        public static async Task<bool> RemoveStock(int productId, int quantity)
        {
            var product = await GetProductById(productId);
            if (product == null || product.Quantity < quantity) return false;
            product.RemoveStock(quantity);
            await UpdateProduct(product);
            return true;
        }

        // ==================== SUPPLIER OPERATIONS ====================
        public static async Task<int> GetNextSupplierId()
        {
            try
            {
                var lastSupplier = await _suppliersCollection.Find(_ => true)
                    .SortByDescending(s => s.Id)
                    .Limit(1)
                    .FirstOrDefaultAsync();
                return (lastSupplier?.Id ?? 0) + 1;
            }
            catch
            {
                return 1;
            }
        }

        public static async Task AddSupplier(Supplier supplier)
        {
            try
            {
                supplier.Id = await GetNextSupplierId();
                await _suppliersCollection.InsertOneAsync(supplier);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add supplier: {ex.Message}");
            }
        }

        public static async Task UpdateSupplier(Supplier supplier)
        {
            await _suppliersCollection.ReplaceOneAsync(s => s.Id == supplier.Id, supplier);
        }

        public static async Task<bool> RemoveSupplier(int id)
        {
            var productsExist = await _productsCollection.Find(p => p.SupplierId == id).AnyAsync();
            if (productsExist) return false;
            var result = await _suppliersCollection.DeleteOneAsync(s => s.Id == id);
            return result.DeletedCount > 0;
        }

        public static async Task<Supplier> GetSupplierById(int id)
        {
            return await _suppliersCollection.Find(s => s.Id == id).FirstOrDefaultAsync();
        }

        public static async Task<List<Supplier>> GetAllSuppliers()
        {
            var suppliers = await _suppliersCollection.Find(_ => true).ToListAsync();
            return suppliers.OrderBy(s => s.Id).ToList();
        }

        // ==================== CUSTOMER OPERATIONS ====================
        public static async Task<int> GetNextCustomerId()
        {
            try
            {
                var lastCustomer = await _customersCollection.Find(_ => true)
                    .SortByDescending(c => c.Id)
                    .Limit(1)
                    .FirstOrDefaultAsync();
                return (lastCustomer?.Id ?? 0) + 1;
            }
            catch
            {
                return 1;
            }
        }

        public static async Task AddCustomer(Customer customer)
        {
            try
            {
                customer.Id = await GetNextCustomerId();
                await _customersCollection.InsertOneAsync(customer);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add customer: {ex.Message}");
            }
        }

        public static async Task UpdateCustomer(Customer customer)
        {
            await _customersCollection.ReplaceOneAsync(c => c.Id == customer.Id, customer);
        }

        public static async Task<Customer> GetCustomerById(int id)
        {
            return await _customersCollection.Find(c => c.Id == id).FirstOrDefaultAsync();
        }

        public static async Task<Customer> GetCustomerByEmail(string email)
        {
            var filter = Builders<Customer>.Filter.Regex(c => c.Email, new BsonRegularExpression($"^{email}$", "i"));
            return await _customersCollection.Find(filter).FirstOrDefaultAsync();
        }

        public static async Task<List<Customer>> GetAllCustomers()
        {
            return await _customersCollection.Find(_ => true).ToListAsync();
        }

        public static async Task<bool> CustomerExists(string email)
        {
            return await GetCustomerByEmail(email) != null;
        }

        public static async Task<Customer> AuthenticateCustomer(string email, string password)
        {
            var filter = Builders<Customer>.Filter.Regex(c => c.Email, new BsonRegularExpression($"^{email}$", "i"));
            var customer = await _customersCollection.Find(filter).FirstOrDefaultAsync();
            if (customer != null && customer.Password == password)
            {
                customer.LastLogin = DateTime.Now;
                await UpdateCustomer(customer);
                return customer;
            }
            return null;
        }

        // ==================== ORDER OPERATIONS ====================
        public static async Task<int> GetNextOrderId()
        {
            try
            {
                var lastOrder = await _ordersCollection.Find(_ => true)
                    .SortByDescending(o => o.OrderId)
                    .Limit(1)
                    .FirstOrDefaultAsync();
                return (lastOrder?.OrderId ?? 999) + 1;
            }
            catch
            {
                return 1000;
            }
        }

        public static async Task AddOrder(Order order)
        {
            try
            {
                order.OrderId = await GetNextOrderId();
                await _ordersCollection.InsertOneAsync(order);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add order: {ex.Message}");
            }
        }

        public static async Task UpdateOrder(Order order)
        {
            await _ordersCollection.ReplaceOneAsync(o => o.OrderId == order.OrderId, order);
        }

        public static async Task<Order> GetOrderById(int id)
        {
            return await _ordersCollection.Find(o => o.OrderId == id).FirstOrDefaultAsync();
        }

        public static async Task<List<Order>> GetAllOrders()
        {
            return await _ordersCollection.Find(_ => true).ToListAsync();
        }

        public static async Task<List<Order>> GetOrdersByCustomer(string customerName)
        {
            var filter = Builders<Order>.Filter.Regex(o => o.CustomerName, new BsonRegularExpression($"^{customerName}$", "i"));
            return await _ordersCollection.Find(filter).ToListAsync();
        }

        public static async Task<List<Order>> GetOrdersByCustomerId(int customerId)
        {
            return await _ordersCollection.Find(o => o.CustomerId == customerId).ToListAsync();
        }

        public static async Task<List<Order>> GetPendingOrders()
        {
            return await _ordersCollection.Find(o => o.Status == OrderStatus.Pending).ToListAsync();
        }

        public static async Task<List<Order>> GetCompletedOrders()
        {
            return await _ordersCollection.Find(o => o.Status == OrderStatus.Completed).ToListAsync();
        }

        public static async Task<bool> CancelOrder(int orderId)
        {
            var order = await GetOrderById(orderId);
            if (order == null || order.Status != OrderStatus.Pending) return false;

            foreach (var item in order.Items)
            {
                var product = await GetProductById(item.ProductId);
                if (product != null)
                {
                    product.AddStock(item.Quantity);
                    await UpdateProduct(product);
                }
            }
            order.Cancel();
            await UpdateOrder(order);
            return true;
        }

        // ==================== CART OPERATIONS ====================
        public static async Task SaveCart(int customerId, ShoppingCart cart)
        {
            try
            {
                var filter = Builders<CustomerCart>.Filter.Eq(c => c.CustomerId, customerId);
                var customerCart = CustomerCart.FromShoppingCart(customerId, cart);
                await _cartsCollection.ReplaceOneAsync(filter, customerCart, new ReplaceOptions { IsUpsert = true });
            }
            catch (Exception ex)
            {
                ConsoleUI.ShowError($"Failed to save cart: {ex.Message}");
            }
        }

        public static async Task<ShoppingCart> LoadCart(int customerId, string customerName = null)
        {
            try
            {
                var filter = Builders<CustomerCart>.Filter.Eq(c => c.CustomerId, customerId);
                var customerCart = await _cartsCollection.Find(filter).FirstOrDefaultAsync();
                if (customerCart != null)
                {
                    return customerCart.ToShoppingCart(customerName);
                }
            }
            catch { }
            return new ShoppingCart { CustomerName = customerName };
        }

        public static async Task ClearCart(int customerId)
        {
            try
            {
                var filter = Builders<CustomerCart>.Filter.Eq(c => c.CustomerId, customerId);
                await _cartsCollection.DeleteOneAsync(filter);
            }
            catch { }
        }

        // ==================== REPORT GENERATION ====================
        public static async Task<StockReport> GetStockReport()
        {
            var products = await GetAllProducts();
            return new StockReport
            {
                TotalProducts = products.Count,
                TotalStockValue = products.Sum(p => p.Price * p.Quantity),
                TotalItemsInStock = products.Sum(p => p.Quantity),
                ProductsByCategory = products.GroupBy(p => p.Category).ToDictionary(g => g.Key, g => g.Count()),
                Products = products
            };
        }

        public static async Task<LowStockReport> GetLowStockReport()
        {
            var lowStockList = await GetLowStockProducts();
            var outOfStockList = await GetOutOfStockProducts();
            return new LowStockReport
            {
                LowStockCount = lowStockList.Count,
                OutOfStockCount = outOfStockList.Count,
                LowStockProducts = lowStockList,
                OutOfStockProducts = outOfStockList,
                UrgentRestockNeeded = lowStockList.Where(p => p.Quantity <= p.Threshold / 2).ToList()
            };
        }

        public static async Task<DatabaseStats> GetDatabaseStats()
        {
            try
            {
                return new DatabaseStats
                {
                    DatabaseName = DatabaseName,
                    ConnectionStatus = IsConnected ? "Connected" : "Disconnected",
                    ProductCount = await _productsCollection.CountDocumentsAsync(FilterDefinition<Product>.Empty),
                    SupplierCount = await _suppliersCollection.CountDocumentsAsync(FilterDefinition<Supplier>.Empty),
                    OrderCount = await _ordersCollection.CountDocumentsAsync(FilterDefinition<Order>.Empty),
                    CustomerCount = await _customersCollection.CountDocumentsAsync(FilterDefinition<Customer>.Empty),
                    TotalRevenue = (await GetAllOrders()).Where(o => o.Status == OrderStatus.Completed).Sum(o => o.Total)
                };
            }
            catch
            {
                return new DatabaseStats
                {
                    DatabaseName = DatabaseName,
                    ConnectionStatus = "Error",
                    ProductCount = 0,
                    SupplierCount = 0,
                    OrderCount = 0,
                    CustomerCount = 0,
                    TotalRevenue = 0
                };
            }
        }

        public static async Task<Dictionary<string, decimal>> GetMonthlyRevenue()
        {
            var orders = await GetAllOrders();
            var completed = orders.Where(o => o.Status == OrderStatus.Completed);
            var monthly = completed
                .GroupBy(o => o.OrderDate.ToString("MMM yyyy"))
                .OrderBy(g => DateTime.ParseExact(g.Key, "MMM yyyy", null))
                .ToDictionary(g => g.Key, g => g.Sum(o => o.Total));
            return monthly;
        }

        public static async Task<Dictionary<string, decimal>> GetDailyRevenue(int days = 7)
        {
            var orders = await GetAllOrders();
            var completed = orders.Where(o => o.Status == OrderStatus.Completed);
            var startDate = DateTime.Now.Date.AddDays(-days + 1);
            var daily = completed
                .Where(o => o.OrderDate.Date >= startDate)
                .GroupBy(o => o.OrderDate.Date)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key.ToString("dd MMM"), g => g.Sum(o => o.Total));
            return daily;
        }

        public static async Task<Dictionary<string, decimal>> GetWeeklyRevenue(int weeks = 4)
        {
            var orders = await GetAllOrders();
            var completed = orders.Where(o => o.Status == OrderStatus.Completed);
            var startDate = DateTime.Now.Date.AddDays(-weeks * 7 + 1);
            var weekly = completed
                .Where(o => o.OrderDate.Date >= startDate)
                .GroupBy(o => new { Week = System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(o.OrderDate, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday), Year = o.OrderDate.Year })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Week)
                .ToDictionary(g => $"{g.Key.Year}-W{g.Key.Week}", g => g.Sum(o => o.Total));
            return weekly;
        }

        public static async Task<Dictionary<string, decimal>> GetTopProductsRevenue(int top = 10)
        {
            var orders = await GetAllOrders();
            var completed = orders.Where(o => o.Status == OrderStatus.Completed);
            var revenueByProduct = completed
                .SelectMany(o => o.Items)
                .GroupBy(i => i.ProductName)
                .Select(g => new { ProductName = g.Key, Revenue = g.Sum(i => i.SubTotal) })
                .OrderByDescending(x => x.Revenue)
                .Take(top)
                .ToDictionary(x => x.ProductName, x => x.Revenue);
            return revenueByProduct;
        }
    }
}