using System;
using System.Drawing;
using System.Windows.Forms;

namespace Hardware.winforms
{
    public partial class Login : Form
    {
        public int AuthenticatedCompanyId { get; private set; } = 1;
        public string AuthenticatedTenantEmail { get; private set; } = "tenant1@email";
        public string AuthenticatedRole { get; private set; } = "Owner";
        public string AuthenticatedUserEmail { get; private set; } = "owner1@email";

        private int currentStage = 1; // 1 = Tenant Company Login, 2 = Staff User Login

        // Modern Visual Theme
        private readonly Color BgDark = Color.FromArgb(18, 18, 18);
        private readonly Color CardBg = Color.FromArgb(28, 28, 28);
        private readonly Color TextPrimary = Color.FromArgb(243, 244, 246);
        private readonly Color TextMuted = Color.FromArgb(156, 163, 175);
        private readonly Color AccentGreen = Color.FromArgb(16, 185, 129);
        private readonly Color AccentBlue = Color.FromArgb(59, 130, 246);

        // Header Controls
        private Label lblHeader;
        private Label lblSubHeader;

        // Stage 1 Controls (Tenant Company Login)
        private Panel pnlStage1;
        private TextBox txtTenantEmail;
        private TextBox txtTenantPassword;
        private Button btnTenantLogin;

        // Stage 2 Controls (Staff Role Login)
        private Panel pnlStage2;
        private ComboBox cbUserRole;
        private TextBox txtUserEmail;
        private TextBox txtUserPassword;
        private Button btnUserLogin;
        private Button btnBackToStage1;

        public Login()
        {
            InitializeComponentCustom();
        }

        private void InitializeComponentCustom()
        {
            this.Text = "Sign In - Small Enterprise ERP Workspace";
            this.Size = new Size(500, 560);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = BgDark;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            lblHeader = new Label
            {
                Text = "⚡ CORE SMALL ERP",
                ForeColor = TextPrimary,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(45, 30),
                AutoSize = true
            };

            lblSubHeader = new Label
            {
                Text = "Stage 1: Tenant Company Authentication",
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 10),
                Location = new Point(45, 70),
                AutoSize = true
            };

            BuildStage1Panel();
            BuildStage2Panel();

            this.Controls.AddRange(new Control[] { lblHeader, lblSubHeader, pnlStage1, pnlStage2 });
            ShowStage(1);
        }

