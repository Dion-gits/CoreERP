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
        private string _userRole;
        private string _tenantEmail;
        private string _userEmail;

        private List<CartItemDto> _cart = new List<CartItemDto>();
        private SalesSummaryReportDto? _currentReport;
        private List<InventoryViewDto> _allProducts = new List<InventoryViewDto>();
        private List<PayrollRecordDto> _payrollList = new List<PayrollRecordDto>();
        private List<PurchaseOrderDto> _poList = new List<PurchaseOrderDto>();
        private List<StockAuditDto> _auditList = new List<StockAuditDto>();

        // Modern Dark Theme Palette
        private readonly Color BgDark = Color.FromArgb(18, 18, 18);
        private readonly Color CardBg = Color.FromArgb(28, 28, 28);
        private readonly Color BorderColor = Color.FromArgb(45, 45, 45);
        private readonly Color AccentGreen = Color.FromArgb(16, 185, 129);
        private readonly Color AccentBlue = Color.FromArgb(59, 130, 246);
        private readonly Color AccentOrange = Color.FromArgb(245, 158, 11);
        private readonly Color AccentRed = Color.FromArgb(239, 68, 68);
        private readonly Color AccentPurple = Color.FromArgb(139, 92, 246);
        private readonly Color TextPrimary = Color.FromArgb(243, 244, 246);
        private readonly Color TextMuted = Color.FromArgb(156, 163, 175);

        // Layout Containers
        private Panel pnlSidebar = null!;
        private Panel pnlMainContent = null!;

        // Navigation Buttons
        private List<Button> navButtons = new List<Button>();
        private Button btnNavDashboard = null!;
        private Button btnNavSales = null!;
        private Button btnNavInventory = null!;
        private Button btnNavPayroll = null!;
        private Button btnNavSupplierOrders = null!;
        private Button btnNavReports = null!;
        private Button btnNavTerms = null!;
        private Button btnLogout = null!;

        // View Panels
        private Panel viewDashboard = null!;
        private Panel viewSales = null!;
        private Panel viewInventory = null!;
        private Panel viewPayroll = null!;
        private Panel viewSupplierOrders = null!;
        private Panel viewReports = null!;
        private Panel viewTerms = null!;

        // Dashboard Controls
        private FlowLayoutPanel pnlDataCards = null!;
        private Panel pnlGraphContainer = null!;
        private DataGridView dgvDashboardGrid = null!;
        private Label lblDashContext = null!;

        // Inventory & Conversion Controls
        private DataGridView dgvInventory = null!;
        private TextBox txtProdCode = null!, txtProdName = null!, txtProdPrice = null!;
        private TextBox txtAdjustProductId = null!, txtAdjustQty = null!, txtAdjustReorder = null!;
        private TextBox txtConvertBoxes = null!, txtFactorUnits = null!;
        private DataGridView dgvAuditGrid = null!;
        private TextBox txtAuditPhysQty = null!;

        // Sales POS Controls
        private DataGridView dgvCart = null!;
        private TextBox txtSearchPOS = null!;
        private DataGridView dgvSearchResults = null!;
        private TextBox txtInvoiceNum = null!, txtCustomerId = null!, txtSaleProductId = null!, txtSaleQty = null!, txtUnitPrice = null!;
        private Label lblTotalAmount = null!;

        // Payroll Controls
        private DataGridView dgvPayroll = null!;
        private TextBox txtEmpName = null!, txtEmpRole = null!, txtBaseSalary = null!, txtBonus = null!, txtDeductions = null!;

        // Supplier Orders Controls
        private DataGridView dgvSupplierOrders = null!;
        private ComboBox cbSuppliers = null!;
        private TextBox txtPoProdId = null!, txtPoQty = null!, txtPoCost = null!;

        // Reports Controls
        private Label lblRevenueValue = null!, lblTransValue = null!, lblTopProdValue = null!;
        private DataGridView dgvReportDetails = null!;
        private Label lblReportContext = null!;
        private Panel pnlReportChart = null!;

        // Store T&C Controls
        private RichTextBox rtbReturnPolicy = null!;
        private RichTextBox rtbCreditRules = null!;
        private RichTextBox rtbGeneralTerms = null!;

        public MainErpForm(int companyId = 1, string userRole = "Owner", string tenantEmail = "tenant1@email", string userEmail = "owner1@email")
        {
            _currentCompanyId = companyId;
            _userRole = string.IsNullOrWhiteSpace(userRole) ? "Owner" : userRole;
            _tenantEmail = string.IsNullOrWhiteSpace(tenantEmail) ? "tenant1@email" : tenantEmail;
            _userEmail = string.IsNullOrWhiteSpace(userEmail) ? "owner1@email" : userEmail;

            InitializeComponentCustom();
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7166/") };
        }

        private void InitializeComponentCustom()
        {
            this.Text = $"Small Enterprise ERP - Tenant {_currentCompanyId} [{_userRole}]";
            this.WindowState = FormWindowState.Maximized;
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
                Width = 270,
                BackColor = CardBg
            };

            Label lblAppTitle = new Label
            {
                Text = "⚡ CORE ERP SMALL",
                ForeColor = TextPrimary,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            Label lblRoleBadge = new Label
            {
                Text = $"👤 {_userRole}\n✉ {_userEmail}",
                ForeColor = AccentBlue,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(20, 50),
                AutoSize = true
            };

            pnlSidebar.Controls.Add(lblAppTitle);
            pnlSidebar.Controls.Add(lblRoleBadge);

            int btnTop = 100;
            btnNavDashboard = CreateSidebarButton("📈  Dashboard & BI", ref btnTop);
            btnNavSales = CreateSidebarButton("🛒  Point of Sale (POS)", ref btnTop);
            btnNavInventory = CreateSidebarButton("📦  Inventory & Stock", ref btnTop);
            btnNavPayroll = CreateSidebarButton("💵  HR & Payroll", ref btnTop);
            btnNavSupplierOrders = CreateSidebarButton("🏭  Supplier Buying & PO", ref btnTop);
            btnNavReports = CreateSidebarButton("📊  Reports & Analytics", ref btnTop);
            btnNavTerms = CreateSidebarButton("📜  Store T&C Configuration", ref btnTop);

            btnNavDashboard.Click += (s, e) => SwitchView(viewDashboard, btnNavDashboard);
            btnNavSales.Click += (s, e) => SwitchView(viewSales, btnNavSales);
            btnNavInventory.Click += (s, e) => SwitchView(viewInventory, btnNavInventory);
            btnNavPayroll.Click += (s, e) => SwitchView(viewPayroll, btnNavPayroll);
            btnNavSupplierOrders.Click += (s, e) => SwitchView(viewSupplierOrders, btnNavSupplierOrders);
            btnNavReports.Click += (s, e) => SwitchView(viewReports, btnNavReports);
            btnNavTerms.Click += (s, e) => SwitchView(viewTerms, btnNavTerms);

            ApplyRoleBasedNavigation();

            btnLogout = new Button
            {
                Text = "🚪  Sign Out Workspace",
                Dock = DockStyle.Bottom,
                Height = 50,
                FlatStyle = FlatStyle.Flat,
                ForeColor = AccentRed,
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
                    _userRole = loginForm.AuthenticatedRole;
                    _tenantEmail = loginForm.AuthenticatedTenantEmail;
                    _userEmail = loginForm.AuthenticatedUserEmail;
                    lblRoleBadge.Text = $"👤 {_userRole}\n✉ {_userEmail}";
                    this.Text = $"Small Enterprise ERP - Tenant {_currentCompanyId} [{_userRole}]";
                    ApplyRoleBasedNavigation();
                    this.Show();
                    _ = RefreshAllDataAsync();
                }
                else { Application.Exit(); }
            };

            pnlSidebar.Controls.Add(btnLogout);

            // ==========================================
            // 2. MAIN CONTENT AREA 
            // ==========================================
            pnlMainContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BgDark
            };

            BuildDashboardView();
            BuildSalesView();
            BuildInventoryView();
            BuildPayrollView();
            BuildSupplierOrdersView();
            BuildReportsView();
            BuildTermsView();

            this.Controls.Add(pnlMainContent);
            this.Controls.Add(pnlSidebar);

            // Default view based on role
            if (_userRole == "Cashier")
            {
                SwitchView(viewSales, btnNavSales);
            }
            else
            {
                SwitchView(viewDashboard, btnNavDashboard);
            }

            this.Load += async (s, e) => await RefreshAllDataAsync();
        }

        private Button CreateSidebarButton(string text, ref int top)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(12, top),
                Size = new Size(245, 45),
                FlatStyle = FlatStyle.Flat,
                ForeColor = TextMuted,
                BackColor = CardBg,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
            top += 52;
            pnlSidebar.Controls.Add(btn);
            navButtons.Add(btn);
            return btn;
        }

        private void ApplyRoleBasedNavigation()
        {
            btnNavDashboard.Visible = false;
            btnNavSales.Visible = false;
            btnNavInventory.Visible = false;
            btnNavPayroll.Visible = false;
            btnNavSupplierOrders.Visible = false;
            btnNavReports.Visible = false;
            btnNavTerms.Visible = false;

            switch (_userRole)
            {
                case "Super Admin": // UC1, UC8
                    btnNavDashboard.Visible = true;
                    btnNavTerms.Visible = true;
                    btnNavReports.Visible = true;
                    break;
                case "Owner": // UC2, UC3, UC4, UC5, UC9, UC21
                    btnNavDashboard.Visible = true;
                    btnNavSales.Visible = true;
                    btnNavInventory.Visible = true;
                    btnNavSupplierOrders.Visible = true;
                    btnNavPayroll.Visible = true;
                    btnNavReports.Visible = true;
                    btnNavTerms.Visible = true;
                    break;
                case "HR Manager": // UC4, UC6, UC7, UC14, UC15
                    btnNavDashboard.Visible = true;
                    btnNavPayroll.Visible = true;
                    btnNavSupplierOrders.Visible = true;
                    btnNavReports.Visible = true;
                    break;
                case "Branch Manager": // UC4, UC11, UC12, UC18, UC21, UC25
                    btnNavDashboard.Visible = true;
                    btnNavInventory.Visible = true;
                    btnNavSupplierOrders.Visible = true;
                    btnNavReports.Visible = true;
                    break;
                case "Cashier": // UC10, UC19, UC20 (No dashboard, direct POS access)
                    btnNavSales.Visible = true;
                    break;
                case "Inventory Staff": // UC13, UC16, UC17, UC22, UC23, UC24, UC25
                    btnNavDashboard.Visible = true;
                    btnNavInventory.Visible = true;
                    btnNavSupplierOrders.Visible = true;
                    break;
            }
        }

        private void SwitchView(Panel targetView, Button activeBtn)
        {
            pnlMainContent.Controls.Clear();
            targetView.Dock = DockStyle.Fill;
            pnlMainContent.Controls.Add(targetView);

            foreach (var btn in navButtons)
            {
                btn.BackColor = CardBg;
                btn.ForeColor = TextMuted;
            }

            activeBtn.BackColor = Color.FromArgb(40, 40, 40);
            activeBtn.ForeColor = TextPrimary;
        }

        // ==========================================
        // MODULE 0: DASHBOARD & DATA CARDS WITH CLICK NAVIGATION
        // ==========================================
        private void BuildDashboardView()
        {
            viewDashboard = new Panel { Padding = new Padding(24), AutoScroll = true };

            Label lblTitle = new Label { Text = $"Dashboard Workspace — [{_userRole}]", ForeColor = TextPrimary, Font = new Font("Segoe UI", 16, FontStyle.Bold), AutoSize = true, Location = new Point(24, 20) };
            Label lblSub = new Label { Text = "Interactive data cards (press card to navigate directly to workspace)", ForeColor = TextMuted, Font = new Font("Segoe UI", 10), AutoSize = true, Location = new Point(24, 52) };

            pnlDataCards = new FlowLayoutPanel
            {
                Location = new Point(24, 85),
                Size = new Size(1250, 125),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoScroll = true
            };

            lblDashContext = new Label { Text = "📊 Interactive BI Visualizer", ForeColor = TextPrimary, Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true, Location = new Point(24, 225) };

            pnlGraphContainer = new Panel
            {
                Location = new Point(24, 255),
                Size = new Size(1250, 220),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = CardBg
            };
            pnlGraphContainer.Paint += RenderDashboardGraph;

            dgvDashboardGrid = new DataGridView
            {
                Location = new Point(24, 495),
                Size = new Size(1250, 250),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = CardBg,
                ForeColor = TextPrimary,
                GridColor = BorderColor,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false
            };
            dgvDashboardGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(35, 35, 35);
            dgvDashboardGrid.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary;
            dgvDashboardGrid.DefaultCellStyle.BackColor = CardBg;
            dgvDashboardGrid.DefaultCellStyle.ForeColor = TextPrimary;

            viewDashboard.Controls.AddRange(new Control[] { lblTitle, lblSub, pnlDataCards, lblDashContext, pnlGraphContainer, dgvDashboardGrid });
        }

        private void LoadDashboardRoleCards()
        {
            pnlDataCards.Controls.Clear();

            switch (_userRole)
            {
                case "Super Admin":
                    pnlDataCards.Controls.Add(CreateDataCard("Tenant Accounts", "3 Active Tenants", AccentBlue, (s, e) => SwitchView(viewReports, btnNavReports)));
                    pnlDataCards.Controls.Add(CreateDataCard("System Rules (UC8)", "Configured", AccentPurple, (s, e) => SwitchView(viewTerms, btnNavTerms)));
                    break;
                case "Owner":
                    pnlDataCards.Controls.Add(CreateDataCard("Total Revenue", "$" + (_currentReport?.TotalRevenue ?? 0).ToString("N2"), AccentGreen, (s, e) => SwitchView(viewReports, btnNavReports)));
                    pnlDataCards.Controls.Add(CreateDataCard("Product Catalog", $"{_allProducts.Count} Items", AccentBlue, (s, e) => SwitchView(viewInventory, btnNavInventory)));
                    pnlDataCards.Controls.Add(CreateDataCard("User & Role Mgmt", "6 Active Roles", AccentOrange, (s, e) => SwitchView(viewPayroll, btnNavPayroll)));
                    pnlDataCards.Controls.Add(CreateDataCard("Store T&Cs", "Configured", AccentPurple, (s, e) => SwitchView(viewTerms, btnNavTerms)));
                    break;
                case "HR Manager":
                    pnlDataCards.Controls.Add(CreateDataCard("Needs PO Approval", $"{_poList.Count(p => p.Status == "Validated")} Pending POs", AccentOrange, (s, e) => SwitchView(viewSupplierOrders, btnNavSupplierOrders)));
                    pnlDataCards.Controls.Add(CreateDataCard("Staff Payroll Records", $"{_payrollList.Count} Staff", AccentPurple, (s, e) => SwitchView(viewPayroll, btnNavPayroll)));
                    pnlDataCards.Controls.Add(CreateDataCard("Track Unpaid Expenses", "2 Expenses", AccentRed, (s, e) => SwitchView(viewPayroll, btnNavPayroll)));
                    pnlDataCards.Controls.Add(CreateDataCard("Sales & Financials", "$" + (_currentReport?.TotalRevenue ?? 0).ToString("N2"), AccentGreen, (s, e) => SwitchView(viewReports, btnNavReports)));
                    break;
                case "Branch Manager":
                    pnlDataCards.Controls.Add(CreateDataCard("Sales Validation", $"{_currentReport?.TotalTransactions ?? 0} Trans", AccentGreen, (s, e) => SwitchView(viewReports, btnNavReports)));
                    pnlDataCards.Controls.Add(CreateDataCard("Stock Order Validation", $"{_poList.Count(p => p.Status == "Draft")} Draft POs", AccentOrange, (s, e) => SwitchView(viewSupplierOrders, btnNavSupplierOrders)));
                    pnlDataCards.Controls.Add(CreateDataCard("Physical Audit Requests", $"{_auditList.Count(a => a.Status == "Pending")} Pending", AccentRed, (s, e) => SwitchView(viewInventory, btnNavInventory)));
                    pnlDataCards.Controls.Add(CreateDataCard("Manage Product List", $"{_allProducts.Count} Products", AccentBlue, (s, e) => SwitchView(viewInventory, btnNavInventory)));
                    break;
                case "Inventory Staff":
                    pnlDataCards.Controls.Add(CreateDataCard("Boxes to Convert", "12 Boxes", AccentPurple, (s, e) => SwitchView(viewInventory, btnNavInventory)));
                    pnlDataCards.Controls.Add(CreateDataCard("Low Stock Warning", $"{_allProducts.Count(p => p.IsLowStock)} Items", AccentRed, (s, e) => SwitchView(viewInventory, btnNavInventory)));
                    pnlDataCards.Controls.Add(CreateDataCard("Validated Audit Req", $"{_auditList.Count(a => a.Status != "Pending")} Validated", AccentGreen, (s, e) => SwitchView(viewInventory, btnNavInventory)));
                    pnlDataCards.Controls.Add(CreateDataCard("Create Purchase Order", $"{_poList.Count} Orders", AccentBlue, (s, e) => SwitchView(viewSupplierOrders, btnNavSupplierOrders)));
                    break;
            }
        }

        private Panel CreateDataCard(string title, string value, Color stripColor, EventHandler onClick)
        {
            Panel card = new Panel { Size = new Size(280, 95), Margin = new Padding(0, 0, 15, 0), BackColor = CardBg, Cursor = Cursors.Hand };
            Panel strip = new Panel { Size = new Size(5, 95), Dock = DockStyle.Left, BackColor = stripColor };

            Label lblTitle = new Label { Text = title, ForeColor = TextMuted, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(18, 12), AutoSize = true };
            Label lblValue = new Label { Text = value, ForeColor = TextPrimary, Font = new Font("Segoe UI", 15, FontStyle.Bold), Location = new Point(16, 38), AutoSize = true };
            Label lblNavHint = new Label { Text = "Press to navigate ➔", ForeColor = AccentBlue, Font = new Font("Segoe UI", 8, FontStyle.Italic), Location = new Point(16, 70), AutoSize = true };

            card.Click += onClick;
            strip.Click += onClick;
            lblTitle.Click += onClick;
            lblValue.Click += onClick;
            lblNavHint.Click += onClick;

            card.Controls.AddRange(new Control[] { strip, lblTitle, lblValue, lblNavHint });
            return card;
        }

        private void RenderDashboardGraph(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(CardBg);

            using Font titleFont = new Font("Segoe UI", 10, FontStyle.Bold);
            using Font labelFont = new Font("Segoe UI", 8);
            using Brush textBrush = new SolidBrush(TextPrimary);
            using Brush mutedBrush = new SolidBrush(TextMuted);

            g.DrawString($"Business Analytics & Intelligence — [{_userRole}]", titleFont, textBrush, 15, 12);

            int startX = 60;
            int startY = 180;
            int maxHeight = 120;
            int barWidth = 45;
            int spacing = 35;

            using Pen linePen = new Pen(BorderColor, 2);
            g.DrawLine(linePen, 40, startY, pnlGraphContainer.Width - 40, startY);

            int count = Math.Min(_allProducts.Count > 0 ? _allProducts.Count : 5, 8);
            if (count == 0) return;

            decimal maxVal = _allProducts.Max(p => p.QuantityOnHand);
            if (maxVal <= 0) maxVal = 100;

            for (int i = 0; i < count; i++)
            {
                var prod = _allProducts[i];
                int barHeight = (int)((prod.QuantityOnHand / maxVal) * maxHeight);
                if (barHeight < 10) barHeight = 10;

                int x = startX + i * (barWidth + spacing);
                int y = startY - barHeight;

                Color barColor = prod.IsLowStock ? AccentRed : AccentBlue;
                using Brush barBrush = new SolidBrush(barColor);
                g.FillRectangle(barBrush, x, y, barWidth, barHeight);

                g.DrawString($"{prod.QuantityOnHand:N0}", labelFont, textBrush, x + 8, y - 18);
                string shortName = prod.ProductName.Length > 8 ? prod.ProductName.Substring(0, 8) + ".." : prod.ProductName;
                g.DrawString(shortName, labelFont, mutedBrush, x - 2, startY + 5);
            }
        }

        // ==========================================
        // MODULE 1: EFFICIENT POS VIEW (WITH QUANTITY INPUT & T&C RECEIPT)
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

            Label lblCartTitle = new Label { Text = "Point of Sale — Active Cart Items", ForeColor = TextPrimary, Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };

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

            pnlCartCard.Controls.AddRange(new Control[] { lblCartTitle, dgvCart });

            Panel pnlCheckoutCard = new Panel
            {
                Location = new Point(728, 24),
                Size = new Size(500, 710),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = CardBg
            };

            GroupBox grpSearch = new GroupBox { Text = "🔍 Search Item / Product Catalog", ForeColor = TextMuted, Font = new Font("Segoe UI", 10), Location = new Point(20, 15), Size = new Size(460, 220) };

            txtSearchPOS = new TextBox { Location = new Point(15, 30), Width = 310, BackColor = BgDark, ForeColor = TextPrimary, Font = new Font("Segoe UI", 10) };
            Button btnSearchPOS = new Button { Text = "Search", Location = new Point(335, 28), Width = 110, Height = 30, BackColor = AccentBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnSearchPOS.FlatAppearance.BorderSize = 0;
            btnSearchPOS.Click += (s, e) => FilterPOSProducts();

            dgvSearchResults = new DataGridView
            {
                Location = new Point(15, 68),
                Size = new Size(430, 138),
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = BgDark,
                ForeColor = TextPrimary,
                GridColor = BorderColor,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                EnableHeadersVisualStyles = false
            };
            dgvSearchResults.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(35, 35, 35);
            dgvSearchResults.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary;
            dgvSearchResults.DefaultCellStyle.BackColor = BgDark;
            dgvSearchResults.DefaultCellStyle.ForeColor = TextPrimary;

            dgvSearchResults.CellClick += (s, e) =>
            {
                if (dgvSearchResults.CurrentRow != null && dgvSearchResults.CurrentRow.DataBoundItem is InventoryViewDto selected)
                {
                    txtSaleProductId.Text = selected.ProductId.ToString();
                    txtUnitPrice.Text = selected.UnitPrice.ToString("F2");
                }
            };

            grpSearch.Controls.AddRange(new Control[] { txtSearchPOS, btnSearchPOS, dgvSearchResults });

            GroupBox grpAdd = new GroupBox { Text = "Efficient Item Input", ForeColor = TextMuted, Font = new Font("Segoe UI", 10), Location = new Point(20, 245), Size = new Size(460, 180) };

            grpAdd.Controls.Add(new Label { Text = "Product ID:", ForeColor = TextPrimary, Location = new Point(15, 35), AutoSize = true });
            txtSaleProductId = new TextBox { Location = new Point(115, 32), Width = 120, BackColor = BgDark, ForeColor = TextPrimary };
            grpAdd.Controls.Add(txtSaleProductId);

            grpAdd.Controls.Add(new Label { Text = "Quantity:", ForeColor = TextPrimary, Location = new Point(250, 35), AutoSize = true });
            txtSaleQty = new TextBox { Location = new Point(330, 32), Width = 110, Text = "1", BackColor = BgDark, ForeColor = TextPrimary };
            grpAdd.Controls.Add(txtSaleQty);

            grpAdd.Controls.Add(new Label { Text = "Unit Price ($):", ForeColor = TextPrimary, Location = new Point(15, 80), AutoSize = true });
            txtUnitPrice = new TextBox { Location = new Point(115, 77), Width = 120, BackColor = BgDark, ForeColor = TextPrimary };
            grpAdd.Controls.Add(txtUnitPrice);

            Button btnAddCart = new Button { Text = "➕ Add Item to Cart", Location = new Point(250, 75), Width = 190, Height = 35, BackColor = AccentBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnAddCart.FlatAppearance.BorderSize = 0;
            btnAddCart.Click += (s, e) => AddToCart();
            grpAdd.Controls.Add(btnAddCart);

            GroupBox grpMeta = new GroupBox { Text = "Transaction Info", ForeColor = TextMuted, Font = new Font("Segoe UI", 10), Location = new Point(20, 435), Size = new Size(460, 110) };

            grpMeta.Controls.Add(new Label { Text = "Invoice #:", ForeColor = TextPrimary, Location = new Point(15, 35), AutoSize = true });
            txtInvoiceNum = new TextBox { Location = new Point(100, 32), Width = 140, Text = "INV-" + DateTime.Now.ToString("fff"), BackColor = BgDark, ForeColor = TextPrimary };
            grpMeta.Controls.Add(txtInvoiceNum);

            grpMeta.Controls.Add(new Label { Text = "Customer ID:", ForeColor = TextPrimary, Location = new Point(255, 35), AutoSize = true });
            txtCustomerId = new TextBox { Location = new Point(350, 32), Width = 90, Text = "1", BackColor = BgDark, ForeColor = TextPrimary };
            grpMeta.Controls.Add(txtCustomerId);

            lblTotalAmount = new Label { Text = "Total Amount: $0.00", ForeColor = AccentGreen, Location = new Point(20, 560), Font = new Font("Segoe UI", 16, FontStyle.Bold), AutoSize = true };

            Button btnCheckout = new Button { Text = "💳 Complete Sale & Print Receipt", Location = new Point(20, 610), Width = 460, Height = 55, BackColor = AccentGreen, ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnCheckout.FlatAppearance.BorderSize = 0;
            btnCheckout.Click += async (s, e) => await CompleteSaleAsync();

            pnlCheckoutCard.Controls.AddRange(new Control[] { grpSearch, grpAdd, grpMeta, lblTotalAmount, btnCheckout });

            viewSales.Controls.AddRange(new Control[] { pnlCartCard, pnlCheckoutCard });
        }

        private void FilterPOSProducts()
        {
            string search = txtSearchPOS.Text.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(search))
            {
                dgvSearchResults.DataSource = _allProducts.Take(10).ToList();
            }
            else
            {
                dgvSearchResults.DataSource = _allProducts.Where(p => p.ProductName.ToLower().Contains(search) || p.ProductCode.ToLower().Contains(search)).ToList();
            }
        }

        // ==========================================
        // MODULE 2: INVENTORY, CONVERSIONS & AUDITS
        // ==========================================
        private void BuildInventoryView()
        {
            viewInventory = new Panel { Padding = new Padding(24), AutoScroll = true };

            Label lblTitle = new Label { Text = "Stock Management & Physical Audits (UC21, UC22, UC23, UC24, UC25)", ForeColor = TextPrimary, Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, Location = new Point(24, 24) };

            dgvInventory = new DataGridView
            {
                Location = new Point(24, 65),
                Size = new Size(1200, 240),
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

            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "InventoryId", HeaderText = "Inv ID", Width = 70 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductId", HeaderText = "Prod ID", Width = 70 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Product Name", Width = 250 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductCode", HeaderText = "Code", Width = 140 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UnitPrice", HeaderText = "Unit Price", Width = 120, DefaultCellStyle = { Format = "C2" } });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "QuantityOnHand", HeaderText = "Stock Qty", Width = 110 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ReorderLevel", HeaderText = "Reorder", Width = 110 });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "IsLowStock", HeaderText = "Low Stock?", Width = 110 });

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
                Text = "Manage Product Catalog & Adjustments (UC21, UC22)",
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 10),
                Location = new Point(24, 315),
                Size = new Size(1200, 150),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            txtAdjustProductId = new TextBox { Visible = false };
            grpManage.Controls.Add(txtAdjustProductId);

            grpManage.Controls.Add(new Label { Text = "Code:", ForeColor = TextMuted, Location = new Point(25, 35), AutoSize = true });
            txtProdCode = new TextBox { Location = new Point(75, 32), Width = 120, BackColor = BgDark, ForeColor = TextPrimary };

            grpManage.Controls.Add(new Label { Text = "Name:", ForeColor = TextMuted, Location = new Point(210, 35), AutoSize = true });
            txtProdName = new TextBox { Location = new Point(260, 32), Width = 220, BackColor = BgDark, ForeColor = TextPrimary };

            grpManage.Controls.Add(new Label { Text = "Price:", ForeColor = TextMuted, Location = new Point(495, 35), AutoSize = true });
            txtProdPrice = new TextBox { Location = new Point(540, 32), Width = 90, BackColor = BgDark, ForeColor = TextPrimary };

            grpManage.Controls.Add(new Label { Text = "Reorder:", ForeColor = TextMuted, Location = new Point(645, 35), AutoSize = true });
            txtAdjustReorder = new TextBox { Location = new Point(710, 32), Width = 80, Text = "5", BackColor = BgDark, ForeColor = TextPrimary };

            grpManage.Controls.AddRange(new Control[] { txtProdCode, txtProdName, txtProdPrice, txtAdjustReorder });

            Button btnCreateProd = new Button { Text = "Add New", Location = new Point(25, 80), Width = 110, Height = 38, BackColor = AccentGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnCreateProd.FlatAppearance.BorderSize = 0;
            btnCreateProd.Click += async (s, e) => await CreateProductAsync();

            Button btnUpdateProd = new Button { Text = "Update", Location = new Point(145, 80), Width = 110, Height = 38, BackColor = AccentBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnUpdateProd.FlatAppearance.BorderSize = 0;
            btnUpdateProd.Click += async (s, e) => await UpdateProductAsync();

            Button btnDeleteProd = new Button { Text = "Delete", Location = new Point(265, 80), Width = 110, Height = 38, BackColor = AccentRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnDeleteProd.FlatAppearance.BorderSize = 0;
            btnDeleteProd.Click += async (s, e) => await DeleteProductAsync();

            Panel pnlStock = new Panel { Location = new Point(400, 75), Size = new Size(780, 50), BackColor = BgDark };
            pnlStock.Controls.Add(new Label { Text = "Adjust Qty (+/-):", ForeColor = TextPrimary, Location = new Point(15, 14), AutoSize = true });
            txtAdjustQty = new TextBox { Location = new Point(135, 11), Width = 70, Text = "0", BackColor = CardBg, ForeColor = TextPrimary };
            Button btnAdjust = new Button { Text = "Apply Stock Change", Location = new Point(215, 9), Width = 200, Height = 32, BackColor = AccentOrange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnAdjust.FlatAppearance.BorderSize = 0;
            btnAdjust.Click += async (s, e) => await AdjustStockAsync();
            pnlStock.Controls.AddRange(new Control[] { txtAdjustQty, btnAdjust });

            grpManage.Controls.AddRange(new Control[] { btnCreateProd, btnUpdateProd, btnDeleteProd, pnlStock });

            // BOX CONVERSION & PHYSICAL AUDITS
            GroupBox grpConvert = new GroupBox
            {
                Text = "Box-to-Product Unit Conversion (UC23) & Physical Stock Audit Request (UC25)",
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 10),
                Location = new Point(24, 480),
                Size = new Size(1200, 160),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            grpConvert.Controls.Add(new Label { Text = "Boxes to Convert:", ForeColor = TextPrimary, Location = new Point(20, 35), AutoSize = true });
            txtConvertBoxes = new TextBox { Location = new Point(150, 32), Width = 80, Text = "1", BackColor = BgDark, ForeColor = TextPrimary };
            grpConvert.Controls.Add(new Label { Text = "Units per Box:", ForeColor = TextPrimary, Location = new Point(245, 35), AutoSize = true });
            txtFactorUnits = new TextBox { Location = new Point(345, 32), Width = 80, Text = "20", BackColor = BgDark, ForeColor = TextPrimary };

            Button btnConvert = new Button { Text = "📦 Convert Boxes to Products", Location = new Point(445, 30), Width = 230, Height = 35, BackColor = AccentPurple, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnConvert.FlatAppearance.BorderSize = 0;
            btnConvert.Click += async (s, e) => await ConvertBoxesAsync();

            grpConvert.Controls.Add(new Label { Text = "Physical Count Qty:", ForeColor = TextPrimary, Location = new Point(20, 95), AutoSize = true });
            txtAuditPhysQty = new TextBox { Location = new Point(150, 92), Width = 100, Text = "0", BackColor = BgDark, ForeColor = TextPrimary };

            Button btnAuditReq = new Button { Text = "📋 Send Audit Request to Manager", Location = new Point(265, 90), Width = 270, Height = 35, BackColor = AccentOrange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnAuditReq.FlatAppearance.BorderSize = 0;
            btnAuditReq.Click += async (s, e) => await RequestStockAuditAsync();

            Button btnValidateAudit = new Button { Text = "✅ Manager Approve Audit (UC25)", Location = new Point(550, 90), Width = 260, Height = 35, BackColor = AccentGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnValidateAudit.FlatAppearance.BorderSize = 0;
            btnValidateAudit.Click += async (s, e) => await ApproveStockAuditAsync();

            grpConvert.Controls.AddRange(new Control[] { txtConvertBoxes, txtFactorUnits, btnConvert, txtAuditPhysQty, btnAuditReq, btnValidateAudit });

            dgvAuditGrid = new DataGridView
            {
                Location = new Point(24, 650),
                Size = new Size(1200, 160),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = CardBg,
                ForeColor = TextPrimary,
                GridColor = BorderColor,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false
            };
            dgvAuditGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(35, 35, 35);
            dgvAuditGrid.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary;
            dgvAuditGrid.DefaultCellStyle.BackColor = CardBg;
            dgvAuditGrid.DefaultCellStyle.ForeColor = TextPrimary;

            viewInventory.Controls.AddRange(new Control[] { lblTitle, dgvInventory, grpManage, grpConvert, dgvAuditGrid });
        }

        // ==========================================
        // MODULE 3: PAYROLL VIEW (UC14, UC15 FOR HR MANAGER)
        // ==========================================
        private void BuildPayrollView()
        {
            viewPayroll = new Panel { Padding = new Padding(24) };

            Label lblTitle = new Label { Text = "HR & Employee Payroll Workspace (UC14, UC15)", ForeColor = TextPrimary, Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, Location = new Point(24, 24) };

            dgvPayroll = new DataGridView
            {
                Location = new Point(24, 70),
                Size = new Size(1200, 320),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = CardBg,
                ForeColor = TextPrimary,
                GridColor = BorderColor,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false
            };
            dgvPayroll.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(35, 35, 35);
            dgvPayroll.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary;
            dgvPayroll.DefaultCellStyle.BackColor = CardBg;
            dgvPayroll.DefaultCellStyle.ForeColor = TextPrimary;

            GroupBox grpAddPayroll = new GroupBox { Text = "Process & Issue Employee Payroll Records", ForeColor = TextMuted, Font = new Font("Segoe UI", 10), Location = new Point(24, 410), Size = new Size(1200, 210) };

            grpAddPayroll.Controls.Add(new Label { Text = "Emp Name:", ForeColor = TextPrimary, Location = new Point(20, 35), AutoSize = true });
            txtEmpName = new TextBox { Location = new Point(110, 32), Width = 180, BackColor = BgDark, ForeColor = TextPrimary };

            grpAddPayroll.Controls.Add(new Label { Text = "Role:", ForeColor = TextPrimary, Location = new Point(310, 35), AutoSize = true });
            txtEmpRole = new TextBox { Location = new Point(360, 32), Width = 140, Text = "Staff", BackColor = BgDark, ForeColor = TextPrimary };

            grpAddPayroll.Controls.Add(new Label { Text = "Base Salary ($):", ForeColor = TextPrimary, Location = new Point(520, 35), AutoSize = true });
            txtBaseSalary = new TextBox { Location = new Point(630, 32), Width = 110, Text = "3000.00", BackColor = BgDark, ForeColor = TextPrimary };

            grpAddPayroll.Controls.Add(new Label { Text = "Bonuses ($):", ForeColor = TextPrimary, Location = new Point(760, 35), AutoSize = true });
            txtBonus = new TextBox { Location = new Point(850, 32), Width = 90, Text = "200.00", BackColor = BgDark, ForeColor = TextPrimary };

            grpAddPayroll.Controls.Add(new Label { Text = "Deductions ($):", ForeColor = TextPrimary, Location = new Point(960, 35), AutoSize = true });
            txtDeductions = new TextBox { Location = new Point(1070, 32), Width = 90, Text = "150.00", BackColor = BgDark, ForeColor = TextPrimary };

            grpAddPayroll.Controls.AddRange(new Control[] { txtEmpName, txtEmpRole, txtBaseSalary, txtBonus, txtDeductions });

            Button btnProcessPayroll = new Button { Text = "⚡ Process & Save Payroll Record", Location = new Point(20, 95), Width = 300, Height = 42, BackColor = AccentPurple, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnProcessPayroll.FlatAppearance.BorderSize = 0;
            btnProcessPayroll.Click += async (s, e) => await ProcessPayrollAsync();

            Button btnApprovePayroll = new Button { Text = "✅ Approve & Pay Selected Record", Location = new Point(340, 95), Width = 300, Height = 42, BackColor = AccentGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnApprovePayroll.FlatAppearance.BorderSize = 0;
            btnApprovePayroll.Click += async (s, e) => await ApprovePayrollAsync();

            grpAddPayroll.Controls.AddRange(new Control[] { btnProcessPayroll, btnApprovePayroll });

            viewPayroll.Controls.AddRange(new Control[] { lblTitle, dgvPayroll, grpAddPayroll });
        }

        // ==========================================
        // MODULE 4: SUPPLIER ORDERS VIEW
        // ==========================================
        private void BuildSupplierOrdersView()
        {
            viewSupplierOrders = new Panel { Padding = new Padding(24) };

            Label lblTitle = new Label { Text = "Supplier Purchasing & Order Approvals (UC7, UC16, UC17, UC18)", ForeColor = TextPrimary, Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, Location = new Point(24, 24) };

            dgvSupplierOrders = new DataGridView
            {
                Location = new Point(24, 70),
                Size = new Size(1200, 320),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = CardBg,
                ForeColor = TextPrimary,
                GridColor = BorderColor,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false
            };
            dgvSupplierOrders.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(35, 35, 35);
            dgvSupplierOrders.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary;
            dgvSupplierOrders.DefaultCellStyle.BackColor = CardBg;
            dgvSupplierOrders.DefaultCellStyle.ForeColor = TextPrimary;

            GroupBox grpPO = new GroupBox { Text = "Create & Approve Purchase Orders", ForeColor = TextMuted, Font = new Font("Segoe UI", 10), Location = new Point(24, 410), Size = new Size(1200, 210) };

            grpPO.Controls.Add(new Label { Text = "Supplier Name:", ForeColor = TextPrimary, Location = new Point(20, 35), AutoSize = true });
            cbSuppliers = new ComboBox { Location = new Point(130, 32), Width = 220, BackColor = BgDark, ForeColor = TextPrimary, DropDownStyle = ComboBoxStyle.DropDownList };
            cbSuppliers.Items.AddRange(new object[] { "Apex Hardware Wholesale", "BuildRight Supply Corp", "Master Fasteners Ltd" });
            cbSuppliers.SelectedIndex = 0;
            grpPO.Controls.Add(cbSuppliers);

            grpPO.Controls.Add(new Label { Text = "Product ID:", ForeColor = TextPrimary, Location = new Point(370, 35), AutoSize = true });
            txtPoProdId = new TextBox { Location = new Point(450, 32), Width = 90, Text = "1", BackColor = BgDark, ForeColor = TextPrimary };
            grpPO.Controls.Add(txtPoProdId);

            grpPO.Controls.Add(new Label { Text = "Order Qty:", ForeColor = TextPrimary, Location = new Point(560, 35), AutoSize = true });
            txtPoQty = new TextBox { Location = new Point(640, 32), Width = 90, Text = "50", BackColor = BgDark, ForeColor = TextPrimary };
            grpPO.Controls.Add(txtPoQty);

            grpPO.Controls.Add(new Label { Text = "Unit Cost ($):", ForeColor = TextPrimary, Location = new Point(750, 35), AutoSize = true });
            txtPoCost = new TextBox { Location = new Point(840, 32), Width = 90, Text = "12.50", BackColor = BgDark, ForeColor = TextPrimary };
            grpPO.Controls.Add(txtPoCost);

            Button btnCreatePO = new Button { Text = "📝 Create PO (UC16)", Location = new Point(20, 95), Width = 220, Height = 42, BackColor = AccentBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnCreatePO.FlatAppearance.BorderSize = 0;
            btnCreatePO.Click += async (s, e) => await CreatePOAsync();

            Button btnValidatePO = new Button { Text = "🔍 Validate PO (UC18)", Location = new Point(255, 95), Width = 220, Height = 42, BackColor = AccentOrange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnValidatePO.FlatAppearance.BorderSize = 0;
            btnValidatePO.Click += async (s, e) => await ChangePOStatusAsync("Validated");

            Button btnApproveFunds = new Button { Text = "💰 Approve PO Funds (UC7)", Location = new Point(490, 95), Width = 240, Height = 42, BackColor = AccentGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnApproveFunds.FlatAppearance.BorderSize = 0;
            btnApproveFunds.Click += async (s, e) => await ChangePOStatusAsync("Funds Approved");

            Button btnReceiveDelivery = new Button { Text = "📦 Receive Delivery (UC17)", Location = new Point(745, 95), Width = 240, Height = 42, BackColor = AccentPurple, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnReceiveDelivery.FlatAppearance.BorderSize = 0;
            btnReceiveDelivery.Click += async (s, e) => await ChangePOStatusAsync("Received");

            grpPO.Controls.AddRange(new Control[] { btnCreatePO, btnValidatePO, btnApproveFunds, btnReceiveDelivery });

            viewSupplierOrders.Controls.AddRange(new Control[] { lblTitle, dgvSupplierOrders, grpPO });
        }

        // ==========================================
        // MODULE 5: REPORTS & TRANSACTION HISTORY VIEW
        // ==========================================
        private void BuildReportsView()
        {
            viewReports = new Panel { Padding = new Padding(24) };

            Label lblTitle = new Label { Text = "Transaction History & Financial Analytics", ForeColor = TextPrimary, Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, Location = new Point(24, 24) };

            Button btnRefresh = new Button { Text = "🔄 Refresh Data", Location = new Point(1080, 20), Size = new Size(140, 38), BackColor = CardBg, ForeColor = TextPrimary, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnRefresh.FlatAppearance.BorderColor = BorderColor;
            btnRefresh.Click += async (s, e) => await LoadReportsAsync();

            Panel cardRevenue = CreateReportDataCard("Total Sales Revenue", out lblRevenueValue, new Point(24, 75), AccentBlue, (s, e) => LoadReportContext("Revenue & Sales Summary"));
            Panel cardTrans = CreateReportDataCard("Completed Transactions", out lblTransValue, new Point(360, 75), AccentGreen, (s, e) => LoadReportContext("Transaction Logs"));
            Panel cardTopProd = CreateReportDataCard("Top Selling Catalog Items", out lblTopProdValue, new Point(696, 75), AccentOrange, (s, e) => LoadReportContext("Top Selling Products"));

            lblReportContext = new Label { Text = "Detailed View: Top Selling Products", ForeColor = TextMuted, Font = new Font("Segoe UI", 11, FontStyle.Italic), AutoSize = true, Location = new Point(24, 200) };

            pnlReportChart = new Panel
            {
                Location = new Point(24, 230),
                Size = new Size(1200, 180),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = CardBg
            };
            pnlReportChart.Paint += RenderReportChart;

            dgvReportDetails = new DataGridView
            {
                Location = new Point(24, 425),
                Size = new Size(1200, 300),
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

            viewReports.Controls.AddRange(new Control[] { lblTitle, btnRefresh, cardRevenue, cardTrans, cardTopProd, lblReportContext, pnlReportChart, dgvReportDetails });
        }

        private Panel CreateReportDataCard(string title, out Label valLabel, Point loc, Color stripColor, EventHandler onClick)
        {
            Panel card = new Panel { Size = new Size(312, 105), Location = loc, BackColor = CardBg, Cursor = Cursors.Hand };
            Panel strip = new Panel { Size = new Size(4, 105), Dock = DockStyle.Left, BackColor = stripColor };

            Label lblTitle = new Label { Text = title, ForeColor = TextMuted, Font = new Font("Segoe UI", 10), Location = new Point(20, 18), AutoSize = true };
            valLabel = new Label { Text = "-", ForeColor = TextPrimary, Font = new Font("Segoe UI", 18, FontStyle.Bold), Location = new Point(18, 48), AutoSize = true };

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
            pnlReportChart.Invalidate();
        }

        private void RenderReportChart(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(CardBg);

            using Font titleFont = new Font("Segoe UI", 10, FontStyle.Bold);
            using Font labelFont = new Font("Segoe UI", 8);
            using Brush textBrush = new SolidBrush(TextPrimary);
            using Brush mutedBrush = new SolidBrush(TextMuted);

            g.DrawString($"Graph Visualizer — {_userRole} Revenue & Sales Analysis", titleFont, textBrush, 15, 12);

            if (_currentReport == null || !_currentReport.TopSellingProducts.Any()) return;

            int startX = 70;
            int startY = 140;
            int maxHeight = 90;
            int barWidth = 55;
            int spacing = 45;

            using Pen linePen = new Pen(BorderColor, 2);
            g.DrawLine(linePen, 40, startY, pnlReportChart.Width - 40, startY);

            decimal maxRev = _currentReport.TopSellingProducts.Max(p => p.TotalRevenue);
            if (maxRev <= 0) maxRev = 100;

            for (int i = 0; i < _currentReport.TopSellingProducts.Count; i++)
            {
                var item = _currentReport.TopSellingProducts[i];
                int barHeight = (int)((item.TotalRevenue / maxRev) * maxHeight);
                if (barHeight < 10) barHeight = 10;

                int x = startX + i * (barWidth + spacing);
                int y = startY - barHeight;

                using Brush barBrush = new SolidBrush(AccentGreen);
                g.FillRectangle(barBrush, x, y, barWidth, barHeight);

                g.DrawString($"${item.TotalRevenue:N0}", labelFont, textBrush, x, y - 18);
                string shortName = item.ProductName.Length > 9 ? item.ProductName.Substring(0, 9) + ".." : item.ProductName;
                g.DrawString(shortName, labelFont, mutedBrush, x - 2, startY + 5);
            }
        }

        // ==========================================
        // MODULE 6: STORE TERMS & CONDITIONS VIEW (UC8, UC9)
        // ==========================================
        private void BuildTermsView()
        {
            viewTerms = new Panel { Padding = new Padding(24) };

            Label lblTitle = new Label { Text = "Store Terms & Conditions Configuration (UC8, UC9)", ForeColor = TextPrimary, Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, Location = new Point(24, 24) };

            GroupBox grpTerms = new GroupBox { Text = "Configure Terms, Return Policies & Credit Rules", ForeColor = TextMuted, Font = new Font("Segoe UI", 10), Location = new Point(24, 70), Size = new Size(1200, 560) };

            grpTerms.Controls.Add(new Label { Text = "Return Policy Text:", ForeColor = TextPrimary, Location = new Point(20, 35), AutoSize = true });
            rtbReturnPolicy = new RichTextBox { Location = new Point(20, 60), Size = new Size(1150, 100), BackColor = BgDark, ForeColor = TextPrimary, Text = "Items can be returned within 7 days of purchase with valid official store receipt." };

            grpTerms.Controls.Add(new Label { Text = "Store Credit & Rules Text:", ForeColor = TextPrimary, Location = new Point(20, 180), AutoSize = true });
            rtbCreditRules = new RichTextBox { Location = new Point(20, 205), Size = new Size(1150, 100), BackColor = BgDark, ForeColor = TextPrimary, Text = "Store credit will be issued upon approval. Returned products must be in original condition." };

            grpTerms.Controls.Add(new Label { Text = "General Sales Terms & Conditions:", ForeColor = TextPrimary, Location = new Point(20, 325), AutoSize = true });
            rtbGeneralTerms = new RichTextBox { Location = new Point(20, 350), Size = new Size(1150, 120), BackColor = BgDark, ForeColor = TextPrimary, Text = "All sales final after 7 days. Guarantee voids if seal broken or altered." };

            Button btnSaveTerms = new Button { Text = "💾 Save Store Terms & Conditions", Location = new Point(20, 490), Size = new Size(320, 45), BackColor = AccentGreen, ForeColor = Color.White, Font = new Font("Segoe UI", 11, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnSaveTerms.FlatAppearance.BorderSize = 0;
            btnSaveTerms.Click += async (s, e) => await SaveStoreTermsAsync();

            grpTerms.Controls.AddRange(new Control[] { rtbReturnPolicy, rtbCreditRules, rtbGeneralTerms, btnSaveTerms });
            viewTerms.Controls.AddRange(new Control[] { lblTitle, grpTerms });
        }

        private async Task SaveStoreTermsAsync()
        {
            var payload = new StoreTermsDto
            {
                ReturnPolicy = rtbReturnPolicy.Text,
                CreditRules = rtbCreditRules.Text,
                GeneralTerms = rtbGeneralTerms.Text
            };

            var response = await _httpClient.PutAsJsonAsync($"tenant/{_currentCompanyId}/terms", payload);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Store Terms & Conditions updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ==========================================
        // API CALLS & ACTION HANDLERS
        // ==========================================

        private async Task CreateProductAsync()
        {
            if (string.IsNullOrWhiteSpace(txtProdCode.Text) || string.IsNullOrWhiteSpace(txtProdName.Text) || !decimal.TryParse(txtProdPrice.Text, out decimal price))
            {
                MessageBox.Show("Please enter valid product code, name, and price.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show($"Are you sure you want to add product '{txtProdName.Text}' (${price:F2}) to catalog?", "Confirm New Product", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            var response = await _httpClient.PostAsJsonAsync($"tenant/{_currentCompanyId}/products", new { productCode = txtProdCode.Text.Trim(), productName = txtProdName.Text.Trim(), unitPrice = price, isActive = true });
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Product created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtProdCode.Clear(); txtProdName.Clear(); txtProdPrice.Clear();
                await LoadInventoryAsync();
            }
        }

        private async Task UpdateProductAsync()
        {
            if (string.IsNullOrWhiteSpace(txtAdjustProductId.Text) || !int.TryParse(txtAdjustProductId.Text, out int prodId))
            {
                MessageBox.Show("Please select a product from the inventory table first.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!decimal.TryParse(txtProdPrice.Text, out decimal price) || !decimal.TryParse(txtAdjustReorder.Text, out decimal reorder)) return;

            var confirm = MessageBox.Show($"Are you sure you want to update Product ID #{prodId}?", "Confirm Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            var response = await _httpClient.PutAsJsonAsync($"tenant/{_currentCompanyId}/products/{prodId}?reorderLevel={reorder}",
                new { productCode = txtProdCode.Text.Trim(), productName = txtProdName.Text.Trim(), unitPrice = price });

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Product updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadInventoryAsync();
            }
        }

        private async Task DeleteProductAsync()
        {
            if (string.IsNullOrWhiteSpace(txtAdjustProductId.Text) || !int.TryParse(txtAdjustProductId.Text, out int prodId))
            {
                MessageBox.Show("Please select a product from the inventory table first.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show($"⚠️ ARE YOU SURE YOU WANT TO DELETE PRODUCT #{prodId}?\nThis action cannot be undone.", "Confirm Delete Product", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            var response = await _httpClient.DeleteAsync($"tenant/{_currentCompanyId}/products/{prodId}");
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Product deleted from catalog.", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtProdCode.Clear(); txtProdName.Clear(); txtProdPrice.Clear(); txtAdjustProductId.Clear();
                await LoadInventoryAsync();
            }
        }

        private async Task AdjustStockAsync()
        {
            if (!int.TryParse(txtAdjustProductId.Text, out int prodId) || !decimal.TryParse(txtAdjustQty.Text, out decimal qty) || !decimal.TryParse(txtAdjustReorder.Text, out decimal reorder))
            {
                MessageBox.Show("Please select a product and enter a valid quantity change.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show($"Adjust stock quantity by {qty} for Product #{prodId}?", "Confirm Stock Adjustment", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            var response = await _httpClient.PostAsync($"tenant/{_currentCompanyId}/inventory/adjust?productId={prodId}&quantity={qty}&reorderLevel={reorder}", null);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Stock adjusted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtAdjustQty.Text = "0";
                await LoadInventoryAsync();
            }
        }

        private async Task ConvertBoxesAsync()
        {
            if (!int.TryParse(txtAdjustProductId.Text, out int prodId) || !decimal.TryParse(txtConvertBoxes.Text, out decimal boxes) || !decimal.TryParse(txtFactorUnits.Text, out decimal factor))
            {
                MessageBox.Show("Please select a product and enter valid box conversion numbers.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var response = await _httpClient.PostAsync($"tenant/{_currentCompanyId}/inventory/convert-boxes?productId={prodId}&boxesToConvert={boxes}&factorToUnits={factor}", null);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show($"Converted {boxes} boxes into product units!", "Conversion Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadInventoryAsync();
            }
        }

        private async Task RequestStockAuditAsync()
        {
            if (!int.TryParse(txtAdjustProductId.Text, out int prodId) || !decimal.TryParse(txtAuditPhysQty.Text, out decimal physQty))
            {
                MessageBox.Show("Please select a product and enter physical stock count.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var currentProduct = _allProducts.FirstOrDefault(p => p.ProductId == prodId);
            decimal sysQty = currentProduct?.QuantityOnHand ?? 0;

            var auditReq = new StockAuditDto
            {
                ProductId = prodId,
                RequestedBy = _userRole,
                SystemQty = sysQty,
                PhysicalQty = physQty
            };

            var response = await _httpClient.PostAsJsonAsync($"tenant/{_currentCompanyId}/stock-audits", auditReq);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Physical Stock Audit request sent to Branch Manager!", "Audit Requested", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAuditsAsync();
            }
        }

        private async Task ApproveStockAuditAsync()
        {
            if (dgvAuditGrid.CurrentRow == null || !(dgvAuditGrid.CurrentRow.DataBoundItem is StockAuditDto sel))
            {
                MessageBox.Show("Select an audit request from the audit table first.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var response = await _httpClient.PutAsync($"tenant/{_currentCompanyId}/stock-audits/{sel.AuditRequestId}/validate?status=Approved&validatedBy={_userRole}", null);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Stock Audit approved and stock updated!", "Audit Approved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAuditsAsync();
                await LoadInventoryAsync();
            }
        }

        private async Task LoadAuditsAsync()
        {
            try
            {
                _auditList = await _httpClient.GetFromJsonAsync<List<StockAuditDto>>($"tenant/{_currentCompanyId}/stock-audits") ?? new List<StockAuditDto>();
                dgvAuditGrid.DataSource = _auditList;
                LoadDashboardRoleCards();
            }
            catch { }
        }

        private async Task LoadInventoryAsync()
        {
            try
            {
                _allProducts = await _httpClient.GetFromJsonAsync<List<InventoryViewDto>>($"tenant/{_currentCompanyId}/inventory") ?? new List<InventoryViewDto>();
                dgvInventory.DataSource = _allProducts;
                dgvSearchResults.DataSource = _allProducts.Take(10).ToList();
                LoadDashboardRoleCards();
            }
            catch { }
        }

        private void AddToCart()
        {
            if (!int.TryParse(txtSaleProductId.Text, out int prodId) || !decimal.TryParse(txtSaleQty.Text, out decimal qty) || !decimal.TryParse(txtUnitPrice.Text, out decimal price))
            {
                MessageBox.Show("Please select or enter valid Product ID, Quantity, and Price.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var prodName = _allProducts.FirstOrDefault(p => p.ProductId == prodId)?.ProductName ?? $"Product #{prodId}";

            _cart.Add(new CartItemDto { ProductId = prodId, ProductName = prodName, Quantity = qty, UnitPrice = price, SubTotal = qty * price });
            dgvCart.DataSource = null;
            dgvCart.DataSource = _cart;
            lblTotalAmount.Text = $"Total Amount: ${_cart.Sum(x => x.SubTotal):F2}";
            LoadDashboardRoleCards();
        }

        private async Task CompleteSaleAsync()
        {
            if (!_cart.Any())
            {
                MessageBox.Show("Cart is empty! Add items before checkout.", "Empty Cart", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var total = _cart.Sum(x => x.SubTotal);
            var confirm = MessageBox.Show($"Process Checkout for ${_cart.Count} items? Total Amount: ${total:F2}", "Confirm Sale Checkout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

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
                // SHOW CUSTOMER RECEIPT MODAL DIALOG WITH T&CS
                ShowReceiptModal(salePayload.InvoiceNumber, total);

                _cart.Clear();
                dgvCart.DataSource = null;
                lblTotalAmount.Text = "Total Amount: $0.00";
                txtInvoiceNum.Text = "INV-" + DateTime.Now.ToString("fff");
                await RefreshAllDataAsync();
            }
            else
            {
                MessageBox.Show($"Transaction failed: {await response.Content.ReadAsStringAsync()}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowReceiptModal(string invoiceNum, decimal totalAmount)
        {
            Form dlgReceipt = new Form
            {
                Text = "🧾 Official Customer Receipt (UC10, UC20)",
                Size = new Size(450, 600),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(28, 28, 28),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false
            };

            Label lblHeader = new Label
            {
                Text = $"⚡ TENANT {_currentCompanyId} HARDWARE ENTERPRISE\nOFFICIAL SALES RECEIPT",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = AccentGreen,
                TextAlign = ContentAlignment.TopCenter,
                Dock = DockStyle.Top,
                Height = 60
            };

            RichTextBox rtbReceipt = new RichTextBox
            {
                Location = new Point(20, 70),
                Size = new Size(395, 410),
                BackColor = Color.FromArgb(18, 18, 18),
                ForeColor = TextPrimary,
                Font = new Font("Consolas", 10),
                ReadOnly = true,
                BorderStyle = BorderStyle.None
            };

            rtbReceipt.AppendText($"Invoice #: {invoiceNum}\n");
            rtbReceipt.AppendText($"Date: {DateTime.Now:yyyy-MM-dd HH:mm}\n");
            rtbReceipt.AppendText($"Cashier: {_userRole} ({_userEmail})\n");
            rtbReceipt.AppendText("----------------------------------------\n");
            rtbReceipt.AppendText(string.Format("{0,-18} {1,5} {2,12}\n", "Item Name", "Qty", "Subtotal"));
            rtbReceipt.AppendText("----------------------------------------\n");

            foreach (var item in _cart)
            {
                string name = item.ProductName.Length > 18 ? item.ProductName.Substring(0, 18) : item.ProductName;
                rtbReceipt.AppendText(string.Format("{0,-18} {1,5} ${2,11:F2}\n", name, item.Quantity, item.SubTotal));
            }

            rtbReceipt.AppendText("----------------------------------------\n");
            rtbReceipt.AppendText($"GRAND TOTAL: ${totalAmount:F2}\n");
            rtbReceipt.AppendText("========================================\n");
            rtbReceipt.AppendText("STORE TERMS & CONDITIONS (UC10):\n");
            rtbReceipt.AppendText("• Return Policy: 7 Days with receipt.\n");
            rtbReceipt.AppendText("• Guarantee void if seal broken.\n");
            rtbReceipt.AppendText("   Thank you for your business!   \n");

            Button btnClose = new Button
            {
                Text = "Close Receipt Dialog",
                Location = new Point(20, 495),
                Size = new Size(395, 42),
                BackColor = AccentBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => dlgReceipt.Close();

            dlgReceipt.Controls.AddRange(new Control[] { lblHeader, rtbReceipt, btnClose });
            dlgReceipt.ShowDialog();
        }

        private async Task ProcessPayrollAsync()
        {
            if (string.IsNullOrWhiteSpace(txtEmpName.Text) || !decimal.TryParse(txtBaseSalary.Text, out decimal baseSal))
            {
                MessageBox.Show("Please enter employee name and valid base salary.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal.TryParse(txtBonus.Text, out decimal bonus);
            decimal.TryParse(txtDeductions.Text, out decimal ded);

            var confirm = MessageBox.Show($"Process Payroll record for '{txtEmpName.Text}'?\nBase: ${baseSal:F2}, Net Pay: ${(baseSal + bonus - ded):F2}", "Confirm Payroll", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            var rec = new PayrollRecordDto
            {
                EmployeeId = new Random().Next(100, 999),
                EmployeeName = txtEmpName.Text.Trim(),
                Role = txtEmpRole.Text.Trim(),
                BaseSalary = baseSal,
                Bonuses = bonus,
                Deductions = ded,
                PayPeriod = DateTime.Now.ToString("yyyy-MM")
            };

            var response = await _httpClient.PostAsJsonAsync($"tenant/{_currentCompanyId}/payroll/process", rec);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Payroll record processed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadPayrollAsync();
            }
        }

        private async Task ApprovePayrollAsync()
        {
            if (dgvPayroll.CurrentRow == null || !(dgvPayroll.CurrentRow.DataBoundItem is PayrollRecordDto sel))
            {
                MessageBox.Show("Select a payroll record from the list first.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show($"Approve and issue payment of ${sel.NetPay:F2} for {sel.EmployeeName}?", "Confirm Payroll Payment", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            var response = await _httpClient.PutAsync($"tenant/{_currentCompanyId}/payroll/{sel.PayrollId}/approve", null);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Payroll payment approved and disbursed!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadPayrollAsync();
            }
        }

        private async Task LoadPayrollAsync()
        {
            try
            {
                _payrollList = await _httpClient.GetFromJsonAsync<List<PayrollRecordDto>>($"tenant/{_currentCompanyId}/payroll") ?? new List<PayrollRecordDto>();
                dgvPayroll.DataSource = _payrollList;
                LoadDashboardRoleCards();
            }
            catch { }
        }

        private async Task CreatePOAsync()
        {
            if (!int.TryParse(txtPoProdId.Text, out int prodId) || !decimal.TryParse(txtPoQty.Text, out decimal qty) || !decimal.TryParse(txtPoCost.Text, out decimal cost))
            {
                MessageBox.Show("Please enter valid PO details (Product ID, Qty, Cost).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var suppName = cbSuppliers.SelectedItem?.ToString() ?? "Supplier";
            var confirm = MessageBox.Show($"Create Purchase Order to '{suppName}' for {qty} units?", "Confirm PO Creation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            var prodName = _allProducts.FirstOrDefault(p => p.ProductId == prodId)?.ProductName ?? $"Product #{prodId}";

            var po = new PurchaseOrderDto
            {
                SupplierId = cbSuppliers.SelectedIndex + 1,
                SupplierName = suppName,
                Items = new List<PurchaseOrderItemDto> { new PurchaseOrderItemDto { ProductId = prodId, ProductName = prodName, Quantity = qty, UnitCost = cost } }
            };

            var response = await _httpClient.PostAsJsonAsync($"tenant/{_currentCompanyId}/purchase-orders", po);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Purchase Order created in Draft state!", "PO Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadPurchaseOrdersAsync();
            }
        }

        private async Task ChangePOStatusAsync(string targetStatus)
        {
            if (dgvSupplierOrders.CurrentRow == null || !(dgvSupplierOrders.CurrentRow.DataBoundItem is PurchaseOrderDto sel))
            {
                MessageBox.Show("Select a Purchase Order from the table first.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show($"Transition Purchase Order #{sel.OrderNumber} status to '{targetStatus}'?", "Confirm PO Action", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            var response = await _httpClient.PutAsync($"tenant/{_currentCompanyId}/purchase-orders/{sel.PurchaseOrderId}/status?status={targetStatus}", null);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show($"Purchase Order status updated to '{targetStatus}'!", "Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadPurchaseOrdersAsync();
                await LoadInventoryAsync();
            }
        }

        private async Task LoadPurchaseOrdersAsync()
        {
            try
            {
                _poList = await _httpClient.GetFromJsonAsync<List<PurchaseOrderDto>>($"tenant/{_currentCompanyId}/purchase-orders") ?? new List<PurchaseOrderDto>();
                dgvSupplierOrders.DataSource = _poList;
                LoadDashboardRoleCards();
            }
            catch { }
        }

        private async Task LoadReportsAsync()
        {
            try
            {
                _currentReport = await _httpClient.GetFromJsonAsync<SalesSummaryReportDto>($"tenant/{_currentCompanyId}/reports/sales-summary");
                if (_currentReport != null)
                {
                    lblRevenueValue.Text = $"${_currentReport.TotalRevenue:N2}";
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
            await LoadPayrollAsync();
            await LoadPurchaseOrdersAsync();
            await LoadReportsAsync();
            await LoadAuditsAsync();
        }
    }

    public class StoreTermsDto
    {
        public string ReturnPolicy { get; set; } = string.Empty;
        public string CreditRules { get; set; } = string.Empty;
        public string GeneralTerms { get; set; } = string.Empty;
    }

    public class StockAuditDto
    {
        public int AuditRequestId { get; set; }
        public int ProductId { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public decimal SystemQty { get; set; }
        public decimal PhysicalQty { get; set; }
        public decimal VarianceQty { get; set; }
        public string Status { get; set; } = "Pending";
        public string ValidatedBy { get; set; } = string.Empty;
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
        public string ProductName { get; set; } = string.Empty;
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

    public class PayrollRecordDto
    {
        public int PayrollId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public decimal BaseSalary { get; set; }
        public decimal Bonuses { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetPay { get; set; }
        public string PayPeriod { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime ProcessedAt { get; set; }
    }

    public class PurchaseOrderDto
    {
        public int PurchaseOrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Draft";
        public DateTime CreatedAt { get; set; }
        public List<PurchaseOrderItemDto> Items { get; set; } = new List<PurchaseOrderItemDto>();
    }

    public class PurchaseOrderItemDto
    {
        public int PurchaseOrderItemId { get; set; }
        public int PurchaseOrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal SubTotal { get; set; }
    }
}
