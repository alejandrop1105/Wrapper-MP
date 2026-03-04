using System;
using System.Drawing;
using System.Windows.Forms;
using MercadoPago.Wrapper.Models.Stores;
using MercadoPago.Wrapper.Models.Pos;
using Newtonsoft.Json;
using Serilog;

namespace MercadoPago.Demo.WinForms.Forms
{
    /// <summary>Panel de gestión de sucursales y cajas.</summary>
    public class StoresPanel : UserControl
    {
        private readonly MainForm _main;
        private DataGridView _gridStores;
        private DataGridView _gridCashiers;
        private RichTextBox _txtResult;

        public StoresPanel(MainForm main)
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
                SplitterDistance = 350
            };

            // ─── Panel superior: Stores y Cajas ───
            var topSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 500
            };

            // Stores
            var storePanel = new Panel { Dock = DockStyle.Fill };
            var storeHeader = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 45,
                AutoSize = false
            };
            storeHeader.Controls.Add(new Label
            {
                Text = "🏪 Sucursales",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 8, 15, 0)
            });
            var btnLoadStores = CreateButton("Cargar", Color.FromArgb(0, 122, 204));
            btnLoadStores.Click += BtnLoadStores_Click;
            var btnCreateStore = CreateButton("+ Crear", Color.FromArgb(40, 167, 69));
            btnCreateStore.Click += BtnCreateStore_Click;
            storeHeader.Controls.AddRange(new Control[] { btnLoadStores, btnCreateStore });

            _gridStores = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false
            };

            storePanel.Controls.Add(_gridStores);
            storePanel.Controls.Add(storeHeader);
            topSplit.Panel1.Controls.Add(storePanel);

            // Cashiers
            var cashierPanel = new Panel { Dock = DockStyle.Fill };
            var cashierHeader = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 45,
                AutoSize = false
            };
            cashierHeader.Controls.Add(new Label
            {
                Text = "📦 Cajas (POS)",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 8, 15, 0)
            });
            var btnLoadCashiers = CreateButton("Cargar", Color.FromArgb(0, 122, 204));
            btnLoadCashiers.Click += BtnLoadCashiers_Click;
            var btnCreateCashier = CreateButton("+ Crear", Color.FromArgb(40, 167, 69));
            btnCreateCashier.Click += BtnCreateCashier_Click;
            cashierHeader.Controls.AddRange(new Control[] { btnLoadCashiers, btnCreateCashier });

            _gridCashiers = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false
            };

            cashierPanel.Controls.Add(_gridCashiers);
            cashierPanel.Controls.Add(cashierHeader);
            topSplit.Panel2.Controls.Add(cashierPanel);

            mainSplit.Panel1.Controls.Add(topSplit);

            // ─── Panel inferior: Resultado ───
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
        }

        private async void BtnLoadStores_Click(object sender, EventArgs e)
        {
            if (_main.MpClient == null) return;
            try
            {
                _main.SetStatus("Cargando sucursales...");
                var result = await _main.MpClient.Stores.SearchAsync();
                if (result.IsSuccess && result.Data?.Results != null)
                {
                    _gridStores.DataSource = result.Data.Results;
                    _main.SetStatus($"{result.Data.Results.Count} sucursales cargadas.");
                }
                else
                {
                    _txtResult.Text = $"Error: {result.ErrorMessage}\n{result.RawJson}";
                }
            }
            catch (Exception ex)
            {
                _txtResult.Text = $"Error: {ex.Message}";
                Log.Error(ex, "Error cargando sucursales.");
            }
        }

        private async void BtnCreateStore_Click(object sender, EventArgs e)
        {
            if (_main.MpClient == null) return;
            var name = Microsoft.VisualBasic.Interaction.InputBox(
                "Nombre de la sucursal:", "Nueva Sucursal", "Sucursal Demo");
            if (string.IsNullOrEmpty(name)) return;

            var street = Microsoft.VisualBasic.Interaction.InputBox(
                "Calle:", "Ubicación", "Av. Corrientes");
            if (string.IsNullOrEmpty(street)) return;

            var number = Microsoft.VisualBasic.Interaction.InputBox(
                "Número:", "Ubicación", "1234");

            var city = Microsoft.VisualBasic.Interaction.InputBox(
                "Ciudad (del catálogo MP, ej: La Plata, Mar del Plata, Quilmes):",
                "Ubicación", "La Plata");

            var state = Microsoft.VisualBasic.Interaction.InputBox(
                "Provincia/Estado:", "Ubicación", "Buenos Aires");

            try
            {
                _main.SetStatus("Creando sucursal...");
                var result = await _main.MpClient.Stores.CreateAsync(
                    new StoreCreateRequest
                    {
                        Name = name,
                        ExternalId = "store-" + DateTime.Now.Ticks,
                        Location = new StoreLocationRequest
                        {
                            StreetName = street,
                            StreetNumber = number,
                            CityName = city,
                            StateName = state,
                            Latitude = -34.603722,
                            Longitude = -58.381592
                        }
                    });

                _txtResult.Text = result.IsSuccess
                    ? $"✅ Sucursal creada: {result.Data.Id}\n{JsonConvert.SerializeObject(result.Data, Formatting.Indented)}"
                    : $"❌ Error: {result.ErrorMessage}\n{result.RawJson}";
                _main.SetStatus("Listo.");
            }
            catch (Exception ex)
            {
                _txtResult.Text = $"Error: {ex.Message}";
            }
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
                    _gridCashiers.DataSource = result.Data.Results;
                    _main.SetStatus($"{result.Data.Results.Count} cajas cargadas.");
                }
                else
                {
                    _txtResult.Text = $"Error: {result.ErrorMessage}\n{result.RawJson}";
                }
            }
            catch (Exception ex)
            {
                _txtResult.Text = $"Error: {ex.Message}";
                Log.Error(ex, "Error cargando cajas.");
            }
        }

        private async void BtnCreateCashier_Click(object sender, EventArgs e)
        {
            if (_main.MpClient == null) return;
            var name = Microsoft.VisualBasic.Interaction.InputBox(
                "Nombre de la caja:", "Nueva Caja", "Caja 1");
            if (string.IsNullOrEmpty(name)) return;

            try
            {
                _main.SetStatus("Creando caja...");
                var result = await _main.MpClient.Cashiers.CreateAsync(
                    new PosCreateRequest
                    {
                        Name = name,
                        ExternalId = "pos-" + DateTime.Now.Ticks,
                        FixedAmount = false
                    });

                _txtResult.Text = result.IsSuccess
                    ? $"✅ Caja creada: {result.Data.Id}\n{JsonConvert.SerializeObject(result.Data, Formatting.Indented)}"
                    : $"❌ Error: {result.ErrorMessage}\n{result.RawJson}";
                _main.SetStatus("Listo.");
            }
            catch (Exception ex)
            {
                _txtResult.Text = $"Error: {ex.Message}";
            }
        }

        private Button CreateButton(string text, Color color)
        {
            return new Button
            {
                Text = text,
                Width = 90,
                Height = 30,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 6, 5, 0)
            };
        }
    }
}
