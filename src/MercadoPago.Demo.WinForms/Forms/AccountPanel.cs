using System;
using System.Drawing;
using System.Windows.Forms;
using Newtonsoft.Json;
using Serilog;

namespace MercadoPago.Demo.WinForms.Forms
{
    /// <summary>Panel de información de cuenta y recursos del sitio.</summary>
    public class AccountPanel : UserControl
    {
        private readonly MainForm _main;
        private RichTextBox _txtResult;
        private DataGridView _gridPaymentMethods;
        private DataGridView _gridIdTypes;

        public AccountPanel(MainForm main)
        {
            _main = main;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Padding = new Padding(10);

            // Header con botones
            var header = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 45
            };

            var btnUserInfo = CreateButton("👤 Mi Cuenta", Color.FromArgb(0, 122, 204));
            btnUserInfo.Click += BtnUserInfo_Click;

            var btnBalance = CreateButton("💰 Balance", Color.FromArgb(40, 167, 69));
            btnBalance.Click += BtnBalance_Click;

            var btnPayMethods = CreateButton("💳 Métodos Pago", Color.FromArgb(108, 117, 125));
            btnPayMethods.Click += BtnPayMethods_Click;

            var btnIdTypes = CreateButton("🪪 Tipos Doc.", Color.FromArgb(156, 39, 176));
            btnIdTypes.Click += BtnIdTypes_Click;

            var btnInstallments = CreateButton("📊 Cuotas", Color.FromArgb(255, 152, 0));
            btnInstallments.Click += BtnInstallments_Click;

            header.Controls.AddRange(new Control[]
            { btnUserInfo, btnBalance, btnPayMethods, btnIdTypes, btnInstallments });

            // Split: grillas arriba, JSON abajo
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 300
            };

            // Tabs para las grillas
            var tabs = new TabControl { Dock = DockStyle.Fill };

