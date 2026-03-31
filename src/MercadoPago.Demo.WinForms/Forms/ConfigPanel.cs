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
        private TextBox _txtPlatformId;
        private TextBox _txtClientId;
        private TextBox _txtClientSecret;
        private TextBox _txtRefreshToken;
        private TextBox _txtRedirectUri;
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

            // Homologación
            AddHeader(layout, "Homologación MercadoPago", ref row);

            AddLabel(layout, "Platform ID:", row);
            _txtPlatformId = AddTextBox(layout, row++);

            // OAuth
            AddHeader(layout, "OAuth (Opcional)", ref row);

            AddLabel(layout, "Client ID:", row);
            _txtClientId = AddTextBox(layout, row++);

            AddLabel(layout, "Client Secret:", row);
            _txtClientSecret = AddTextBox(layout, row++, true);

            AddLabel(layout, "Refresh Token:", row);
            _txtRefreshToken = AddTextBox(layout, row++, true);

            AddLabel(layout, "Redirect URI:", row);
            _txtRedirectUri = AddTextBox(layout, row++);
            _txtRedirectUri.Text = "https://www.tusistema.com/oauth-callback"; // Default sugerido

            // Herramientas OAuth integradas
            var pnlOAuth = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Margin = new Padding(0, 5, 0, 5)
            };
            
            var btnAuthUrl = new Button { Text = "1. Abrir URL de Autorización", Width = 170, Height = 30, BackColor = Color.LightGray, FlatStyle = FlatStyle.Flat };
            btnAuthUrl.Click += BtnAuthUrl_Click;
            
            var btnExchange = new Button { Text = "2. Canjear Código (Code)", Width = 170, Height = 30, BackColor = Color.LightGray, FlatStyle = FlatStyle.Flat };
            btnExchange.Click += BtnExchange_Click;
            
            pnlOAuth.Controls.AddRange(new Control[] { btnAuthUrl, btnExchange });
            layout.Controls.Add(pnlOAuth, 1, row++);

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
            _txtPlatformId.Text = cfg.PlatformId;
            _txtClientId.Text = cfg.ClientId;
            _txtClientSecret.Text = cfg.ClientSecret;
            _txtRefreshToken.Text = cfg.RefreshToken;
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
                WebhookSecret = _txtWebhookSecret.Text.Trim(),
                PlatformId = _txtPlatformId.Text.Trim(),
                ClientId = _txtClientId.Text.Trim(),
                ClientSecret = _txtClientSecret.Text.Trim(),
                RefreshToken = _txtRefreshToken.Text.Trim()
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

        private void BtnAuthUrl_Click(object sender, EventArgs e)
        {
            var clientId = _txtClientId.Text.Trim();
            var redirUri = _txtRedirectUri.Text.Trim();

            if (string.IsNullOrEmpty(clientId))
            {
                MessageBox.Show("Debes ingresar el Client ID primero.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // MP usa 'mp' como platform_id para el entorno general y pasa tu redirect uri
            var url = $"https://auth.mercadopago.com/authorization?client_id={clientId}&response_type=code&platform_id=mp&redirect_uri={Uri.EscapeDataString(redirUri)}";
            
            try
            {
                System.Diagnostics.Process.Start(url);
                _txtTestResult.Text = "Navegador abierto con la URL de autorización.\n\nUna vez autorizado, te redirigirá a tu Redirect URI con un parámetro ?code=XXXX.\n\nCopia ese valor y presiona 'Canjear Código'.";
            }
            catch(Exception ex)
            {
                _txtTestResult.Text = $"Copia y pega esta URL en tu navegador de forma manual:\n\n{url}\n\nError al abrir navegador: {ex.Message}";
            }
        }

        private async void BtnExchange_Click(object sender, EventArgs e)
        {
            if (_main.MpClient == null)
            {
                MessageBox.Show("Guarda la configuración primero para inicializar el cliente MP antes de canjear el código.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var code = Microsoft.VisualBasic.Interaction.InputBox(
                "Pega aquí el código ('code') que obtuviste de la redirección:", 
                "Canjear Código OAuth", "");

            if (string.IsNullOrWhiteSpace(code)) return;

            _txtTestResult.Text = "Intercambiando código de autorización con MercadoPago...";
            
            try
            {
                var redirUri = _txtRedirectUri.Text.Trim();
                var resp = await _main.MpClient.OAuth.ExchangeCodeAsync(code.Trim(), redirUri);
                
                if (resp.IsSuccess && resp.Data != null)
                {
                    _txtAccessToken.Text = resp.Data.AccessToken ?? _txtAccessToken.Text;
                    _txtRefreshToken.Text = resp.Data.RefreshToken ?? _txtRefreshToken.Text;
                    
                    if (resp.Data.UserId.HasValue)
                        _txtUserId.Text = resp.Data.UserId.Value.ToString();
                    
                    _txtTestResult.Text = $"✅ OAuth Exitoso!\nAccess Token: {resp.Data.AccessToken?.Substring(0, 15)}...\nExpira en: {resp.Data.ExpiresIn}s";
                    MessageBox.Show("Tokens obtenidos y asignados en pantalla.\n\nRecuerda darle a guardar para persistirlos en la base de datos de la demo.", 
                        "OAuth Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _txtTestResult.Text = $"❌ Error OAuth: {resp.ErrorMessage}\n{resp.RawJson}";
                }
            }
            catch(Exception ex)
            {
                 _txtTestResult.Text = $"❌ Error: {ex.Message}";
                 Log.Error(ex, "Error en ExchangeCodeAsync");
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
