using System;
using System.Drawing;
using System.Windows.Forms;
using MercadoPago.Wrapper.Models.Customers;
using Newtonsoft.Json;
using Serilog;

namespace MercadoPago.Demo.WinForms.Forms
{
    /// <summary>Panel de gestión de clientes.</summary>
    public class CustomersPanel : UserControl
    {
        private readonly MainForm _main;
        private DataGridView _gridCustomers;
        private TextBox _txtEmail;
        private TextBox _txtFirstName;
        private TextBox _txtLastName;
        private RichTextBox _txtResult;

        public CustomersPanel(MainForm main)
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
                SplitterDistance = 300
            };

            // ─── Superior: Formulario y grilla ───
            var formPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 160,
                ColumnCount = 2,
                Padding = new Padding(5)
            };
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            int row = 0;
            formPanel.Controls.Add(new Label { Text = "Email:", AutoSize = true }, 0, row);
            _txtEmail = new TextBox { Width = 300 };
            formPanel.Controls.Add(_txtEmail, 1, row++);

            formPanel.Controls.Add(new Label { Text = "Nombre:", AutoSize = true }, 0, row);
            _txtFirstName = new TextBox { Width = 200 };
            formPanel.Controls.Add(_txtFirstName, 1, row++);

            formPanel.Controls.Add(new Label { Text = "Apellido:", AutoSize = true }, 0, row);
            _txtLastName = new TextBox { Width = 200 };
            formPanel.Controls.Add(_txtLastName, 1, row++);

            var btnPanel = new FlowLayoutPanel { AutoSize = true };
            var btnCreate = CreateButton("+ Crear", Color.FromArgb(40, 167, 69));
            btnCreate.Click += BtnCreate_Click;
            var btnSearch = CreateButton("🔍 Buscar", Color.FromArgb(0, 122, 204));
            btnSearch.Click += BtnSearch_Click;
            btnPanel.Controls.AddRange(new Control[] { btnCreate, btnSearch });
            formPanel.Controls.Add(btnPanel, 1, row);

            _gridCustomers = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false
            };

            var topPanel = new Panel { Dock = DockStyle.Fill };
            topPanel.Controls.Add(_gridCustomers);
            topPanel.Controls.Add(formPanel);
            split.Panel1.Controls.Add(topPanel);

            // ─── Inferior: Resultado ───
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

        private async void BtnCreate_Click(object sender, EventArgs e)
        {
            if (_main.MpClient == null) return;
            try
            {
                _main.SetStatus("Creando cliente...");
                var result = await _main.MpClient.Customers.CreateAsync(
                    new CustomerCreateRequest
                    {
                        Email = _txtEmail.Text,
                        FirstName = _txtFirstName.Text,
                        LastName = _txtLastName.Text
                    });

                _txtResult.Text = result.IsSuccess
                    ? $"✅ Cliente creado: {result.Data.Id}\n{JsonConvert.SerializeObject(result.Data, Formatting.Indented)}"
                    : $"❌ {result.ErrorMessage}\n{result.RawJson}";
                _main.SetStatus("Listo.");
            }
            catch (Exception ex)
            {
                _txtResult.Text = $"Error: {ex.Message}";
                Log.Error(ex, "Error creando cliente.");
            }
        }

        private async void BtnSearch_Click(object sender, EventArgs e)
        {
            if (_main.MpClient == null) return;
            try
            {
                _main.SetStatus("Buscando clientes...");
                var result = await _main.MpClient.Customers.SearchAsync(
                    new CustomerSearchRequest
                    {
                        Email = string.IsNullOrEmpty(_txtEmail.Text)
                            ? null : _txtEmail.Text
                    });

                if (result.IsSuccess && result.Data?.Results != null)
                {
                    _gridCustomers.DataSource = result.Data.Results;
                    _main.SetStatus($"{result.Data.Results.Count} clientes encontrados.");
                }
                else
                {
                    _txtResult.Text = $"❌ {result.ErrorMessage}\n{result.RawJson}";
                }
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
                Width = 100,
                Height = 30,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 5, 0)
            };
        }
    }
}
