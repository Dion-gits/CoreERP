using System;
using System.Drawing;
using System.Windows.Forms;

namespace Hardware.winforms
{
    public partial class Login : Form
    {
        public int AuthenticatedCompanyId { get; private set; } = 1;
        public string AuthenticatedTenantEmail { get; private set; } = "tenant1@email";
        public string AuthenticatedUserEmail { get; private set; } = "owner1@email";
        public string AuthenticatedRole { get; private set; } = "Owner";
        public string AuthenticatedUserName { get; private set; } = "Owner User";

        private int currentStage = 1; // 1 = Tenant Login, 2 = Staff/User Login

        // Header controls
        private Label lblHeader = new Label();
        private Label lblSubHeader = new Label();

        // Stage 1 Controls (Tenant Login)
        private Panel pnlStage1 = new Panel();
        private TextBox txtTenantEmail = new TextBox();
        private TextBox txtTenantPassword = new TextBox();
        private Button btnTenantLogin = new Button();

        // Stage 2 Controls (User Login - ONLY Email and Password)
        private Panel pnlStage2 = new Panel();
        private TextBox txtUserEmail = new TextBox();
        private TextBox txtUserPassword = new TextBox();
        private Button btnUserLogin = new Button();
        private Button btnBackToStage1 = new Button();

        public Login()
        {
            InitializeComponentCustom();
        }

