using System;
using System.Windows.Forms;

namespace Hardware.winforms
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Launch the renamed Login form first
            using (var loginForm = new Login())
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    int tenantId = loginForm.AuthenticatedCompanyId;

                    // Proceed to Main ERP Form after successful login
                    Application.Run(new MainErpForm());
                }
            }
        }
    }
}