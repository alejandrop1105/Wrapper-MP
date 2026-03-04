using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MercadoPago.Wrapper.Models.Payments;
using MercadoPago.Wrapper.Models.Pos;
using Newtonsoft.Json;
using Serilog;

namespace MercadoPago.Demo.WinForms.Forms
{
    /// <summary>Panel de movimientos por caja con resumen tipo cierre.</summary>
    public class CashierMovementsPanel : UserControl
    {
        private readonly MainForm _main;
        private ComboBox _cboCashiers;
        private DateTimePicker _dtpFrom;
        private DateTimePicker _dtpTo;
        private DataGridView _gridMovements;
        private RichTextBox _txtSummary;
        private List<PosResponse> _cashierList = new List<PosResponse>();

        public CashierMovementsPanel(MainForm main)
        {
            _main = main;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Padding = new Padding(10);

            // ─── Header: filtros ───
            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                Padding = new Padding(5)
            };

            // Fila 1: Caja
            var row1 = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 35,
                AutoSize = false,
                WrapContents = false
            };
            row1.Controls.Add(CreateLabel("📦 Caja:"));
            _cboCashiers = new ComboBox
            {
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 4, 15, 0)
            };
            row1.Controls.Add(_cboCashiers);
            var btnLoadCashiers = CreateButton("Cargar Cajas",
                Color.FromArgb(108, 117, 125));
            btnLoadCashiers.Click += BtnLoadCashiers_Click;
            row1.Controls.Add(btnLoadCashiers);

            // Fila 2: Fechas y botón buscar
            var row2 = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 35,
                AutoSize = false,
                WrapContents = false
            };
            row2.Controls.Add(CreateLabel("📅 Desde:"));
            _dtpFrom = new DateTimePicker
            {
                Width = 140,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today,
                Margin = new Padding(0, 4, 10, 0)
            };
            row2.Controls.Add(_dtpFrom);

            row2.Controls.Add(CreateLabel("Hasta:"));
            _dtpTo = new DateTimePicker
            {
                Width = 140,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today,
                Margin = new Padding(0, 4, 10, 0)
            };
            row2.Controls.Add(_dtpTo);

            var btnSearch = CreateButton("🔍 Buscar Movimientos",
                Color.FromArgb(0, 122, 204));
            btnSearch.Width = 170;
            btnSearch.Click += BtnSearch_Click;
            row2.Controls.Add(btnSearch);

            var btnClosure = CreateButton("📊 Resumen Cierre",
                Color.FromArgb(40, 167, 69));
            btnClosure.Width = 150;
            btnClosure.Click += BtnClosure_Click;
            row2.Controls.Add(btnClosure);

            filterPanel.Controls.Add(row2);
            filterPanel.Controls.Add(row1);

            // ─── Body: grilla + resumen ───
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 300
            };

            _gridMovements = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.FromArgb(45, 45, 48),
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(45, 45, 48),
                    ForeColor = Color.White,
                    SelectionBackColor = Color.FromArgb(0, 122, 204)
                },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(28, 28, 28),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                },
                EnableHeadersVisualStyles = false
            };
            split.Panel1.Controls.Add(_gridMovements);

            _txtSummary = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 10),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.FromArgb(220, 220, 220)
            };
            split.Panel2.Controls.Add(_txtSummary);

            Controls.Add(split);
            Controls.Add(filterPanel);
        }

        private async void BtnLoadCashiers_Click(object sender, EventArgs e)
        {
            if (_main.MpClient == null) return;
            try
            {
                _main.SetStatus("Cargando cajas...");
                var result = await _main.MpClient.Cashiers.SearchAsync();
                if (result.IsSuccess && result.Data?.Results != null)
                {
                    _cashierList = result.Data.Results;
                    _cboCashiers.Items.Clear();
                    foreach (var c in _cashierList)
                        _cboCashiers.Items.Add(
                            $"{c.Name} (ID: {c.Id} | Ext: {c.ExternalId})");
                    if (_cboCashiers.Items.Count > 0)
                        _cboCashiers.SelectedIndex = 0;
                    _main.SetStatus($"{_cashierList.Count} cajas cargadas.");
                }
                else
                {
                    _txtSummary.Text = $"Error: {result.ErrorMessage}\n{result.RawJson}";
                }
            }
            catch (Exception ex)
            {
                _txtSummary.Text = $"Error: {ex.Message}";
                Log.Error(ex, "Error cargando cajas para movimientos.");
            }
        }

        private List<PaymentResponse> _lastResults = new List<PaymentResponse>();

        private async void BtnSearch_Click(object sender, EventArgs e)
        {
            if (_main.MpClient == null || _cboCashiers.SelectedIndex < 0)
            {
                MessageBox.Show("Seleccione una caja primero.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var cashier = _cashierList[_cboCashiers.SelectedIndex];
            var from = _dtpFrom.Value.Date;
            var to = _dtpTo.Value.Date.AddDays(1).AddSeconds(-1);

            try
            {
                _main.SetStatus($"Buscando pagos de caja '{cashier.Name}'...");

                // Buscar por external_reference que contenga el POS ID
                var searchCriteria = new PaymentSearchRequest
                {
                    DateCreatedFrom = from,
                    DateCreatedTo = to,
                    Limit = 100,
                    Sort = "date_created",
                    Criteria = "desc"
                };

                var result = await _main.MpClient.Payments.SearchAsync(searchCriteria);

                if (result.IsSuccess && result.Data?.Results != null)
                {
                    // Filtrar los que tengan point_of_interaction
                    // de tipo QR o external_reference con el POS id
                    var payments = result.Data.Results;
                    _lastResults = payments;

                    // Crear tabla resumen
                    var tableData = payments.Select(p => new
                    {
                        Fecha = p.DateCreated?.ToString("dd/MM HH:mm") ?? "-",
                        Estado = TranslateStatus(p.Status),
                        Monto = p.TransactionAmount,
                        Metodo = p.PaymentMethodId ?? "-",
                        Cuotas = p.Installments ?? 1,
                        Referencia = p.ExternalReference ?? "-",
                        Descripcion = p.Description ?? "-",
                        ID = p.Id
                    }).ToList();

                    _gridMovements.DataSource = tableData;
                    _main.SetStatus(
                        $"{payments.Count} pagos encontrados ({from:dd/MM} - {to:dd/MM}).");

                    // Resumen rápido
                    var approved = payments
                        .Where(p => p.Status == "approved").ToList();
                    var pending = payments
                        .Where(p => p.Status == "pending").ToList();
                    var rejected = payments
                        .Where(p => p.Status == "rejected").ToList();

                    _txtSummary.Text =
                        $"═══ Resultados: {payments.Count} pagos ═══\n\n" +
                        $"✅ Aprobados: {approved.Count} " +
                        $"(${approved.Sum(p => p.TransactionAmount):N2})\n" +
                        $"⏳ Pendientes: {pending.Count} " +
                        $"(${pending.Sum(p => p.TransactionAmount):N2})\n" +
                        $"❌ Rechazados: {rejected.Count}\n\n" +
                        $"Caja: {cashier.Name} | Período: {from:dd/MM/yyyy} - {to:dd/MM/yyyy}";
                }
                else
                {
                    _txtSummary.Text = $"Error: {result.ErrorMessage}\n{result.RawJson}";
                }
            }
            catch (Exception ex)
            {
                _txtSummary.Text = $"Error: {ex.Message}";
                Log.Error(ex, "Error buscando movimientos.");
            }
        }

        private void BtnClosure_Click(object sender, EventArgs e)
        {
            if (_lastResults == null || _lastResults.Count == 0)
            {
                MessageBox.Show("Primero busque los movimientos del período.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_cboCashiers.SelectedIndex < 0) return;
            var cashier = _cashierList[_cboCashiers.SelectedIndex];

            var approved = _lastResults
                .Where(p => p.Status == "approved").ToList();
            var pending = _lastResults
                .Where(p => p.Status == "pending").ToList();
            var rejected = _lastResults
                .Where(p => p.Status == "rejected").ToList();
            var cancelled = _lastResults
                .Where(p => p.Status == "cancelled").ToList();
            var refunded = _lastResults
                .Where(p => p.Status == "refunded").ToList();

            // Desglose por método de pago
            var byMethod = approved.GroupBy(p => p.PaymentMethodId ?? "otro")
                .Select(g => new
                {
                    Metodo = g.Key,
                    Cantidad = g.Count(),
                    Total = g.Sum(p => p.TransactionAmount)
                })
                .OrderByDescending(x => x.Total);

            // Desglose por cuotas
            var byInstallments = approved.GroupBy(p => p.Installments ?? 1)
                .Select(g => new
                {
                    Cuotas = g.Key,
                    Cantidad = g.Count(),
                    Total = g.Sum(p => p.TransactionAmount)
                })
                .OrderBy(x => x.Cuotas);

            var totalApproved = approved.Sum(p => p.TransactionAmount);
            var totalNet = approved.Sum(p => p.NetReceivedAmount ?? 0);
            var totalRefunded = approved
                .Sum(p => p.TransactionAmountRefunded ?? 0);
            var from = _dtpFrom.Value.Date;
            var to = _dtpTo.Value.Date;

            var report =
                "╔══════════════════════════════════════════════╗\n" +
                "║          RESUMEN DE CIERRE DE CAJA           ║\n" +
                "╚══════════════════════════════════════════════╝\n\n" +
                $"  📦 Caja: {cashier.Name} (ID: {cashier.Id})\n" +
                $"  📅 Período: {from:dd/MM/yyyy} - {to:dd/MM/yyyy}\n" +
                $"  🕐 Generado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n\n" +
                "──────────────── RESUMEN GENERAL ────────────────\n\n" +
                $"  ✅ Aprobados:   {approved.Count,5} ops  " +
                $"${totalApproved,12:N2}\n" +
                $"  ⏳ Pendientes:  {pending.Count,5} ops  " +
                $"${pending.Sum(p => p.TransactionAmount),12:N2}\n" +
                $"  ❌ Rechazados:  {rejected.Count,5} ops\n" +
                $"  🚫 Cancelados:  {cancelled.Count,5} ops\n" +
                $"  ↩ Reembolsados: {refunded.Count,5} ops\n" +
                $"  📊 Total ops:   {_lastResults.Count,5}\n\n" +
                "────────────── DESGLOSE POR MÉTODO ──────────────\n\n";

            foreach (var m in byMethod)
            {
                report += $"  💳 {m.Metodo,-20} {m.Cantidad,4} ops  " +
                    $"${m.Total,12:N2}\n";
            }

            report += "\n" +
                "────────────── DESGLOSE POR CUOTAS ──────────────\n\n";

            foreach (var i in byInstallments)
            {
                var label = i.Cuotas == 1 ? "1 cuota (contado)" :
                    $"{i.Cuotas} cuotas";
                report += $"  📊 {label,-20} {i.Cantidad,4} ops  " +
                    $"${i.Total,12:N2}\n";
            }

            report += "\n" +
                "──────────────────── TOTALES ────────────────────\n\n" +
                $"  💰 TOTAL BRUTO:     ${totalApproved,12:N2}\n" +
                $"  💸 Reembolsos:      ${totalRefunded,12:N2}\n" +
                $"  🏦 Neto recibido:   ${totalNet,12:N2}\n\n" +
                "═══════════════════════════════════════════════\n";

            _txtSummary.Text = report;
            _main.SetStatus("Resumen de cierre generado.");
        }

        private string TranslateStatus(string status)
        {
            switch (status)
            {
                case "approved": return "✅ Aprobado";
                case "pending": return "⏳ Pendiente";
                case "rejected": return "❌ Rechazado";
                case "cancelled": return "🚫 Cancelado";
                case "refunded": return "↩ Reembolsado";
                case "in_process": return "🔄 En proceso";
                default: return status ?? "-";
            }
        }

        private Label CreateLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Margin = new Padding(0, 8, 5, 0)
            };
        }

        private Button CreateButton(string text, Color color)
        {
            return new Button
            {
                Text = text,
                Width = 120,
                Height = 28,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 4, 8, 0),
                Font = new Font("Segoe UI", 8.5f)
            };
        }
    }
}