        private void BuildStage1Panel()
        {
            pnlStage1 = new Panel
            {
                Location = new Point(45, 110),
                Size = new Size(400, 380),
                BackColor = BgDark
            };

            Label lblEmail = new Label
            {
                Text = "Tenant Company Email (tenant1@email, tenant2@email, tenant3@email)",
                ForeColor = TextPrimary,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(0, 10),
                AutoSize = true
            };

            txtTenantEmail = new TextBox
            {
                Text = "tenant1@email",
                Location = new Point(0, 35),
                Size = new Size(395, 34),
                Font = new Font("Segoe UI", 11),
                BackColor = CardBg,
                ForeColor = TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblPass = new Label
            {
                Text = "Tenant Password (Default: 123123)",
                ForeColor = TextPrimary,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(0, 90),
                AutoSize = true
            };

            txtTenantPassword = new TextBox
            {
                Text = "123123",
                UseSystemPasswordChar = true,
                Location = new Point(0, 115),
                Size = new Size(395, 34),
                Font = new Font("Segoe UI", 11),
                BackColor = CardBg,
                ForeColor = TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnTenantLogin = new Button
            {
                Text = "Authenticate Tenant ➔",
                Location = new Point(0, 180),
                Size = new Size(395, 48),
                BackColor = AccentGreen,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnTenantLogin.FlatAppearance.BorderSize = 0;
            btnTenantLogin.Click += (s, e) => ValidateTenantStage();

            Label lblNotice = new Label
            {
                Text = "Valid Tenant Accounts:\n• tenant1@email  • tenant2@email  • tenant3@email\nPassword for all: 123123",
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                Location = new Point(0, 250),
                AutoSize = true
            };

            pnlStage1.Controls.AddRange(new Control[] { lblEmail, txtTenantEmail, lblPass, txtTenantPassword, btnTenantLogin, lblNotice });
        }

        private void BuildStage2Panel()
        {
            pnlStage2 = new Panel
            {
                Location = new Point(45, 110),
                Size = new Size(400, 380),
                BackColor = BgDark,
                Visible = false
            };

            Label lblRole = new Label
            {
                Text = "Select Position / Role (6 System Roles)",
                ForeColor = TextPrimary,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(0, 5),
                AutoSize = true
            };

            cbUserRole = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(0, 28),
                Size = new Size(395, 32),
                Font = new Font("Segoe UI", 10),
                BackColor = CardBg,
                ForeColor = TextPrimary
            };
            cbUserRole.Items.AddRange(new object[] { "Owner", "Super Admin", "HR Manager", "Branch Manager", "Cashier", "Inventory Staff" });
            cbUserRole.SelectedIndex = 0;

            Label lblUserEmail = new Label
            {
                Text = "Staff / User Email (e.g. owner1@email)",
                ForeColor = TextPrimary,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(0, 75),
                AutoSize = true
            };

            txtUserEmail = new TextBox
            {
                Text = "owner1@email",
                Location = new Point(0, 98),
                Size = new Size(395, 34),
                Font = new Font("Segoe UI", 11),
                BackColor = CardBg,
                ForeColor = TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };

            cbUserRole.SelectedIndexChanged += (s, e) => UpdateRoleEmailFormat();

            Label lblUserPass = new Label
            {
                Text = "User Password (Default: 123123)",
                ForeColor = TextPrimary,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(0, 148),
                AutoSize = true
            };

            txtUserPassword = new TextBox
            {
                Text = "123123",
                UseSystemPasswordChar = true,
                Location = new Point(0, 171),
                Size = new Size(395, 34),
                Font = new Font("Segoe UI", 11),
                BackColor = CardBg,
                ForeColor = TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnUserLogin = new Button
            {
                Text = "Sign In to ERP Workspace 🚀",
                Location = new Point(0, 225),
                Size = new Size(395, 48),
                BackColor = AccentBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnUserLogin.FlatAppearance.BorderSize = 0;
            btnUserLogin.Click += (s, e) => ValidateUserStage();

            btnBackToStage1 = new Button
            {
                Text = "⬅ Change Tenant Company",
                Location = new Point(0, 285),
                Size = new Size(395, 35),
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 9),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnBackToStage1.FlatAppearance.BorderSize = 0;
            btnBackToStage1.Click += (s, e) => ShowStage(1);

            pnlStage2.Controls.AddRange(new Control[] { lblRole, cbUserRole, lblUserEmail, txtUserEmail, lblUserPass, txtUserPassword, btnUserLogin, btnBackToStage1 });
        }

        private void UpdateRoleEmailFormat()
        {
            string roleName = cbUserRole.SelectedItem?.ToString() ?? "Owner";
            string rolePrefix = roleName switch
            {
                "Super Admin" => "superadmin",
                "Owner" => "owner",
                "HR Manager" => "hr",
                "Branch Manager" => "manager",
                "Cashier" => "cashier",
                "Inventory Staff" => "inventory",
                _ => "user"
            };

            txtUserEmail.Text = $"{rolePrefix}{AuthenticatedCompanyId}@email";
        }

        private void ShowStage(int stage)
        {
            currentStage = stage;
            if (stage == 1)
            {
                lblSubHeader.Text = "Stage 1 of 2: Tenant Company Authentication";
                pnlStage1.Visible = true;
                pnlStage2.Visible = false;
            }
            else
            {
                lblSubHeader.Text = $"Stage 2 of 2: Staff Login — Tenant {AuthenticatedCompanyId} ({AuthenticatedTenantEmail})";
                pnlStage1.Visible = false;
                pnlStage2.Visible = true;
                UpdateRoleEmailFormat();
            }
        }

        private void ValidateTenantStage()
        {
            string email = txtTenantEmail.Text.Trim().ToLower();
            string pass = txtTenantPassword.Text;

            if (pass != "123123")
            {
                MessageBox.Show("Invalid Tenant Password. Please use '123123'.", "Authentication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int tenantId = email switch
            {
                "tenant1@email" => 1,
                "tenant2@email" => 2,
                "tenant3@email" => 3,
                _ => 0
            };

            if (tenantId == 0)
            {
                MessageBox.Show("Tenant Email not recognized. Please use tenant1@email, tenant2@email, or tenant3@email.", "Invalid Tenant", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            AuthenticatedCompanyId = tenantId;
            AuthenticatedTenantEmail = email;
            ShowStage(2);
        }

        private void ValidateUserStage()
        {
            string pass = txtUserPassword.Text;
            if (pass != "123123")
            {
                MessageBox.Show("Invalid User Password. Please enter '123123'.", "Authentication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            AuthenticatedRole = cbUserRole.SelectedItem?.ToString() ?? "Owner";
            AuthenticatedUserEmail = txtUserEmail.Text.Trim();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
