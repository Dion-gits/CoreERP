using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hardware.winforms
{
    public partial class Login : Form
    {
        private readonly HttpClient _httpClient;

        // UI Controls
        private Panel pnlLeftBranding = null!;
        private Panel pnlRightForm = null!;
        private PictureBox picLogo = null!;
        private Label lblTitle = null!;
        private Label lblSubtitle = null!;
        private Label lblEmail = null!;
        private Label lblPassword = null!;
        private TextBox txtEmail = null!;
        private TextBox txtPassword = null!;
        private Button btnLogin = null!;
        private Label lblClose = null!;

        public int AuthenticatedCompanyId { get; private set; } = -1;

        public Login()
        {
            InitializeComponentCustom();
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7166/") };
        }

        private void InitializeComponentCustom()
        {
            this.Text = "Micro-Enterprise ERP - Login";
            this.Size = new Size(960, 600);
            this.MinimumSize = new Size(900, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(248, 250, 252);

            // ==========================================
            // 1. LEFT BRANDING PANEL
            // ==========================================
            pnlLeftBranding = new Panel
            {
                Dock = DockStyle.Left,
                Width = 400,
                BackColor = Color.FromArgb(15, 23, 42)
            };
            pnlLeftBranding.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var brush = new LinearGradientBrush(
                    pnlLeftBranding.ClientRectangle,
                    Color.FromArgb(15, 23, 42),
                    Color.FromArgb(30, 41, 59),
                    45f);
                e.Graphics.FillRectangle(brush, pnlLeftBranding.ClientRectangle);
            };

            picLogo = new PictureBox
            {
                Size = new Size(80, 80),
                Location = new Point(160, 140),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
            try { picLogo.Image = Image.FromFile("logo.png"); } catch { }

            Label lblBrandTitle = new Label
            {
                Text = "Hardware ERP",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(50, 235),
                Size = new Size(300, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            Label lblBrandSubtitle = new Label
            {
                Text = "Multi-Tenant Enterprise System",
                ForeColor = Color.FromArgb(148, 163, 184),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Location = new Point(50, 275),
                Size = new Size(300, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            pnlLeftBranding.Controls.AddRange(new Control[] { picLogo, lblBrandTitle, lblBrandSubtitle });

            // ==========================================
            // 2. RIGHT FORM PANEL
            // ==========================================
            pnlRightForm = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(40)
            };

            // Fully functional close button
            lblClose = new Label
            {
                Text = "✕",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(520, 15),
                Size = new Size(30, 30),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            lblClose.Click += (s, e) => Application.Exit();
            lblClose.MouseEnter += (s, e) => lblClose.ForeColor = Color.Red;
            lblClose.MouseLeave += (s, e) => lblClose.ForeColor = Color.FromArgb(100, 116, 139);

            lblTitle = new Label
            {
                Text = "Log In",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(65, 85),
                AutoSize = true
            };

            lblSubtitle = new Label
            {
                Text = "Enter your credentials to log in",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(68, 135),
                AutoSize = true
            };

            lblEmail = new Label
            {
                Text = "Email Address",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Location = new Point(68, 180),
                AutoSize = true
            };

            txtEmail = new TextBox
            {
                Location = new Point(68, 205),
                Size = new Size(380, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle
            };

            lblPassword = new Label
            {
                Text = "Password",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Location = new Point(68, 260),
                AutoSize = true
            };

            txtPassword = new TextBox
            {
                Location = new Point(68, 285),
                Size = new Size(380, 32),
                Font = new Font("Segoe UI", 11),
                PasswordChar = '●',
                BorderStyle = BorderStyle.FixedSingle
            };

            btnLogin = new Button
            {
                Text = "Log In",
                Location = new Point(68, 355),
                Size = new Size(380, 48),
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += async (s, e) => await HandleLoginAsync();

            pnlRightForm.Controls.AddRange(new Control[] { lblClose, lblTitle, lblSubtitle, lblEmail, txtEmail, lblPassword, txtPassword, btnLogin });

            this.Controls.Add(pnlRightForm);
            this.Controls.Add(pnlLeftBranding);

            // Only enable dragging on the left branding panel to keep right-side inputs & close button fully interactive
            EnableWindowDragging(pnlLeftBranding);

            this.Resize += Login_Resize;
        }

        private void EnableWindowDragging(Control control)
        {
            control.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            };
        }

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        private void Login_Resize(object sender, EventArgs e)
        {
            if (this.Width > 850)
            {
                pnlLeftBranding.Width = (int)(this.Width * 0.42);
                lblClose.Location = new Point(pnlRightForm.Width - 40, 15);
            }
        }

        private async Task HandleLoginAsync()
        {
            string email = txtEmail.Text.Trim().ToLower();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both email and password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Hardcoded tenant credentials mapping (Password: 123123 for all 3 tenants)
            if (password == "123123")
            {
                if (email == "tenant1@email") { AuthenticatedCompanyId = 1; FinishLogin(); return; }
                if (email == "tenant2@email") { AuthenticatedCompanyId = 2; FinishLogin(); return; }
                if (email == "tenant3@email") { AuthenticatedCompanyId = 3; FinishLogin(); return; }
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("auth/login", new { Email = email, Password = password });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                    if (result != null)
                    {
                        AuthenticatedCompanyId = result.CompanyId;
                        FinishLogin();
                    }
                }
                else
                {
                    MessageBox.Show("Invalid login credentials or unassigned tenant database.", "Authentication Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FinishLogin()
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }

    public class LoginResponseDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
    }
}