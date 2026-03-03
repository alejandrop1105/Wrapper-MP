using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using MercadoPago.Demo.WinForms.Data.Repositories;
using Newtonsoft.Json;
using Serilog;

namespace MercadoPago.Demo.WinForms.Forms
{
    /// <summary>Panel de configuración de credenciales y conexión.</summary>
    public class ConfigPanel : UserControl
    {
        private readonly MainForm _main;
        private TextBox _txtAccessToken;
        private TextBox _txtPublicKey;
        private TextBox _txtUserId;
        private ComboBox _cboEnvironment;
        private ComboBox _cboCountry;
        private NumericUpDown _nudWebhookPort;
        private TextBox _txtWebhookSecret;
        private RichTextBox _txtTestResult;

        public ConfigPanel(MainForm main)
        {
            _main = main;
            InitializeComponent();
            LoadConfig();
        }

        private void InitializeComponent()
        {
            Padding = new Padding(15);
            AutoScroll = true;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(5)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            int row = 0;

            // Credenciales
            AddHeader(layout, "Credenciales de MercadoPago", ref row);

            AddLabel(layout, "Access Token:", row);
            _txtAccessToken = AddTextBox(layout, row++, true);

            AddLabel(layout, "Public Key:", row);
            _txtPublicKey = AddTextBox(layout, row++);

            AddLabel(layout, "User ID:", row);
            _txtUserId = AddTextBox(layout, row++);

            AddLabel(layout, "Entorno:", row);
            _cboEnvironment = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 200
            };
            _cboEnvironment.Items.AddRange(new[] { "sandbox", "production" });
            _cboEnvironment.SelectedIndex = 0;
            layout.Controls.Add(_cboEnvironment, 1, row++);

            AddLabel(layout, "País:", row);
            _cboCountry = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 100
            };
            _cboCountry.Items.AddRange(new[] { "AR", "BR", "MX", "CO", "CL", "UY", "PE" });
            _cboCountry.SelectedIndex = 0;
            layout.Controls.Add(_cboCountry, 1, row++);

            // Webhook
            AddHeader(layout, "Webhook Listener", ref row);

            AddLabel(layout, "Puerto:", row);
            _nudWebhookPort = new NumericUpDown
            {
                Minimum = 1024,
                Maximum = 65535,
                Value = 5100,
                Width = 100
            };
            layout.Controls.Add(_nudWebhookPort, 1, row++);

            AddLabel(layout, "Secreto:", row);
            _txtWebhookSecret = AddTextBox(layout, row++);

            // Botones
            var btnPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 10)
            };

            var btnSave = new Button
            {
                Text = "💾 Guardar",
                Width = 120,
                Height = 35,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += BtnSave_Click;

            var btnTest = new Button
            {
                Text = "🔌 Test Conexión",
                Width = 140,
                Height = 35,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnTest.Click += BtnTest_Click;

            btnPanel.Controls.AddRange(new Control[] { btnSave, btnTest });
            layout.Controls.Add(btnPanel, 1, row++);

            // Resultado del test
            _txtTestResult = new RichTextBox
            {
                ReadOnly = true,
                Height = 150,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9)
            };
            layout.Controls.Add(_txtTestResult, 0, row);
            layout.SetColumnSpan(_txtTestResult, 2);

            Controls.Add(layout);
        }

        private void LoadConfig()
        {
            var cfg = _main.ConfigRepo.Get();
            if (cfg == null) return;

            _txtAccessToken.Text = cfg.AccessToken;
            _txtPublicKey.Text = cfg.PublicKey;
            _txtUserId.Text = cfg.UserId;
            _cboEnvironment.SelectedItem = cfg.Environment ?? "sandbox";
            _cboCountry.SelectedItem = cfg.Country ?? "AR";
            _nudWebhookPort.Value = cfg.WebhookPort > 0 ? cfg.WebhookPort : 5100;
            _txtWebhookSecret.Text = cfg.WebhookSecret;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            var cfg = new AppConfigEntity
            {
                Id = 1,
                AccessToken = _txtAccessToken.Text.Trim(),
                PublicKey = _txtPublicKey.Text.Trim(),
                UserId = _txtUserId.Text.Trim(),
                Environment = _cboEnvironment.SelectedItem?.ToString() ?? "sandbox",
                Country = _cboCountry.SelectedItem?.ToString() ?? "AR",
                WebhookPort = (int)_nudWebhookPort.Value,
                WebhookSecret = _txtWebhookSecret.Text.Trim()
            };

            _main.ConfigRepo.Save(cfg);
            _main.TryInitializeMpClient();
            _main.SetStatus("Configuración guardada exitosamente.");
            MessageBox.Show("Configuración guardada.", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void BtnTest_Click(object sender, EventArgs e)
        {
            if (_main.MpClient == null)
            {
                MessageBox.Show("Primero guarde las credenciales.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _txtTestResult.Text = "Probando conexión...";
            _main.SetStatus("Probando conexión con MercadoPago...");

            try
            {
                var result = await _main.MpClient.TestConnectionAsync();
                if (result.IsSuccess)
                {
                    var json = JsonConvert.SerializeObject(
                        result.Data, Formatting.Indented);
                    _txtTestResult.Text = $"✅ Conexión exitosa!\n\n{json}";
                    _main.SetStatus("Conexión exitosa con MercadoPago.");
                }
                else
                {
                    _txtTestResult.Text =
                        $"❌ Error: {result.ErrorMessage}\n{result.RawJson}";
                    _main.SetStatus("Error de conexión.");
                }
            }
            catch (Exception ex)
            {
                _txtTestResult.Text = $"❌ Excepción: {ex.Message}";
                _main.SetStatus("Error de conexión.");
                Log.Error(ex, "Error en test de conexión.");
            }
        }

        // ─── Helpers UI ───

        private void AddHeader(TableLayoutPanel layout,
            string text, ref int row)
        {
            var lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 15, 0, 5)
            };
            layout.Controls.Add(lbl, 0, row);
            layout.SetColumnSpan(lbl, 2);
            row++;
        }

        private void AddLabel(TableLayoutPanel layout,
            string text, int row)
        {
            layout.Controls.Add(new Label
            {
                Text = text,
                AutoSize = true,
                Margin = new Padding(0, 6, 0, 0)
            }, 0, row);
        }

        private TextBox AddTextBox(TableLayoutPanel layout,
            int row, bool password = false)
        {
            var txt = new TextBox
            {
                Width = 400,
                UseSystemPasswordChar = password
            };
            layout.Controls.Add(txt, 1, row);
            return txt;
        }
    }
}
