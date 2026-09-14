using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hardware.winforms
{
    public partial class MainErpForm : Form
    {
        private readonly HttpClient _httpClient;
        private int _currentCompanyId;
        private List<CartItemDto> _cart = new List<CartItemDto>();
        private SalesSummaryReportDto _currentReport;

        // Layout Containers
        private Panel pnlSidebar;
        private Panel pnlMainContent;

        // Navigation Buttons
        private Button btnNavSales;
        private Button btnNavInventory;
        private Button btnNavReports;
        private Button btnLogout;

        // View Panels
        private Panel viewSales;
        private Panel viewInventory;
        private Panel viewReports;

        // Inventory Controls
        private DataGridView dgvInventory;
        private TextBox txtProdCode, txtProdName, txtProdPrice;
        private TextBox txtAdjustProductId, txtAdjustQty, txtAdjustReorder;

        // Sales Controls
        private DataGridView dgvCart;
        private TextBox txtInvoiceNum, txtCustomerId, txtSaleProductId, txtSaleQty, txtUnitPrice;
        private Label lblTotalAmount;

        // Transaction History & Reports Controls
        private Label lblRevenueValue, lblTransValue, lblTopProdValue;
        private DataGridView dgvReportDetails;
        private Label lblReportContext;

        public MainErpForm(int companyId = 1)
        {
            _currentCompanyId = companyId;
            InitializeComponentCustom();
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7166/") };
        }

        private void InitializeComponentCustom()
        {
            this.Text = "Micro-Enterprise ERP - Hardware System";
            this.Size = new Size(1280, 800);
            this.MinimumSize = new Size(1200, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(24, 24, 24);

            // ==========================================
            // 1. SIDEBAR PANEL 
            // ==========================================
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = Color.FromArgb(32, 32, 32)
            };

            Label lblAppTitle = new Label
            {
                Text = "⚡ HARDWARE ERP",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(15, 20),
                AutoSize = true
            };
            pnlSidebar.Controls.Add(lblAppTitle);

            btnNavSales = CreateSidebarButton("🛒  Sales & POS", 80);
            btnNavInventory = CreateSidebarButton("📦  Inventory", 130);
            btnNavReports = CreateSidebarButton("📊  Transaction History", 180);

            btnNavSales.Click += (s, e) => SwitchView(viewSales, btnNavSales);
            btnNavInventory.Click += (s, e) => SwitchView(viewInventory, btnNavInventory);
            btnNavReports.Click += (s, e) => SwitchView(viewReports, btnNavReports);

            btnLogout = new Button
            {
                Text = "🚪  Log Out",
                Dock = DockStyle.Bottom,
                Height = 45,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(239, 68, 68),
                BackColor = Color.FromArgb(32, 32, 32),
                Font = new Font("Segoe UI", 10.5f, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
            btnLogout.Click += (s, e) =>
            {
                this.Hide();
                using var loginForm = new Login();
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    _currentCompanyId = loginForm.AuthenticatedCompanyId;
                    this.Show();
                    _ = RefreshAllDataAsync();
                }
                else
                {
                    Application.Exit();
                }
            };

            pnlSidebar.Controls.Add(btnLogout);
            pnlSidebar.Controls.AddRange(new Control[] { btnNavSales, btnNavInventory, btnNavReports });

            // ==========================================
            // 2. MAIN CONTENT CONTAINER 
            // ==========================================
            pnlMainContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(24, 24, 24)
            };

            BuildSalesView();
            BuildInventoryView();
            BuildReportsView();

            this.Controls.Add(pnlMainContent);
            this.Controls.Add(pnlSidebar);

            SwitchView(viewSales, btnNavSales);
            this.Load += async (s, e) => await RefreshAllDataAsync();
        }

        private Button CreateSidebarButton(string text, int top)
        {
            return new Button
            {
                Text = text,
                Location = new Point(10, top),
                Size = new Size(200, 45),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.LightGray,
                BackColor = Color.FromArgb(32, 32, 32),
                Font = new Font("Segoe UI", 10.5f, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
        }

        private void SwitchView(Panel targetView, Button activeBtn)
        {
            pnlMainContent.Controls.Clear();
            targetView.Dock = DockStyle.Fill;
            pnlMainContent.Controls.Add(targetView);

            foreach (Control c in pnlSidebar.Controls)
            {
                if (c is Button btn && btn != btnLogout)
                {
                    btn.BackColor = Color.FromArgb(32, 32, 32);
                    btn.ForeColor = Color.LightGray;
                }
            }

            activeBtn.BackColor = Color.FromArgb(50, 50, 50);
            activeBtn.ForeColor = Color.White;
        }

        // ==========================================
        // MODULE 1: SALES & POS
        // ==========================================
        private void BuildSalesView()
        {
            viewSales = new Panel();

            Panel pnlCart = new Panel { Dock = DockStyle.Left, Width = 680, Padding = new Padding(20) };
            Label lblCartTitle = new Label { Text = "Current Sale Items", ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };

            dgvCart = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(640, 550),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.Black,
                RowTemplate = { Height = 35 }
            };
            pnlCart.Controls.AddRange(new Control[] { lblCartTitle, dgvCart });

            Panel pnlCheckout = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            GroupBox grpAdd = new GroupBox { Text = "Add Item via ID", ForeColor = Color.White, Font = new Font("Segoe UI", 11), Location = new Point(20, 20), Size = new Size(320, 280) };

            grpAdd.Controls.Add(new Label { Text = "Product ID:", ForeColor = Color.White, Location = new Point(15, 42), AutoSize = true });
            txtSaleProductId = new TextBox { Location = new Point(125, 39), Width = 170 };
            grpAdd.Controls.Add(txtSaleProductId);

            grpAdd.Controls.Add(new Label { Text = "Quantity:", ForeColor = Color.White, Location = new Point(15, 85), AutoSize = true });
            txtSaleQty = new TextBox { Location = new Point(125, 82), Width = 170, Text = "1" };
            grpAdd.Controls.Add(txtSaleQty);

            grpAdd.Controls.Add(new Label { Text = "Unit Price:", ForeColor = Color.White, Location = new Point(15, 128), AutoSize = true });
            txtUnitPrice = new TextBox { Location = new Point(125, 125), Width = 170 };
            grpAdd.Controls.Add(txtUnitPrice);

            Button btnAddCart = new Button { Text = "Add to Cart", Location = new Point(15, 175), Width = 280, Height = 45, BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAddCart.Click += (s, e) => AddToCart();
            grpAdd.Controls.Add(btnAddCart);

            GroupBox grpMeta = new GroupBox { Text = "Transaction Details", ForeColor = Color.White, Font = new Font("Segoe UI", 11), Location = new Point(20, 320), Size = new Size(320, 150) };

            grpMeta.Controls.Add(new Label { Text = "Invoice #:", ForeColor = Color.White, Location = new Point(15, 42), AutoSize = true });
            txtInvoiceNum = new TextBox { Location = new Point(125, 39), Width = 170, Text = "INV-" + DateTime.Now.ToString("fff") };
            grpMeta.Controls.Add(txtInvoiceNum);

            grpMeta.Controls.Add(new Label { Text = "Cust ID:", ForeColor = Color.White, Location = new Point(15, 85), AutoSize = true });
            txtCustomerId = new TextBox { Location = new Point(125, 82), Width = 170, Text = "1" };
            grpMeta.Controls.Add(txtCustomerId);

            lblTotalAmount = new Label { Text = "Total: $0.00", ForeColor = Color.LightGreen, Location = new Point(20, 490), Font = new Font("Segoe UI", 20, FontStyle.Bold), AutoSize = true };

            Button btnCheckout = new Button { Text = "Process Checkout", Location = new Point(20, 545), Width = 320, Height = 60, BackColor = Color.SeaGreen, ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnCheckout.Click += async (s, e) => await CompleteSaleAsync();

            pnlCheckout.Controls.AddRange(new Control[] { grpAdd, grpMeta, lblTotalAmount, btnCheckout });

            viewSales.Controls.Add(pnlCheckout);
            viewSales.Controls.Add(pnlCart);
        }

        // ==========================================
        // MODULE 2: INVENTORY
        // ==========================================
        private void BuildInventoryView()
        {
            viewInventory = new Panel { Size = new Size(1060, 800), Padding = new Padding(20) };

            Label lblTitle = new Label { Text = "Inventory & Product Management", ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };

            dgvInventory = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(1020, 310),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                BackgroundColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.Black,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = false
            };

            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "InventoryId", HeaderText = "Inv ID", Width = 70 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductId", HeaderText = "Prod ID", Width = 70 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Product Name", Width = 200 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductCode", HeaderText = "Product Code", Width = 130 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UnitPrice", HeaderText = "Unit Price", Width = 100, DefaultCellStyle = { Format = "C2" } });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "QuantityOnHand", HeaderText = "Stock Qty", Width = 100 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ReorderLevel", HeaderText = "Reorder Lvl", Width = 100 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "IsLowStock", HeaderText = "Low Stock?", Width = 90 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "LastUpdatedAt", HeaderText = "Last Updated", Width = 140 });

            dgvInventory.CellClick += (s, e) =>
            {
                if (dgvInventory.CurrentRow != null && dgvInventory.CurrentRow.DataBoundItem is InventoryViewDto item)
                {
                    txtAdjustProductId.Text = item.ProductId.ToString();
                    txtProdCode.Text = item.ProductCode;
                    txtProdName.Text = item.ProductName;
                    txtProdPrice.Text = item.UnitPrice.ToString("F2");
                    txtAdjustReorder.Text = item.ReorderLevel.ToString("F2");
                    txtAdjustQty.Text = "0";
                }
            };

            GroupBox grpManage = new GroupBox
            {
                Text = "Product Details (Click a row in the grid to edit)",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 385),
                Size = new Size(1020, 195),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            txtAdjustProductId = new TextBox { Visible = false };
            grpManage.Controls.Add(txtAdjustProductId);

            grpManage.Controls.Add(new Label { Text = "Code:", ForeColor = Color.LightGray, Location = new Point(100, 38), AutoSize = true });
            txtProdCode = new TextBox { Location = new Point(155, 35), Width = 140 };

            grpManage.Controls.Add(new Label { Text = "Name:", ForeColor = Color.LightGray, Location = new Point(315, 38), AutoSize = true });
            txtProdName = new TextBox { Location = new Point(375, 35), Width = 210 };

            grpManage.Controls.Add(new Label { Text = "Price $:", ForeColor = Color.LightGray, Location = new Point(605, 38), AutoSize = true });
            txtProdPrice = new TextBox { Location = new Point(675, 35), Width = 90 };

            grpManage.Controls.Add(new Label { Text = "Reorder:", ForeColor = Color.LightGray, Location = new Point(785, 38), AutoSize = true });
            txtAdjustReorder = new TextBox { Location = new Point(860, 35), Width = 80, Text = "5" };

            grpManage.Controls.AddRange(new Control[] { txtProdCode, txtProdName, txtProdPrice, txtAdjustReorder });

            Button btnCreateProd = new Button { Text = "Add New Product", Location = new Point(20, 95), Width = 150, Height = 45, BackColor = Color.SeaGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnCreateProd.Click += async (s, e) => await CreateProductAsync();

            Button btnUpdateProd = new Button { Text = "Update Selected", Location = new Point(180, 95), Width = 150, Height = 45, BackColor = Color.DodgerBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnUpdateProd.Click += async (s, e) => await UpdateProductAsync();

            Button btnDeleteProd = new Button { Text = "Delete Selected", Location = new Point(340, 95), Width = 150, Height = 45, BackColor = Color.IndianRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDeleteProd.Click += async (s, e) => await DeleteProductAsync();

            Panel pnlStock = new Panel { Location = new Point(520, 90), Size = new Size(480, 55), BackColor = Color.FromArgb(40, 40, 40) };
            pnlStock.Controls.Add(new Label { Text = "Stock Adjust (+/-):", ForeColor = Color.White, Location = new Point(15, 18), AutoSize = true });
            txtAdjustQty = new TextBox { Location = new Point(165, 15), Width = 80, Text = "0" };
            Button btnAdjust = new Button { Text = "Apply Stock", Location = new Point(255, 13), Width = 210, Height = 30, BackColor = Color.DarkOrange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAdjust.Click += async (s, e) => await AdjustStockAsync();
            pnlStock.Controls.AddRange(new Control[] { txtAdjustQty, btnAdjust });

            grpManage.Controls.AddRange(new Control[] { btnCreateProd, btnUpdateProd, btnDeleteProd, pnlStock });
            viewInventory.Controls.AddRange(new Control[] { lblTitle, dgvInventory, grpManage });
        }

        // ==========================================
        // MODULE 3: TRANSACTION HISTORY & ANALYTICS
        // ==========================================
        private void BuildReportsView()
        {
            viewReports = new Panel { Size = new Size(1060, 800), Padding = new Padding(20) };

            Label lblTitle = new Label { Text = "Transaction History & Business Analytics", ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };

            Button btnRefresh = new Button { Text = "🔄  Refresh Data", Location = new Point(880, 15), Width = 140, Height = 35, BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnRefresh.Click += async (s, e) => await LoadReportsAsync();

            // Interactive Summary Cards
            Panel cardRevenue = CreateDataCard("Total Revenue", out lblRevenueValue, new Point(20, 70), Color.DarkSlateBlue, (s, e) => LoadReportContext("Revenue & Sales Summary"));
            Panel cardTrans = CreateDataCard("Total Transactions", out lblTransValue, new Point(350, 70), Color.Teal, (s, e) => LoadReportContext("Transaction Logs"));
            Panel cardTopProd = CreateDataCard("Top Products", out lblTopProdValue, new Point(680, 70), Color.Sienna, (s, e) => LoadReportContext("Top Selling Products (Top 5)"));

            lblReportContext = new Label { Text = "Detailed View: Top Selling Products (Top 5)", ForeColor = Color.LightGray, Font = new Font("Segoe UI", 11, FontStyle.Italic), AutoSize = true, Location = new Point(20, 200) };

            dgvReportDetails = new DataGridView
            {
                Location = new Point(20, 240),
                Size = new Size(1020, 430),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.Black
            };

            viewReports.Controls.AddRange(new Control[] { lblTitle, btnRefresh, cardRevenue, cardTrans, cardTopProd, lblReportContext, dgvReportDetails });
        }

        private Panel CreateDataCard(string title, out Label valLabel, Point loc, Color stripColor, EventHandler onClick)
        {
            Panel card = new Panel { Size = new Size(300, 110), Location = loc, BackColor = Color.FromArgb(35, 35, 35), Cursor = Cursors.Hand };
            Panel strip = new Panel { Size = new Size(5, 110), Dock = DockStyle.Left, BackColor = stripColor };

            Label lblTitle = new Label { Text = title, ForeColor = Color.LightGray, Font = new Font("Segoe UI", 10), Location = new Point(20, 15), AutoSize = true };
            valLabel = new Label { Text = "-", ForeColor = Color.White, Font = new Font("Segoe UI", 20, FontStyle.Bold), Location = new Point(18, 45), AutoSize = true };

            card.Click += onClick;
            strip.Click += onClick;
            lblTitle.Click += onClick;
            valLabel.Click += onClick;

            card.Controls.AddRange(new Control[] { strip, lblTitle, valLabel });
            return card;
        }

        private void LoadReportContext(string context)
        {
            lblReportContext.Text = $"Detailed View: {context}";
            if (_currentReport == null) return;

            dgvReportDetails.DataSource = null;
            if (context.Contains("Top Products"))
            {
                dgvReportDetails.DataSource = _currentReport.TopSellingProducts;
            }
            else
            {
                dgvReportDetails.DataSource = _currentReport.TopSellingProducts;
            }
        }

        // ==========================================
        // API CALLS & LOGIC HANDLERS
        // ==========================================
        private async Task CreateProductAsync()
        {
            if (string.IsNullOrWhiteSpace(txtProdCode.Text) || string.IsNullOrWhiteSpace(txtProdName.Text) || !decimal.TryParse(txtProdPrice.Text, out decimal price))
            {
                MessageBox.Show("Please enter valid product details.", "Validation Error"); return;
            }
            var response = await _httpClient.PostAsJsonAsync($"tenant/{_currentCompanyId}/products", new { productCode = txtProdCode.Text.Trim(), productName = txtProdName.Text.Trim(), unitPrice = price, isActive = true });
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Product created successfully!");
                txtProdCode.Clear(); txtProdName.Clear(); txtProdPrice.Clear();
                await LoadInventoryAsync();
            }
        }

        private async Task UpdateProductAsync()
        {
            if (string.IsNullOrWhiteSpace(txtAdjustProductId.Text) || !int.TryParse(txtAdjustProductId.Text, out int prodId))
            {
                MessageBox.Show("Please select a product from the grid first.", "Notice"); return;
            }
            if (!decimal.TryParse(txtProdPrice.Text, out decimal price) || !decimal.TryParse(txtAdjustReorder.Text, out decimal reorder)) return;

            var response = await _httpClient.PutAsJsonAsync($"tenant/{_currentCompanyId}/products/{prodId}?reorderLevel={reorder}",
                new { productCode = txtProdCode.Text.Trim(), productName = txtProdName.Text.Trim(), unitPrice = price });

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Product updated successfully!");
                await LoadInventoryAsync();
            }
        }

        private async Task DeleteProductAsync()
        {
            if (string.IsNullOrWhiteSpace(txtAdjustProductId.Text) || !int.TryParse(txtAdjustProductId.Text, out int prodId))
            {
                MessageBox.Show("Please select a product from the grid first.", "Notice"); return;
            }

            if (MessageBox.Show("Are you sure you want to delete this product?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                var response = await _httpClient.DeleteAsync($"tenant/{_currentCompanyId}/products/{prodId}");
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Product deleted.");
                    txtProdCode.Clear(); txtProdName.Clear(); txtProdPrice.Clear(); txtAdjustProductId.Clear();
                    await LoadInventoryAsync();
                }
            }
        }

        private async Task AdjustStockAsync()
        {
            if (!int.TryParse(txtAdjustProductId.Text, out int prodId) || !decimal.TryParse(txtAdjustQty.Text, out decimal qty) || !decimal.TryParse(txtAdjustReorder.Text, out decimal reorder))
            {
                MessageBox.Show("Please select a product and enter valid inventory values.", "Validation Error"); return;
            }
            var response = await _httpClient.PostAsync($"tenant/{_currentCompanyId}/inventory/adjust?productId={prodId}&quantity={qty}&reorderLevel={reorder}", null);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Stock adjusted successfully!");
                txtAdjustQty.Text = "0";
                await LoadInventoryAsync();
            }
        }

        private async Task LoadInventoryAsync()
        {
            try
            {
                var inventory = await _httpClient.GetFromJsonAsync<List<InventoryViewDto>>($"tenant/{_currentCompanyId}/inventory");
                dgvInventory.DataSource = inventory;
                foreach (DataGridViewRow row in dgvInventory.Rows)
                {
                    if (row.DataBoundItem is InventoryViewDto item && item.IsLowStock)
                        row.DefaultCellStyle.BackColor = Color.LightCoral;
                }
            }
            catch { }
        }

        private void AddToCart()
        {
            if (!int.TryParse(txtSaleProductId.Text, out int prodId) || !decimal.TryParse(txtSaleQty.Text, out decimal qty) || !decimal.TryParse(txtUnitPrice.Text, out decimal price))
            {
                MessageBox.Show("Please enter valid numeric sale items.", "Validation Error"); return;
            }
            _cart.Add(new CartItemDto { ProductId = prodId, Quantity = qty, UnitPrice = price, SubTotal = qty * price });
            dgvCart.DataSource = null;
            dgvCart.DataSource = _cart;
            lblTotalAmount.Text = $"Total: ${_cart.Sum(x => x.SubTotal):F2}";
        }

        private async Task CompleteSaleAsync()
        {
            if (!_cart.Any()) return;
            int.TryParse(txtCustomerId.Text, out int customerId);

            var salePayload = new SaleRequestDto
            {
                InvoiceNumber = string.IsNullOrWhiteSpace(txtInvoiceNum.Text) ? "INV-" + DateTime.Now.ToString("fff") : txtInvoiceNum.Text,
                CustomerId = customerId > 0 ? customerId : null,
                SaleItems = _cart
            };

            var response = await _httpClient.PostAsJsonAsync($"tenant/{_currentCompanyId}/sales", salePayload);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Sale processed successfully!", "Success");
                _cart.Clear(); dgvCart.DataSource = null;
                lblTotalAmount.Text = "Total: $0.00";
                txtInvoiceNum.Text = "INV-" + DateTime.Now.ToString("fff");
                await RefreshAllDataAsync();
            }
            else
            {
                MessageBox.Show($"Transaction failed: {await response.Content.ReadAsStringAsync()}", "Error");
            }
        }

        private async Task LoadReportsAsync()
        {
            try
            {
                _currentReport = await _httpClient.GetFromJsonAsync<SalesSummaryReportDto>($"tenant/{_currentCompanyId}/reports/sales-summary");
                if (_currentReport != null)
                {
                    lblRevenueValue.Text = $"${_currentReport.TotalRevenue:F2}";
                    lblTransValue.Text = $"{_currentReport.TotalTransactions}";
                    lblTopProdValue.Text = $"{_currentReport.TopSellingProducts.Count} Items";
                    LoadReportContext("Top Selling Products (Top 5)");
                }
            }
            catch { }
        }

        private async Task RefreshAllDataAsync()
        {
            await LoadInventoryAsync();
            await LoadReportsAsync();
        }
    }

    public class InventoryViewDto
    {
        public int InventoryId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal QuantityOnHand { get; set; }
        public decimal ReorderLevel { get; set; }
        public bool IsLowStock { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }

    public class CartItemDto
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }
    }

    public class SaleRequestDto
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public List<CartItemDto> SaleItems { get; set; } = new List<CartItemDto>();
    }

    public class SalesSummaryReportDto
    {
        public int TenantId { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalTransactions { get; set; }
        public List<TopProductDto> TopSellingProducts { get; set; } = new List<TopProductDto>();
    }

    public class TopProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}