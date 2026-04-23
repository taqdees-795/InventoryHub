using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace InventoryHub
{
    public static class ConsoleUI
    {
        public const ConsoleColor C_PRIMARY = ConsoleColor.Cyan;
        public const ConsoleColor C_ACCENT = ConsoleColor.Yellow;
        public const ConsoleColor C_SUCCESS = ConsoleColor.Green;
        public const ConsoleColor C_ERROR = ConsoleColor.Red;
        public const ConsoleColor C_WARNING = ConsoleColor.DarkYellow;
        public const ConsoleColor C_INFO = ConsoleColor.Blue;
        public const ConsoleColor C_MUTED = ConsoleColor.DarkGray;
        public const ConsoleColor C_HEADER = ConsoleColor.White;
        public const ConsoleColor C_HIGHLIGHT = ConsoleColor.DarkCyan;

        private const int TABLE_WIDTH = 108;

        // ==================== SPLASH SCREEN ====================
        public static void ShowSplash()
        {
            Console.Clear();
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine($"╔{new string('═', TABLE_WIDTH)}╗");
            Console.WriteLine($"║{CenterText("", TABLE_WIDTH)}║");
            Console.WriteLine($"║{CenterText("INVENTORY HUB - PROFESSIONAL INVENTORY MANAGEMENT SYSTEM", TABLE_WIDTH)}║");
            Console.WriteLine($"║{CenterText("", TABLE_WIDTH)}║");
            Console.WriteLine($"║{CenterText(" ██╗███╗   ██╗██╗   ██╗███████╗███╗  ██╗████████╗ ██████╗ ██████╗ ██╗   ██╗   ██╗  ██╗██╗   ██╗ ██████╗", TABLE_WIDTH)}║");
            Console.WriteLine($"║{CenterText(" ██║████╗  ██║██║   ██║██╔════╝████╗ ██║╚══██╔══╝██╔═══██╗██╔══██╗╚██╗ ██╔╝   ██║  ██║██║   ██║ ██╔══██╗", TABLE_WIDTH)}║");
            Console.WriteLine($"║{CenterText(" ██║██╔██╗ ██║██║   ██║█████╗  ██╔██╗██║   ██║   ██║   ██║██████╔╝ ╚████╔╝    ███████║██║   ██║ ██████╔╝", TABLE_WIDTH)}║");
            Console.WriteLine($"║{CenterText(" ██║██║╚████║ ╚██╗ ██╔╝██╔══╝  ██║╚████║   ██║   ██║   ██║██╔══██╗  ╚██╔╝     ██╔══██║██║   ██║ ██╔══██╗", TABLE_WIDTH)}║");
            Console.WriteLine($"║{CenterText(" ██║██║ ╚███║  ╚████╔╝ ███████╗██║ ╚███║   ██║   ╚██████╔╝██║  ██║   ██║      ██║  ██║╚██████╔╝ ██████╔╝", TABLE_WIDTH)}║");
            Console.WriteLine($"║{CenterText(" ╚═╝╚═╝  ╚══╝   ╚═══╝  ╚══════╝╚═╝  ╚══╝   ╚═╝    ╚═════╝ ╚═╝  ╚═╝   ╚═╝      ╚═╝  ╚═╝ ╚═════╝  ╚═════╝", TABLE_WIDTH)}║");
            Console.WriteLine($"║{CenterText("INVENTORY HUB - Version 3.0 | Professional Edition", TABLE_WIDTH)}║");
            Console.WriteLine($"║{CenterText("", TABLE_WIDTH)}║");
            Console.WriteLine($"╚{new string('═', TABLE_WIDTH)}╝");
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = C_ACCENT;
            Console.Write(new string(' ', (TABLE_WIDTH - 30) / 2) + "Press any key to continue...");
            Console.ResetColor();
            Console.ReadKey(true);
            Console.Clear();
        }

        // ==================== MENU SYSTEM ====================
        public static int ShowMenu(string title, string[] options)
        {
            int selected = 0;
            ConsoleKey key;
            do
            {
                Console.Clear();
                Console.CursorVisible = false;

                DrawHeader($"INVENTORY HUB  ─  {title}");
                Console.WriteLine();

                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selected)
                    {
                        Console.ForegroundColor = C_ACCENT;
                        Console.Write("     >  ");
                        Console.ForegroundColor = C_HEADER;
                        Console.WriteLine(options[i]);
                    }
                    else
                    {
                        Console.ForegroundColor = C_MUTED;
                        Console.Write("        ");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine(options[i]);
                    }
                }

                int currentLine = Console.CursorTop;
                int linesToFill = Math.Max(0, Console.WindowHeight - currentLine - 5);
                for (int i = 0; i < linesToFill; i++)
                    Console.WriteLine(new string(' ', TABLE_WIDTH));

                Console.SetCursorPosition(0, currentLine);
                Console.WriteLine();
                Console.ForegroundColor = C_MUTED;
                Console.WriteLine("     ↑/↓ : Navigate    Enter : Select    Esc : Back");
                Console.ResetColor();
                DrawFooter();

                Console.CursorVisible = true;
                key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.UpArrow) selected = (selected - 1 + options.Length) % options.Length;
                else if (key == ConsoleKey.DownArrow) selected = (selected + 1) % options.Length;
                else if (key == ConsoleKey.Escape) return -1;
            } while (key != ConsoleKey.Enter);

            return selected;
        }

        public static bool ConfirmDelete(string message)
        {
            Console.ForegroundColor = C_WARNING;
            Console.Write($"\n  ⚠  {message} (y/n): ");
            var key = Console.ReadKey(true).Key;
            Console.ResetColor();
            Console.WriteLine();
            return key == ConsoleKey.Y;
        }

        // ==================== COLUMN WIDTH CALCULATOR ====================
        private static int[] CalculateColumnWidths(int[] desiredWidths, int targetTotalWidth)
        {
            int colCount = desiredWidths.Length;
            int sumDesired = desiredWidths.Sum();
            int requiredSum = targetTotalWidth - 3 - 3 * colCount;

            if (sumDesired > requiredSum)
            {
                double ratio = (double)requiredSum / sumDesired;
                for (int i = 0; i < colCount; i++)
                    desiredWidths[i] = Math.Max(3, (int)(desiredWidths[i] * ratio));
                int currentSum = desiredWidths.Sum();
                while (currentSum < requiredSum) { desiredWidths[colCount - 1]++; currentSum++; }
                while (currentSum > requiredSum) { desiredWidths[colCount - 1] = Math.Max(3, desiredWidths[colCount - 1] - 1); currentSum--; }
            }
            else if (sumDesired < requiredSum)
            {
                desiredWidths[colCount - 1] += requiredSum - sumDesired;
            }
            return desiredWidths;
        }

        // ==================== PRODUCT TABLE ====================
        private static readonly int[] _prodWidths = CalculateColumnWidths(new int[] { 5, 42, 14, 11, 7, 9, 14 }, TABLE_WIDTH);
        private static int PC_ID => _prodWidths[0];
        private static int PC_NAME => _prodWidths[1];
        private static int PC_CAT => _prodWidths[2];
        private static int PC_PRICE => _prodWidths[3];
        private static int PC_STOCK => _prodWidths[4];
        private static int PC_THRESH => _prodWidths[5];
        private static int PC_STATUS => _prodWidths[6];

        private static string ProdTopBorder() => "  ┌" + string.Join("┬", _prodWidths.Select(w => new string('─', w + 2))) + "┐";
        private static string ProdMidBorder() => "  ├" + string.Join("┼", _prodWidths.Select(w => new string('─', w + 2))) + "┤";
        private static string ProdBotBorder() => "  └" + string.Join("┴", _prodWidths.Select(w => new string('─', w + 2))) + "┘";

        public static void ShowProducts(List<Product> products, bool compact = false)
        {
            Console.Clear();
            Console.CursorVisible = false;

            DrawBoxTitle("  📦  PRODUCT INVENTORY", TABLE_WIDTH - 4);
            Console.WriteLine();

            if (products == null || products.Count == 0)
            {
                ShowWarning("No products found in database.");
                Pause();
                Console.CursorVisible = true;
                return;
            }

            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine(ProdTopBorder());

            Console.Write("  │ ");
            PrintCell("ID", PC_ID, C_HEADER, false);
            PrintCell("Product Name", PC_NAME, C_HEADER, false);
            PrintCell("Category", PC_CAT, C_HEADER, false);
            PrintCell("Price (PKR)", PC_PRICE, C_HEADER, true);
            PrintCell("Stock", PC_STOCK, C_HEADER, true);
            PrintCell("Threshold", PC_THRESH, C_HEADER, true);
            Console.ForegroundColor = C_HEADER;
            Console.Write(PadRight("Status", PC_STATUS));
            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine(" │");

            Console.WriteLine(ProdMidBorder());

            foreach (var p in products)
            {
                string status = p.GetStockStatus().ToString();
                ConsoleColor sc = status == "InStock" ? C_SUCCESS :
                                    status == "LowStock" ? C_WARNING : C_ERROR;
                string statusText = status == "InStock" ? "✔ IN STOCK" :
                                    status == "LowStock" ? "▲ LOW STOCK" : "✘ OUT OF STOCK";

                Console.ForegroundColor = C_PRIMARY; Console.Write("  │ ");
                Console.ForegroundColor = C_MUTED; Console.Write(PadRight(p.Id.ToString(), PC_ID));
                Console.ForegroundColor = C_PRIMARY; Console.Write(" │ ");
                Console.ForegroundColor = C_HEADER; Console.Write(PadRight(Truncate(p.Name, PC_NAME), PC_NAME));
                Console.ForegroundColor = C_PRIMARY; Console.Write(" │ ");
                Console.ForegroundColor = C_INFO; Console.Write(PadRight(Truncate(p.Category.ToString(), PC_CAT), PC_CAT));
                Console.ForegroundColor = C_PRIMARY; Console.Write(" │ ");
                Console.ForegroundColor = C_ACCENT; Console.Write(PadLeft(p.Price.ToString("N0"), PC_PRICE));
                Console.ForegroundColor = C_PRIMARY; Console.Write(" │ ");
                Console.ForegroundColor = p.Quantity > 0 ? C_SUCCESS : C_ERROR;
                Console.Write(PadLeft(p.Quantity.ToString(), PC_STOCK));
                Console.ForegroundColor = C_PRIMARY; Console.Write(" │ ");
                Console.ForegroundColor = C_MUTED; Console.Write(PadLeft(p.Threshold.ToString(), PC_THRESH));
                Console.ForegroundColor = C_PRIMARY; Console.Write(" │ ");
                Console.ForegroundColor = sc; Console.Write(PadRight(statusText, PC_STATUS));
                Console.ForegroundColor = C_PRIMARY; Console.WriteLine(" │");
            }

            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine(ProdBotBorder());
            Console.ResetColor();

            if (!compact)
            {
                Console.WriteLine();
                ShowInfo($"Total Products: {products.Count}   |   Total Value: PKR {products.Sum(p => p.Price * p.Quantity):N0}");
                Console.WriteLine();
                Pause();
            }
            Console.CursorVisible = true;
        }

        private static void PrintCell(string text, int width, ConsoleColor color, bool rightAlign)
        {
            Console.ForegroundColor = color;
            Console.Write(rightAlign ? PadLeft(text, width) : PadRight(text, width));
            Console.ForegroundColor = C_PRIMARY;
            Console.Write(" │ ");
        }

        // ==================== SUPPLIER TABLE ====================
        private static readonly int[] _supWidths = CalculateColumnWidths(new int[] { 5, 42, 18, 34, 13 }, TABLE_WIDTH);
        private static int SC_ID => _supWidths[0];
        private static int SC_NAME => _supWidths[1];
        private static int SC_CONTACT => _supWidths[2];
        private static int SC_EMAIL => _supWidths[3];
        private static int SC_CITY => _supWidths[4];

        private static string SupTopBorder() => "  ┌" + string.Join("┬", _supWidths.Select(w => new string('─', w + 2))) + "┐";
        private static string SupMidBorder() => "  ├" + string.Join("┼", _supWidths.Select(w => new string('─', w + 2))) + "┤";
        private static string SupBotBorder() => "  └" + string.Join("┴", _supWidths.Select(w => new string('─', w + 2))) + "┘";

        public static void ShowSuppliers(List<Supplier> suppliers)
        {
            Console.Clear();
            Console.CursorVisible = false;

            DrawBoxTitle("  🏭  SUPPLIER DIRECTORY", TABLE_WIDTH - 4);
            Console.WriteLine();

            if (suppliers == null || suppliers.Count == 0)
            {
                ShowWarning("No suppliers found in database.");
                Pause();
                Console.CursorVisible = true;
                return;
            }

            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine(SupTopBorder());

            Console.Write("  │ ");
            PrintCell("ID", SC_ID, C_HEADER, false);
            PrintCell("Supplier Name", SC_NAME, C_HEADER, false);
            PrintCell("Contact", SC_CONTACT, C_HEADER, false);
            PrintCell("Email", SC_EMAIL, C_HEADER, false);
            Console.ForegroundColor = C_HEADER;
            Console.Write(PadRight("City", SC_CITY));
            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine(" │");

            Console.WriteLine(SupMidBorder());
            Console.ResetColor();

            foreach (var s in suppliers)
            {
                Console.ForegroundColor = C_PRIMARY; Console.Write("  │ ");
                Console.ForegroundColor = C_MUTED; Console.Write(PadRight(s.Id.ToString(), SC_ID));
                Console.ForegroundColor = C_PRIMARY; Console.Write(" │ ");
                Console.ForegroundColor = C_HEADER; Console.Write(PadRight(Truncate(s.Name, SC_NAME), SC_NAME));
                Console.ForegroundColor = C_PRIMARY; Console.Write(" │ ");
                Console.ForegroundColor = C_INFO; Console.Write(PadRight(Truncate(s.Contact ?? "", SC_CONTACT), SC_CONTACT));
                Console.ForegroundColor = C_PRIMARY; Console.Write(" │ ");
                Console.ForegroundColor = C_ACCENT; Console.Write(PadRight(Truncate(s.Email ?? "", SC_EMAIL), SC_EMAIL));
                Console.ForegroundColor = C_PRIMARY; Console.Write(" │ ");
                Console.ForegroundColor = C_SUCCESS; Console.Write(PadRight(Truncate(s.City ?? "", SC_CITY), SC_CITY));
                Console.ForegroundColor = C_PRIMARY; Console.WriteLine(" │");
            }

            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine(SupBotBorder());
            Console.ResetColor();

            Console.WriteLine();
            ShowInfo($"Total Suppliers: {suppliers.Count}");

            Console.WriteLine();
            Pause();
            Console.CursorVisible = true;
        }

        // ==================== ORDERS TABLE ====================
        private static readonly int[] _ordWidths = CalculateColumnWidths(new int[] { 8, 28, 19, 12, 6, 16 }, TABLE_WIDTH);
        private static int OC_ORDERID => _ordWidths[0];
        private static int OC_CUSTOMER => _ordWidths[1];
        private static int OC_DATE => _ordWidths[2];
        private static int OC_STATUS => _ordWidths[3];
        private static int OC_ITEMS => _ordWidths[4];
        private static int OC_TOTAL => _ordWidths[5];

        private static string OrdTopBorder() => "  ┌" + string.Join("┬", _ordWidths.Select(w => new string('─', w + 2))) + "┐";
        private static string OrdMidBorder() => "  ├" + string.Join("┼", _ordWidths.Select(w => new string('─', w + 2))) + "┤";
        private static string OrdBotBorder() => "  └" + string.Join("┴", _ordWidths.Select(w => new string('─', w + 2))) + "┘";

        public static void ShowOrders(List<Order> orders)
        {
            Console.Clear();
            Console.CursorVisible = false;

            DrawBoxTitle("  🧾  ORDER HISTORY", TABLE_WIDTH - 4);
            Console.WriteLine();

            if (orders == null || orders.Count == 0)
            {
                ShowWarning("No orders found in database.");
                Pause();
                Console.CursorVisible = true;
                return;
            }

            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine(OrdTopBorder());

            Console.Write("  │ ");
            PrintCell("Order #", OC_ORDERID, C_HEADER, false);
            PrintCell("Customer", OC_CUSTOMER, C_HEADER, false);
            PrintCell("Date", OC_DATE, C_HEADER, false);
            PrintCell("Status", OC_STATUS, C_HEADER, false);
            PrintCell("Items", OC_ITEMS, C_HEADER, true);
            Console.ForegroundColor = C_HEADER;
            Console.Write(PadLeft("Total (PKR)", OC_TOTAL));
            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine(" │");

            Console.WriteLine(OrdMidBorder());
            Console.ResetColor();

            foreach (var o in orders)
            {
                ConsoleColor sc = o.Status == OrderStatus.Completed ? C_SUCCESS :
                                  o.Status == OrderStatus.Cancelled ? C_ERROR : C_WARNING;

                Console.ForegroundColor = C_PRIMARY; Console.Write("  │ ");
                Console.ForegroundColor = C_MUTED; Console.Write(PadRight("#" + o.OrderId, OC_ORDERID));
                Console.ForegroundColor = C_PRIMARY; Console.Write(" │ ");
                Console.ForegroundColor = C_HEADER; Console.Write(PadRight(Truncate(o.CustomerName ?? "", OC_CUSTOMER), OC_CUSTOMER));
                Console.ForegroundColor = C_PRIMARY; Console.Write(" │ ");
                Console.ForegroundColor = C_INFO; Console.Write(PadRight(o.OrderDate.ToString("yyyy-MM-dd HH:mm"), OC_DATE));
                Console.ForegroundColor = C_PRIMARY; Console.Write(" │ ");
                Console.ForegroundColor = sc; Console.Write(PadRight(o.Status.ToString(), OC_STATUS));
                Console.ForegroundColor = C_PRIMARY; Console.Write(" │ ");
                Console.ForegroundColor = C_MUTED; Console.Write(PadLeft(o.Items.Count.ToString(), OC_ITEMS));
                Console.ForegroundColor = C_PRIMARY; Console.Write(" │ ");
                Console.ForegroundColor = C_ACCENT; Console.Write(PadLeft(o.Total.ToString("N0"), OC_TOTAL));
                Console.ForegroundColor = C_PRIMARY; Console.WriteLine(" │");
            }

            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine(OrdBotBorder());
            Console.ResetColor();

            Console.WriteLine();
            decimal totalRevenue = orders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.Total);
            ShowInfo($"Total Orders: {orders.Count}   |   Total Revenue: PKR {totalRevenue:N0}");

            Console.WriteLine();
            Pause();
            Console.CursorVisible = true;
        }

        // ==================== SHOPPING CART ====================
        private static readonly int[] _cartWidths = CalculateColumnWidths(new int[] { 3, 48, 12, 5, 16 }, TABLE_WIDTH);
        private static int CC_IDX => _cartWidths[0];
        private static int CC_PRODUCT => _cartWidths[1];
        private static int CC_PRICE => _cartWidths[2];
        private static int CC_QTY => _cartWidths[3];
        private static int CC_SUBTOTAL => _cartWidths[4];

        private static string CartTopBorder() => "  ┌" + string.Join("┬", _cartWidths.Select(w => new string('─', w + 2))) + "┐";
        private static string CartMidBorder() => "  ├" + string.Join("┼", _cartWidths.Select(w => new string('─', w + 2))) + "┤";

        public static void ShowCart(ShoppingCart cart)
        {
            Console.Clear();
            Console.CursorVisible = false;

            DrawBoxTitle($"  🛒  SHOPPING CART  —  {cart.CustomerName ?? "Guest"}", TABLE_WIDTH - 4);
            Console.WriteLine();

            if (cart.Items.Count == 0)
            {
                ShowWarning("Your cart is empty.");
                Pause();
                Console.CursorVisible = true;
                return;
            }

            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine(CartTopBorder());

            Console.Write("  │ ");
            PrintCell("#", CC_IDX, C_HEADER, true);
            PrintCell("Product", CC_PRODUCT, C_HEADER, false);
            PrintCell("Unit Price", CC_PRICE, C_HEADER, true);
            PrintCell("Qty", CC_QTY, C_HEADER, true);
            Console.ForegroundColor = C_HEADER;
            Console.Write(PadLeft("Subtotal", CC_SUBTOTAL));
            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine(" │");

            Console.WriteLine(CartMidBorder());
            Console.ResetColor();

            int index = 1;
            foreach (var item in cart.Items)
            {
                Console.ForegroundColor = C_PRIMARY; Console.Write("  │ ");
                Console.ForegroundColor = C_MUTED; Console.Write(PadLeft(index++.ToString(), CC_IDX));
                Console.ForegroundColor = C_PRIMARY; Console.Write(" │ ");
                Console.ForegroundColor = C_HEADER; Console.Write(PadRight(Truncate(item.ProductName ?? "", CC_PRODUCT), CC_PRODUCT));
                Console.ForegroundColor = C_PRIMARY; Console.Write(" │ ");
                Console.ForegroundColor = C_ACCENT; Console.Write(PadLeft(item.UnitPrice.ToString("N0"), CC_PRICE));
                Console.ForegroundColor = C_PRIMARY; Console.Write(" │ ");
                Console.ForegroundColor = C_INFO; Console.Write(PadLeft(item.Quantity.ToString(), CC_QTY));
                Console.ForegroundColor = C_PRIMARY; Console.Write(" │ ");
                Console.ForegroundColor = C_SUCCESS; Console.Write(PadLeft(item.SubTotal.ToString("N0"), CC_SUBTOTAL));
                Console.ForegroundColor = C_PRIMARY; Console.WriteLine(" │");
            }

            int mergedWidth = (CC_IDX + 2) + 1 + (CC_PRODUCT + 2) + 1 + (CC_PRICE + 2) + 1 + (CC_QTY + 2);

            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine("  ├" + new string('─', mergedWidth) + "┴" + new string('─', CC_SUBTOTAL + 2) + "┤");

            Console.Write("  │ ");
            Console.ForegroundColor = C_MUTED;
            Console.Write(PadRight("TOTAL ITEMS:", mergedWidth - 1));
            Console.ForegroundColor = C_PRIMARY; Console.Write("│ ");
            Console.ForegroundColor = C_INFO;
            Console.Write(PadLeft(cart.ItemCount.ToString(), CC_SUBTOTAL));
            Console.ForegroundColor = C_PRIMARY; Console.WriteLine(" │");

            Console.Write("  │ ");
            Console.ForegroundColor = C_MUTED;
            Console.Write(PadRight("GRAND TOTAL (PKR):", mergedWidth - 1));
            Console.ForegroundColor = C_PRIMARY; Console.Write("│ ");
            Console.ForegroundColor = C_SUCCESS;
            Console.Write(PadLeft(cart.Total.ToString("N0"), CC_SUBTOTAL));
            Console.ForegroundColor = C_PRIMARY; Console.WriteLine(" │");

            Console.WriteLine("  └" + new string('─', mergedWidth) + "┴" + new string('─', CC_SUBTOTAL + 2) + "┘");
            Console.ResetColor();

            Console.WriteLine();
            Pause();
            Console.CursorVisible = true;
        }

        // ==================== DATABASE STATISTICS ====================
        public static void ShowDatabaseStats(
            string dbName,
            bool isConnected,
            int productCount,
            int supplierCount,
            int customerCount,
            int orderCount,
            decimal totalRevenue)
        {
            Console.Clear();
            Console.CursorVisible = false;

            var rows = new (string label, string value, ConsoleColor? valueColor)[]
            {
                ("Database Name", dbName, null),
                ("Connection Status", isConnected ? "● Connected" : "○ Disconnected", isConnected ? C_SUCCESS : C_ERROR),
                ("Products Count", productCount.ToString("N0"), C_INFO),
                ("Suppliers Count", supplierCount.ToString("N0"), C_INFO),
                ("Customers Count", customerCount.ToString("N0"), C_INFO),
                ("Orders Count", orderCount.ToString("N0"), C_INFO),
                ("Total Revenue", $"PKR {totalRevenue:N0}", C_SUCCESS)
            };

            int labelW = Math.Max(rows.Max(r => r.label.Length), 20);
            int valueW = Math.Max(rows.Max(r => r.value.Length), 30);
            int innerW = labelW + valueW + 3;

            DrawBoxTitle("  📊  MONGODB DATABASE STATISTICS", innerW + 2);
            Console.WriteLine();

            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine($"  ┌{new string('─', innerW + 2)}┐");

            Console.Write("  │ ");
            Console.ForegroundColor = C_ACCENT;
            Console.Write(CenterText("DATABASE INFORMATION", innerW));
            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine(" │");

            Console.WriteLine($"  ├{new string('─', innerW + 2)}┤");
            Console.ResetColor();

            for (int i = 0; i < rows.Length; i++)
            {
                var (label, value, valueColor) = rows[i];

                Console.Write("  │ ");
                Console.ForegroundColor = C_MUTED;
                Console.Write(PadRight(label, labelW));
                Console.ForegroundColor = C_PRIMARY;
                Console.Write(" : ");
                Console.ForegroundColor = valueColor ?? C_HEADER;
                Console.Write(PadRight(value, valueW));
                Console.ForegroundColor = C_PRIMARY;
                Console.WriteLine(" │");

                if (i == 1 || i == 5)
                {
                    Console.ForegroundColor = C_PRIMARY;
                    Console.WriteLine($"  ├{new string('─', innerW + 2)}┤");
                    Console.ResetColor();
                }
            }

            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine($"  └{new string('─', innerW + 2)}┘");
            Console.ResetColor();

            Console.WriteLine();
            Pause();
            Console.CursorVisible = true;
        }

        // ==================== INVOICE / BILL ====================
        private const int INV_WIDTH = 70;

        public static bool ShowBill(Order order)
        {
            Console.Clear();
            Console.CursorVisible = false;
            const string indent = "     ";

            const int itemW = 30;
            const int qtyW = 6;
            const int priceW = 10;
            const int totalW = 12;

            Console.WriteLine();
            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine(indent + "╔" + new string('═', INV_WIDTH) + "╗");

            WriteBillLine(indent, CenterText("INVENTORY HUB", INV_WIDTH), C_HEADER);
            WriteBillLine(indent, CenterText("── TAX INVOICE / RECEIPT ──", INV_WIDTH), C_PRIMARY);
            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine(indent + "╠" + new string('═', INV_WIDTH) + "╣");

            string leftPart = $" Invoice #: {order.OrderId}";
            string rightPart = $"Date: {order.OrderDate:dd MMM yyyy HH:mm} ";
            int gap = INV_WIDTH - leftPart.Length - rightPart.Length;
            if (gap < 1) gap = 1;
            Console.ForegroundColor = C_PRIMARY; Console.Write(indent + "║");
            Console.ForegroundColor = C_INFO;
            Console.Write(PadRight(leftPart + new string(' ', gap) + rightPart, INV_WIDTH));
            Console.ForegroundColor = C_PRIMARY; Console.WriteLine("║");

            string custLine = $" Customer: {Truncate(order.CustomerName ?? "Guest", INV_WIDTH - 13)}";
            Console.ForegroundColor = C_PRIMARY; Console.Write(indent + "║");
            Console.ForegroundColor = C_INFO;
            Console.Write(PadRight(custLine, INV_WIDTH));
            Console.ForegroundColor = C_PRIMARY; Console.WriteLine("║");

            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine(indent + "╠" + new string('─', INV_WIDTH) + "╣");

            Console.ForegroundColor = C_PRIMARY; Console.Write(indent + "║ ");
            Console.ForegroundColor = C_HEADER;
            Console.Write(PadRight("Item", itemW));
            Console.ForegroundColor = C_PRIMARY; Console.Write(" │ ");
            Console.ForegroundColor = C_HEADER;
            Console.Write(PadLeft("Qty", qtyW));
            Console.ForegroundColor = C_PRIMARY; Console.Write(" │ ");
            Console.ForegroundColor = C_HEADER;
            Console.Write(PadLeft("Unit Price", priceW));
            Console.ForegroundColor = C_PRIMARY; Console.Write(" │ ");
            Console.ForegroundColor = C_HEADER;
            Console.Write(PadLeft("Total", totalW));
            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine("  ║");

            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine(indent + "╟" +
                new string('─', itemW + 2) + "┼" +
                new string('─', qtyW + 2) + "┼" +
                new string('─', priceW + 2) + "┼" +
                new string('─', totalW + 3) + "╢");

            foreach (var item in order.Items)
            {
                Console.ForegroundColor = C_PRIMARY; Console.Write(indent + "║ ");
                Console.ForegroundColor = C_HEADER; Console.Write(PadRight(Truncate(item.ProductName ?? "", itemW), itemW));
                Console.ForegroundColor = C_PRIMARY; Console.Write(" │ ");
                Console.ForegroundColor = C_INFO; Console.Write(PadLeft(item.Quantity.ToString(), qtyW));
                Console.ForegroundColor = C_PRIMARY; Console.Write(" │ ");
                Console.ForegroundColor = C_ACCENT; Console.Write(PadLeft(item.UnitPrice.ToString("N0"), priceW));
                Console.ForegroundColor = C_PRIMARY; Console.Write(" │ ");
                Console.ForegroundColor = C_SUCCESS; Console.Write(PadLeft(item.SubTotal.ToString("N0"), totalW));
                Console.ForegroundColor = C_PRIMARY;
                Console.WriteLine("  ║");
            }

            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine(indent + "╟" +
                new string('─', itemW + 2) + "┴" +
                new string('─', qtyW + 2) + "┴" +
                new string('─', priceW + 2) + "┴" +
                new string('─', totalW + 3) + "╢");

            string subtotalLabel = "  Subtotal";
            string subtotalValue = $"PKR {order.Total:N0}";
            int subGap = INV_WIDTH - subtotalLabel.Length - subtotalValue.Length - 1;
            if (subGap < 1) subGap = 1;
            Console.ForegroundColor = C_PRIMARY; Console.Write(indent + "║");
            Console.ForegroundColor = C_MUTED; Console.Write(subtotalLabel + new string(' ', subGap));
            Console.ForegroundColor = C_ACCENT; Console.Write(subtotalValue);
            Console.ForegroundColor = C_PRIMARY; Console.WriteLine(" ║");

            Console.WriteLine(indent + "╠" + new string('═', INV_WIDTH) + "╣");

            string gtLabel = "  GRAND TOTAL";
            string gtValue = $"PKR {order.Total:N0}";
            int gtGap = INV_WIDTH - gtLabel.Length - gtValue.Length - 1;
            if (gtGap < 1) gtGap = 1;
            Console.ForegroundColor = C_PRIMARY; Console.Write(indent + "║");
            Console.ForegroundColor = C_HEADER; Console.Write(gtLabel + new string(' ', gtGap));
            Console.ForegroundColor = C_SUCCESS; Console.Write(gtValue);
            Console.ForegroundColor = C_PRIMARY; Console.WriteLine(" ║");

            Console.WriteLine(indent + "╟" + new string('─', INV_WIDTH) + "╢");

            string psLabel = "  Payment Status";
            string psValue = order.Status.ToString();
            int psGap = INV_WIDTH - psLabel.Length - psValue.Length - 1;
            if (psGap < 1) psGap = 1;
            ConsoleColor statusColor = order.Status == OrderStatus.Completed ? C_SUCCESS : C_WARNING;
            Console.ForegroundColor = C_PRIMARY; Console.Write(indent + "║");
            Console.ForegroundColor = C_MUTED; Console.Write(psLabel + new string(' ', psGap));
            Console.ForegroundColor = statusColor; Console.Write(psValue);
            Console.ForegroundColor = C_PRIMARY; Console.WriteLine(" ║");

            Console.WriteLine(indent + "╠" + new string('═', INV_WIDTH) + "╣");

            WriteBillLine(indent, CenterText("★  Thank you for your purchase!  ★", INV_WIDTH), C_SUCCESS);
            WriteBillLine(indent, CenterText("Please keep this receipt for your records.", INV_WIDTH), C_SUCCESS);
            WriteBillLine(indent, CenterText("www.inventoryhub.pk  |  support@inventoryhub.pk", INV_WIDTH), C_MUTED);

            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine(indent + "╚" + new string('═', INV_WIDTH) + "╝");
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = C_ACCENT;
            Console.Write(indent + "Press any key to continue shopping, or ESC to exit...");
            Console.ResetColor();

            var key = Console.ReadKey(true).Key;
            Console.CursorVisible = true;
            return key != ConsoleKey.Escape;
        }

        private static void WriteBillLine(string indent, string content, ConsoleColor color)
        {
            Console.ForegroundColor = C_PRIMARY;
            Console.Write(indent + "║");
            Console.ForegroundColor = color;
            Console.Write(PadRight(content.Length > INV_WIDTH ? content.Substring(0, INV_WIDTH) : content, INV_WIDTH));
            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine("║");
        }

        // ==================== CHARTS ====================
        private static void ShowBarChartInternal(
            List<KeyValuePair<string, decimal>> sortedData,
            string chartTitle,
            int maxBarLength = 40,
            int labelWidth = 20,
            bool showScale = true,
            bool useFiveColors = false)
        {
            decimal maxRevenue = sortedData.Max(kvp => kvp.Value);
            if (maxRevenue == 0) maxRevenue = 1;

            int innerWidth = TABLE_WIDTH - 4;

            Console.ForegroundColor = C_PRIMARY;
            string topLine = "  ┌" + new string('─', innerWidth) + "┐";
            string midLine = "  ├" + new string('─', innerWidth) + "┤";
            string botLine = "  └" + new string('─', innerWidth) + "┘";

            Console.WriteLine(topLine);
            Console.Write("  │");
            Console.ForegroundColor = C_ACCENT;
            Console.Write(CenterText(chartTitle, innerWidth));
            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine("│");
            Console.WriteLine(midLine);

            // Color Legend
            if (useFiveColors)
            {
                Console.ForegroundColor = C_MUTED;
                Console.Write("  │  ");
                Console.ForegroundColor = C_MUTED;
                Console.Write("Legend: ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("█ 1st  ");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write("█ 2nd  ");
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("█ 3rd  ");
                Console.ForegroundColor = C_INFO;
                Console.Write("█ 4th-5th  ");
                Console.ForegroundColor = C_MUTED;
                Console.Write("█ Others");

                // Add 2 extra spaces to push the right border forward (only for Top Products)
                int fixedSpaces = innerWidth - 53 + 2;
                if (fixedSpaces > 0) Console.Write(new string(' ', fixedSpaces));
                Console.ForegroundColor = C_PRIMARY;
                Console.WriteLine(" │");
                Console.WriteLine(midLine);
            }
            else
            {
                Console.ForegroundColor = C_MUTED;
                Console.Write("  │  Legend: ");
                Console.ForegroundColor = C_SUCCESS;
                Console.Write("█ Highest  ");
                Console.ForegroundColor = C_ACCENT;
                Console.Write("█ 2nd Highest  ");
                Console.ForegroundColor = C_INFO;
                Console.Write("█ Others");
                // Total printed: 5 + 8 + 10 + 14 + 8 = 45
                int fixedSpaces = innerWidth - 45;
                if (fixedSpaces > 0) Console.Write(new string(' ', fixedSpaces));
                Console.ForegroundColor = C_PRIMARY;
                Console.WriteLine(" │");
                Console.WriteLine(midLine);
            }

            // Scale indicator (optional)
            if (showScale)
            {
                decimal scaleValue = maxRevenue / maxBarLength;
                Console.ForegroundColor = C_MUTED;
                Console.Write("  │  ");
                Console.Write($"Scale: Each █ = PKR {scaleValue:N0}".PadRight(innerWidth - 2));
                Console.ForegroundColor = C_PRIMARY;
                Console.WriteLine("│");
                Console.WriteLine(midLine);
            }
            Console.ResetColor();

            for (int i = 0; i < sortedData.Count; i++)
            {
                var kvp = sortedData[i];
                string label = kvp.Key;
                decimal revenue = kvp.Value;

                int barLength = (int)Math.Round((revenue / maxRevenue) * maxBarLength);
                if (barLength == 0 && revenue > 0) barLength = 1;

                ConsoleColor barColor;
                if (useFiveColors)
                {
                    if (i == 0) barColor = ConsoleColor.DarkYellow;
                    else if (i == 1) barColor = ConsoleColor.Magenta;
                    else if (i == 2) barColor = ConsoleColor.DarkRed;
                    else if (i >= 3 && i <= 4) barColor = C_INFO;
                    else barColor = C_MUTED;
                }
                else
                {
                    if (revenue == maxRevenue)
                        barColor = C_SUCCESS;
                    else if (i == sortedData.Count - 2 && sortedData.Count > 1)
                        barColor = C_ACCENT;
                    else
                        barColor = C_INFO;
                }

                string leftPart = PadRight(Truncate(label, labelWidth), labelWidth);
                string barPart = new string('█', barLength) + new string('░', maxBarLength - barLength);
                string valuePart = $"PKR {revenue:N0}";

                int usedWidth = 2 + labelWidth + 3 + maxBarLength + 3 + valuePart.Length;
                int rightPadding = innerWidth - usedWidth;
                if (rightPadding < 0) rightPadding = 0;

                Console.Write("  │ ");
                Console.ForegroundColor = C_MUTED;
                Console.Write(leftPart);
                Console.ForegroundColor = C_PRIMARY;
                Console.Write(" │ ");
                Console.ForegroundColor = barColor;
                Console.Write(barPart);
                Console.ForegroundColor = C_PRIMARY;
                Console.Write(" │ ");
                Console.ForegroundColor = C_ACCENT;
                Console.Write(valuePart);
                Console.ForegroundColor = C_PRIMARY;
                Console.Write(new string(' ', rightPadding));
                Console.WriteLine(" │");
            }

            Console.WriteLine(botLine);
            Console.ResetColor();

            decimal total = sortedData.Sum(kvp => kvp.Value);
            decimal average = total / sortedData.Count;
            Console.WriteLine();
            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine($"  Total Revenue: PKR {total:N0}   |   Average: PKR {average:N0}");
            Console.ResetColor();
        }

        public static void ShowRevenueChart(Dictionary<string, decimal> monthlyRevenue)
        {
            Console.Clear();
            Console.CursorVisible = false;

            DrawBoxTitle("  📈  MONTHLY REVENUE ANALYSIS", TABLE_WIDTH - 4);
            Console.WriteLine();

            if (monthlyRevenue == null || monthlyRevenue.Count == 0)
            {
                ShowWarning("No revenue data available.");
                Pause();
                Console.CursorVisible = true;
                return;
            }

            var sortedData = monthlyRevenue
                .OrderBy(kvp => DateTime.ParseExact(kvp.Key, "MMM yyyy", null))
                .ToList();

            ShowBarChartInternal(sortedData, "MONTHLY REVENUE (PKR)");
            Pause();
        }

        public static void ShowDailyRevenueChart(Dictionary<string, decimal> dailyRevenue)
        {
            Console.Clear();
            Console.CursorVisible = false;

            DrawBoxTitle("  📆  DAILY REVENUE (LAST 7 DAYS)", TABLE_WIDTH - 4);
            Console.WriteLine();

            if (dailyRevenue == null || dailyRevenue.Count == 0)
            {
                ShowWarning("No revenue data available for this period.");
                Pause();
                Console.CursorVisible = true;
                return;
            }

            var sortedData = dailyRevenue.OrderBy(kvp => kvp.Key).ToList();
            ShowBarChartInternal(sortedData, "DAILY REVENUE (PKR)");
            Pause();
        }

        public static void ShowWeeklyRevenueChart(Dictionary<string, decimal> weeklyRevenue)
        {
            Console.Clear();
            Console.CursorVisible = false;

            DrawBoxTitle("  📅  WEEKLY REVENUE (LAST 4 WEEKS)", TABLE_WIDTH - 4);
            Console.WriteLine();

            if (weeklyRevenue == null || weeklyRevenue.Count == 0)
            {
                ShowWarning("No revenue data available for this period.");
                Pause();
                Console.CursorVisible = true;
                return;
            }

            var sortedData = weeklyRevenue.OrderBy(kvp => kvp.Key).ToList();
            ShowBarChartInternal(sortedData, "WEEKLY REVENUE (PKR)");
            Pause();
        }

        public static void ShowCombinedRevenueCharts(Dictionary<string, decimal> daily, Dictionary<string, decimal> weekly, Dictionary<string, decimal> monthly)
        {
            Console.Clear();
            Console.CursorVisible = false;

            DrawBoxTitle("  📊  REVENUE CHART", TABLE_WIDTH - 4);
            Console.WriteLine();

            // Daily
            if (daily != null && daily.Count > 0)
            {
                var sortedDaily = daily.OrderBy(kvp => kvp.Key).ToList();
                ShowBarChartInternal(sortedDaily, "DAILY REVENUE (LAST 7 DAYS)", showScale: false, useFiveColors: false);
                Console.WriteLine();
                Console.WriteLine();
            }
            else
            {
                ShowWarning("No daily revenue data.");
                Console.WriteLine();
            }

            // Weekly
            if (weekly != null && weekly.Count > 0)
            {
                var sortedWeekly = weekly.OrderBy(kvp => kvp.Key).ToList();
                ShowBarChartInternal(sortedWeekly, "WEEKLY REVENUE (LAST 4 WEEKS)", showScale: false, useFiveColors: false);
                Console.WriteLine();
                Console.WriteLine();
            }
            else
            {
                ShowWarning("No weekly revenue data.");
                Console.WriteLine();
            }

            // Monthly
            if (monthly != null && monthly.Count > 0)
            {
                var sortedMonthly = monthly
                    .OrderBy(kvp => DateTime.ParseExact(kvp.Key, "MMM yyyy", null))
                    .ToList();
                ShowBarChartInternal(sortedMonthly, "MONTHLY REVENUE", showScale: false, useFiveColors: false);
                Console.WriteLine();
            }
            else
            {
                ShowWarning("No monthly revenue data.");
                Console.WriteLine();
            }

            Pause();
            Console.CursorVisible = true;
        }

        public static void ShowTopProductsChart(Dictionary<string, decimal> topProducts)
        {
            Console.Clear();
            Console.CursorVisible = false;

            DrawBoxTitle("  🏆  TOP PRODUCTS BY REVENUE", TABLE_WIDTH - 4);
            Console.WriteLine();

            if (topProducts == null || topProducts.Count == 0)
            {
                ShowWarning("No product revenue data available.");
                Pause();
                Console.CursorVisible = true;
                return;
            }

            var sortedData = topProducts.OrderByDescending(kvp => kvp.Value).ToList();
            ShowBarChartInternal(sortedData, "PRODUCT REVENUE (PKR)", maxBarLength: 40, labelWidth: 22, showScale: false, useFiveColors: true);
            Pause();
        }

        // ==================== REPORTS ====================
        public static void ShowStockReport(StockReport report)
        {
            Console.Clear();
            Console.CursorVisible = false;

            const int rw = 60;
            DrawBoxTitle("  📋  INVENTORY REPORT", rw + 2);
            Console.WriteLine();

            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine("  ┌" + new string('─', rw) + "┐");
            Console.Write("  │");
            Console.ForegroundColor = C_ACCENT;
            Console.Write(CenterText("SUMMARY", rw));
            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine("│");
            Console.WriteLine("  ├" + new string('─', rw) + "┤");
            Console.ResetColor();

            WriteReportRow("Total Products", report.TotalProducts.ToString(), rw, C_INFO);
            WriteReportRow("Total Items in Stock", report.TotalItemsInStock.ToString(), rw, C_INFO);
            WriteReportRow("Total Stock Value", "PKR " + report.TotalStockValue.ToString("N0"), rw, C_SUCCESS);

            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine("  └" + new string('─', rw) + "┘");
            Console.ResetColor();

            if (report.ProductsByCategory != null && report.ProductsByCategory.Count > 0)
            {
                Console.WriteLine();
                ShowInfo("PRODUCTS BY CATEGORY:");
                foreach (var kvp in report.ProductsByCategory)
                {
                    Console.ForegroundColor = C_MUTED; Console.Write("       ");
                    Console.ForegroundColor = C_INFO; Console.Write($"► {kvp.Key,-16}");
                    Console.ForegroundColor = C_HEADER; Console.WriteLine($" {kvp.Value} product(s)");
                    Console.ResetColor();
                }
            }

            Console.WriteLine();
            Pause();
            Console.CursorVisible = true;
        }

        private static void WriteReportRow(string label, string value, int rowWidth, ConsoleColor valueColor)
        {
            const int labelW = 24;
            int valueW = rowWidth - labelW - 5;

            Console.ForegroundColor = C_PRIMARY; Console.Write("  │  ");
            Console.ForegroundColor = C_MUTED; Console.Write(PadRight(label, labelW));
            Console.ForegroundColor = C_MUTED; Console.Write(" : ");
            Console.ForegroundColor = valueColor; Console.Write(PadRight(Truncate(value, valueW), valueW));
            Console.ForegroundColor = C_PRIMARY; Console.WriteLine("│");
            Console.ResetColor();
        }

        public static void ShowLowStockReport(LowStockReport report)
        {
            Console.Clear();
            Console.CursorVisible = false;

            const int rw = 60;
            DrawBoxTitle("  ⚠  LOW STOCK REPORT", rw + 2);
            Console.WriteLine();

            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine("  ┌" + new string('─', rw) + "┐");
            Console.Write("  │");
            Console.ForegroundColor = C_WARNING;
            Console.Write(CenterText("STOCK ALERTS", rw));
            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine("│");
            Console.WriteLine("  ├" + new string('─', rw) + "┤");
            Console.ResetColor();

            WriteReportRow("Low Stock Items", report.LowStockCount.ToString(), rw, C_WARNING);
            WriteReportRow("Out of Stock", report.OutOfStockCount.ToString(), rw, C_ERROR);

            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine("  └" + new string('─', rw) + "┘");
            Console.ResetColor();

            if (report.LowStockProducts != null && report.LowStockProducts.Count > 0)
            {
                Console.WriteLine();
                ShowWarning("LOW STOCK PRODUCTS:");
                foreach (var p in report.LowStockProducts)
                    ShowInfo($"  ► {p.Name}  —  Only {p.Quantity} left  (Threshold: {p.Threshold})");
            }

            if (report.OutOfStockProducts != null && report.OutOfStockProducts.Count > 0)
            {
                Console.WriteLine();
                ShowError("OUT OF STOCK — RESTOCK IMMEDIATELY:");
                foreach (var p in report.OutOfStockProducts)
                    ShowError($"  ► {p.Name}  —  COMPLETELY SOLD OUT");
            }

            if (report.LowStockCount == 0 && report.OutOfStockCount == 0)
                ShowSuccess("All products are well-stocked!");

            Console.WriteLine();
            Pause();
            Console.CursorVisible = true;
        }

        public static void ShowStockStatus(List<Product> lowStock, List<Product> outOfStock)
        {
            Console.Clear();
            Console.CursorVisible = false;

            const int rw = 60;
            DrawBoxTitle("  📊  STOCK STATUS DASHBOARD", rw + 2);
            Console.WriteLine();

            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine("  ┌" + new string('─', rw) + "┐");
            Console.Write("  │");
            Console.ForegroundColor = C_ACCENT;
            Console.Write(CenterText("OVERVIEW", rw));
            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine("│");
            Console.WriteLine("  ├" + new string('─', rw) + "┤");
            Console.ResetColor();

            WriteReportRow("Low Stock (need attention)", lowStock.Count + " product(s)", rw, C_WARNING);
            WriteReportRow("Out of Stock (need restock)", outOfStock.Count + " product(s)", rw, C_ERROR);

            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine("  └" + new string('─', rw) + "┘");
            Console.ResetColor();

            if (lowStock.Count > 0)
            {
                Console.WriteLine();
                ShowWarning("LOW STOCK ALERTS:");
                foreach (var p in lowStock)
                    ShowInfo($"  ► {p.Name}  —  Only {p.Quantity} left  (Min: {p.Threshold})");
            }

            if (outOfStock.Count > 0)
            {
                Console.WriteLine();
                ShowError("OUT OF STOCK ALERTS:");
                foreach (var p in outOfStock)
                    ShowError($"  ► {p.Name}  —  COMPLETELY SOLD OUT — RESTOCK NOW");
            }

            if (lowStock.Count == 0 && outOfStock.Count == 0)
                ShowSuccess("All products are well-stocked!");

            Console.WriteLine();
            Pause();
            Console.CursorVisible = true;
        }

        public static void ShowGoodbye()
        {
            Console.Clear();
            Console.CursorVisible = false;

            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine($"╔{new string('═', TABLE_WIDTH)}╗");
            Console.WriteLine($"║{CenterText("", TABLE_WIDTH)}║");

            Console.ForegroundColor = C_ACCENT;
            Console.WriteLine($"║{CenterText("THANK YOU FOR USING INVENTORY HUB", TABLE_WIDTH)}║");

            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine($"║{CenterText("", TABLE_WIDTH)}║");
            Console.WriteLine($"║{CenterText("Your trusted inventory management partner", TABLE_WIDTH)}║");
            Console.WriteLine($"║{CenterText("", TABLE_WIDTH)}║");

            Console.ForegroundColor = C_SUCCESS;
            Console.WriteLine($"║{CenterText("Have a great day!  ✔", TABLE_WIDTH)}║");

            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine($"║{CenterText("", TABLE_WIDTH)}║");
            Console.WriteLine($"╚{new string('═', TABLE_WIDTH)}╝");

            Console.ResetColor();
            Thread.Sleep(2000);
        }

        // ==================== INPUT HELPERS ====================
        public static string GetInput(string prompt, bool allowEmpty = false)
        {
            while (true)
            {
                Console.ForegroundColor = C_ACCENT;
                Console.Write($"\n  ► {prompt}: ");
                Console.ForegroundColor = C_HEADER;
                string input = (Console.ReadLine() ?? "").Trim();
                Console.ResetColor();
                if (!string.IsNullOrEmpty(input) || allowEmpty) return input;
                ShowError("Input cannot be empty!");
            }
        }

        public static int GetInt(string prompt, int minValue = 0, int maxValue = int.MaxValue)
        {
            while (true)
            {
                string input = GetInput(prompt);
                if (int.TryParse(input, out int value) && value >= minValue && value <= maxValue)
                    return value;
                ShowError($"Please enter a number between {minValue} and {maxValue}.");
            }
        }

        public static decimal GetDecimal(string prompt)
        {
            while (true)
            {
                string input = GetInput(prompt);
                if (decimal.TryParse(input, out decimal value) && value >= 0) return value;
                ShowError("Please enter a valid positive number.");
            }
        }

        public static DateTime GetDate(string prompt)
        {
            while (true)
            {
                string input = GetInput(prompt);
                if (DateTime.TryParse(input, out DateTime date)) return date;
                ShowError("Invalid date format. Use yyyy-MM-dd.");
            }
        }

        public static ProductType SelectProductType()
        {
            string[] options = Enum.GetNames(typeof(ProductType));
            int choice = ShowMenu("SELECT PRODUCT TYPE", options);
            if (choice >= 0 && choice < options.Length)
                return (ProductType)Enum.Parse(typeof(ProductType), options[choice]);
            return ProductType.Other;
        }

        // ==================== MESSAGE HELPERS ====================
        public static void ShowSuccess(string message)
        {
            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine($"\n  [ ✔ ] {message}");
            Console.ResetColor();
            Thread.Sleep(1000);
        }

        public static void ShowError(string message)
        {
            Console.ForegroundColor = C_ERROR;
            Console.WriteLine($"\n  [ ✘ ] {message}");
            Console.ResetColor();
            Thread.Sleep(1500);
        }

        public static void ShowWarning(string message)
        {
            Console.ForegroundColor = C_WARNING;
            Console.WriteLine($"\n  [ ⚠ ] {message}");
            Console.ResetColor();
            Thread.Sleep(1000);
        }

        public static void ShowInfo(string message)
        {
            Console.ForegroundColor = C_INFO;
            Console.WriteLine($"\n  [ i ] {message}");
            Console.ResetColor();
        }

        public static void Pause()
        {
            Console.ForegroundColor = C_MUTED;
            Console.Write("\n  Press any key to continue...");
            Console.ReadKey(true);
            Console.ResetColor();
        }

        // ==================== HEADER / FOOTER ====================
        public static void DrawHeader(string title, ConsoleColor color = C_PRIMARY)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"╔{new string('═', TABLE_WIDTH)}╗");
            Console.WriteLine($"║{CenterText(title, TABLE_WIDTH)}║");
            Console.WriteLine($"╠{new string('═', TABLE_WIDTH)}╣");
            Console.ResetColor();
        }

        private static void DrawBoxTitle(string title, int innerWidth)
        {
            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine($"  ╔{new string('═', innerWidth)}╗");
            Console.Write($"  ║");
            Console.ForegroundColor = C_HEADER;
            Console.Write(CenterText(title, innerWidth));
            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine($"║");
            Console.WriteLine($"  ╚{new string('═', innerWidth)}╝");
            Console.ResetColor();
        }

        private static void DrawFooter(ConsoleColor color = C_PRIMARY)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"╚{new string('═', TABLE_WIDTH)}╝");
            Console.ResetColor();
        }

        // ==================== STRING UTILITIES ====================
        public static string CenterText(string text, int width)
        {
            if (string.IsNullOrEmpty(text)) return new string(' ', width);
            if (text.Length >= width) return text.Substring(0, width);
            int pad = (width - text.Length) / 2;
            return new string(' ', pad) + text + new string(' ', width - text.Length - pad);
        }

        public static string Truncate(string text, int max)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Length <= max ? text : text.Substring(0, max - 3) + "...";
        }

        private static string PadRight(string text, int width)
        {
            text = text ?? "";
            if (text.Length >= width) return text.Substring(0, width);
            return text + new string(' ', width - text.Length);
        }

        private static string PadLeft(string text, int width)
        {
            text = text ?? "";
            if (text.Length >= width) return text.Substring(0, width);
            return new string(' ', width - text.Length) + text;
        }
    }
}