            var tabMethods = new TabPage("Métodos de Pago");
            _gridPaymentMethods = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false
            };
            tabMethods.Controls.Add(_gridPaymentMethods);

            var tabIdTypes = new TabPage("Tipos de Documento");
            _gridIdTypes = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false
            };
            tabIdTypes.Controls.Add(_gridIdTypes);

            tabs.TabPages.Add(tabMethods);
            tabs.TabPages.Add(tabIdTypes);
            split.Panel1.Controls.Add(tabs);

            _txtResult = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.FromArgb(220, 220, 220)
            };
            split.Panel2.Controls.Add(_txtResult);

            Controls.Add(split);
            Controls.Add(header);
        }

        private async void BtnUserInfo_Click(object sender, EventArgs e)
        {
            if (_main.MpClient == null) { ShowNoClient(); return; }
            try
            {
                _main.SetStatus("Obteniendo info de usuario...");
                var result = await _main.MpClient.Account.GetUserInfoAsync();
                if (result.IsSuccess)
                {
                    var u = result.Data;
                    var info = $"═══ Mi Cuenta de MercadoPago ═══\n\n" +
                        $"🆔 User ID: {u.Id}\n" +
                        $"👤 Nombre: {u.FirstName} {u.LastName}\n" +
                        $"📧 Email: {u.Email}\n" +
                        $"🏷 Nickname: {u.Nickname}\n" +
                        $"🌐 Site: {u.SiteId}\n" +
                        $"🏳 País: {u.CountryId}\n" +
                        $"📅 Registro: {u.RegistrationDate}\n" +
                        $"🔗 Permalink: {u.Permalink}\n\n" +
                        "─── Detalle completo ───\n" +
                        JsonConvert.SerializeObject(u, Formatting.Indented);
                    ShowResult(info);
                    _main.SetStatus($"Usuario: {u.FirstName} {u.LastName} (ID: {u.Id})");
                }
                else ShowResult($"❌ {result.ErrorMessage}\n{result.RawJson}");
            }
            catch (Exception ex) { ShowResult($"Error: {ex.Message}"); }
        }

        private async void BtnBalance_Click(object sender, EventArgs e)
        {
            if (_main.MpClient == null) { ShowNoClient(); return; }
            try
            {
                _main.SetStatus("Obteniendo balance...");
                var result = await _main.MpClient.Account.GetAccountBalanceAsync();
                if (result.IsSuccess)
                {
                    var b = result.Data;
                    ShowResult(
                        $"═══ Balance de Cuenta ═══\n\n" +
                        $"💰 Balance Total: ${b.TotalAmount:N2} {b.CurrencyId}\n" +
                        $"✅ Disponible: ${b.AvailableBalance:N2}\n" +
                        $"⏳ No disponible: ${b.UnavailableBalance:N2}\n\n" +
                        JsonConvert.SerializeObject(b, Formatting.Indented));
                    _main.SetStatus($"Balance: ${b.AvailableBalance:N2} disponible");
                }
                else
                {
                    var msg = result.StatusCode == 403
                        ? "⚠ El endpoint de Balance requiere permisos de cuenta Marketplace.\n" +
                          "Este recurso no está disponible para cuentas estándar.\n\n"
                        : "";
                    ShowResult($"❌ {msg}{result.ErrorMessage}\n{result.RawJson}");
                }
            }
            catch (Exception ex) { ShowResult($"Error: {ex.Message}"); }
        }

        private async void BtnPayMethods_Click(object sender, EventArgs e)
        {
            if (_main.MpClient == null) { ShowNoClient(); return; }
            try
            {
                _main.SetStatus("Cargando métodos de pago...");
                var result = await _main.MpClient.Site.GetPaymentMethodsAsync();
                if (result.IsSuccess)
                {
                    _gridPaymentMethods.DataSource = result.Data;
                    _main.SetStatus($"{result.Data.Count} métodos de pago cargados.");
                    ShowResult(JsonConvert.SerializeObject(result.Data, Formatting.Indented));
                }
                else ShowResult($"❌ {result.ErrorMessage}\n{result.RawJson}");
            }
            catch (Exception ex)
            {
                ShowResult($"Error: {ex.Message}");
                Log.Error(ex, "Error cargando métodos de pago.");
            }
        }

        private async void BtnIdTypes_Click(object sender, EventArgs e)
        {
            if (_main.MpClient == null) { ShowNoClient(); return; }
            try
            {
                _main.SetStatus("Cargando tipos de documento...");
                var result = await _main.MpClient.Site.GetIdentificationTypesAsync();
                if (result.IsSuccess)
                {
                    _gridIdTypes.DataSource = result.Data;
                    _main.SetStatus($"{result.Data.Count} tipos de documento cargados.");
                    ShowResult(JsonConvert.SerializeObject(result.Data, Formatting.Indented));
                }
                else ShowResult($"❌ {result.ErrorMessage}\n{result.RawJson}");
            }
            catch (Exception ex)
            {
                ShowResult($"Error: {ex.Message}");
                Log.Error(ex, "Error cargando tipos de documento.");
            }
        }

        private async void BtnInstallments_Click(object sender, EventArgs e)
        {
            if (_main.MpClient == null) { ShowNoClient(); return; }
            var amountStr = Microsoft.VisualBasic.Interaction.InputBox(
                "Monto para calcular cuotas:", "Cuotas", "10000");
            decimal amount;
            if (!decimal.TryParse(amountStr, out amount)) return;

            try
            {
                _main.SetStatus("Calculando cuotas...");
                var result = await _main.MpClient.Site.GetInstallmentsAsync(amount);
                if (result.IsSuccess)
                {
                    var text = $"═══ Cuotas para ${amount:N2} ═══\n\n";
                    foreach (var info in result.Data)
                    {
                        text += $"📎 {info.PaymentMethodId} ({info.PaymentTypeId})\n";
                        if (info.Issuer != null)
                            text += $"   Emisor: {info.Issuer.Name}\n";
                        if (info.PayerCosts != null)
                        {
                            foreach (var cost in info.PayerCosts)
                            {
                                text += $"   {cost.Installments}x ${cost.InstallmentAmount:N2}" +
                                    $" = ${cost.TotalAmount:N2}" +
                                    $" ({cost.RecommendedMessage})\n";
                            }
                        }
                        text += "\n";
                    }
                    ShowResult(text);
                    _main.SetStatus("Cuotas calculadas.");
                }
                else ShowResult($"❌ {result.ErrorMessage}\n{result.RawJson}");
            }
            catch (Exception ex) { ShowResult($"Error: {ex.Message}"); }
        }

        private void ShowResult(string text)
        {
            if (InvokeRequired) { Invoke(new Action(() => _txtResult.Text = text)); return; }
            _txtResult.Text = text;
        }

        private void ShowNoClient()
        {
            MessageBox.Show("Configure las credenciales primero.",
                "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private Button CreateButton(string text, Color color)
        {
            return new Button
            {
                Text = text,
                Width = 140,
                Height = 32,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 5, 8, 0),
                Font = new Font("Segoe UI", 9)
            };
        }
    }
}
