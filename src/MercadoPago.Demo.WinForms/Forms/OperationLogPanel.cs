using System;
using System.Drawing;
using System.Windows.Forms;
using MercadoPago.Demo.WinForms.Data.Repositories;
using Newtonsoft.Json;

namespace MercadoPago.Demo.WinForms.Forms
{
    /// <summary>Panel de historial de operaciones.</summary>
    public class OperationLogPanel : UserControl
    {
        private readonly MainForm _main;
        private DataGridView _grid;
        private RichTextBox _txtDetail;

        public OperationLogPanel(MainForm main)
        {
            _main = main;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Padding = new Padding(10);

            // Header
            var header = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40
            };
            var btnRefresh = new Button
            {
                Text = "🔄 Actualizar",
                Width = 120,
                Height = 30,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += (s, e) => LoadLog();
            header.Controls.Add(btnRefresh);

            // Split
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 300
            };

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false
            };
            _grid.SelectionChanged += Grid_SelectionChanged;
            split.Panel1.Controls.Add(_grid);

            _txtDetail = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.FromArgb(220, 220, 220)
            };
            split.Panel2.Controls.Add(_txtDetail);

            Controls.Add(split);
            Controls.Add(header);

            LoadLog();
        }

        private void LoadLog()
        {
            try
            {
                var entries = _main.LogRepo.GetRecent(200);
                _grid.DataSource = entries;

                // Ocultar columnas de JSON pesadas
                if (_grid.Columns.Contains("RequestJson"))
                    _grid.Columns["RequestJson"].Visible = false;
                if (_grid.Columns.Contains("ResponseJson"))
                    _grid.Columns["ResponseJson"].Visible = false;
            }
            catch (Exception ex)
            {
                _txtDetail.Text = $"Error cargando log: {ex.Message}";
            }
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (_grid.CurrentRow?.DataBoundItem is OperationLogEntity entry)
            {
                var detail = $"═══ Operación: {entry.OperationType} ═══\n" +
                    $"ID: {entry.ExternalId}\n" +
                    $"Ref: {entry.ExternalReference}\n" +
                    $"Status: {entry.Status}\n" +
                    $"Monto: {entry.Amount}\n" +
                    $"Fecha: {entry.CreatedAt}\n";

                if (!string.IsNullOrEmpty(entry.ErrorMessage))
                    detail += $"\n❌ Error: {entry.ErrorMessage}\n";

                if (!string.IsNullOrEmpty(entry.RequestJson))
                {
                    try
                    {
                        var formatted = JsonConvert.SerializeObject(
                            JsonConvert.DeserializeObject(entry.RequestJson),
                            Formatting.Indented);
                        detail += $"\n─── Request ───\n{formatted}\n";
                    }
                    catch { detail += $"\n─── Request ───\n{entry.RequestJson}\n"; }
                }

                if (!string.IsNullOrEmpty(entry.ResponseJson))
                {
                    try
                    {
                        var formatted = JsonConvert.SerializeObject(
                            JsonConvert.DeserializeObject(entry.ResponseJson),
                            Formatting.Indented);
                        detail += $"\n─── Response ───\n{formatted}\n";
                    }
                    catch { detail += $"\n─── Response ───\n{entry.ResponseJson}\n"; }
                }

                _txtDetail.Text = detail;
            }
        }
    }
}
