using System;
using System.Drawing;
using System.Windows.Forms;
using MercadoPago.Demo.WinForms.Data.Repositories;
using MercadoPago.Wrapper;
using MercadoPago.Wrapper.Configuration;
using Serilog;

namespace MercadoPago.Demo.WinForms.Forms
{
    /// <summary>Formulario principal con navegación por tabs.</summary>
    public class MainForm : Form
    {
        private TabControl _tabControl;
        private ToolStripStatusLabel _statusLabel;
        private ToolStripStatusLabel _connectionLabel;

        // Servicios compartidos
        internal MpWrapperClient MpClient { get; private set; }
        internal ConfigRepository ConfigRepo { get; } = new ConfigRepository();
        internal OperationLogRepository LogRepo { get; } = new OperationLogRepository();
        internal WebhookLogRepository WebhookRepo { get; } = new WebhookLogRepository();

        public MainForm()
        {
            InitializeComponent();
            TryInitializeMpClient();
        }

        private void InitializeComponent()
        {
            Text = "MercadoPago Wrapper - Demo";
            Size = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(900, 600);

            // StatusBar
            var statusBar = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("Listo")
            { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            _connectionLabel = new ToolStripStatusLabel("⚪ Sin configurar")
            { Alignment = ToolStripItemAlignment.Right };
            statusBar.Items.AddRange(new ToolStripItem[]
            { _statusLabel, _connectionLabel });
            Controls.Add(statusBar);

            // Tab control
            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5f)
            };

            // Crear tabs
            _tabControl.TabPages.Add(CreateConfigTab());
            _tabControl.TabPages.Add(CreatePaymentsTab());
            _tabControl.TabPages.Add(CreateStoresTab());
            _tabControl.TabPages.Add(CreateCustomersTab());
            _tabControl.TabPages.Add(CreateOperationLogTab());
            _tabControl.TabPages.Add(CreateWebhookTab());

            Controls.Add(_tabControl);
        }

        internal void TryInitializeMpClient()
        {
            var cfg = ConfigRepo.Get();
            if (cfg == null || string.IsNullOrWhiteSpace(cfg.AccessToken))
            {
                UpdateConnectionStatus(false);
                return;
            }

            try
            {
                MpClient?.Dispose();

                var env = cfg.Environment == "production"
                    ? MpEnvironment.Production
                    : MpEnvironment.Sandbox;

                var config = new MpWrapperConfig.Builder()
                    .WithAccessToken(cfg.AccessToken)
                    .WithPublicKey(cfg.PublicKey ?? "")
                    .WithEnvironment(env)
                    .WithCountry(cfg.Country ?? "AR")
                    .Build();

                MpClient = new MpWrapperClient(
                    config,
                    cfg.UserId,
                    Log.Logger);

                UpdateConnectionStatus(true, env.ToString());
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "No se pudo inicializar el cliente MP.");
                UpdateConnectionStatus(false);
            }
        }

        internal void UpdateConnectionStatus(
            bool connected, string env = "")
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                    UpdateConnectionStatus(connected, env)));
                return;
            }

            if (connected)
            {
                _connectionLabel.Text = $"🟢 Conectado ({env})";
                _connectionLabel.ForeColor = Color.DarkGreen;
            }
            else
            {
                _connectionLabel.Text = "🔴 Desconectado";
                _connectionLabel.ForeColor = Color.DarkRed;
            }
        }

        internal void SetStatus(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetStatus(message)));
                return;
            }
            _statusLabel.Text = message;
        }

        // ─── Creación de Tabs ───

        private TabPage CreateConfigTab()
        {
            var tab = new TabPage("⚙ Configuración");
            tab.Controls.Add(new ConfigPanel(this) { Dock = DockStyle.Fill });
            return tab;
        }

        private TabPage CreatePaymentsTab()
        {
            var tab = new TabPage("💳 Pagos");
            tab.Controls.Add(new PaymentsPanel(this) { Dock = DockStyle.Fill });
            return tab;
        }

        private TabPage CreateStoresTab()
        {
            var tab = new TabPage("🏪 Sucursales / Cajas");
            tab.Controls.Add(new StoresPanel(this) { Dock = DockStyle.Fill });
            return tab;
        }

        private TabPage CreateCustomersTab()
        {
            var tab = new TabPage("👤 Clientes");
            tab.Controls.Add(new CustomersPanel(this) { Dock = DockStyle.Fill });
            return tab;
        }

        private TabPage CreateOperationLogTab()
        {
            var tab = new TabPage("📋 Log Operaciones");
            tab.Controls.Add(new OperationLogPanel(this) { Dock = DockStyle.Fill });
            return tab;
        }

        private TabPage CreateWebhookTab()
        {
            var tab = new TabPage("🔔 Webhooks");
            tab.Controls.Add(new WebhookPanel(this) { Dock = DockStyle.Fill });
            return tab;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            MpClient?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
