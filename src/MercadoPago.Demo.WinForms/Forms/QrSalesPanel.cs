using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MercadoPago.Wrapper.Models.Pos;
using MercadoPago.Wrapper.Models.QrCode;
using MercadoPago.Wrapper.Models.Payments;
using MercadoPago.Demo.WinForms.Data.Repositories;
using Newtonsoft.Json;
using Serilog;

namespace MercadoPago.Demo.WinForms.Forms
{
    /// <summary>Panel de venta por QR en modo caja (modelo atendido/dinámico).</summary>
    public class QrSalesPanel : UserControl
    {
        private readonly MainForm _main;

        // Selección de caja
        private ComboBox _cboCashiers;
        private Button _btnRefreshCashiers;
        private Label _lblCashierStatus;

        // Ítems de la venta
        private DataGridView _gridItems;
        private TextBox _txtSku;
        private TextBox _txtTitle;
        private NumericUpDown _nudQuantity;
        private NumericUpDown _nudPrice;
        private Button _btnAddItem;
        private Button _btnRemoveItem;
        private Label _lblTotal;

        // Referencia y acciones
        private TextBox _txtExternalRef;
        private TextBox _txtNotificationUrl;
        private Button _btnSendToQr;
        private Button _btnClearQr;
        private Button _btnCheckPayment;

        // Estado y resultado
        private Panel _statusPanel;
        private Label _lblOrderStatus;
        private RichTextBox _txtResult;
        private Timer _pollingTimer;

        // Datos internos
        private List<QrSaleItem> _saleItems = new List<QrSaleItem>();
        private List<PosResponse> _cashierList = new List<PosResponse>();
        private string _currentOrderId;
        private bool _orderActive;

        public QrSalesPanel(MainForm main)
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
                SplitterDistance = 450
            };

            // ═══════════════════════════════════════════
            // PANEL SUPERIOR: Formulario de venta
            // ═══════════════════════════════════════════
            var topPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

