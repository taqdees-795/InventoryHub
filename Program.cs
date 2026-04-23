#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryHub
{
    class Program
    {
        private static Customer _currentCustomer = null;
        private static ShoppingCart _currentCart = null;

        static async Task Main()
        {
            Console.Title = "Inventory Hub - Professional Inventory System";
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            try
            {
                try
                {
                    if (Console.LargestWindowWidth >= 100)
                    {
                        Console.WindowWidth = 100;
                        Console.BufferWidth = 100;
                    }
                }
                catch { }

                Product.OnStockAlert += HandleStockAlert;
                ConsoleUI.ShowSplash();

                if (!DatabaseHelper.IsConnected)
                {
                    ConsoleUI.ShowError("MongoDB Connection Failed!");
                    ConsoleUI.ShowInfo("Please ensure MongoDB is running on localhost:27017");
                    ConsoleUI.Pause();
                    return;
                }

                ConsoleUI.ShowSuccess("Connected to MongoDB Database Successfully!");
                await Task.Delay(1000);
                await DatabaseHelper.Initialize();
                await RunMainMenu();
            }
            catch (Exception ex)
            {
                ConsoleUI.ShowError($"Fatal Error: {ex.Message}");
            }
            finally
            {
                ConsoleUI.ShowGoodbye();
            }
        }

        private static void HandleStockAlert(Product product, string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"\n  [STOCK ALERT] {message}");
            Console.ResetColor();
        }

        static async Task RunMainMenu()
        {
            var options = new[] { "🔐 Admin Panel", "👤 Customer Panel", "🚪 Exit" };

            while (true)
            {
                int choice = ConsoleUI.ShowMenu("MAIN MENU", options);
                switch (choice)
                {
                    case 0: await AdminLogin(); break;
                    case 1: await RunCustomerPanel(); break;
                    default: return;
                }
            }
        }

        static async Task AdminLogin()
        {
            Console.Clear();
            ConsoleUI.DrawHeader("🔐 ADMIN LOGIN", ConsoleUI.C_PRIMARY);
            Console.WriteLine();

            var authenticator = new AdminAuthenticator();
            string username = ConsoleUI.GetInput("Username");
            string password = ConsoleUI.GetInput("Password");

            if (authenticator.Authenticate(username, password))
            {
                ConsoleUI.ShowSuccess("Login successful! Welcome, Administrator.");
                await Task.Delay(1000);
                await RunAdminPanel();
            }
            else
            {
                ConsoleUI.ShowError("Invalid username or password!");
                ConsoleUI.Pause();
            }
        }

        static async Task RunAdminPanel()
        {
            var options = new[] { "📦 Manage Inventory", "🏭 Manage Suppliers", "🛒 Manage Orders", "📊 Reports", "🗄️ Database Stats", "🚪 Logout" };

            while (true)
            {
                int choice = ConsoleUI.ShowMenu("🔐 ADMIN PANEL", options);
                switch (choice)
                {
                    case 0: await RunInventoryManagement(); break;
                    case 1: await RunSupplierManagement(); break;
                    case 2: await RunOrderManagement(); break;
                    case 3: await RunReports(); break;
                    case 4: await ShowDatabaseStats(); break;
                    default: return;
                }
            }
        }

        static async Task ShowDatabaseStats()
        {
            try
            {
                var stats = await DatabaseHelper.GetDatabaseStats();
                ConsoleUI.ShowDatabaseStats(
                    dbName: stats.DatabaseName,
                    isConnected: stats.ConnectionStatus == "Connected",
                    productCount: (int)stats.ProductCount,
                    supplierCount: (int)stats.SupplierCount,
                    customerCount: (int)stats.CustomerCount,
                    orderCount: (int)stats.OrderCount,
                    totalRevenue: (decimal)stats.TotalRevenue
                );
            }
            catch (Exception ex)
            {
                ConsoleUI.ShowError($"Error: {ex.Message}");
                ConsoleUI.Pause();
            }
        }

        static async Task RunInventoryManagement()
        {
            var options = new[] { "➕ Add Product", "✏️ Update Product", "🗑️ Delete Product", "📋 View All Products",
                                  "🔍 Search Product", "🔙 Back" };

            while (true)
            {
                int choice = ConsoleUI.ShowMenu("📦 MANAGE INVENTORY", options);
                switch (choice)
                {
                    case 0: await AddProduct(); break;
                    case 1: await UpdateProduct(); break;
                    case 2: await DeleteProduct(); break;
                    case 3: await ShowAllProducts(); break;
                    case 4: await SearchProductMenu(); break;
                    default: return;
                }
            }
        }

        static async Task SearchProductMenu()
        {
            try
            {
                var productNames = await DatabaseHelper.GetAllProductNames();
                if (productNames.Count == 0)
                {
                    ConsoleUI.ShowInfo("No products available.");
                    ConsoleUI.Pause();
                    return;
                }

                Console.Clear();
                ConsoleUI.DrawHeader("🔍 SEARCH PRODUCT");
                Console.WriteLine();
                ConsoleUI.ShowInfo("Available Products:");
                Console.WriteLine();

                int count = 0;
                foreach (var name in productNames)
                {
                    Console.ForegroundColor = ConsoleUI.C_HEADER;
                    Console.Write($"     • {name,-30}");
                    Console.ResetColor();
                    count++;
                    if (count % 2 == 0) Console.WriteLine();
                }
                if (count % 2 != 0) Console.WriteLine();

                Console.WriteLine();
                string searchName = ConsoleUI.GetInput("Enter product name to search");
                var product = await DatabaseHelper.SearchProductByName(searchName);

                if (product == null)
                    ConsoleUI.ShowError($"Product '{searchName}' is not available!");
                else
                    ConsoleUI.ShowProducts(new List<Product> { product });
            }
            catch (Exception ex)
            {
                ConsoleUI.ShowError($"Error: {ex.Message}");
                ConsoleUI.Pause();
            }
        }

        static async Task CustomerSearchProduct()
        {
            await SearchProductMenu();
        }

        static async Task ShowAllProducts()
        {
            var products = await DatabaseHelper.GetAllProducts();
            ConsoleUI.ShowProducts(products);
        }

        static async Task AddProduct()
        {
            try
            {
                Console.Clear();
                ConsoleUI.DrawHeader("➕ ADD NEW PRODUCT");
                Console.WriteLine();

                var type = ConsoleUI.SelectProductType();
                string name = ConsoleUI.GetInput("Product Name");

                if (await DatabaseHelper.ProductExists(name))
                {
                    ConsoleUI.ShowError($"Product '{name}' already exists!");
                    ConsoleUI.Pause();
                    return;
                }

                decimal price = ConsoleUI.GetDecimal("Price (PKR)");
                int qty = ConsoleUI.GetInt("Initial Quantity", 0);
                int threshold = ConsoleUI.GetInt("Low Stock Threshold", 1);

                Product product = CreateProductByType(type, name, price, qty, threshold);
                if (product == null) return;

                await AssignSupplier(product);

                await DatabaseHelper.AddProduct(product);

                Console.Clear();
                ConsoleUI.DrawHeader("✅ PRODUCT ADDED SUCCESSFULLY", ConsoleUI.C_SUCCESS);
                Console.WriteLine();
                Console.ForegroundColor = ConsoleUI.C_HEADER;
                Console.WriteLine($"  Product ID   : {product.Id}");
                Console.WriteLine($"  Name         : {product.Name}");
                Console.WriteLine($"  Category     : {product.Category}");
                Console.WriteLine($"  Price        : PKR {product.Price:N0}");
                Console.WriteLine($"  Quantity     : {product.Quantity}");
                Console.WriteLine($"  Threshold    : {product.Threshold}");
                Console.WriteLine($"  Supplier     : {product.SupplierName}");
                Console.ResetColor();
                ConsoleUI.Pause();
            }
            catch (Exception ex)
            {
                ConsoleUI.ShowError($"Error adding product: {ex.Message}");
                ConsoleUI.Pause();
            }
        }

        static Product CreateProductByType(ProductType type, string name, decimal price, int qty, int threshold)
        {
            switch (type)
            {
                case ProductType.Electronics:
                    int warranty = ConsoleUI.GetInt("Warranty (months)", 0);
                    return new ElectronicsProduct(name, price, qty, threshold, warranty);
                case ProductType.Clothing:
                    string size = ConsoleUI.GetInput("Size (S/M/L/XL)");
                    string material = ConsoleUI.GetInput("Material");
                    return new ClothingProduct(name, price, qty, threshold, size, material);
                case ProductType.Furniture:
                    string furnitureType = ConsoleUI.GetInput("Furniture Type (Sofa/Bed/Chair/Table)");
                    string matType = ConsoleUI.GetInput("Material Type");
                    decimal weight = ConsoleUI.GetDecimal("Weight (kg)");
                    var furniture = new FurnitureProduct(name, price, qty, threshold, matType, weight);
                    furniture.FurnitureType = furnitureType;
                    return furniture;
                case ProductType.Perishable:
                    DateTime expiry = ConsoleUI.GetDate("Expiry Date (yyyy-MM-dd)");
                    return new PerishableProduct(name, price, qty, threshold, expiry);
                case ProductType.Stationery:
                    int pages = ConsoleUI.GetInt("Number of Pages", 0);
                    return new StationeryProduct(name, price, qty, threshold, pages);
                default:
                    string desc = ConsoleUI.GetInput("Description");
                    return new OtherProduct(name, price, qty, threshold, desc);
            }
        }

        static async Task AssignSupplier(Product product)
        {
            try
            {
                var suppliers = await DatabaseHelper.GetAllSuppliers();
                if (suppliers == null || suppliers.Count == 0)
                {
                    product.SupplierId = 0;
                    product.SupplierName = "Not Assigned";
                    return;
                }

                var supplierNames = suppliers.Select(s => $"{s.Id}: {s.Name}").ToArray();
                var options = new[] { "-- Skip (No Supplier) --" }.Concat(supplierNames).ToArray();
                int choice = ConsoleUI.ShowMenu("ASSIGN SUPPLIER", options);

                if (choice > 0 && choice <= suppliers.Count)
                {
                    var supplier = suppliers[choice - 1];
                    product.SupplierId = supplier.Id;
                    product.SupplierName = supplier.Name;
                }
                else
                {
                    product.SupplierId = 0;
                    product.SupplierName = "Not Assigned";
                }
            }
            catch
            {
                product.SupplierId = 0;
                product.SupplierName = "Not Assigned";
            }
        }

        static async Task UpdateProduct()
        {
            try
            {
                var products = await DatabaseHelper.GetAllProducts();
                if (products.Count == 0) { ConsoleUI.ShowInfo("No products available."); return; }

                ConsoleUI.ShowProducts(products);
                int id = ConsoleUI.GetInt("Enter Product ID to update");
                var product = await DatabaseHelper.GetProductById(id);
                if (product == null) { ConsoleUI.ShowError("Product not found!"); return; }

                // Base fields
                string newName = ConsoleUI.GetInput($"Name [{product.Name}]", true);
                string newPriceStr = ConsoleUI.GetInput($"Price [{product.Price:N0}]", true);
                string newQtyStr = ConsoleUI.GetInput($"Quantity [{product.Quantity}]", true);
                string newThresholdStr = ConsoleUI.GetInput($"Threshold [{product.Threshold}]", true);

                if (!string.IsNullOrEmpty(newName)) product.Name = newName;
                if (!string.IsNullOrEmpty(newPriceStr) && decimal.TryParse(newPriceStr, out decimal p) && p >= 0) product.Price = p;
                if (!string.IsNullOrEmpty(newQtyStr) && int.TryParse(newQtyStr, out int q) && q >= 0) product.Quantity = q;
                if (!string.IsNullOrEmpty(newThresholdStr) && int.TryParse(newThresholdStr, out int t) && t >= 0) product.Threshold = t;

                // Type-specific fields
                if (product is ElectronicsProduct elec)
                {
                    string newWarranty = ConsoleUI.GetInput($"Warranty (months) [{elec.WarrantyMonths}]", true);
                    if (!string.IsNullOrEmpty(newWarranty) && int.TryParse(newWarranty, out int w) && w >= 0)
                        elec.WarrantyMonths = w;
                }
                else if (product is ClothingProduct cloth)
                {
                    string newSize = ConsoleUI.GetInput($"Size [{cloth.Size}]", true);
                    if (!string.IsNullOrEmpty(newSize)) cloth.Size = newSize;
                    string newMaterial = ConsoleUI.GetInput($"Material [{cloth.Material}]", true);
                    if (!string.IsNullOrEmpty(newMaterial)) cloth.Material = newMaterial;
                }
                else if (product is FurnitureProduct furn)
                {
                    string newFurnType = ConsoleUI.GetInput($"Furniture Type [{furn.FurnitureType}]", true);
                    if (!string.IsNullOrEmpty(newFurnType)) furn.FurnitureType = newFurnType;
                    string newMatType = ConsoleUI.GetInput($"Material Type [{furn.MaterialType}]", true);
                    if (!string.IsNullOrEmpty(newMatType)) furn.MaterialType = newMatType;
                    string newWeight = ConsoleUI.GetInput($"Weight (kg) [{furn.WeightKg}]", true);
                    if (!string.IsNullOrEmpty(newWeight) && decimal.TryParse(newWeight, out decimal wt) && wt >= 0)
                        furn.WeightKg = wt;
                }
                else if (product is PerishableProduct perish)
                {
                    string currentExpiry = perish.ExpiryDate == DateTime.MinValue ? "Not set" : perish.ExpiryDate.ToString("yyyy-MM-dd");
                    string newExpiry = ConsoleUI.GetInput($"Expiry Date (yyyy-MM-dd) [{currentExpiry}]", true);
                    if (!string.IsNullOrEmpty(newExpiry) && DateTime.TryParse(newExpiry, out DateTime exp))
                        perish.ExpiryDate = exp.Date;
                }
                else if (product is StationeryProduct stat)
                {
                    string newPages = ConsoleUI.GetInput($"Number of Pages [{stat.PageCount}]", true);
                    if (!string.IsNullOrEmpty(newPages) && int.TryParse(newPages, out int pg) && pg >= 0)
                        stat.PageCount = pg;
                }
                else if (product is OtherProduct other)
                {
                    string newDesc = ConsoleUI.GetInput($"Description [{other.Description}]", true);
                    if (!string.IsNullOrEmpty(newDesc)) other.Description = newDesc;
                }

                await DatabaseHelper.UpdateProduct(product);
                ConsoleUI.ShowSuccess("Product updated successfully!");
            }
            catch (Exception ex) { ConsoleUI.ShowError($"Error: {ex.Message}"); ConsoleUI.Pause(); }
        }

        static async Task DeleteProduct()
        {
            try
            {
                var products = await DatabaseHelper.GetAllProducts();
                if (products.Count == 0) { ConsoleUI.ShowInfo("No products available."); return; }

                ConsoleUI.ShowProducts(products);
                int id = ConsoleUI.GetInt("Enter Product ID to delete");
                var product = await DatabaseHelper.GetProductById(id);
                if (product == null) { ConsoleUI.ShowError("Product not found!"); return; }

                if (ConsoleUI.ConfirmDelete($"Delete '{product.Name}'?"))
                {
                    await DatabaseHelper.RemoveProduct(id);
                    ConsoleUI.ShowSuccess("Product deleted successfully!");
                }
            }
            catch (Exception ex) { ConsoleUI.ShowError($"Error: {ex.Message}"); ConsoleUI.Pause(); }
        }

        static async Task RunSupplierManagement()
        {
            var options = new[] { "➕ Add Supplier", "✏️ Update Supplier", "🗑️ Delete Supplier", "📋 View Suppliers",
                                  "🔗 Link Supplier to Product", "🔙 Back" };

            while (true)
            {
                int choice = ConsoleUI.ShowMenu("🏭 MANAGE SUPPLIERS", options);
                switch (choice)
                {
                    case 0: await AddSupplier(); break;
                    case 1: await UpdateSupplier(); break;
                    case 2: await DeleteSupplier(); break;
                    case 3: await ShowAllSuppliers(); break;
                    case 4: await LinkSupplierToProduct(); break;
                    default: return;
                }
            }
        }

        static async Task ShowAllSuppliers()
        {
            var suppliers = await DatabaseHelper.GetAllSuppliers();
            ConsoleUI.ShowSuppliers(suppliers);
        }

        static async Task AddSupplier()
        {
            try
            {
                string name = ConsoleUI.GetInput("Supplier Name");
                string contact = ConsoleUI.GetInput("Contact Number");
                string email = ConsoleUI.GetInput("Email");
                string city = ConsoleUI.GetInput("City");
                await DatabaseHelper.AddSupplier(new Supplier(name, contact, email, city));
                ConsoleUI.ShowSuccess($"Supplier '{name}' added successfully!");
            }
            catch (Exception ex) { ConsoleUI.ShowError($"Error: {ex.Message}"); ConsoleUI.Pause(); }
        }

        static async Task UpdateSupplier()
        {
            try
            {
                var suppliers = await DatabaseHelper.GetAllSuppliers();
                if (suppliers.Count == 0) { ConsoleUI.ShowInfo("No suppliers available."); return; }

                ConsoleUI.ShowSuppliers(suppliers);
                int id = ConsoleUI.GetInt("Enter Supplier ID to update");
                var supplier = await DatabaseHelper.GetSupplierById(id);
                if (supplier == null) { ConsoleUI.ShowError("Supplier not found!"); return; }

                string newName = ConsoleUI.GetInput($"Name [{supplier.Name}]", true);
                string newContact = ConsoleUI.GetInput($"Contact [{supplier.Contact}]", true);
                string newEmail = ConsoleUI.GetInput($"Email [{supplier.Email}]", true);
                string newCity = ConsoleUI.GetInput($"City [{supplier.City}]", true);

                if (!string.IsNullOrEmpty(newName)) supplier.Name = newName;
                if (!string.IsNullOrEmpty(newContact)) supplier.Contact = newContact;
                if (!string.IsNullOrEmpty(newEmail)) supplier.Email = newEmail;
                if (!string.IsNullOrEmpty(newCity)) supplier.City = newCity;

                await DatabaseHelper.UpdateSupplier(supplier);
                ConsoleUI.ShowSuccess("Supplier updated successfully!");
            }
            catch (Exception ex) { ConsoleUI.ShowError($"Error: {ex.Message}"); ConsoleUI.Pause(); }
        }

        static async Task DeleteSupplier()
        {
            try
            {
                var suppliers = await DatabaseHelper.GetAllSuppliers();
                if (suppliers.Count == 0) { ConsoleUI.ShowInfo("No suppliers available."); return; }

                ConsoleUI.ShowSuppliers(suppliers);
                int id = ConsoleUI.GetInt("Enter Supplier ID to delete");
                var supplier = await DatabaseHelper.GetSupplierById(id);
                if (supplier == null) { ConsoleUI.ShowError("Supplier not found!"); return; }

                if (ConsoleUI.ConfirmDelete($"Delete supplier '{supplier.Name}'?"))
                {
                    if (await DatabaseHelper.RemoveSupplier(id))
                        ConsoleUI.ShowSuccess("Supplier deleted successfully!");
                    else
                        ConsoleUI.ShowError("Cannot delete supplier with associated products.");
                }
            }
            catch (Exception ex) { ConsoleUI.ShowError($"Error: {ex.Message}"); ConsoleUI.Pause(); }
        }

        static async Task LinkSupplierToProduct()
        {
            try
            {
                var products = await DatabaseHelper.GetAllProducts();
                var suppliers = await DatabaseHelper.GetAllSuppliers();
                if (products.Count == 0 || suppliers.Count == 0)
                { ConsoleUI.ShowInfo("Products or suppliers not available."); return; }

                ConsoleUI.ShowProducts(products);
                int productId = ConsoleUI.GetInt("Enter Product ID");
                var product = await DatabaseHelper.GetProductById(productId);
                if (product == null) { ConsoleUI.ShowError("Product not found!"); return; }

                ConsoleUI.ShowSuppliers(suppliers);
                int supplierId = ConsoleUI.GetInt("Enter Supplier ID");
                var supplier = await DatabaseHelper.GetSupplierById(supplierId);
                if (supplier == null) { ConsoleUI.ShowError("Supplier not found!"); return; }

                product.SupplierId = supplier.Id;
                product.SupplierName = supplier.Name;
                await DatabaseHelper.UpdateProduct(product);
                ConsoleUI.ShowSuccess($"Supplier '{supplier.Name}' linked to '{product.Name}'!");
            }
            catch (Exception ex) { ConsoleUI.ShowError($"Error: {ex.Message}"); ConsoleUI.Pause(); }
        }

        static async Task RunOrderManagement()
        {
            var options = new[] { "📋 View All Orders", "🔍 Search Orders by Customer", "🔙 Back" };

            while (true)
            {
                int choice = ConsoleUI.ShowMenu("🛒 MANAGE ORDERS", options);
                switch (choice)
                {
                    case 0: await ViewAllOrders(); break;
                    case 1: await SearchOrdersByCustomer(); break;
                    default: return;
                }
            }
        }

        static async Task ViewAllOrders()
        {
            var orders = await DatabaseHelper.GetAllOrders();
            if (orders.Count == 0) ConsoleUI.ShowInfo("No orders found.");
            else ConsoleUI.ShowOrders(orders);
        }

        static async Task SearchOrdersByCustomer()
        {
            string customerName = ConsoleUI.GetInput("Enter customer name");
            var orders = await DatabaseHelper.GetOrdersByCustomer(customerName);
            if (orders.Count == 0) ConsoleUI.ShowInfo($"No orders found for '{customerName}'.");
            else ConsoleUI.ShowOrders(orders);
        }

        static async Task RunCustomerPanel()
        {
            var options = new[] { "🔐 Login", "📝 Register", "🔙 Back" };

            while (true)
            {
                int choice = ConsoleUI.ShowMenu("👤 CUSTOMER", options);
                switch (choice)
                {
                    case 0: if (await CustomerLogin()) await RunCustomerDashboard(); break;
                    case 1: await CustomerRegister(); break;
                    default: return;
                }
            }
        }

        static async Task<bool> CustomerLogin()
        {
            Console.Clear();
            ConsoleUI.DrawHeader("🔐 CUSTOMER LOGIN", ConsoleUI.C_PRIMARY);
            Console.WriteLine();

            string email = ConsoleUI.GetInput("Email");
            string password = ConsoleUI.GetInput("Password");
            _currentCustomer = await DatabaseHelper.AuthenticateCustomer(email, password);

            if (_currentCustomer != null)
            {
                ConsoleUI.ShowSuccess($"Welcome back, {_currentCustomer.Name}!");
                _currentCart = await DatabaseHelper.LoadCart(_currentCustomer.Id, _currentCustomer.Name);
                await Task.Delay(1000);
                return true;
            }
            ConsoleUI.ShowError("Invalid email or password!");
            ConsoleUI.Pause();
            return false;
        }

        static async Task CustomerRegister()
        {
            Console.Clear();
            ConsoleUI.DrawHeader("📝 CUSTOMER REGISTRATION", ConsoleUI.C_PRIMARY);
            Console.WriteLine();

            string name = ConsoleUI.GetInput("Full Name");
            string email = ConsoleUI.GetInput("Email");

            if (await DatabaseHelper.CustomerExists(email))
            {
                ConsoleUI.ShowError("Email already registered!");
                ConsoleUI.Pause();
                return;
            }

            string password = ConsoleUI.GetInput("Password");
            string confirm = ConsoleUI.GetInput("Confirm Password");

            if (password != confirm)
            {
                ConsoleUI.ShowError("Passwords do not match!");
                ConsoleUI.Pause();
                return;
            }

            string phone = ConsoleUI.GetInput("Phone Number");
            string address = ConsoleUI.GetInput("Address");

            var customer = new Customer(name, email, password, phone, address);
            await DatabaseHelper.AddCustomer(customer);
            ConsoleUI.ShowSuccess("Registration successful! You can now login.");
            ConsoleUI.Pause();
        }

        static async Task RunCustomerDashboard()
        {
            var options = new[] { "📋 View Products", "🔍 Search Product", "🛒 Add to Cart",
                                  "🛍️ View Cart", "❌ Remove from Cart", "💳 Checkout", "📜 My Order History",
                                  "👤 My Profile", "🚪 Logout" };

            while (true)
            {
                Console.Clear();
                ConsoleUI.DrawHeader($"👤 WELCOME, {_currentCustomer.Name.ToUpper()}!", ConsoleUI.C_SUCCESS);
                Console.WriteLine();
                Console.ForegroundColor = ConsoleUI.C_INFO;
                Console.WriteLine($"     Email: {_currentCustomer.Email}  |  Member since: {_currentCustomer.RegisteredAt:dd MMM yyyy}");
                Console.ResetColor();
                Console.WriteLine();

                int choice = ConsoleUI.ShowMenu("CUSTOMER DASHBOARD", options);
                switch (choice)
                {
                    case 0: await ShowAllProducts(); break;
                    case 1: await CustomerSearchProduct(); break;
                    case 2: await AddToCart(); break;
                    case 3: ConsoleUI.ShowCart(_currentCart); break;
                    case 4: await RemoveFromCart(); break;
                    case 5:
                        if (_currentCart.Items.Count == 0) { ConsoleUI.ShowWarning("Cart is empty!"); continue; }
                        await CustomerCheckout();
                        break;
                    case 6: await ViewMyOrderHistory(); break;
                    case 7: await ViewMyProfile(); break;
                    default:
                        await DatabaseHelper.SaveCart(_currentCustomer.Id, _currentCart);
                        _currentCustomer = null;
                        _currentCart = null;
                        return;
                }
            }
        }

        static async Task AddToCart()
        {
            try
            {
                var products = await DatabaseHelper.GetAvailableProducts();
                if (products.Count == 0) { ConsoleUI.ShowInfo("No products available."); return; }

                ConsoleUI.ShowProducts(products, compact: true);
                int id = ConsoleUI.GetInt("Enter Product ID to add");
                var product = products.FirstOrDefault(p => p.Id == id);
                if (product == null) { ConsoleUI.ShowError("Product not found or out of stock!"); return; }

                var existing = _currentCart.Items.FirstOrDefault(i => i.ProductId == id);
                int available = product.Quantity - (existing?.Quantity ?? 0);
                if (available <= 0) { ConsoleUI.ShowError($"Cannot add more. Available: {product.Quantity}"); return; }

                int qty = ConsoleUI.GetInt($"Quantity (Available: {available})", 1, available);
                _currentCart.AddItem(product, qty);
                await DatabaseHelper.SaveCart(_currentCustomer.Id, _currentCart);
                ConsoleUI.ShowSuccess($"Added {qty} x {product.Name} to cart");
            }
            catch (Exception ex) { ConsoleUI.ShowError($"Error: {ex.Message}"); ConsoleUI.Pause(); }
        }

        static async Task RemoveFromCart()
        {
            if (_currentCart.Items.Count == 0)
            {
                ConsoleUI.ShowWarning("Your cart is empty.");
                ConsoleUI.Pause();
                return;
            }

            ConsoleUI.ShowCart(_currentCart);
            string name = ConsoleUI.GetInput("Enter Product Name to remove");
            var item = _currentCart.Items.FirstOrDefault(i =>
                i.ProductName.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (item == null)
            {
                ConsoleUI.ShowError("Product not found in cart!");
                return;
            }

            if (ConsoleUI.ConfirmDelete($"Remove '{item.ProductName}' from cart?"))
            {
                _currentCart.RemoveItem(item.ProductId);
                await DatabaseHelper.SaveCart(_currentCustomer.Id, _currentCart);
                ConsoleUI.ShowSuccess("Item removed from cart.");
            }
        }

        static async Task CustomerCheckout()
        {
            try
            {
                var order = new Order(_currentCustomer.Name)
                {
                    CustomerId = _currentCustomer.Id,
                    CustomerEmail = _currentCustomer.Email
                };
                bool allAvailable = true;

                foreach (var item in _currentCart.Items)
                {
                    var product = await DatabaseHelper.GetProductById(item.ProductId);
                    if (product != null && product.Quantity >= item.Quantity)
                    {
                        await DatabaseHelper.RemoveStock(product.Id, item.Quantity);
                        order.AddItem(product, item.Quantity);
                    }
                    else
                    {
                        allAvailable = false;
                        ConsoleUI.ShowError($"Insufficient stock for {item.ProductName}");
                    }
                }

                if (allAvailable && order.Items.Count > 0)
                {
                    order.Complete();
                    await DatabaseHelper.AddOrder(order);

                    _currentCart.Clear();
                    await DatabaseHelper.ClearCart(_currentCustomer.Id);

                    bool continueShopping = ConsoleUI.ShowBill(order);
                    if (!continueShopping)
                    {
                        ConsoleUI.ShowGoodbye();
                        Environment.Exit(0);
                    }
                }
            }
            catch (Exception ex) { ConsoleUI.ShowError($"Error: {ex.Message}"); ConsoleUI.Pause(); }
        }

        static async Task ViewMyOrderHistory()
        {
            try
            {
                var orders = await DatabaseHelper.GetAllOrders();
                var myOrders = orders.Where(o => o.CustomerEmail != null &&
                                                 o.CustomerEmail.Equals(_currentCustomer.Email, StringComparison.OrdinalIgnoreCase))
                                     .OrderByDescending(o => o.OrderDate).ToList();

                if (myOrders.Count == 0)
                    ConsoleUI.ShowInfo("You have no orders yet.");
                else
                {
                    Console.Clear();
                    ConsoleUI.DrawHeader($"📜 ORDER HISTORY - {_currentCustomer.Name.ToUpper()}");
                    ConsoleUI.ShowOrders(myOrders);
                }
            }
            catch (Exception ex) { ConsoleUI.ShowError($"Error: {ex.Message}"); ConsoleUI.Pause(); }
        }

        static async Task ViewMyProfile()
        {
            Console.Clear();
            ConsoleUI.DrawHeader("👤 MY PROFILE", ConsoleUI.C_PRIMARY);
            Console.WriteLine();

            Console.ForegroundColor = ConsoleUI.C_HEADER;
            Console.WriteLine($"  Name         : {_currentCustomer.Name}");
            Console.WriteLine($"  Email        : {_currentCustomer.Email}");
            Console.WriteLine($"  Phone        : {_currentCustomer.Phone}");
            Console.WriteLine($"  Address      : {_currentCustomer.Address}");
            Console.WriteLine($"  Member Since : {_currentCustomer.RegisteredAt:dd MMMM yyyy}");
            if (_currentCustomer.LastLogin.HasValue)
                Console.WriteLine($"  Last Login   : {_currentCustomer.LastLogin:dd MMM yyyy hh:mm tt}");
            Console.ResetColor();
            Console.WriteLine();
            ConsoleUI.Pause();
        }

        static async Task RunReports()
        {
            var options = new[] { "📊 Inventory Report", "⚠️ Low Stock Report", "📊 Revenue Chart", "🏆 Top Products Revenue", "🔙 Back" };
            while (true)
            {
                int choice = ConsoleUI.ShowMenu("📊 REPORTS", options);
                switch (choice)
                {
                    case 0: await ShowInventoryReport(); break;
                    case 1: await ShowLowStockReport(); break;
                    case 2: await ShowCombinedRevenueCharts(); break;
                    case 3: await ShowTopProductsChart(); break;
                    default: return;
                }
            }
        }

        static async Task ShowInventoryReport()
        {
            var report = await DatabaseHelper.GetStockReport();
            ConsoleUI.ShowStockReport(report);
        }

        static async Task ShowLowStockReport()
        {
            var report = await DatabaseHelper.GetLowStockReport();
            ConsoleUI.ShowLowStockReport(report);
        }

        static async Task ShowCombinedRevenueCharts()
        {
            var daily = await DatabaseHelper.GetDailyRevenue(7);
            var weekly = await DatabaseHelper.GetWeeklyRevenue(4);
            var monthly = await DatabaseHelper.GetMonthlyRevenue();
            ConsoleUI.ShowCombinedRevenueCharts(daily, weekly, monthly);
        }

        static async Task ShowTopProductsChart()
        {
            var topProducts = await DatabaseHelper.GetTopProductsRevenue(10);
            ConsoleUI.ShowTopProductsChart(topProducts);
        }
    }
}