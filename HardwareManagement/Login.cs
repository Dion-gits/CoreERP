using System;
using System.Drawing;
using System.Windows.Forms;

namespace Hardware.winforms
{
    public partial class Login : Form
    {
        public int AuthenticatedCompanyId { get; private set; } = 1;

        private TextBox txtCompanyId;
        private Button btnLogin;

        public Login()
        {
            InitializeComponentCustom();
        }

        private void InitializeComponentCustom()
        {
            this.Text = "Sign In - Core ERP";
            this.Size = new Size(420, 360);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(18, 18, 18);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Label lblTitle = new Label
            {
                Text = "⚡ CORE ERP",
                ForeColor = Color.FromArgb(243, 244, 246),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(40, 40),
                AutoSize = true
            };

            Label lblSubtitle = new Label
            {
                Text = "Enter your Company/Tenant ID to continue",
                ForeColor = Color.FromArgb(156, 163, 175),
                Font = new Font("Segoe UI", 10),
                Location = new Point(40, 75),
                AutoSize = true
            };

            Label lblId = new Label
            {
                Text = "Company ID",
                ForeColor = Color.FromArgb(243, 244, 246),
                Font = new Font("Segoe UI", 10),
                Location = new Point(40, 130),
                AutoSize = true
            };

            txtCompanyId = new TextBox
            {
                Text = "1",
                Location = new Point(40, 155),
                Size = new Size(320, 30),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(28, 28, 28),
                ForeColor = Color.FromArgb(243, 244, 246),
                BorderStyle = BorderStyle.FixedSingle
            };

            btnLogin = new Button
            {
                Text = "Sign In",
                Location = new Point(40, 215),
                Size = new Size(320, 45),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = ColorTranslator.FromHtml("#FFFFFF"),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += (s, e) =>
            {
                if (int.TryParse(txtCompanyId.Text, out int id))
                {
                    AuthenticatedCompanyId = id;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Please enter a valid numeric Company ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            this.Controls.AddRange(new Control[] { lblTitle, lblSubtitle, lblId, txtCompanyId, btnLogin });
        }
    }
}