        private void InitializeComponentCustom()
        {
            this.Text = "Sign In - Small Enterprise ERP";
            this.Size = new Size(480, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(18, 18, 18);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            lblHeader = new Label
            {
                Text = "⚡ ERP WORKSPACE",
                ForeColor = Color.FromArgb(243, 244, 246),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(40, 30),
                AutoSize = true
            };

            lblSubHeader = new Label
            {
                Text = "Step 1 of 2: Tenant Company Login",
                ForeColor = Color.FromArgb(156, 163, 175),
                Font = new Font("Segoe UI", 10),
                Location = new Point(40, 70),
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
                Location = new Point(40, 110),
                Size = new Size(385, 340),
                BackColor = Color.FromArgb(18, 18, 18)
            };

            Label lblEmail = new Label
            {
                Text = "Tenant Company Email",
                ForeColor = Color.FromArgb(243, 244, 246),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(0, 10),
                AutoSize = true
            };

            txtTenantEmail = new TextBox
            {
                Text = "tenant1@email",
                Location = new Point(0, 35),
                Size = new Size(380, 32),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(28, 28, 28),
                ForeColor = Color.FromArgb(243, 244, 246),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblPass = new Label
            {
                Text = "Password",
                ForeColor = Color.FromArgb(243, 244, 246),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(0, 85),
                AutoSize = true
            };

            txtTenantPassword = new TextBox
            {
                Text = "123123",
                UseSystemPasswordChar = true,
                Location = new Point(0, 110),
                Size = new Size(380, 32),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(28, 28, 28),
                ForeColor = Color.FromArgb(243, 244, 246),
                BorderStyle = BorderStyle.FixedSingle
            };

            btnTenantLogin = new Button
            {
                Text = "Next: User Sign In ➔",
                Location = new Point(0, 175),
                Size = new Size(380, 45),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnTenantLogin.FlatAppearance.BorderSize = 0;
            btnTenantLogin.Click += (s, e) => ValidateTenantStage();

            Label lblNotice = new Label
            {
                Text = "Valid Tenant Accounts:\n• tenant1@email  • tenant2@email  • tenant3@email\nPassword: 123123",
                ForeColor = Color.FromArgb(156, 163, 175),
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                Location = new Point(0, 240),
                AutoSize = true
            };

            pnlStage1.Controls.AddRange(new Control[] { lblEmail, txtTenantEmail, lblPass, txtTenantPassword, btnTenantLogin, lblNotice });
        }

        private void BuildStage2Panel()
        {
            pnlStage2 = new Panel
            {
                Location = new Point(40, 110),
                Size = new Size(385, 360),
                BackColor = Color.FromArgb(18, 18, 18),
                Visible = false
            };

            Label lblUserEmail = new Label
            {
                Text = "Staff / User Email (e.g. owner1@email)",
                ForeColor = Color.FromArgb(243, 244, 246),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(0, 10),
                AutoSize = true
            };

            txtUserEmail = new TextBox
            {
                Text = "owner1@email",
                Location = new Point(0, 35),
                Size = new Size(380, 32),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(28, 28, 28),
                ForeColor = Color.FromArgb(243, 244, 246),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblUserPass = new Label
            {
                Text = "User Password",
                ForeColor = Color.FromArgb(243, 244, 246),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(0, 85),
                AutoSize = true
            };

            txtUserPassword = new TextBox
            {
                Text = "123123",
                UseSystemPasswordChar = true,
                Location = new Point(0, 110),
                Size = new Size(380, 32),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(28, 28, 28),
                ForeColor = Color.FromArgb(243, 244, 246),
                BorderStyle = BorderStyle.FixedSingle
            };

            btnUserLogin = new Button
            {
                Text = "Sign In to ERP 🚀",
                Location = new Point(0, 175),
                Size = new Size(380, 45),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnUserLogin.FlatAppearance.BorderSize = 0;
            btnUserLogin.Click += (s, e) => ValidateUserStage();

            btnBackToStage1 = new Button
            {
                Text = "⬅ Change Tenant",
                Location = new Point(0, 235),
                Size = new Size(380, 35),
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.FromArgb(156, 163, 175),
                Font = new Font("Segoe UI", 9),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnBackToStage1.FlatAppearance.BorderSize = 0;
            btnBackToStage1.Click += (s, e) => ShowStage(1);

            Label lblRoleFormats = new Label
            {
                Text = "User Formats (Role auto-detected):\n• owner1@email  • hrmanager1@email  • branchmanager1@email\n• cashier1@email  • inventory1@email  • superadmin1@email\nPassword: 123123",
                ForeColor = Color.FromArgb(156, 163, 175),
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                Location = new Point(0, 280),
                AutoSize = true
            };

            pnlStage2.Controls.AddRange(new Control[] { lblUserEmail, txtUserEmail, lblUserPass, txtUserPassword, btnUserLogin, btnBackToStage1, lblRoleFormats });
        }

        private void ShowStage(int stage)
        {
            currentStage = stage;
            if (stage == 1)
            {
                lblSubHeader.Text = "Step 1 of 2: Tenant Company Login";
                pnlStage1.Visible = true;
                pnlStage2.Visible = false;
            }
            else
            {
                lblSubHeader.Text = $"Step 2 of 2: Staff Login ({AuthenticatedTenantEmail})";
                pnlStage1.Visible = false;
                pnlStage2.Visible = true;
            }
        }

        private void ValidateTenantStage()
        {
            string email = txtTenantEmail.Text.Trim().ToLower();
            string pass = txtTenantPassword.Text;

            if (pass != "123123")
            {
                MessageBox.Show("Invalid Tenant Password. Default password is '123123'.", "Authentication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            // Set default user email for this tenant
            txtUserEmail.Text = $"owner{tenantId}@email";

            ShowStage(2);
        }

        private void ValidateUserStage()
        {
            string email = txtUserEmail.Text.Trim().ToLower();
            string pass = txtUserPassword.Text;

            if (pass != "123123")
            {
                MessageBox.Show("Invalid User Password. Default password is '123123'.", "Authentication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Please enter your staff email.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Determine role from email string format
            string role = ResolveRoleFromEmail(email);

            AuthenticatedUserEmail = email;
            AuthenticatedRole = role;
            AuthenticatedUserName = $"{role} User ({email})";

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public static string ResolveRoleFromEmail(string email)
        {
            string lower = email.ToLower();

            if (lower.Contains("superadmin") || lower.Contains("admin"))
                return "Super Admin";
            if (lower.Contains("owner"))
                return "Owner";
            if (lower.Contains("hrmanager") || lower.Contains("hr"))
                return "HR Manager";
            if (lower.Contains("branchmanager") || lower.Contains("manager"))
                return "Branch Manager";
            if (lower.Contains("cashier"))
                return "Cashier";
            if (lower.Contains("inventory") || lower.Contains("clerk") || lower.Contains("staff"))
                return "Inventory Staff";

            // Default fallback
            return "Owner";
        }
    }
}
