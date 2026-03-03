using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using MercadoPago.Demo.WinForms.Data.Repositories;
using MercadoPago.Wrapper.Models.Payments;
using MercadoPago.Wrapper.Models.Preferences;
using Newtonsoft.Json;
using Serilog;

namespace MercadoPago.Demo.WinForms.Forms
{
    /// <summary>Panel de operaciones de pagos y checkout.</summary>
    public class PaymentsPanel : UserControl
    {
        private readonly MainForm _main;
        private TextBox _txtAmount;
        private TextBox _txtDescription;
        private TextBox _txtExternalRef;
        private TextBox _txtPayerEmail;
        private RichTextBox _txtResult;
        private TextBox _txtPaymentId;

        public PaymentsPanel(MainForm main)
        {
            _main = main;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Padding = new Padding(10);

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 320
            };

            // ─── Panel superior: Formulario de pago ───
            var formPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(5)
            };
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            int row = 0;
            var header = new Label
            {
                Text = "Crear Pago / Checkout",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 10)
            };
            formPanel.Controls.Add(header, 0, row);
            formPanel.SetColumnSpan(header, 2);
            row++;

            formPanel.Controls.Add(new Label { Text = "Monto:", AutoSize = true }, 0, row);
            _txtAmount = new TextBox { Width = 200, Text = "100.00" };
            formPanel.Controls.Add(_txtAmount, 1, row++);

            formPanel.Controls.Add(new Label { Text = "Descripción:", AutoSize = true }, 0, row);
            _txtDescription = new TextBox { Width = 400, Text = "Producto de prueba" };
            formPanel.Controls.Add(_txtDescription, 1, row++);

            formPanel.Controls.Add(new Label { Text = "Ref. externa:", AutoSize = true }, 0, row);
            _txtExternalRef = new TextBox { Width = 300, Text = "REF-" + DateTime.Now.Ticks };
            formPanel.Controls.Add(_txtExternalRef, 1, row++);

            formPanel.Controls.Add(new Label { Text = "Email pagador:", AutoSize = true }, 0, row);
            _txtPayerEmail = new TextBox { Width = 300, Text = "test_user@test.com" };
            formPanel.Controls.Add(_txtPayerEmail, 1, row++);

            // Botones de acción
            var btnPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 5)
            };

            var btnCheckoutPro = CreateButton("🌐 Checkout Pro", Color.FromArgb(0, 158, 227));
            btnCheckoutPro.Click += BtnCheckoutPro_Click;

            var btnSearchPayment = CreateButton("🔍 Buscar Pago", Color.FromArgb(108, 117, 125));
            btnSearchPayment.Click += BtnSearchPayment_Click;

            var btnRefund = CreateButton("↩ Reembolsar", Color.FromArgb(220, 53, 69));
            btnRefund.Click += BtnRefund_Click;

            btnPanel.Controls.AddRange(new Control[] { btnCheckoutPro, btnSearchPayment, btnRefund });
            formPanel.Controls.Add(btnPanel, 0, row);
            formPanel.SetColumnSpan(btnPanel, 2);
            row++;

            // Campo para buscar/reembolsar por ID
            var idPanel = new FlowLayoutPanel { AutoSize = true };
            idPanel.Controls.Add(new Label
            {
                Text = "Payment ID:",
                AutoSize = true,
                Margin = new Padding(0, 6, 5, 0)
            });
            _txtPaymentId = new TextBox { Width = 200 };
            idPanel.Controls.Add(_txtPaymentId);
            formPanel.Controls.Add(idPanel, 0, row);
            formPanel.SetColumnSpan(idPanel, 2);

            split.Panel1.Controls.Add(formPanel);

            // ─── Panel inferior: Resultado ───
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
        }

        private async void BtnCheckoutPro_Click(object sender, EventArgs e)
        {
            if (!ValidateClient()) return;

            try
            {
                _main.SetStatus("Creando preferencia de Checkout Pro...");
                decimal amount;
                if (!decimal.TryParse(_txtAmount.Text, out amount))
                {
                    ShowError("Monto inválido."); return;
                }

                var request = new PreferenceCreateRequest
                {
                    Items = new System.Collections.Generic.List<PreferenceItemRequest>
                    {
                        new PreferenceItemRequest
                        {
                            Title = _txtDescription.Text,
                            Quantity = 1,
                            UnitPrice = amount,
                            CurrencyId = "ARS"
                        }
                    },
                    ExternalReference = _txtExternalRef.Text,
                    Payer = new PreferencePayerRequest
                    {
                        Email = _txtPayerEmail.Text
                    },
                    BackUrls = new BackUrlsRequest
                    {
                        Success = "https://localhost/success",
                        Failure = "https://localhost/failure",
                        Pending = "https://localhost/pending"
                    },
                    AutoReturn = "approved"
                };

                var result = await _main.MpClient.Preferences.CreateAsync(request);
                LogOperation("Preference", result.Data?.Id, amount, request, result);

                if (result.IsSuccess)
                {
                    var url = _main.MpClient.Config.Environment ==
                        Wrapper.Configuration.MpEnvironment.Sandbox
                        ? result.Data.SandboxInitPoint
                        : result.Data.InitPoint;

                    ShowResult($"✅ Preferencia creada: {result.Data.Id}\n" +
                        $"URL: {url}\n\n" +
                        JsonConvert.SerializeObject(result.Data, Formatting.Indented));

                    if (!string.IsNullOrEmpty(url))
                    {
                        var open = MessageBox.Show(
                            "¿Abrir checkout en el navegador?",
                            "Checkout Pro", MessageBoxButtons.YesNo);
                        if (open == DialogResult.Yes)
                            Process.Start(url);
                    }
                }
                else
                {
                    ShowResult($"❌ Error: {result.ErrorMessage}\n{result.RawJson}");
                }

                _main.SetStatus("Listo.");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                Log.Error(ex, "Error creando preferencia.");
            }
        }

        private async void BtnSearchPayment_Click(object sender, EventArgs e)
        {
            if (!ValidateClient()) return;

            try
            {
                long paymentId;
                if (long.TryParse(_txtPaymentId.Text, out paymentId))
                {
                    _main.SetStatus("Buscando pago...");
                    var result = await _main.MpClient.Payments.GetAsync(paymentId);
                    if (result.IsSuccess)
                    {
                        ShowResult(JsonConvert.SerializeObject(
                            result.Data, Formatting.Indented));
                    }
                    else
                    {
                        ShowResult($"❌ {result.ErrorMessage}\n{result.RawJson}");
                    }
                }
                else
                {
                    _main.SetStatus("Buscando pagos por referencia...");
                    var result = await _main.MpClient.Payments.SearchAsync(
                        new PaymentSearchRequest
                        {
                            ExternalReference = _txtExternalRef.Text,
                            Limit = 10
                        });
                    ShowResult(JsonConvert.SerializeObject(
                        result.Data ?? (object)result.RawJson, Formatting.Indented));
                }
                _main.SetStatus("Listo.");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private async void BtnRefund_Click(object sender, EventArgs e)
        {
            if (!ValidateClient()) return;

            long paymentId;
            if (!long.TryParse(_txtPaymentId.Text, out paymentId))
            {
                ShowError("Ingrese un Payment ID válido.");
                return;
            }

            var confirm = MessageBox.Show(
                $"¿Reembolsar pago {paymentId}?",
                "Confirmar reembolso", MessageBoxButtons.YesNo);
            if (confirm != DialogResult.Yes) return;

            try
            {
                _main.SetStatus("Procesando reembolso...");
                var result = await _main.MpClient.Payments.RefundAsync(paymentId);
                LogOperation("Refund", paymentId.ToString(), null, null, result);

                ShowResult(result.IsSuccess
                    ? $"✅ Reembolso exitoso:\n{JsonConvert.SerializeObject(result.Data, Formatting.Indented)}"
                    : $"❌ Error: {result.ErrorMessage}\n{result.RawJson}");
                _main.SetStatus("Listo.");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        // ─── Helpers ───

        private bool ValidateClient()
        {
            if (_main.MpClient != null) return true;
            MessageBox.Show("Configure las credenciales primero.",
                "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        private void ShowResult(string text)
        {
            if (InvokeRequired)
            { Invoke(new Action(() => _txtResult.Text = text)); return; }
            _txtResult.Text = text;
        }

        private void ShowError(string msg)
        {
            _main.SetStatus($"Error: {msg}");
            ShowResult($"❌ {msg}");
        }

        private void LogOperation(string type, string extId,
            decimal? amount, object request, object response)
        {
            _main.LogRepo.Insert(new OperationLogEntity
            {
                OperationType = type,
                ExternalId = extId,
                ExternalReference = _txtExternalRef.Text,
                Amount = amount,
                RequestJson = request != null
                    ? JsonConvert.SerializeObject(request) : null,
                ResponseJson = JsonConvert.SerializeObject(response)
            });
        }

        private Button CreateButton(string text, Color color)
        {
            return new Button
            {
                Text = text,
                Width = 150,
                Height = 35,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 8, 0)
            };
        }
    }
}
