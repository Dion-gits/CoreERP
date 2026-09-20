using System;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hardware.winforms
{
    public partial class Login : Form
    {
        public int AuthenticatedCompanyId { get; private set; } = 1;
        public string AuthenticatedTenantEmail { get; private set; } = "tenant1@email";
        public string AuthenticatedRole { get; private set; } = "Owner";
        public string AuthenticatedUserName { get; private set; } = "Tenant Owner";

        private int currentStage = 1; // 1 = Tenant Login, 2 = User Login

        // Controls
        private Label lblHeader;
        private Label lblSubHeader;

        // Stage 1 Controls
        private Panel pnlStage1;
        private TextBox txtTenantEmail;
        private TextBox txtTenantPassword;
        private Button btnTenantLogin;

        // Stage 2 Controls
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
            this.Text = "Sign In - Small Enterprise ERP Core";
            this.Size = new Size(480, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(18, 18, 18);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            lblHeader = new Label
            {
                Text = "⚡ CORE ERP",
                ForeColor = Color.FromArgb(243, 244, 246),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(40, 30),
                AutoSize = true
            };

            lblSubHeader = new Label
            {
                Text = "Step 1: Tenant Organization Login",
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
                Text = "Tenant Email (e.g. tenant1@email)",
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
                Text = "Tenant Password (Default: 123123)",
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
                Text = "Continue to User Selection ➔",
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
                Text = "Valid Tenant Accounts:\n• tenant1@email  • tenant2@email  • tenant3@email\nPassword for all: 123123",
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

            Label lblRole = new Label
            {
                Text = "Select Position / Role Use Case",
                ForeColor = Color.FromArgb(243, 244, 246),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(0, 5),
                AutoSize = true
            };

            cbUserRole = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(0, 28),
                Size = new Size(380, 32),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(28, 28, 28),
                ForeColor = Color.FromArgb(243, 244, 246)
            };
            cbUserRole.Items.AddRange(new object[] { "Owner", "Super Admin", "HR Manager", "Branch Manager", "Cashier", "Inventory Staff" });
            cbUserRole.SelectedIndex = 0;

            Label lblUserEmail = new Label
            {
                Text = "User Account Email",
                ForeColor = Color.FromArgb(243, 244, 246),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(0, 75),
                AutoSize = true
            };

            txtUserEmail = new TextBox
            {
                Text = "owner@tenant1.com",
                Location = new Point(0, 98),
                Size = new Size(380, 32),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(28, 28, 28),
                ForeColor = Color.FromArgb(243, 244, 246),
                BorderStyle = BorderStyle.FixedSingle
            };

            cbUserRole.SelectedIndexChanged += (s, e) =>
            {
                string roleName = cbUserRole.SelectedItem?.ToString() ?? "Owner";
                txtUserEmail.Text = $"{roleName.Replace(" ", "").ToLower()}@{AuthenticatedTenantEmail.Split('@')[0]}.com";
            };

            Label lblUserPass = new Label
            {
                Text = "User Password (Default: 123123)",
                ForeColor = Color.FromArgb(243, 244, 246),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(0, 145),
                AutoSize = true
            };

            txtUserPassword = new TextBox
            {
                Text = "123123",
                UseSystemPasswordChar = true,
                Location = new Point(0, 168),
                Size = new Size(380, 32),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(28, 28, 28),
                ForeColor = Color.FromArgb(243, 244, 246),
                BorderStyle = BorderStyle.FixedSingle
            };

            btnUserLogin = new Button
            {
                Text = "Sign In to ERP Workspace 🚀",
                Location = new Point(0, 220),
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
                Text = "⬅ Change Tenant Account",
                Location = new Point(0, 275),
                Size = new Size(380, 35),
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.FromArgb(156, 163, 175),
                Font = new Font("Segoe UI", 9),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnBackToStage1.FlatAppearance.BorderSize = 0;
            btnBackToStage1.Click += (s, e) => ShowStage(1);

            pnlStage2.Controls.AddRange(new Control[] { lblRole, cbUserRole, lblUserEmail, txtUserEmail, lblUserPass, txtUserPassword, btnUserLogin, btnBackToStage1 });
        }

        private void ShowStage(int stage)
        {
            currentStage = stage;
            if (stage == 1)
            {
                lblSubHeader.Text = "Step 1 of 2: Tenant Organization Login";
                pnlStage1.Visible = true;
                pnlStage2.Visible = false;
            }
            else
            {
                lblSubHeader.Text = $"Step 2 of 2: User Position Login ({AuthenticatedTenantEmail})";
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
                MessageBox.Show("Invalid Tenant Password. Please enter '123123'.", "Authentication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            txtUserEmail.Text = $"owner@tenant{tenantId}.com";
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
            AuthenticatedUserName = $"{AuthenticatedRole} User";

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
