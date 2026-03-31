using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MercadoPago.Wrapper.Helpers;
using MercadoPago.Wrapper.Models.Orders;
using MercadoPago.Wrapper.Models.PointDevice;
using MercadoPago.Demo.WinForms.Data.Repositories;
using Newtonsoft.Json;
using Serilog;

namespace MercadoPago.Demo.WinForms.Forms
{
    /// <summary>
    /// Panel de Point Smart con Orders Unificadas.
    /// Demuestra: listar terminales, cambiar modo, crear/cancelar/consultar/reembolsar
    /// órdenes usando la API unificada con config.point y platform_id.
    /// </summary>
    public class PointSmartPanel : UserControl
    {
        private readonly MainForm _main;

        // Terminales
        private ComboBox _cboDevices;
        private Button _btnRefreshDevices;
        private Label _lblDeviceStatus;
        private Button _btnModePdv;
        private Button _btnModeStandalone;

        // Orden
        private TextBox _txtAmount;
        private TextBox _txtDescription;
        private TextBox _txtExternalRef;
        private TextBox _txtTicketNumber;
        private CheckBox _chkPrintOnTerminal;
        private TextBox _txtNotificationUrl;

        // Acciones
        private Button _btnCreateOrder;
        private Button _btnCancelOrder;
        private Button _btnCheckOrder;
        private Button _btnRefundOrder;

        // Estado y resultado
        private Label _lblOrderStatus;
        private RichTextBox _txtResult;

        // Datos internos
        private List<PointDeviceResponse> _devices = new List<PointDeviceResponse>();
        private string _currentOrderId;

        public PointSmartPanel(MainForm main)
        {
            _main = main;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Padding = new Padding(10);

            var mainSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 380
            };

            var topPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            var normalFont = new Font("Segoe UI", 9f);

            // ─── Sección: Terminales ───
            var deviceGroup = new GroupBox
            {
                Text = "📟 Terminales Point Smart",
                Dock = DockStyle.Top,
                Height = 80,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Padding = new Padding(10, 5, 10, 5)
            };
            var deviceFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight
            };
            deviceFlow.Controls.Add(new Label
            {
                Text = "Terminal:", AutoSize = true,
                Font = normalFont, Margin = new Padding(0, 6, 5, 0)
            });
            _cboDevices = new ComboBox
            {
                Width = 280, DropDownStyle = ComboBoxStyle.DropDownList,
                Font = normalFont
            };
            deviceFlow.Controls.Add(_cboDevices);

            _btnRefreshDevices = CreateButton("🔄 Cargar", Color.FromArgb(0, 122, 204), 90);
            _btnRefreshDevices.Click += BtnRefreshDevices_Click;
            deviceFlow.Controls.Add(_btnRefreshDevices);

            _btnModePdv = CreateButton("PDV", Color.FromArgb(40, 167, 69), 65);
            _btnModePdv.Click += (s, e) => ChangeMode("PDV");
            deviceFlow.Controls.Add(_btnModePdv);

            _btnModeStandalone = CreateButton("STANDALONE", Color.FromArgb(108, 117, 125), 100);
            _btnModeStandalone.Click += (s, e) => ChangeMode("STANDALONE");
            deviceFlow.Controls.Add(_btnModeStandalone);

            _lblDeviceStatus = new Label
            {
                Text = "", AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.Gray, Margin = new Padding(10, 6, 0, 0)
            };
            deviceFlow.Controls.Add(_lblDeviceStatus);
            deviceGroup.Controls.Add(deviceFlow);
            topPanel.Controls.Add(deviceGroup);

            // ─── Sección: Datos de la orden ───
            var orderGroup = new GroupBox
            {
                Text = "📦 Crear Orden (API Unificada)",
                Dock = DockStyle.Top, Top = 85, Height = 175,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Padding = new Padding(10, 5, 10, 5)
            };

