using System;
using System.Drawing;
using System.Windows.Forms;
using MercadoPago.Demo.WinForms.Data.Repositories;
using MercadoPago.Wrapper.Models.Webhooks;
using Newtonsoft.Json;
using Serilog;

namespace MercadoPago.Demo.WinForms.Forms
{
    /// <summary>Panel de monitoreo de webhooks en tiempo real.</summary>
    public class WebhookPanel : UserControl
    {
        private readonly MainForm _main;
        private ListView _listView;
        private RichTextBox _txtDetail;
        private Button _btnStart;
        private Button _btnStop;
        private Label _lblStatus;

        public WebhookPanel(MainForm main)
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
                Height = 45
            };
            _btnStart = new Button
            {
                Text = "▶ Iniciar",
                Width = 100,
                Height = 30,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnStart.Click += BtnStart_Click;

            _btnStop = new Button
            {
                Text = "⏹ Detener",
                Width = 100,
                Height = 30,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            _btnStop.Click += BtnStop_Click;

            var btnRefresh = new Button
            {
                Text = "🔄 Refrescar",
                Width = 100,
                Height = 30,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += (s, e) => LoadWebhookLog();

            _lblStatus = new Label
            {
                Text = "⚪ Detenido",
                AutoSize = true,
                Margin = new Padding(15, 8, 0, 0),
                Font = new Font("Segoe UI", 10)
            };

            header.Controls.AddRange(new Control[]
            { _btnStart, _btnStop, btnRefresh, _lblStatus });

            // Split
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 300
            };

            _listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9)
            };
            _listView.Columns.Add("Fecha", 160);
            _listView.Columns.Add("Tipo", 120);
            _listView.Columns.Add("Acción", 150);
            _listView.Columns.Add("Resource ID", 200);
            _listView.Columns.Add("Válido", 60);
            _listView.SelectedIndexChanged += ListView_Selected;
            split.Panel1.Controls.Add(_listView);

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

            LoadWebhookLog();
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (_main.MpClient == null)
            {
                MessageBox.Show("Configure las credenciales primero.");
                return;
            }

            try
            {
                var cfg = _main.ConfigRepo.Get();
                var listener = _main.MpClient.ConfigureWebhookListener(
                    cfg?.WebhookPort ?? 5100,
                    "/webhooks/mp",
                    cfg?.WebhookSecret);

                listener.OnNotificationReceived += OnWebhookReceived;

                // Eventos diferenciados de homologación
                listener.OnOrderCancelled += (s2, args) =>
                {
                    if (InvokeRequired)
                        Invoke(new Action(() => AddTaggedWebhook(args, "🚫 CANCELADA", Color.Red)));
                    else
                        AddTaggedWebhook(args, "🚫 CANCELADA", Color.Red);
                };

                listener.OnActionRequired += (s2, args) =>
                {
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() =>
                        {
                            AddTaggedWebhook(args, "⚠️ ACCIÓN REQUERIDA", Color.DarkOrange);
                            MessageBox.Show(
                                "⚠️ Una orden requiere acción manual del operador.\n\n" +
                                $"Order ID: {args.Notification?.Data?.Id}\n\n" +
                                "Registre manualmente la operación según el estado " +
                                "final en el dispositivo.",
                                "Acción Requerida",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }));
                    }
                };

                listener.Start();

                _btnStart.Enabled = false;
                _btnStop.Enabled = true;
                _lblStatus.Text = $"🟢 Escuchando en puerto {listener.Port}";
                _lblStatus.ForeColor = Color.DarkGreen;
                _main.SetStatus($"Webhook listener activo en puerto {listener.Port}.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}\n\nSi es un error de permisos, " +
                    "ejecute la app como Administrador o registre el prefijo URL con:\n" +
                    "netsh http add urlacl url=http://+:5100/webhooks/mp/ user=EVERYONE",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log.Error(ex, "Error iniciando webhook listener.");
            }
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            _main.MpClient?.WebhookListener?.Stop();
            _btnStart.Enabled = true;
            _btnStop.Enabled = false;
            _lblStatus.Text = "⚪ Detenido";
            _lblStatus.ForeColor = Color.Gray;
            _main.SetStatus("Webhook listener detenido.");
        }

        private void OnWebhookReceived(object sender, WebhookEventArgs e)
        {
            // Guardar en DB
            _main.WebhookRepo.Insert(new WebhookLogEntity
            {
                EventType = e.Notification?.Type,
                ResourceId = e.Notification?.Data?.Id,
                Action = e.Notification?.Action,
                RawJson = e.RawJson
            });

            // Actualizar UI en thread principal
            if (InvokeRequired)
            {
                Invoke(new Action(() => AddWebhookToList(e)));
            }
            else
            {
                AddWebhookToList(e);
            }
        }

        private void AddWebhookToList(WebhookEventArgs e)
        {
            var item = new ListViewItem(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            item.SubItems.Add(e.Notification?.Type ?? "?");
            item.SubItems.Add(e.Notification?.Action ?? "?");
            item.SubItems.Add(e.Notification?.Data?.Id ?? "?");
            item.SubItems.Add(e.IsValid ? "✅" : "❌");
            item.Tag = e.RawJson;

            _listView.Items.Insert(0, item);
        }

        private void AddTaggedWebhook(WebhookEventArgs e, string tag, Color color)
        {
            var item = new ListViewItem(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            item.SubItems.Add(e.Notification?.Type ?? "?");
            item.SubItems.Add(tag);
            item.SubItems.Add(e.Notification?.Data?.Id ?? "?");
            item.SubItems.Add(e.IsValid ? "✅" : "❌");
            item.Tag = e.RawJson;
            item.ForeColor = color;

            _listView.Items.Insert(0, item);
        }

        private void ListView_Selected(object sender, EventArgs e)
        {
            if (_listView.SelectedItems.Count > 0)
            {
                var raw = _listView.SelectedItems[0].Tag as string;
                try
                {
                    var formatted = JsonConvert.SerializeObject(
                        JsonConvert.DeserializeObject(raw), Formatting.Indented);
                    _txtDetail.Text = formatted;
                }
                catch
                {
                    _txtDetail.Text = raw;
                }
            }
        }

        private void LoadWebhookLog()
        {
            try
            {
                var entries = _main.WebhookRepo.GetRecent(100);
                _listView.Items.Clear();
                foreach (var entry in entries)
                {
                    var item = new ListViewItem(entry.ReceivedAt);
                    item.SubItems.Add(entry.EventType ?? "?");
                    item.SubItems.Add(entry.Action ?? "?");
                    item.SubItems.Add(entry.ResourceId ?? "?");
                    item.SubItems.Add(entry.Processed == 1 ? "✅" : "⏳");
                    item.Tag = entry.RawJson;
                    _listView.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                _txtDetail.Text = $"Error: {ex.Message}";
            }
        }
    }
}