            // ─── Sección: Selección de caja ───
            var cashierGroup = new GroupBox
            {
                Text = "📦 Selección de Caja",
                Dock = DockStyle.Top,
                Height = 70,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Padding = new Padding(10, 5, 10, 5)
            };
            var cashierFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight
            };
            cashierFlow.Controls.Add(new Label
            {
                Text = "Caja:",
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                Margin = new Padding(0, 6, 5, 0)
            });
            _cboCashiers = new ComboBox
            {
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5f)
            };
            cashierFlow.Controls.Add(_cboCashiers);

            _btnRefreshCashiers = CreateButton("🔄 Cargar", Color.FromArgb(0, 122, 204), 100);
            _btnRefreshCashiers.Click += BtnRefreshCashiers_Click;
            cashierFlow.Controls.Add(_btnRefreshCashiers);

            _lblCashierStatus = new Label
            {
                Text = "",
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.Gray,
                Margin = new Padding(10, 6, 0, 0)
            };
            cashierFlow.Controls.Add(_lblCashierStatus);
            cashierGroup.Controls.Add(cashierFlow);
            topPanel.Controls.Add(cashierGroup);

            // ─── Sección: Ítems de la venta ───
            var itemsGroup = new GroupBox
            {
                Text = "🛒 Ítems de la Venta",
                Dock = DockStyle.Top,
                Top = 75,
                Height = 250,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Padding = new Padding(10, 5, 10, 5)
            };

            // Formulario de agregar ítem
            var addItemPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                FlowDirection = FlowDirection.LeftToRight
            };
            var normalFont = new Font("Segoe UI", 9f);

            addItemPanel.Controls.Add(new Label { Text = "SKU:", AutoSize = true, Font = normalFont, Margin = new Padding(0, 6, 3, 0) });
            _txtSku = new TextBox { Width = 80, Font = normalFont };
            addItemPanel.Controls.Add(_txtSku);

            addItemPanel.Controls.Add(new Label { Text = "Descripción:", AutoSize = true, Font = normalFont, Margin = new Padding(8, 6, 3, 0) });
            _txtTitle = new TextBox { Width = 160, Font = normalFont };
            addItemPanel.Controls.Add(_txtTitle);

            addItemPanel.Controls.Add(new Label { Text = "Cant:", AutoSize = true, Font = normalFont, Margin = new Padding(8, 6, 3, 0) });
            _nudQuantity = new NumericUpDown { Width = 60, Minimum = 1, Maximum = 9999, Value = 1, Font = normalFont };
            addItemPanel.Controls.Add(_nudQuantity);

            addItemPanel.Controls.Add(new Label { Text = "Precio:", AutoSize = true, Font = normalFont, Margin = new Padding(8, 6, 3, 0) });
            _nudPrice = new NumericUpDown { Width = 100, Minimum = 0.01m, Maximum = 99999999, DecimalPlaces = 2, Value = 100, Font = normalFont };
            addItemPanel.Controls.Add(_nudPrice);

            _btnAddItem = CreateButton("+ Agregar", Color.FromArgb(40, 167, 69), 90);
            _btnAddItem.Click += BtnAddItem_Click;
            addItemPanel.Controls.Add(_btnAddItem);

            _btnRemoveItem = CreateButton("- Quitar", Color.FromArgb(220, 53, 69), 80);
            _btnRemoveItem.Click += BtnRemoveItem_Click;
            addItemPanel.Controls.Add(_btnRemoveItem);

            itemsGroup.Controls.Add(addItemPanel);

            // Grilla de ítems
            _gridItems = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                Font = normalFont
            };
            _gridItems.Columns.Add("Sku", "SKU");
            _gridItems.Columns.Add("Title", "Descripción");
            _gridItems.Columns.Add("Quantity", "Cantidad");
            _gridItems.Columns.Add("UnitPrice", "Precio Unit.");
            _gridItems.Columns.Add("Subtotal", "Subtotal");
            _gridItems.Columns["Sku"].Width = 80;
            _gridItems.Columns["Quantity"].Width = 70;
            _gridItems.Columns["UnitPrice"].Width = 100;
            _gridItems.Columns["Subtotal"].Width = 100;

            // Total
            var totalPanel = new Panel { Dock = DockStyle.Bottom, Height = 35 };
            _lblTotal = new Label
            {
                Text = "TOTAL: $0.00",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                Dock = DockStyle.Right,
                AutoSize = true,
                Margin = new Padding(0, 5, 15, 0)
            };
            totalPanel.Controls.Add(_lblTotal);

            itemsGroup.Controls.Add(_gridItems);
            itemsGroup.Controls.Add(totalPanel);
            topPanel.Controls.Add(itemsGroup);

            // ─── Sección: Acciones ───
            var actionsGroup = new GroupBox
            {
                Text = "⚡ Acciones",
                Dock = DockStyle.Top,
                Top = 330,
                Height = 120,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Padding = new Padding(10, 5, 10, 5)
            };

            var refPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 35,
                FlowDirection = FlowDirection.LeftToRight
            };
            refPanel.Controls.Add(new Label { Text = "Ref. externa:", AutoSize = true, Font = normalFont, Margin = new Padding(0, 6, 5, 0) });
            _txtExternalRef = new TextBox { Width = 200, Font = normalFont, Text = "VENTA-QR-" + DateTime.Now.ToString("yyyyMMddHHmmss") };
            refPanel.Controls.Add(_txtExternalRef);

            refPanel.Controls.Add(new Label { Text = "Webhook URL:", AutoSize = true, Font = normalFont, Margin = new Padding(15, 6, 5, 0) });
            _txtNotificationUrl = new TextBox { Width = 250, Font = normalFont };
            refPanel.Controls.Add(_txtNotificationUrl);
            actionsGroup.Controls.Add(refPanel);

            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 45,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 5, 0, 0)
            };

            _btnSendToQr = CreateButton("📤 Enviar a Caja QR", Color.FromArgb(0, 158, 227), 170);
            _btnSendToQr.Click += BtnSendToQr_Click;
            _btnSendToQr.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            _btnSendToQr.Height = 38;
            btnPanel.Controls.Add(_btnSendToQr);

            _btnCheckPayment = CreateButton("🔍 Consultar Pago", Color.FromArgb(108, 117, 125), 150);
            _btnCheckPayment.Click += BtnCheckPayment_Click;
            _btnCheckPayment.Height = 38;
            btnPanel.Controls.Add(_btnCheckPayment);

            _btnClearQr = CreateButton("🧹 Limpiar Caja", Color.FromArgb(220, 53, 69), 130);
            _btnClearQr.Click += BtnClearQr_Click;
            _btnClearQr.Height = 38;
            btnPanel.Controls.Add(_btnClearQr);

            // Estado de la orden
            _statusPanel = new Panel
            {
                Width = 250,
                Height = 35,
                Margin = new Padding(15, 3, 0, 0)
            };
            _lblOrderStatus = new Label
            {
                Text = "⚪ Sin orden activa",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(0, 8)
            };
            _statusPanel.Controls.Add(_lblOrderStatus);
            btnPanel.Controls.Add(_statusPanel);

            actionsGroup.Controls.Add(btnPanel);
            topPanel.Controls.Add(actionsGroup);

            // Ajustar el orden de los controles (los GroupBox se apilan top-down)
            topPanel.Controls.SetChildIndex(cashierGroup, 2);
            topPanel.Controls.SetChildIndex(itemsGroup, 1);
            topPanel.Controls.SetChildIndex(actionsGroup, 0);

            mainSplit.Panel1.Controls.Add(topPanel);

            // ═══════════════════════════════════════════
            // PANEL INFERIOR: Resultado JSON
            // ═══════════════════════════════════════════
            _txtResult = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.FromArgb(220, 220, 220)
            };
            mainSplit.Panel2.Controls.Add(_txtResult);

            Controls.Add(mainSplit);

            // Timer para polling de pago
            _pollingTimer = new Timer { Interval = 5000 };
            _pollingTimer.Tick += PollingTimer_Tick;
        }

        // ═══════════════════════════════════════════
        // EVENTOS
        // ═══════════════════════════════════════════

        private async void BtnRefreshCashiers_Click(object sender, EventArgs e)
        {
            if (_main.MpClient == null) { ShowNoClient(); return; }

            try
            {
                _main.SetStatus("Cargando cajas...");
                _lblCashierStatus.Text = "Cargando...";
                var result = await _main.MpClient.Cashiers.SearchAsync();

                if (result.IsSuccess && result.Data?.Results != null)
                {
                    _cashierList = result.Data.Results;
                    _cboCashiers.Items.Clear();
                    foreach (var pos in _cashierList)
                    {
                        _cboCashiers.Items.Add(
                            $"{pos.Name} (ID externo: {pos.ExternalId ?? pos.Id.ToString()})");
                    }
                    if (_cboCashiers.Items.Count > 0)
                        _cboCashiers.SelectedIndex = 0;

                    _lblCashierStatus.Text = $"✅ {_cashierList.Count} cajas";
                    _lblCashierStatus.ForeColor = Color.DarkGreen;
                    _main.SetStatus($"{_cashierList.Count} cajas cargadas.");
                }
                else
                {
                    _lblCashierStatus.Text = "❌ Error";
                    _lblCashierStatus.ForeColor = Color.Red;
                    ShowResult($"Error: {result.ErrorMessage}\n{result.RawJson}");
                }
            }
            catch (Exception ex)
            {
                _lblCashierStatus.Text = "❌ Error";
                ShowResult($"Error: {ex.Message}");
                Log.Error(ex, "Error cargando cajas.");
            }
        }

        private void BtnAddItem_Click(object sender, EventArgs e)
        {
            var item = new QrSaleItem
            {
                Sku = _txtSku.Text.Trim(),
                Title = _txtTitle.Text.Trim(),
                Quantity = (int)_nudQuantity.Value,
                UnitPrice = _nudPrice.Value
            };

            if (string.IsNullOrEmpty(item.Title))
            {
                MessageBox.Show("Ingrese una descripción.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _saleItems.Add(item);
            RefreshItemsGrid();

            // Limpiar campos
            _txtSku.Text = "";
            _txtTitle.Text = "";
            _nudQuantity.Value = 1;
            _txtTitle.Focus();
        }

        private void BtnRemoveItem_Click(object sender, EventArgs e)
        {
            if (_gridItems.SelectedRows.Count == 0) return;
            int idx = _gridItems.SelectedRows[0].Index;
            if (idx >= 0 && idx < _saleItems.Count)
            {
                _saleItems.RemoveAt(idx);
                RefreshItemsGrid();
            }
        }

        private async void BtnSendToQr_Click(object sender, EventArgs e)
        {
            if (_main.MpClient == null) { ShowNoClient(); return; }
            if (_cboCashiers.SelectedIndex < 0)
            {
                MessageBox.Show("Seleccione una caja primero. Haga click en 'Cargar'.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_saleItems.Count == 0)
            {
                MessageBox.Show("Agregue al menos un ítem a la venta.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var cashier = _cashierList[_cboCashiers.SelectedIndex];
            var externalPosId = cashier.ExternalId ?? cashier.Id.ToString();
            var total = _saleItems.Sum(i => i.Subtotal);

            // Obtener el User ID
            var cfg = _main.ConfigRepo.Get();
            var userId = cfg?.UserId;
            if (string.IsNullOrWhiteSpace(userId))
            {
                MessageBox.Show(
                    "Configure el User ID en la pestaña de Configuración.\n" +
                    "Puede obtenerlo con el botón 'Test Conexión' (campo 'id').",
                    "User ID requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _main.SetStatus("Enviando orden QR a la caja...");
                SetOrderStatus("🔄 Enviando...", Color.DarkOrange);

                var request = new QrOrderRequest
                {
                    ExternalReference = _txtExternalRef.Text.Trim(),
                    Title = $"Venta {_txtExternalRef.Text}",
                    Description = $"{_saleItems.Count} ítem(s)",
                    TotalAmount = total,
                    Items = _saleItems.Select(i => new QrItemRequest
                    {
                        SkuNumber = i.Sku,
                        Title = i.Title,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        TotalAmount = i.Subtotal,
                        UnitMeasure = "unit"
                    }).ToList()
                };

                if (!string.IsNullOrWhiteSpace(_txtNotificationUrl.Text))
                    request.NotificationUrl = _txtNotificationUrl.Text.Trim();

                var result = await _main.MpClient.QrCodes.CreateOrderAsync(
                    userId, externalPosId, request);

                // Loguear operación
                _main.LogRepo.Insert(new OperationLogEntity
                {
                    OperationType = "QR_Order",
                    ExternalId = result.Data?.InStoreOrderId,
                    ExternalReference = _txtExternalRef.Text,
                    Amount = total,
                    RequestJson = JsonConvert.SerializeObject(request),
                    ResponseJson = result.RawJson,
                    Status = result.IsSuccess ? "sent" : "error",
                    ErrorMessage = result.ErrorMessage
                });

                if (result.IsSuccess)
                {
                    _orderActive = true;
                    _currentOrderId = result.Data?.InStoreOrderId;

                    var msg = $"✅ Orden enviada a la caja '{cashier.Name}'\n\n" +
                        $"📦 Caja: {cashier.Name} (External ID: {externalPosId})\n" +
                        $"💰 Total: ${total:N2}\n" +
                        $"🔗 Ref: {_txtExternalRef.Text}\n" +
                        $"📋 Order ID: {_currentOrderId ?? "N/A"}\n\n" +
                        "El cliente ahora puede escanear el QR de la caja\n" +
                        "con la app de MercadoPago para pagar.\n\n" +
                        "─── Respuesta ───\n" +
                        JsonConvert.SerializeObject(result.Data, Formatting.Indented);

                    ShowResult(msg);
                    SetOrderStatus("🟡 Esperando pago del cliente...", Color.DarkOrange);
                    _main.SetStatus($"Orden QR enviada. Esperando pago en caja '{cashier.Name}'...");

                    // Iniciar polling automático
                    _pollingTimer.Start();
                }
                else
                {
                    ShowResult($"❌ Error al enviar orden QR:\n\n" +
                        $"Status: {result.StatusCode}\n" +
                        $"Error: {result.ErrorMessage}\n\n{result.RawJson}");
                    SetOrderStatus("❌ Error al enviar", Color.Red);
                    _main.SetStatus("Error al enviar orden QR.");
                }
            }
            catch (Exception ex)
            {
                ShowResult($"❌ Excepción: {ex.Message}");
                SetOrderStatus("❌ Error", Color.Red);
                Log.Error(ex, "Error enviando orden QR.");
            }
        }

        private async void BtnCheckPayment_Click(object sender, EventArgs e)
        {
            if (_main.MpClient == null) { ShowNoClient(); return; }

            try
            {
                _main.SetStatus("Consultando pago...");
                var extRef = _txtExternalRef.Text.Trim();

                var result = await _main.MpClient.Payments.SearchAsync(
                    new PaymentSearchRequest
                    {
                        ExternalReference = extRef,
                        Limit = 5,
                        Sort = "date_created",
                        Criteria = "desc"
                    });

                if (result.IsSuccess && result.Data?.Results != null
                    && result.Data.Results.Count > 0)
                {
                    var latest = result.Data.Results[0];
                    var statusText = GetPaymentStatusText(latest.Status);

                    var msg = $"🔍 Resultado para ref: {extRef}\n\n" +
                        $"Estado: {statusText}\n" +
                        $"Payment ID: {latest.Id}\n" +
                        $"Monto: ${latest.TransactionAmount:N2}\n" +
                        $"Método: {latest.PaymentMethodId} ({latest.PaymentTypeId})\n" +
                        $"Fecha: {latest.DateCreated:dd/MM/yyyy HH:mm}\n\n" +
                        "─── Detalle completo ───\n" +
                        JsonConvert.SerializeObject(latest, Formatting.Indented);

                    ShowResult(msg);

                    if (latest.Status == "approved")
                    {
                        SetOrderStatus("🟢 PAGADO ✅", Color.DarkGreen);
                        _pollingTimer.Stop();
                        _orderActive = false;
                        _main.SetStatus($"¡Pago aprobado! ID: {latest.Id}");
                    }
                    else if (latest.Status == "rejected")
                    {
                        SetOrderStatus("🔴 RECHAZADO", Color.Red);
                        _main.SetStatus($"Pago rechazado: {latest.StatusDetail}");
                    }
                    else
                    {
                        SetOrderStatus($"🟡 {latest.Status.ToUpper()}", Color.DarkOrange);
                    }
                }
                else if (result.IsSuccess)
                {
                    ShowResult($"No se encontraron pagos para ref: {extRef}\n\n" +
                        "El cliente aún no ha escaneado/pagado.");
                    _main.SetStatus("Sin pagos encontrados para esta referencia.");
                }
                else
                {
                    ShowResult($"Error: {result.ErrorMessage}\n{result.RawJson}");
                }
            }
            catch (Exception ex)
            {
                ShowResult($"Error: {ex.Message}");
                Log.Error(ex, "Error consultando pago QR.");
            }
        }

        private async void BtnClearQr_Click(object sender, EventArgs e)
        {
            if (_main.MpClient == null) { ShowNoClient(); return; }
            if (_cboCashiers.SelectedIndex < 0)
            {
                MessageBox.Show("Seleccione una caja.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var cashier = _cashierList[_cboCashiers.SelectedIndex];
            var externalPosId = cashier.ExternalId ?? cashier.Id.ToString();
            var cfg = _main.ConfigRepo.Get();
            var userId = cfg?.UserId;

            if (string.IsNullOrWhiteSpace(userId))
            {
                MessageBox.Show("Configure el User ID.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _main.SetStatus("Limpiando orden de la caja...");
                var result = await _main.MpClient.QrCodes.DeleteOrderAsync(
                    userId, externalPosId);

                _pollingTimer.Stop();
                _orderActive = false;
                _currentOrderId = null;

                if (result.IsSuccess)
                {
                    ShowResult($"✅ Orden QR eliminada de la caja '{cashier.Name}'.\n" +
                        "La caja está lista para una nueva venta.");
                    SetOrderStatus("⚪ Caja limpia", Color.Gray);
                    _main.SetStatus("Caja limpia. Lista para nueva venta.");

                    // Preparar nueva referencia
                    _txtExternalRef.Text = "VENTA-QR-" +
                        DateTime.Now.ToString("yyyyMMddHHmmss");
                }
                else
                {
                    // Si la caja ya estaba limpia, MP devuelve 400
                    var alreadyClean = result.RawJson != null &&
                        result.RawJson.Contains("in_store_order_delete_error");

                    if (alreadyClean)
                    {
                        ShowResult($"ℹ La caja '{cashier.Name}' ya estaba limpia " +
                            "(no tenía una orden QR activa).\n" +
                            "Puede enviar una nueva orden sin problema.");
                        SetOrderStatus("⚪ Caja limpia", Color.Gray);
                        _main.SetStatus("La caja ya estaba limpia.");
                    }
                    else
                    {
                        ShowResult($"Respuesta: {result.StatusCode}\n{result.RawJson}");
                        SetOrderStatus("⚪ Sin orden activa", Color.Gray);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowResult($"Error: {ex.Message}");
                Log.Error(ex, "Error limpiando orden QR.");
            }
        }

        private async void PollingTimer_Tick(object sender, EventArgs e)
        {
            if (_main.MpClient == null || !_orderActive) return;

            try
            {
                var result = await _main.MpClient.Payments.SearchAsync(
                    new PaymentSearchRequest
                    {
                        ExternalReference = _txtExternalRef.Text.Trim(),
                        Limit = 1,
                        Sort = "date_created",
                        Criteria = "desc"
                    });

                if (result.IsSuccess && result.Data?.Results != null
                    && result.Data.Results.Count > 0)
                {
                    var payment = result.Data.Results[0];

                    if (payment.Status == "approved")
                    {
                        _pollingTimer.Stop();
                        _orderActive = false;

                        SetOrderStatus("🟢 ¡PAGO APROBADO! ✅", Color.DarkGreen);
                        _main.SetStatus($"¡Pago aprobado! ID: {payment.Id} | ${payment.TransactionAmount:N2}");

                        ShowResult(
                            $"🎉 ¡PAGO APROBADO!\n\n" +
                            $"Payment ID: {payment.Id}\n" +
                            $"Monto: ${payment.TransactionAmount:N2}\n" +
                            $"Método: {payment.PaymentMethodId}\n" +
                            $"Cuotas: {payment.Installments}\n" +
                            $"Fecha: {payment.DateApproved:dd/MM/yyyy HH:mm:ss}\n" +
                            $"Ref: {payment.ExternalReference}\n\n" +
                            "─── Detalle ───\n" +
                            JsonConvert.SerializeObject(payment, Formatting.Indented));

                        _main.LogRepo.Insert(new OperationLogEntity
                        {
                            OperationType = "QR_Payment_Approved",
                            ExternalId = payment.Id.ToString(),
                            ExternalReference = payment.ExternalReference,
                            Amount = payment.TransactionAmount,
                            Status = "approved",
                            ResponseJson = JsonConvert.SerializeObject(payment)
                        });
                    }
                    else if (payment.Status == "rejected")
                    {
                        _pollingTimer.Stop();
                        SetOrderStatus("🔴 PAGO RECHAZADO", Color.Red);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error en polling de pago QR.");
            }
        }

        // ═══════════════════════════════════════════
        // HELPERS
        // ═══════════════════════════════════════════

        private void RefreshItemsGrid()
        {
            _gridItems.Rows.Clear();
            foreach (var item in _saleItems)
            {
                _gridItems.Rows.Add(
                    item.Sku, item.Title,
                    item.Quantity, $"${item.UnitPrice:N2}",
                    $"${item.Subtotal:N2}");
            }
            var total = _saleItems.Sum(i => i.Subtotal);
            _lblTotal.Text = $"TOTAL: ${total:N2}";
        }

        private void SetOrderStatus(string text, Color color)
        {
            if (InvokeRequired) { Invoke(new Action(() => SetOrderStatus(text, color))); return; }
            _lblOrderStatus.Text = text;
            _lblOrderStatus.ForeColor = color;
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

        private string GetPaymentStatusText(string status)
        {
            switch (status)
            {
                case "approved": return "✅ APROBADO";
                case "pending": return "⏳ PENDIENTE";
                case "rejected": return "❌ RECHAZADO";
                case "cancelled": return "🚫 CANCELADO";
                case "in_process": return "🔄 EN PROCESO";
                case "refunded": return "↩ REEMBOLSADO";
                default: return status?.ToUpper() ?? "DESCONOCIDO";
            }
        }

        private Button CreateButton(string text, Color color, int width)
        {
            return new Button
            {
                Text = text,
                Width = width,
                Height = 30,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 8, 0),
                Font = new Font("Segoe UI", 9)
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _pollingTimer?.Stop();
                _pollingTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>Ítem interno de una venta QR.</summary>
    internal class QrSaleItem
    {
        public string Sku { get; set; }
        public string Title { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => Quantity * UnitPrice;
    }
}