            var orderLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 4, AutoSize = true
            };
            orderLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            orderLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            orderLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            orderLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            int row = 0;
            orderLayout.Controls.Add(new Label
            { Text = "Monto:", AutoSize = true, Font = normalFont }, 0, row);
            _txtAmount = new TextBox { Width = 150, Font = normalFont, Text = "1000.00" };
            orderLayout.Controls.Add(_txtAmount, 1, row);

            orderLayout.Controls.Add(new Label
            { Text = "Descripción:", AutoSize = true, Font = normalFont }, 2, row);
            _txtDescription = new TextBox
            { Width = 200, Font = normalFont, Text = "Venta Point Smart" };
            orderLayout.Controls.Add(_txtDescription, 3, row++);

            orderLayout.Controls.Add(new Label
            { Text = "Ref. externa:", AutoSize = true, Font = normalFont }, 0, row);
            _txtExternalRef = new TextBox
            {
                Width = 200, Font = normalFont,
                Text = "PS-" + DateTime.Now.ToString("yyyyMMddHHmmss")
            };
            orderLayout.Controls.Add(_txtExternalRef, 1, row);

            orderLayout.Controls.Add(new Label
            { Text = "Nro. Ticket:", AutoSize = true, Font = normalFont }, 2, row);
            _txtTicketNumber = new TextBox
            { Width = 150, Font = normalFont, Text = "T-001" };
            orderLayout.Controls.Add(_txtTicketNumber, 3, row++);

            orderLayout.Controls.Add(new Label
            { Text = "Webhook URL:", AutoSize = true, Font = normalFont }, 0, row);
            _txtNotificationUrl = new TextBox { Width = 200, Font = normalFont };
            orderLayout.Controls.Add(_txtNotificationUrl, 1, row);

            _chkPrintOnTerminal = new CheckBox
            {
                Text = "Imprimir ticket en terminal",
                AutoSize = true, Font = normalFont, Checked = true
            };
            orderLayout.Controls.Add(_chkPrintOnTerminal, 2, row);
            orderLayout.SetColumnSpan(_chkPrintOnTerminal, 2);
            row++;

            orderGroup.Controls.Add(orderLayout);
            topPanel.Controls.Add(orderGroup);

            // ─── Sección: Acciones ───
            var actionsGroup = new GroupBox
            {
                Text = "⚡ Acciones",
                Dock = DockStyle.Top, Top = 265, Height = 90,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Padding = new Padding(10, 5, 10, 5)
            };

            var btnFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 45,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 5, 0, 0)
            };

            _btnCreateOrder = CreateButton("📤 Crear Orden", Color.FromArgb(0, 158, 227), 140);
            _btnCreateOrder.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            _btnCreateOrder.Height = 38;
            _btnCreateOrder.Click += BtnCreateOrder_Click;
            btnFlow.Controls.Add(_btnCreateOrder);

            _btnCheckOrder = CreateButton("🔍 Consultar", Color.FromArgb(108, 117, 125), 120);
            _btnCheckOrder.Height = 38;
            _btnCheckOrder.Click += BtnCheckOrder_Click;
            btnFlow.Controls.Add(_btnCheckOrder);

            _btnCancelOrder = CreateButton("🚫 Cancelar", Color.FromArgb(220, 53, 69), 110);
            _btnCancelOrder.Height = 38;
            _btnCancelOrder.Click += BtnCancelOrder_Click;
            btnFlow.Controls.Add(_btnCancelOrder);

            _btnRefundOrder = CreateButton("↩ Reembolsar", Color.FromArgb(156, 39, 176), 120);
            _btnRefundOrder.Height = 38;
            _btnRefundOrder.Click += BtnRefundOrder_Click;
            btnFlow.Controls.Add(_btnRefundOrder);

            _lblOrderStatus = new Label
            {
                Text = "⚪ Sin orden activa",
                Font = new Font("Segoe UI", 10), AutoSize = true,
                Margin = new Padding(15, 10, 0, 0)
            };
            btnFlow.Controls.Add(_lblOrderStatus);

            actionsGroup.Controls.Add(btnFlow);
            topPanel.Controls.Add(actionsGroup);

            // Ajustar orden top-down
            topPanel.Controls.SetChildIndex(deviceGroup, 2);
            topPanel.Controls.SetChildIndex(orderGroup, 1);
            topPanel.Controls.SetChildIndex(actionsGroup, 0);

            mainSplit.Panel1.Controls.Add(topPanel);

            // ─── Panel inferior: Resultado JSON ───
            _txtResult = new RichTextBox
            {
                Dock = DockStyle.Fill, ReadOnly = true,
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.FromArgb(220, 220, 220)
            };
            mainSplit.Panel2.Controls.Add(_txtResult);

            Controls.Add(mainSplit);
        }

        // ═══════════════════════════════════════════
        // EVENTOS
        // ═══════════════════════════════════════════

        private async void BtnRefreshDevices_Click(object sender, EventArgs e)
        {
            if (_main.MpClient == null) { ShowNoClient(); return; }

            try
            {
                _main.SetStatus("Cargando terminales Point...");
                _lblDeviceStatus.Text = "Cargando...";
                var result = await _main.MpClient.PointDevices.ListDevicesAsync();

                if (result.IsSuccess && result.Data?.Devices != null)
                {
                    _devices = result.Data.Devices;
                    _cboDevices.Items.Clear();
                    foreach (var d in _devices)
                    {
                        var mode = d.OperatingMode ?? "?";
                        _cboDevices.Items.Add(
                            $"{d.Id} ({mode})");
                    }
                    if (_cboDevices.Items.Count > 0)
                        _cboDevices.SelectedIndex = 0;

                    _lblDeviceStatus.Text = $"✅ {_devices.Count} terminal(es)";
                    _lblDeviceStatus.ForeColor = Color.DarkGreen;
                    _main.SetStatus($"{_devices.Count} terminal(es) cargadas.");
                }
                else
                {
                    _lblDeviceStatus.Text = "❌ Error";
                    _lblDeviceStatus.ForeColor = Color.Red;
                    ShowResult($"Error: {result.ErrorMessage}\n{result.RawJson}");
                }
            }
            catch (Exception ex)
            {
                _lblDeviceStatus.Text = "❌ Error";
                ShowResult($"Error: {ex.Message}");
                Log.Error(ex, "Error cargando terminales Point.");
            }
        }

        private async void ChangeMode(string mode)
        {
            if (_main.MpClient == null) { ShowNoClient(); return; }
            if (_cboDevices.SelectedIndex < 0)
            {
                MessageBox.Show("Seleccione un terminal primero.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var device = _devices[_cboDevices.SelectedIndex];
            var confirm = MessageBox.Show(
                $"¿Cambiar terminal {device.Id} a modo {mode}?",
                "Cambiar modo", MessageBoxButtons.YesNo);
            if (confirm != DialogResult.Yes) return;

            try
            {
                _main.SetStatus($"Cambiando modo a {mode}...");
                var result = await _main.MpClient.PointDevices
                    .ChangeOperatingModeAsync(device.Id, mode);

                if (result.IsSuccess)
                {
                    ShowResult(
                        $"✅ Terminal {device.Id} cambiada a modo {mode}\n\n" +
                        JsonConvert.SerializeObject(result.Data, Formatting.Indented));
                    _main.SetStatus($"Terminal en modo {mode}.");
                    BtnRefreshDevices_Click(null, null);
                }
                else
                {
                    ShowResult($"❌ Error: {result.ErrorMessage}\n{result.RawJson}");
                }
            }
            catch (Exception ex)
            {
                ShowResult($"Error: {ex.Message}");
                Log.Error(ex, "Error cambiando modo de operación.");
            }
        }

        private async void BtnCreateOrder_Click(object sender, EventArgs e)
        {
            if (_main.MpClient == null) { ShowNoClient(); return; }

            // Validar referencia externa contra PII
            if (!ExternalReferenceValidator.Validate(
                _txtExternalRef.Text.Trim(), Log.Logger))
            {
                var proceed = MessageBox.Show(
                    "⚠ La referencia externa podría contener información sensible (PII).\n" +
                    "MercadoPago requiere que NO contenga datos personales.\n\n" +
                    "¿Desea continuar de todos modos?",
                    "Advertencia PII", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (proceed != DialogResult.Yes) return;
            }

            // Formatear monto según país
            var cfg = _main.ConfigRepo.Get();
            var country = cfg?.Country ?? "AR";

            decimal amount;
            if (!decimal.TryParse(_txtAmount.Text, out amount))
            {
                MessageBox.Show("Monto inválido.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var formattedAmount = AmountFormatter.Format(amount, country);

            try
            {
                _main.SetStatus("Creando orden unificada...");
                SetOrderStatus("🔄 Creando...", Color.DarkOrange);

                var request = new OrderCreateRequest
                {
                    Type = "online",
                    TotalAmount = formattedAmount,
                    ExternalReference = _txtExternalRef.Text.Trim(),
                    Description = _txtDescription.Text.Trim(),
                    Transactions = new OrderTransactionsRequest
                    {
                        Payments = new List<OrderTransactionPaymentRequest>
                        {
                            new OrderTransactionPaymentRequest
                            {
                                Amount = formattedAmount
                            }
                        }
                    },
                    Config = new OrderConfigRequest
                    {
                        Point = new OrderPointConfigRequest
                        {
                            PrintOnTerminal = _chkPrintOnTerminal.Checked,
                            TicketNumber = _txtTicketNumber.Text.Trim()
                        }
                    }
                };

                // platform_id se toma automáticamente de la config
                if (!string.IsNullOrWhiteSpace(_main.MpClient.Config.PlatformId))
                    request.PlatformId = _main.MpClient.Config.PlatformId;

                if (!string.IsNullOrWhiteSpace(_txtNotificationUrl.Text))
                    request.NotificationUrl = _txtNotificationUrl.Text.Trim();

                var result = await _main.MpClient.Orders.CreateAsync(request);

                _main.LogRepo.Insert(new OperationLogEntity
                {
                    OperationType = "Point_Order",
                    ExternalId = result.Data?.Id,
                    ExternalReference = _txtExternalRef.Text,
                    Amount = amount,
                    RequestJson = JsonConvert.SerializeObject(request),
                    ResponseJson = result.RawJson,
                    Status = result.IsSuccess ? "created" : "error",
                    ErrorMessage = result.ErrorMessage
                });

                if (result.IsSuccess)
                {
                    _currentOrderId = result.Data?.Id;

                    var msg = $"✅ Orden creada exitosamente\n\n" +
                        $"📋 Order ID: {_currentOrderId}\n" +
                        $"💰 Monto: {formattedAmount} ({country})\n" +
                        $"🔗 Ref: {_txtExternalRef.Text}\n" +
                        $"🎫 Ticket: {_txtTicketNumber.Text}\n" +
                        $"🖨 Impresión: {(_chkPrintOnTerminal.Checked ? "Sí" : "No")}\n" +
                        $"🏷 Platform ID: {request.PlatformId ?? "N/A"}\n\n" +
                        "─── Respuesta ───\n" +
                        JsonConvert.SerializeObject(result.Data, Formatting.Indented);

                    ShowResult(msg);
                    SetOrderStatus("🟡 Orden creada, esperando pago...", Color.DarkOrange);
                    _main.SetStatus($"Orden {_currentOrderId} creada.");
                }
                else
                {
                    ShowResult($"❌ Error creando orden:\n\n" +
                        $"Status: {result.StatusCode}\n" +
                        $"Error: {result.ErrorMessage}\n\n{result.RawJson}");
                    SetOrderStatus("❌ Error al crear", Color.Red);
                }
            }
            catch (Exception ex)
            {
                ShowResult($"❌ Excepción: {ex.Message}");
                SetOrderStatus("❌ Error", Color.Red);
                Log.Error(ex, "Error creando orden unificada.");
            }
        }

        private async void BtnCheckOrder_Click(object sender, EventArgs e)
        {
            if (_main.MpClient == null) { ShowNoClient(); return; }

            var orderId = _currentOrderId;
            if (string.IsNullOrWhiteSpace(orderId))
            {
                orderId = Microsoft.VisualBasic.Interaction.InputBox(
                    "Order ID a consultar:", "Consultar Orden", "");
                if (string.IsNullOrWhiteSpace(orderId)) return;
            }

            try
            {
                _main.SetStatus("Consultando orden...");
                var result = await _main.MpClient.Orders.GetAsync(orderId);

                if (result.IsSuccess)
                {
                    var order = result.Data;
                    var statusEmoji = GetStatusEmoji(order.Status);

                    var msg = $"🔍 Orden: {orderId}\n\n" +
                        $"Estado: {statusEmoji} {order.Status?.ToUpper()}\n" +
                        $"Detalle: {order.StatusDetail}\n" +
                        $"Monto: {order.TotalAmount}\n" +
                        $"Ref: {order.ExternalReference}\n\n" +
                        "─── Detalle completo ───\n" +
                        JsonConvert.SerializeObject(order, Formatting.Indented);

                    ShowResult(msg);
                    SetOrderStatus($"{statusEmoji} {order.Status?.ToUpper()}", GetStatusColor(order.Status));
                    _main.SetStatus($"Orden {orderId}: {order.Status}");
                }
                else
                {
                    ShowResult($"❌ {result.ErrorMessage}\n{result.RawJson}");
                }
            }
            catch (Exception ex)
            {
                ShowResult($"Error: {ex.Message}");
                Log.Error(ex, "Error consultando orden.");
            }
        }

        private async void BtnCancelOrder_Click(object sender, EventArgs e)
        {
            if (_main.MpClient == null) { ShowNoClient(); return; }

            var orderId = _currentOrderId;
            if (string.IsNullOrWhiteSpace(orderId))
            {
                MessageBox.Show("No hay orden activa para cancelar.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"¿Cancelar orden {orderId}?",
                "Confirmar cancelación", MessageBoxButtons.YesNo);
            if (confirm != DialogResult.Yes) return;

            try
            {
                _main.SetStatus("Cancelando orden...");
                var result = await _main.MpClient.Orders.CancelAsync(orderId);

                _main.LogRepo.Insert(new OperationLogEntity
                {
                    OperationType = "Point_Order_Cancel",
                    ExternalId = orderId,
                    Status = result.IsSuccess ? "cancelled" : "error",
                    ResponseJson = result.RawJson,
                    ErrorMessage = result.ErrorMessage
                });

                if (result.IsSuccess)
                {
                    ShowResult($"✅ Orden {orderId} cancelada.\n\n" +
                        JsonConvert.SerializeObject(result.Data, Formatting.Indented));
                    SetOrderStatus("🚫 CANCELADA", Color.Red);
                    _currentOrderId = null;
                    _main.SetStatus($"Orden {orderId} cancelada.");
                }
                else
                {
                    ShowResult($"❌ Error: {result.ErrorMessage}\n{result.RawJson}");
                }
            }
            catch (Exception ex)
            {
                ShowResult($"Error: {ex.Message}");
                Log.Error(ex, "Error cancelando orden.");
            }
        }

        private async void BtnRefundOrder_Click(object sender, EventArgs e)
        {
            if (_main.MpClient == null) { ShowNoClient(); return; }

            var orderId = _currentOrderId;
            if (string.IsNullOrWhiteSpace(orderId))
            {
                orderId = Microsoft.VisualBasic.Interaction.InputBox(
                    "Order ID a reembolsar:", "Reembolsar Orden", "");
                if (string.IsNullOrWhiteSpace(orderId)) return;
            }

            var confirm = MessageBox.Show(
                $"¿Reembolsar orden {orderId}?",
                "Confirmar reembolso", MessageBoxButtons.YesNo);
            if (confirm != DialogResult.Yes) return;

            try
            {
                _main.SetStatus("Procesando reembolso...");
                var result = await _main.MpClient.Orders.RefundAsync(orderId);

                _main.LogRepo.Insert(new OperationLogEntity
                {
                    OperationType = "Point_Order_Refund",
                    ExternalId = orderId,
                    Status = result.IsSuccess ? "refunded" : "error",
                    ResponseJson = result.RawJson,
                    ErrorMessage = result.ErrorMessage
                });

                if (result.IsSuccess)
                {
                    ShowResult($"✅ Reembolso exitoso para orden {orderId}.\n\n" +
                        JsonConvert.SerializeObject(result.Data, Formatting.Indented));
                    SetOrderStatus("↩ REEMBOLSADA", Color.DarkViolet);
                    _main.SetStatus($"Orden {orderId} reembolsada.");
                }
                else
                {
                    ShowResult($"❌ Error: {result.ErrorMessage}\n{result.RawJson}");
                }
            }
            catch (Exception ex)
            {
                ShowResult($"Error: {ex.Message}");
                Log.Error(ex, "Error reembolsando orden.");
            }
        }

        // ═══════════════════════════════════════════
        // HELPERS
        // ═══════════════════════════════════════════

        private void SetOrderStatus(string text, Color color)
        {
            if (InvokeRequired)
            { Invoke(new Action(() => SetOrderStatus(text, color))); return; }
            _lblOrderStatus.Text = text;
            _lblOrderStatus.ForeColor = color;
        }

        private void ShowResult(string text)
        {
            if (InvokeRequired)
            { Invoke(new Action(() => _txtResult.Text = text)); return; }
            _txtResult.Text = text;
        }

        private void ShowNoClient()
        {
            MessageBox.Show("Configure las credenciales primero.",
                "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private string GetStatusEmoji(string status)
        {
            switch (status)
            {
                case "approved": return "🟢";
                case "rejected": return "🔴";
                case "cancelled": return "🚫";
                case "action_required": return "⚠️";
                case "pending": return "🟡";
                default: return "⚪";
            }
        }

        private Color GetStatusColor(string status)
        {
            switch (status)
            {
                case "approved": return Color.DarkGreen;
                case "rejected": return Color.Red;
                case "cancelled": return Color.DarkRed;
                case "action_required": return Color.DarkOrange;
                case "pending": return Color.DarkOrange;
                default: return Color.Gray;
            }
        }

        private Button CreateButton(string text, Color color, int width)
        {
            return new Button
            {
                Text = text, Width = width, Height = 30,
                BackColor = color, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 8, 0),
                Font = new Font("Segoe UI", 9)
            };
        }
    }
}
