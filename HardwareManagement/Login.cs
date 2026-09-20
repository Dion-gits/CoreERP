using System;
using System.Drawing;
using System.Windows.Forms;

namespace Hardware.winforms
{
    public partial class Login : Form
    {
        public int AuthenticatedCompanyId { get; private set; } = 1;

        private TextBox txtCompanyId = null!;
        private Button btnLogin = null!;

        public Login()
        {
            InitializeComponentCustom();
        }

        private void InitializeComponentCustom()
        {
            this.Text = "Sign In - Core ERP";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(15, 23, 42); // Deep modern dark slate
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;

            // Outer centered container panel
            Panel pnlCenterWrapper = new Panel
            {
                Size = new Size(460, 520),
                BackColor = Color.Transparent
            };

            // Main Glassmorphism/Card Container
            Panel cardPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 41, 59), // Slate 800
                Padding = new Padding(40)
            };

            // Accent Top Border Strip
            Panel topAccentStrip = new Panel
            {
                Dock = DockStyle.Top,
                Height = 5,
                BackColor = Color.FromArgb(16, 185, 129) // Emerald 500
            };

            // Brand Logo / Title
            Label lblBrand = new Label
            {
                Text = "⚡ CORE ERP",
                ForeColor = Color.FromArgb(248, 250, 252),
                Font = new Font("Segoe UI Semibold", 20, FontStyle.Bold),
                Location = new Point(40, 40),
                AutoSize = true
            };

            Label lblSubtitle = new Label
            {
                Text = "Micro-Enterprise Management Portal",
                ForeColor = Color.FromArgb(148, 163, 184), // Slate 400
                Font = new Font("Segoe UI", 10),
                Location = new Point(42, 80),
                AutoSize = true
            };

            Label lblDivider = new Label
            {
                BorderStyle = BorderStyle.Fixed3D,
                Location = new Point(40, 120),
                Size = new Size(380, 2)
            };

            Label lblCompanyIdLabel = new Label
            {
                Text = "TENANT / COMPANY ID",
                ForeColor = Color.FromArgb(203, 213, 225), // Slate 300
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(40, 150),
                AutoSize = true
            };

            // Input Wrapper Panel for smooth styling
            Panel txtWrapper = new Panel
            {
                Location = new Point(40, 180),
                Size = new Size(380, 48),
                BackColor = Color.FromArgb(15, 23, 42), // Dark slate input background
                Padding = new Padding(12, 10, 12, 10)
            };

            txtCompanyId = new TextBox
            {
                Text = "1",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.FromArgb(248, 250, 252),
                BorderStyle = BorderStyle.None
            };
            txtWrapper.Controls.Add(txtCompanyId);

            Label lblHint = new Label
            {
                Text = "Tip: Enter 1, 2, or 3 for standard tenant databases",
                ForeColor = Color.FromArgb(100, 116, 139), // Slate 500
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                Location = new Point(40, 235),
                AutoSize = true
            };

            btnLogin = new Button
            {
                Text = "SIGN IN TO WORKSPACE",
                Location = new Point(40, 290),
                Size = new Size(380, 52),
                BackColor = Color.FromArgb(16, 185, 129), // Emerald
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;

            // Hover state animation
            btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = Color.FromArgb(5, 150, 105);
            btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = Color.FromArgb(16, 185, 129);

            btnLogin.Click += (s, e) =>
            {
                if (int.TryParse(txtCompanyId.Text.Trim(), out int id) && id > 0)
                {
                    AuthenticatedCompanyId = id;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Please enter a valid numeric Company/Tenant ID.", "Authentication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            Button btnExit = new Button
            {
                Text = "Exit Application",
                Location = new Point(40, 360),
                Size = new Size(380, 42),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(148, 163, 184),
                Font = new Font("Segoe UI", 10),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                Application.Exit();
            };

            cardPanel.Controls.AddRange(new Control[] {
                lblBrand, lblSubtitle, lblDivider, lblCompanyIdLabel, txtWrapper, lblHint, btnLogin, btnExit
            });

            pnlCenterWrapper.Controls.Add(topAccentStrip);
            pnlCenterWrapper.Controls.Add(cardPanel);

            this.Controls.Add(pnlCenterWrapper);

            // Center the card panel when form resizes / loads
            this.Resize += (s, e) =>
            {
                pnlCenterWrapper.Location = new Point(
                    (this.ClientSize.Width - pnlCenterWrapper.Width) / 2,
                    (this.ClientSize.Height - pnlCenterWrapper.Height) / 2
                );
            };

            this.Load += (s, e) =>
            {
                pnlCenterWrapper.Location = new Point(
                    (this.ClientSize.Width - pnlCenterWrapper.Width) / 2,
                    (this.ClientSize.Height - pnlCenterWrapper.Height) / 2
                );
            };
        }
    }
}
