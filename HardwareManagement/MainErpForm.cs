using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
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
        private SalesSummaryReportDto? _currentReport;

        // Modern Minimalist Palette (Slate & Emerald Theme)
        private readonly Color BgDark = Color.FromArgb(15, 23, 42);          // Slate 900
        private readonly Color SidebarBg = Color.FromArgb(30, 41, 59);        // Slate 800
        private readonly Color CardBg = Color.FromArgb(30, 41, 59);           // Slate 800
        private readonly Color InputBg = Color.FromArgb(15, 23, 42);          // Slate 900
        private readonly Color BorderColor = Color.FromArgb(51, 65, 85);       // Slate 700
        private readonly Color AccentGreen = Color.FromArgb(16, 185, 129);     // Emerald 500
        private readonly Color AccentBlue = Color.FromArgb(59, 130, 246);      // Blue 500
        private readonly Color AccentAmber = Color.FromArgb(245, 158, 11);     // Amber 500
        private readonly Color AccentRed = Color.FromArgb(239, 68, 68);        // Red 500
        private readonly Color TextPrimary = Color.FromArgb(248, 250, 252);    // Slate 50
        private readonly Color TextMuted = Color.FromArgb(148, 163, 184);      // Slate 400

        // Layout Containers
        private Panel pnlSidebar = null!;
        private Panel pnlMainContent = null!;
        private Label lblHeaderTitle = null!;
        private Label lblTenantBadge = null!;

        // Navigation Buttons
        private Button btnNavSales = null!;
        private Button btnNavInventory = null!;
        private Button btnNavReports = null!;
        private Button btnLogout = null!;

        // View Panels
        private Panel viewSales = null!;
        private Panel viewInventory = null!;
        private Panel viewReports = null!;

        // Inventory Controls
        private DataGridView dgvInventory = null!;
        private TextBox txtProdCode = null!, txtProdName = null!, txtProdPrice = null!;
        private TextBox txtAdjustProductId = null!, txtAdjustQty = null!, txtAdjustReorder = null!;

        // Sales Controls
        private DataGridView dgvCart = null!;
        private TextBox txtInvoiceNum = null!, txtCustomerId = null!, txtSaleProductId = null!, txtSaleQty = null!, txtUnitPrice = null!;
        private Label lblTotalAmount = null!;

        // Transaction History & Reports Controls
        private Label lblRevenueValue = null!, lblTransValue = null!, lblTopProdValue = null!;
        private DataGridView dgvReportDetails = null!;
        private Label lblReportContext = null!;

        public MainErpForm(int companyId = 1)
        {
            _currentCompanyId = companyId;
            InitializeComponentCustom();
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7166/") };
        }

        private void InitializeComponentCustom()
        {
            this.Text = "CORE ERP - Micro-Enterprise Platform";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1024, 700);
            this.BackColor = BgDark;
            this.DoubleBuffered = true;

            // ==========================================
            // 1. MINIMALIST SIDEBAR
            // ==========================================
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 260,
                BackColor = SidebarBg
            };

            // Top Brand Header Panel
            Panel pnlBrand = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(24, 20, 24, 0)
            };

            Label lblAppTitle = new Label
            {
                Text = "⚡ CORE ERP",
                ForeColor = TextPrimary,
                Font = new Font("Segoe UI Semibold", 16, FontStyle.Bold),
                Dock = DockStyle.Top,
                AutoSize = true
            };

            lblTenantBadge = new Label
            {
                Text = $"Tenant Workspace #{_currentCompanyId}",
                ForeColor = AccentGreen,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Dock = DockStyle.Top,
                AutoSize = true
            };

            pnlBrand.Controls.Add(lblTenantBadge);
            pnlBrand.Controls.Add(lblAppTitle);

            // Nav Container Panel
            Panel pnlNavContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 16, 12, 16)
            };

            btnNavSales = CreateSidebarButton("🛒  Point of Sale", 0);
            btnNavInventory = CreateSidebarButton("📦  Inventory Hub", 54);
            btnNavReports = CreateSidebarButton("📊  Analytics & History", 108);

            btnNavSales.Click += (s, e) => SwitchView(viewSales, btnNavSales, "Point of Sale (POS)");
            btnNavInventory.Click += (s, e) => SwitchView(viewInventory, btnNavInventory, "Inventory Hub & Stock Management");
            btnNavReports.Click += (s, e) => SwitchView(viewReports, btnNavReports, "Transaction History & Analytics");

            pnlNavContainer.Controls.AddRange(new Control[] { btnNavSales, btnNavInventory, btnNavReports });

            // Bottom Logout Button
            btnLogout = new Button
            {
                Text = "🚪  Sign Out",
                Dock = DockStyle.Bottom,
                Height = 55,
                FlatStyle = FlatStyle.Flat,
                ForeColor = AccentRed,
                BackColor = SidebarBg,
                Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(24, 0, 0, 0),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
            btnLogout.MouseEnter += (s, e) => btnLogout.BackColor = Color.FromArgb(45, 55, 72);
            btnLogout.MouseLeave += (s, e) => btnLogout.BackColor = SidebarBg;

            btnLogout.Click += (s, e) =>
            {
                this.Hide();
                using var loginForm = new Login();
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    _currentCompanyId = loginForm.AuthenticatedCompanyId;
                    lblTenantBadge.Text = $"Tenant Workspace #{_currentCompanyId}";
                    this.Show();
                    _ = RefreshAllDataAsync();
                }
                else { Application.Exit(); }
            };

            pnlSidebar.Controls.Add(pnlNavContainer);
            pnlSidebar.Controls.Add(pnlBrand);
            pnlSidebar.Controls.Add(btnLogout);

            // ==========================================
            // 2. TOP HEADER BAR
            // ==========================================
            Panel pnlTopBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = SidebarBg,
                Padding = new Padding(24, 0, 24, 0)
            };

            lblHeaderTitle = new Label
            {
                Text = "Point of Sale (POS)",
                ForeColor = TextPrimary,
                Font = new Font("Segoe UI Semibold", 15, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(24, 18)
            };

            Label lblDateTime = new Label
            {
                Text = DateTime.Now.ToString("dddd, MMMM d, yyyy"),
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            lblDateTime.Location = new Point(pnlTopBar.Width - 220, 22);
            pnlTopBar.Resize += (s, e) => lblDateTime.Location = new Point(pnlTopBar.Width - 220, 22);

            pnlTopBar.Controls.AddRange(new Control[] { lblHeaderTitle, lblDateTime });

            // ==========================================
            // 3. MAIN CONTENT AREA
            // ==========================================
            pnlMainContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BgDark,
                Padding = new Padding(24)
            };

            BuildSalesView();
            BuildInventoryView();
            BuildReportsView();

            this.Controls.Add(pnlMainContent);
            this.Controls.Add(pnlTopBar);
            this.Controls.Add(pnlSidebar);

            SwitchView(viewSales, btnNavSales, "Point of Sale (POS)");
            this.Load += async (s, e) => await RefreshAllDataAsync();
        }

        private Button CreateSidebarButton(string text, int topPosition)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(0, topPosition),
                Size = new Size(236, 48),
                FlatStyle = FlatStyle.Flat,
                ForeColor = TextMuted,
                BackColor = SidebarBg,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 0, 0),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };

            btn.MouseEnter += (s, e) =>
            {
                if (btn.BackColor != Color.FromArgb(15, 23, 42))
                    btn.BackColor = Color.FromArgb(45, 55, 72);
            };
            btn.MouseLeave += (s, e) =>
            {
                if (btn.BackColor != Color.FromArgb(15, 23, 42))
                    btn.BackColor = SidebarBg;
            };

            return btn;
        }

        private void SwitchView(Panel targetView, Button activeBtn, string headerTitle)
        {
            pnlMainContent.Controls.Clear();
            targetView.Dock = DockStyle.Fill;
            pnlMainContent.Controls.Add(targetView);
            lblHeaderTitle.Text = headerTitle;

            if (btnNavSales.Parent != null)
            {
                foreach (Control c in btnNavSales.Parent.Controls)
                {
                    if (c is Button btn)
                    {
                        btn.BackColor = SidebarBg;
                        btn.ForeColor = TextMuted;
                        btn.Font = new Font("Segoe UI", 10, FontStyle.Regular);
                    }
                }
            }

            activeBtn.BackColor = BgDark;
            activeBtn.ForeColor = AccentGreen;
            activeBtn.Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold);
        }

        private void ApplyDataGridStyle(DataGridView dgv)
        {
            dgv.BackgroundColor = CardBg;
            dgv.ForeColor = TextPrimary;
            dgv.GridColor = BorderColor;
            dgv.BorderStyle = BorderStyle.None;
            dgv.RowHeadersVisible = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.RowTemplate.Height = 42;
            dgv.EnableHeadersVisualStyles = false;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextMuted;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 45;

            dgv.DefaultCellStyle.BackColor = CardBg;
            dgv.DefaultCellStyle.ForeColor = TextPrimary;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(51, 65, 85);
            dgv.DefaultCellStyle.SelectionForeColor = TextPrimary;
            dgv.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
        }

        // ==========================================
        // MODULE 1: MODERN POS / SALES VIEW
        // ==========================================
        private void BuildSalesView()
        {
            viewSales = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0) };

            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));

            // Left Panel: Cart Grid Card
            Panel pnlCartCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg,
                Margin = new Padding(0, 0, 16, 0),
                Padding = new Padding(24)
            };

            Label lblCartTitle = new Label
            {
                Text = "Active Checkout Cart",
                ForeColor = TextPrimary,
                Font = new Font("Segoe UI Semibold", 13, FontStyle.Bold),
                Dock = DockStyle.Top,
                AutoSize = true
            };

            dgvCart = new DataGridView
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 16, 0, 0),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            ApplyDataGridStyle(dgvCart);

            pnlCartCard.Controls.Add(dgvCart);
            pnlCartCard.Controls.Add(lblCartTitle);

            // Right Panel: Form Actions & Total Card
            Panel pnlCheckoutCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg,
                Margin = new Padding(16, 0, 0, 0),
                Padding = new Padding(24),
                AutoScroll = true
            };

            // Section 1: Item Entry
            Label lblAddTitle = new Label
            {
                Text = "Add Product to Cart",
                ForeColor = TextPrimary,
                Font = new Font("Segoe UI Semibold", 12, FontStyle.Bold),
                Location = new Point(24, 20),
                AutoSize = true
            };

            Label lblProdId = new Label { Text = "Product ID", ForeColor = TextMuted, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(24, 60), AutoSize = true };
            txtSaleProductId = new TextBox { Location = new Point(24, 82), Width = 310, Font = new Font("Segoe UI", 11), BackColor = InputBg, ForeColor = TextPrimary, BorderStyle = BorderStyle.FixedSingle };

            Label lblQty = new Label { Text = "Quantity", ForeColor = TextMuted, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(24, 125), AutoSize = true };
            txtSaleQty = new TextBox { Location = new Point(24, 147), Width = 310, Text = "1", Font = new Font("Segoe UI", 11), BackColor = InputBg, ForeColor = TextPrimary, BorderStyle = BorderStyle.FixedSingle };

            Label lblPrice = new Label { Text = "Unit Price ($)", ForeColor = TextMuted, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(24, 190), AutoSize = true };
            txtUnitPrice = new TextBox { Location = new Point(24, 212), Width = 310, Font = new Font("Segoe UI", 11), BackColor = InputBg, ForeColor = TextPrimary, BorderStyle = BorderStyle.FixedSingle };

            Button btnAddCart = new Button
            {
                Text = "➕ Add Item to Cart",
                Location = new Point(24, 260),
                Width = 310,
                Height = 45,
                BackColor = AccentBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAddCart.FlatAppearance.BorderSize = 0;
            btnAddCart.Click += (s, e) => AddToCart();

            // Section 2: Invoice & Customer Info
            Label lblDivider = new Label { BorderStyle = BorderStyle.Fixed3D, Location = new Point(24, 325), Size = new Size(310, 2) };

            Label lblInvoice = new Label { Text = "Invoice Number", ForeColor = TextMuted, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(24, 345), AutoSize = true };
            txtInvoiceNum = new TextBox { Location = new Point(24, 367), Width = 310, Text = "INV-" + DateTime.Now.ToString("fff", CultureInfo.InvariantCulture), Font = new Font("Segoe UI", 11), BackColor = InputBg, ForeColor = TextPrimary, BorderStyle = BorderStyle.FixedSingle };

            Label lblCustId = new Label { Text = "Customer ID (Optional)", ForeColor = TextMuted, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(24, 410), AutoSize = true };
            txtCustomerId = new TextBox { Location = new Point(24, 432), Width = 310, Text = "1", Font = new Font("Segoe UI", 11), BackColor = InputBg, ForeColor = TextPrimary, BorderStyle = BorderStyle.FixedSingle };

            // Section 3: Summary & Checkout Button
            lblTotalAmount = new Label
            {
                Text = "Total Amount: $0.00",
                ForeColor = AccentGreen,
                Location = new Point(24, 485),
                Font = new Font("Segoe UI Semibold", 16, FontStyle.Bold),
                AutoSize = true
            };

            Button btnCheckout = new Button
            {
                Text = "💳 Process Checkout",
                Location = new Point(24, 530),
                Width = 310,
                Height = 55,
                BackColor = AccentGreen,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCheckout.FlatAppearance.BorderSize = 0;
            btnCheckout.Click += async (s, e) => await CompleteSaleAsync();

            pnlCheckoutCard.Controls.AddRange(new Control[] {
                lblAddTitle, lblProdId, txtSaleProductId, lblQty, txtSaleQty, lblPrice, txtUnitPrice, btnAddCart,
                lblDivider, lblInvoice, txtInvoiceNum, lblCustId, txtCustomerId, lblTotalAmount, btnCheckout
            });

            mainLayout.Controls.Add(pnlCartCard, 0, 0);
            mainLayout.Controls.Add(pnlCheckoutCard, 1, 0);

            viewSales.Controls.Add(mainLayout);
        }

        // ==========================================
        // MODULE 2: INVENTORY HUB VIEW
        // ==========================================
        private void BuildInventoryView()
        {
            viewInventory = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0) };

            TableLayoutPanel invLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0)
            };
            invLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            invLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));

            // Grid Container Card
            Panel pnlGridCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg,
                Margin = new Padding(0, 0, 0, 16),
                Padding = new Padding(24)
            };

            dgvInventory = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            ApplyDataGridStyle(dgvInventory);

            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "InventoryId", HeaderText = "Inv ID", FillWeight = 8 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductId", HeaderText = "Prod ID", FillWeight = 8 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Product Name", FillWeight = 28 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductCode", HeaderText = "Code", FillWeight = 16 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UnitPrice", HeaderText = "Unit Price", FillWeight = 14, DefaultCellStyle = { Format = "C2" } });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "QuantityOnHand", HeaderText = "Stock Qty", FillWeight = 12 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ReorderLevel", HeaderText = "Reorder", FillWeight = 12 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "IsLowStock", HeaderText = "Low Stock?", FillWeight = 12 });

            dgvInventory.CellClick += (s, e) =>
            {
                if (dgvInventory.CurrentRow != null && dgvInventory.CurrentRow.DataBoundItem is InventoryViewDto item)
                {
                    txtAdjustProductId.Text = item.ProductId.ToString(CultureInfo.InvariantCulture);
                    txtProdCode.Text = item.ProductCode;
                    txtProdName.Text = item.ProductName;
                    txtProdPrice.Text = item.UnitPrice.ToString("F2", CultureInfo.InvariantCulture);
                    txtAdjustReorder.Text = item.ReorderLevel.ToString("F2", CultureInfo.InvariantCulture);
                    txtAdjustQty.Text = "0";
                }
            };

            pnlGridCard.Controls.Add(dgvInventory);

            // Management Form Panel Card
            Panel pnlManageCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg,
                Margin = new Padding(0, 16, 0, 0),
                Padding = new Padding(24),
                AutoScroll = true
            };

            Label lblManageTitle = new Label { Text = "Product & Stock Actions", ForeColor = TextPrimary, Font = new Font("Segoe UI Semibold", 12, FontStyle.Bold), Location = new Point(24, 16), AutoSize = true };

            txtAdjustProductId = new TextBox { Visible = false };

            Label lblCode = new Label { Text = "Code", ForeColor = TextMuted, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(24, 52), AutoSize = true };
            txtProdCode = new TextBox { Location = new Point(24, 74), Width = 140, Font = new Font("Segoe UI", 10), BackColor = InputBg, ForeColor = TextPrimary, BorderStyle = BorderStyle.FixedSingle };

            Label lblName = new Label { Text = "Product Name", ForeColor = TextMuted, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(180, 52), AutoSize = true };
            txtProdName = new TextBox { Location = new Point(180, 74), Width = 260, Font = new Font("Segoe UI", 10), BackColor = InputBg, ForeColor = TextPrimary, BorderStyle = BorderStyle.FixedSingle };

            Label lblPrice = new Label { Text = "Unit Price", ForeColor = TextMuted, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(455, 52), AutoSize = true };
            txtProdPrice = new TextBox { Location = new Point(455, 74), Width = 110, Font = new Font("Segoe UI", 10), BackColor = InputBg, ForeColor = TextPrimary, BorderStyle = BorderStyle.FixedSingle };

            Label lblReorder = new Label { Text = "Reorder Level", ForeColor = TextMuted, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(580, 52), AutoSize = true };
            txtAdjustReorder = new TextBox { Location = new Point(580, 74), Width = 110, Text = "5", Font = new Font("Segoe UI", 10), BackColor = InputBg, ForeColor = TextPrimary, BorderStyle = BorderStyle.FixedSingle };

            Button btnCreateProd = new Button { Text = "✨ Create Product", Location = new Point(24, 120), Width = 140, Height = 42, BackColor = AccentGreen, ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnCreateProd.FlatAppearance.BorderSize = 0;
            btnCreateProd.Click += async (s, e) => await CreateProductAsync();

            Button btnUpdateProd = new Button { Text = "✏️ Update Details", Location = new Point(180, 120), Width = 140, Height = 42, BackColor = AccentBlue, ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnUpdateProd.FlatAppearance.BorderSize = 0;
            btnUpdateProd.Click += async (s, e) => await UpdateProductAsync();

            Button btnDeleteProd = new Button { Text = "🗑️ Delete Product", Location = new Point(336, 120), Width = 140, Height = 42, BackColor = AccentRed, ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnDeleteProd.FlatAppearance.BorderSize = 0;
            btnDeleteProd.Click += async (s, e) => await DeleteProductAsync();

            // Stock Adjustment Box
            Panel pnlStockBox = new Panel { Location = new Point(500, 115), Size = new Size(420, 52), BackColor = InputBg, Padding = new Padding(12, 8, 12, 8) };
            Label lblStockQty = new Label { Text = "Stock +/-:", ForeColor = TextMuted, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(10, 15), AutoSize = true };
            txtAdjustQty = new TextBox { Location = new Point(90, 12), Width = 80, Text = "0", Font = new Font("Segoe UI", 10), BackColor = CardBg, ForeColor = TextPrimary, BorderStyle = BorderStyle.FixedSingle };
            Button btnAdjust = new Button { Text = "⚡ Apply Stock Adjustment", Location = new Point(185, 10), Width = 220, Height = 32, BackColor = AccentAmber, ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnAdjust.FlatAppearance.BorderSize = 0;
            btnAdjust.Click += async (s, e) => await AdjustStockAsync();
            pnlStockBox.Controls.AddRange(new Control[] { lblStockQty, txtAdjustQty, btnAdjust });

            pnlManageCard.Controls.AddRange(new Control[] {
                lblManageTitle, txtAdjustProductId, lblCode, txtProdCode, lblName, txtProdName, lblPrice, txtProdPrice, lblReorder, txtAdjustReorder,
                btnCreateProd, btnUpdateProd, btnDeleteProd, pnlStockBox
            });

            invLayout.Controls.Add(pnlGridCard, 0, 0);
            invLayout.Controls.Add(pnlManageCard, 0, 1);

            viewInventory.Controls.Add(invLayout);
        }

        // ==========================================
        // MODULE 3: TRANSACTION HISTORY & ANALYTICS
        // ==========================================
        private void BuildReportsView()
        {
            viewReports = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0) };

            TableLayoutPanel reportLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(0)
            };
            reportLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 130F));
            reportLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
            reportLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // Row 1: Key Metrics Data Cards
            TableLayoutPanel metricsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1
            };
            metricsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            metricsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            metricsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            metricsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));

            Panel cardRevenue = CreateDataCard("Total Revenue", out lblRevenueValue, AccentBlue, (s, e) => LoadReportContext("Revenue & Sales Summary"));
            Panel cardTrans = CreateDataCard("Total Transactions", out lblTransValue, AccentGreen, (s, e) => LoadReportContext("Transaction Logs"));
            Panel cardTopProd = CreateDataCard("Top Catalog Items", out lblTopProdValue, AccentAmber, (s, e) => LoadReportContext("Top Selling Products"));

            Button btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                Dock = DockStyle.Fill,
                Margin = new Padding(8, 0, 0, 0),
                BackColor = CardBg,
                ForeColor = TextPrimary,
                Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderColor = BorderColor;
            btnRefresh.Click += async (s, e) => await LoadReportsAsync();

            metricsLayout.Controls.Add(cardRevenue, 0, 0);
            metricsLayout.Controls.Add(cardTrans, 1, 0);
            metricsLayout.Controls.Add(cardTopProd, 2, 0);
            metricsLayout.Controls.Add(btnRefresh, 3, 0);

            // Row 2: Context Label Header
            lblReportContext = new Label
            {
                Text = "Detailed Analytics View: Top Selling Products",
                ForeColor = TextMuted,
                Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Row 3: Analytics Table Card
            Panel pnlReportGridCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg,
                Padding = new Padding(24)
            };

            dgvReportDetails = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            ApplyDataGridStyle(dgvReportDetails);

            pnlReportGridCard.Controls.Add(dgvReportDetails);

            reportLayout.Controls.Add(metricsLayout, 0, 0);
            reportLayout.Controls.Add(lblReportContext, 0, 1);
            reportLayout.Controls.Add(pnlReportGridCard, 0, 2);

            viewReports.Controls.Add(reportLayout);
        }

        private Panel CreateDataCard(string title, out Label valLabel, Color stripColor, EventHandler onClick)
        {
            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg,
                Margin = new Padding(0, 0, 12, 0),
                Cursor = Cursors.Hand,
                Padding = new Padding(16)
            };

            Panel strip = new Panel
            {
                Size = new Size(4, 110),
                Dock = DockStyle.Left,
                BackColor = stripColor
            };

            Label lblTitle = new Label { Text = title, ForeColor = TextMuted, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Location = new Point(20, 18), AutoSize = true };
            valLabel = new Label { Text = "-", ForeColor = TextPrimary, Font = new Font("Segoe UI Semibold", 22, FontStyle.Bold), Location = new Point(18, 48), AutoSize = true };

            card.Click += onClick;
            strip.Click += onClick;
            lblTitle.Click += onClick;
            valLabel.Click += onClick;

            card.Controls.AddRange(new Control[] { strip, lblTitle, valLabel });
            return card;
        }

        private void LoadReportContext(string context)
        {
            lblReportContext.Text = $"Detailed Analytics View: {context}";
            if (_currentReport == null) return;

            dgvReportDetails.DataSource = null;
            dgvReportDetails.DataSource = _currentReport.TopSellingProducts;
        }

        // ==========================================
        // API CALLS & LOGIC HANDLERS
        // ==========================================
        private async Task CreateProductAsync()
        {
            if (string.IsNullOrWhiteSpace(txtProdCode.Text) || string.IsNullOrWhiteSpace(txtProdName.Text) ||
                !decimal.TryParse(txtProdPrice.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
            {
                MessageBox.Show("Please enter valid product details.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync($"tenant/{_currentCompanyId}/products", new { productCode = txtProdCode.Text.Trim(), productName = txtProdName.Text.Trim(), unitPrice = price, isActive = true });
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Product created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtProdCode.Clear(); txtProdName.Clear(); txtProdPrice.Clear();
                    await LoadInventoryAsync();
                }
                else
                {
                    MessageBox.Show($"Failed to create product: {await response.Content.ReadAsStringAsync()}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating product: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task UpdateProductAsync()
        {
            if (string.IsNullOrWhiteSpace(txtAdjustProductId.Text) || !int.TryParse(txtAdjustProductId.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out int prodId))
            {
                MessageBox.Show("Please select a product from the grid first.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!decimal.TryParse(txtProdPrice.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price) ||
                !decimal.TryParse(txtAdjustReorder.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal reorder))
            {
                MessageBox.Show("Please enter valid price and reorder level.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var response = await _httpClient.PutAsJsonAsync($"tenant/{_currentCompanyId}/products/{prodId}?reorderLevel={reorder.ToString(CultureInfo.InvariantCulture)}",
                    new { productCode = txtProdCode.Text.Trim(), productName = txtProdName.Text.Trim(), unitPrice = price });

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Product updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadInventoryAsync();
                }
                else
                {
                    MessageBox.Show($"Failed to update product: {await response.Content.ReadAsStringAsync()}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating product: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DeleteProductAsync()
        {
            if (string.IsNullOrWhiteSpace(txtAdjustProductId.Text) || !int.TryParse(txtAdjustProductId.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out int prodId))
            {
                MessageBox.Show("Please select a product from the grid first.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this product?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    var response = await _httpClient.DeleteAsync($"tenant/{_currentCompanyId}/products/{prodId}");
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Product deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtProdCode.Clear(); txtProdName.Clear(); txtProdPrice.Clear(); txtAdjustProductId.Clear();
                        await LoadInventoryAsync();
                    }
                    else
                    {
                        MessageBox.Show($"Failed to delete product: {await response.Content.ReadAsStringAsync()}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting product: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task AdjustStockAsync()
        {
            if (!int.TryParse(txtAdjustProductId.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out int prodId) ||
                !decimal.TryParse(txtAdjustQty.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal qty) ||
                !decimal.TryParse(txtAdjustReorder.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal reorder))
            {
                MessageBox.Show("Please select a product and enter valid inventory values.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var response = await _httpClient.PostAsync($"tenant/{_currentCompanyId}/inventory/adjust?productId={prodId}&quantity={qty.ToString(CultureInfo.InvariantCulture)}&reorderLevel={reorder.ToString(CultureInfo.InvariantCulture)}", null);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Stock adjusted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtAdjustQty.Text = "0";
                    await LoadInventoryAsync();
                }
                else
                {
                    MessageBox.Show($"Failed to adjust stock: {await response.Content.ReadAsStringAsync()}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adjusting stock: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadInventoryAsync()
        {
            try
            {
                var inventory = await _httpClient.GetFromJsonAsync<List<InventoryViewDto>>($"tenant/{_currentCompanyId}/inventory");
                dgvInventory.DataSource = inventory;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading inventory: {ex.Message}");
            }
        }

        private void AddToCart()
        {
            if (!int.TryParse(txtSaleProductId.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out int prodId) ||
                !decimal.TryParse(txtSaleQty.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal qty) ||
                !decimal.TryParse(txtUnitPrice.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
            {
                MessageBox.Show("Please enter valid numeric sale items.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _cart.Add(new CartItemDto { ProductId = prodId, Quantity = qty, UnitPrice = price, SubTotal = qty * price });
            dgvCart.DataSource = null;
            dgvCart.DataSource = _cart;
            lblTotalAmount.Text = $"Total Amount: ${_cart.Sum(x => x.SubTotal).ToString("F2", CultureInfo.InvariantCulture)}";
        }

        private async Task CompleteSaleAsync()
        {
            if (!_cart.Any()) return;
            int.TryParse(txtCustomerId.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out int customerId);

            var salePayload = new SaleRequestDto
            {
                InvoiceNumber = string.IsNullOrWhiteSpace(txtInvoiceNum.Text) ? "INV-" + DateTime.Now.ToString("fff", CultureInfo.InvariantCulture) : txtInvoiceNum.Text,
                CustomerId = customerId > 0 ? customerId : null,
                SaleItems = _cart
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync($"tenant/{_currentCompanyId}/sales", salePayload);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Sale processed and committed to database successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _cart.Clear();
                    dgvCart.DataSource = null;
                    lblTotalAmount.Text = "Total Amount: $0.00";
                    txtInvoiceNum.Text = "INV-" + DateTime.Now.ToString("fff", CultureInfo.InvariantCulture);
                    await RefreshAllDataAsync();
                }
                else
                {
                    MessageBox.Show($"Transaction failed: {await response.Content.ReadAsStringAsync()}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing sale: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadReportsAsync()
        {
            try
            {
                _currentReport = await _httpClient.GetFromJsonAsync<SalesSummaryReportDto>($"tenant/{_currentCompanyId}/reports/sales-summary");
                if (_currentReport != null)
                {
                    lblRevenueValue.Text = $"${_currentReport.TotalRevenue.ToString("F2", CultureInfo.InvariantCulture)}";
                    lblTransValue.Text = $"{_currentReport.TotalTransactions}";
                    lblTopProdValue.Text = $"{_currentReport.TopSellingProducts.Count} Items";
                    LoadReportContext("Top Selling Products");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading reports: {ex.Message}");
            }
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
