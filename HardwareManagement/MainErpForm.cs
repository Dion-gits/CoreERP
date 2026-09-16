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

        // Modern Theme Palette
        private readonly Color BgDark = Color.FromArgb(18, 18, 18);
        private readonly Color CardBg = Color.FromArgb(28, 28, 28);
        private readonly Color BorderColor = Color.FromArgb(45, 45, 45);
        private readonly Color AccentGreen = Color.FromArgb(16, 185, 129);
        private readonly Color AccentBlue = Color.FromArgb(59, 130, 246);
        private readonly Color TextPrimary = Color.FromArgb(243, 244, 246);
        private readonly Color TextMuted = Color.FromArgb(156, 163, 175);

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
            this.Text = "Micro-Enterprise ERP - Modern Core";
            this.Size = new Size(1320, 820);
            this.MinimumSize = new Size(1200, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = BgDark;
            this.DoubleBuffered = true;

            // ==========================================
            // 1. MINIMALIST SIDEBAR 
            // ==========================================
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 240,
                BackColor = CardBg
            };

            Label lblAppTitle = new Label
            {
                Text = "⚡ CORE ERP",
                ForeColor = TextPrimary,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Location = new Point(20, 24),
                AutoSize = true
            };
            pnlSidebar.Controls.Add(lblAppTitle);

            btnNavSales = CreateSidebarButton("🛒  Point of Sale", 85);
            btnNavInventory = CreateSidebarButton("📦  Inventory Hub", 140);
            btnNavReports = CreateSidebarButton("📊  Transaction History", 195);

            btnNavSales.Click += (s, e) => SwitchView(viewSales, btnNavSales);
            btnNavInventory.Click += (s, e) => SwitchView(viewInventory, btnNavInventory);
            btnNavReports.Click += (s, e) => SwitchView(viewReports, btnNavReports);

            btnLogout = new Button
            {
                Text = "🚪  Sign Out",
                Dock = DockStyle.Bottom,
                Height = 50,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(239, 68, 68),
                BackColor = CardBg,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
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
                else { Application.Exit(); }
            };

            pnlSidebar.Controls.Add(btnLogout);
            pnlSidebar.Controls.AddRange(new Control[] { btnNavSales, btnNavInventory, btnNavReports });

            // ==========================================
            // 2. MAIN CONTENT AREA 
            // ==========================================
            pnlMainContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BgDark
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
                Location = new Point(12, top),
                Size = new Size(216, 45),
                FlatStyle = FlatStyle.Flat,
                ForeColor = TextMuted,
                BackColor = CardBg,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
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
                    btn.BackColor = CardBg;
                    btn.ForeColor = TextMuted;
                }
            }

            activeBtn.BackColor = Color.FromArgb(40, 40, 40);
            activeBtn.ForeColor = TextPrimary;
        }

        // ==========================================
        // MODULE 1: MODERN POS / SALES VIEW
        // ==========================================
        private void BuildSalesView()
        {
            viewSales = new Panel { Padding = new Padding(24) };

            Panel pnlCartCard = new Panel
            {
                Location = new Point(24, 24),
                Size = new Size(680, 710),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
                BackColor = CardBg
            };

            Label lblCartTitle = new Label { Text = "Active Cart Items", ForeColor = TextPrimary, Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };

            dgvCart = new DataGridView
            {
                Location = new Point(20, 65),
                Size = new Size(640, 620),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = CardBg,
                ForeColor = TextPrimary,
                GridColor = BorderColor,
                BorderStyle = BorderStyle.None,
                RowTemplate = { Height = 38 },
                EnableHeadersVisualStyles = false
            };
            dgvCart.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(35, 35, 35);
            dgvCart.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary;
            dgvCart.DefaultCellStyle.BackColor = CardBg;
            dgvCart.DefaultCellStyle.ForeColor = TextPrimary;
            dgvCart.DefaultCellStyle.SelectionBackColor = Color.FromArgb(50, 50, 50);

            pnlCartCard.Controls.AddRange(new Control[] { lblCartTitle, dgvCart });

            Panel pnlCheckoutCard = new Panel
            {
                Location = new Point(728, 24),
                Size = new Size(350, 710),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = CardBg
            };

            GroupBox grpAdd = new GroupBox { Text = "Add Item via ID", ForeColor = TextMuted, Font = new Font("Segoe UI", 10), Location = new Point(20, 20), Size = new Size(310, 260) };

            grpAdd.Controls.Add(new Label { Text = "Product ID:", ForeColor = TextPrimary, Location = new Point(15, 38), AutoSize = true });
            txtSaleProductId = new TextBox { Location = new Point(115, 35), Width = 175, BackColor = BgDark, ForeColor = TextPrimary };
            grpAdd.Controls.Add(txtSaleProductId);

            grpAdd.Controls.Add(new Label { Text = "Quantity:", ForeColor = TextPrimary, Location = new Point(15, 82), AutoSize = true });
            txtSaleQty = new TextBox { Location = new Point(115, 79), Width = 175, Text = "1", BackColor = BgDark, ForeColor = TextPrimary };
            grpAdd.Controls.Add(txtSaleQty);

            grpAdd.Controls.Add(new Label { Text = "Unit Price:", ForeColor = TextPrimary, Location = new Point(15, 126), AutoSize = true });
            txtUnitPrice = new TextBox { Location = new Point(115, 123), Width = 175, BackColor = BgDark, ForeColor = TextPrimary };
            grpAdd.Controls.Add(txtUnitPrice);

            Button btnAddCart = new Button { Text = "Add to Cart", Location = new Point(15, 180), Width = 275, Height = 42, BackColor = AccentBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnAddCart.FlatAppearance.BorderSize = 0;
            btnAddCart.Click += (s, e) => AddToCart();
            grpAdd.Controls.Add(btnAddCart);

            GroupBox grpMeta = new GroupBox { Text = "Transaction Details", ForeColor = TextMuted, Font = new Font("Segoe UI", 10), Location = new Point(20, 295), Size = new Size(310, 150) };

            grpMeta.Controls.Add(new Label { Text = "Invoice #:", ForeColor = TextPrimary, Location = new Point(15, 42), AutoSize = true });
            txtInvoiceNum = new TextBox { Location = new Point(115, 39), Width = 175, Text = "INV-" + DateTime.Now.ToString("fff"), BackColor = BgDark, ForeColor = TextPrimary };
            grpMeta.Controls.Add(txtInvoiceNum);

            grpMeta.Controls.Add(new Label { Text = "Cust ID:", ForeColor = TextPrimary, Location = new Point(15, 86), AutoSize = true });
            txtCustomerId = new TextBox { Location = new Point(115, 83), Width = 175, Text = "1", BackColor = BgDark, ForeColor = TextPrimary };
            grpMeta.Controls.Add(txtCustomerId);

            lblTotalAmount = new Label { Text = "Total: $0.00", ForeColor = AccentGreen, Location = new Point(25, 470), Font = new Font("Segoe UI", 18, FontStyle.Bold), AutoSize = true };

            Button btnCheckout = new Button { Text = "Process Checkout", Location = new Point(20, 530), Width = 310, Height = 55, BackColor = AccentGreen, ForeColor = Color.White, Font = new Font("Segoe UI", 11, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnCheckout.FlatAppearance.BorderSize = 0;
            btnCheckout.Click += async (s, e) => await CompleteSaleAsync();

            pnlCheckoutCard.Controls.AddRange(new Control[] { grpAdd, grpMeta, lblTotalAmount, btnCheckout });

            viewSales.Controls.AddRange(new Control[] { pnlCartCard, pnlCheckoutCard });
        }

        // ==========================================
        // MODULE 2: INVENTORY HUB VIEW
        // ==========================================
        private void BuildInventoryView()
        {
            viewInventory = new Panel { Padding = new Padding(24) };

            Label lblTitle = new Label { Text = "Inventory & Stock Hub", ForeColor = TextPrimary, Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, Location = new Point(24, 24) };

            dgvInventory = new DataGridView
            {
                Location = new Point(24, 70),
                Size = new Size(1034, 320),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                BackgroundColor = CardBg,
                ForeColor = TextPrimary,
                GridColor = BorderColor,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = false,
                EnableHeadersVisualStyles = false
            };
            dgvInventory.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(35, 35, 35);
            dgvInventory.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary;
            dgvInventory.DefaultCellStyle.BackColor = CardBg;
            dgvInventory.DefaultCellStyle.ForeColor = TextPrimary;
            dgvInventory.DefaultCellStyle.SelectionBackColor = Color.FromArgb(50, 50, 50);

            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "InventoryId", HeaderText = "Inv ID", Width = 70 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductId", HeaderText = "Prod ID", Width = 70 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Product Name", Width = 220 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductCode", HeaderText = "Code", Width = 130 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UnitPrice", HeaderText = "Unit Price", Width = 110, DefaultCellStyle = { Format = "C2" } });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "QuantityOnHand", HeaderText = "Stock Qty", Width = 100 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ReorderLevel", HeaderText = "Reorder", Width = 100 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "IsLowStock", HeaderText = "Low Stock?", Width = 100 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "LastUpdatedAt", HeaderText = "Last Updated", Width = 150 });

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
                Text = "Product Management",
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 10),
                Location = new Point(24, 410),
                Size = new Size(1034, 210),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            txtAdjustProductId = new TextBox { Visible = false };
            grpManage.Controls.Add(txtAdjustProductId);

            grpManage.Controls.Add(new Label { Text = "Code:", ForeColor = TextMuted, Location = new Point(25, 38), AutoSize = true });
            txtProdCode = new TextBox { Location = new Point(75, 35), Width = 130, BackColor = BgDark, ForeColor = TextPrimary };

            grpManage.Controls.Add(new Label { Text = "Name:", ForeColor = TextMuted, Location = new Point(225, 38), AutoSize = true });
            txtProdName = new TextBox { Location = new Point(275, 35), Width = 220, BackColor = BgDark, ForeColor = TextPrimary };

            grpManage.Controls.Add(new Label { Text = "Price:", ForeColor = TextMuted, Location = new Point(515, 38), AutoSize = true });
            txtProdPrice = new TextBox { Location = new Point(560, 35), Width = 90, BackColor = BgDark, ForeColor = TextPrimary };

            grpManage.Controls.Add(new Label { Text = "Reorder:", ForeColor = TextMuted, Location = new Point(670, 38), AutoSize = true });
            txtAdjustReorder = new TextBox { Location = new Point(735, 35), Width = 80, Text = "5", BackColor = BgDark, ForeColor = TextPrimary };

            grpManage.Controls.AddRange(new Control[] { txtProdCode, txtProdName, txtProdPrice, txtAdjustReorder });

            Button btnCreateProd = new Button { Text = "Add New", Location = new Point(25, 95), Width = 130, Height = 42, BackColor = AccentGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnCreateProd.FlatAppearance.BorderSize = 0;
            btnCreateProd.Click += async (s, e) => await CreateProductAsync();

            Button btnUpdateProd = new Button { Text = "Update", Location = new Point(165, 95), Width = 130, Height = 42, BackColor = AccentBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnUpdateProd.FlatAppearance.BorderSize = 0;
            btnUpdateProd.Click += async (s, e) => await UpdateProductAsync();

            Button btnDeleteProd = new Button { Text = "Delete", Location = new Point(305, 95), Width = 130, Height = 42, BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnDeleteProd.FlatAppearance.BorderSize = 0;
            btnDeleteProd.Click += async (s, e) => await DeleteProductAsync();

            Panel pnlStock = new Panel { Location = new Point(460, 90), Size = new Size(545, 52), BackColor = BgDark };
            pnlStock.Controls.Add(new Label { Text = "Stock Adjust (+/-):", ForeColor = TextPrimary, Location = new Point(15, 16), AutoSize = true });
            txtAdjustQty = new TextBox { Location = new Point(150, 13), Width = 80, Text = "0", BackColor = CardBg, ForeColor = TextPrimary };
            Button btnAdjust = new Button { Text = "Apply Stock Change", Location = new Point(245, 11), Width = 280, Height = 32, BackColor = Color.FromArgb(217, 119, 6), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnAdjust.FlatAppearance.BorderSize = 0;
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
            viewReports = new Panel { Padding = new Padding(24) };

            Label lblTitle = new Label { Text = "Transaction History & Analytics", ForeColor = TextPrimary, Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, Location = new Point(24, 24) };

            Button btnRefresh = new Button { Text = "🔄 Refresh", Location = new Point(918, 20), Size = new Size(140, 38), BackColor = CardBg, ForeColor = TextPrimary, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnRefresh.FlatAppearance.BorderColor = BorderColor;
            btnRefresh.Click += async (s, e) => await LoadReportsAsync();

            Panel cardRevenue = CreateDataCard("Total Revenue", out lblRevenueValue, new Point(24, 75), AccentBlue, (s, e) => LoadReportContext("Revenue & Sales Summary"));
            Panel cardTrans = CreateDataCard("Total Transactions", out lblTransValue, new Point(360, 75), AccentGreen, (s, e) => LoadReportContext("Transaction Logs"));
            Panel cardTopProd = CreateDataCard("Top Catalog Items", out lblTopProdValue, new Point(696, 75), Color.FromArgb(217, 119, 6), (s, e) => LoadReportContext("Top Selling Products"));

            lblReportContext = new Label { Text = "Detailed View: Top Selling Products", ForeColor = TextMuted, Font = new Font("Segoe UI", 11, FontStyle.Italic), AutoSize = true, Location = new Point(24, 205) };

            dgvReportDetails = new DataGridView
            {
                Location = new Point(24, 240),
                Size = new Size(1034, 490),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = CardBg,
                ForeColor = TextPrimary,
                GridColor = BorderColor,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false
            };
            dgvReportDetails.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(35, 35, 35);
            dgvReportDetails.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary;
            dgvReportDetails.DefaultCellStyle.BackColor = CardBg;
            dgvReportDetails.DefaultCellStyle.ForeColor = TextPrimary;

            viewReports.Controls.AddRange(new Control[] { lblTitle, btnRefresh, cardRevenue, cardTrans, cardTopProd, lblReportContext, dgvReportDetails });
        }

        private Panel CreateDataCard(string title, out Label valLabel, Point loc, Color stripColor, EventHandler onClick)
        {
            Panel card = new Panel { Size = new Size(312, 110), Location = loc, BackColor = CardBg, Cursor = Cursors.Hand };
            Panel strip = new Panel { Size = new Size(4, 110), Dock = DockStyle.Left, BackColor = stripColor };

            Label lblTitle = new Label { Text = title, ForeColor = TextMuted, Font = new Font("Segoe UI", 10), Location = new Point(20, 18), AutoSize = true };
            valLabel = new Label { Text = "-", ForeColor = TextPrimary, Font = new Font("Segoe UI", 20, FontStyle.Bold), Location = new Point(18, 48), AutoSize = true };

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
            dgvReportDetails.DataSource = _currentReport.TopSellingProducts;
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
                MessageBox.Show("Sale processed and committed to database successfully!", "Success");
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
                    LoadReportContext("Top Selling Products");
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