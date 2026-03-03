using System;
using System.Windows.Forms;
using MercadoPago.Demo.WinForms.Data;
using Serilog;

namespace MercadoPago.Demo.WinForms
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Configurar Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/mpwrapper_demo_.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7)
                .CreateLogger();

            try
            {
                Log.Information("Iniciando MercadoPago Demo...");

                // Inicializar base de datos
                DatabaseInitializer.Initialize();

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Forms.MainForm());
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Error fatal en la aplicación.");
                MessageBox.Show(
                    $"Error fatal: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
