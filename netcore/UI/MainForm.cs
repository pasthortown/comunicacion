using ImageActivityMonitor.App;
using ImageActivityMonitor.Infrastructure;
using ImageActivityMonitor.Models;
using ImageActivityMonitor.Services;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace ImageActivityMonitor.UI
{
    public partial class MainForm : Form
    {
        private NotifyIcon notifyIcon;
        private static bool yaInicializado = false;
        private List<(int messageId, DateTime schedule, bool showed)> agenda = new();
        private Dictionary<int, dynamic> mensajes = new();
        private System.Timers.Timer refreshTimer;
        private System.Timers.Timer mostrarTimer;
        private MessageDisplayService? messageDisplayService;

        public MainForm()
        {
            InitializeComponent();

            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;
            this.Visible = false;
            this.Opacity = 0;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Load += MainForm_Load;

            InicializarNotifyIcon();
        }

        private void InicializarNotifyIcon()
        {
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new Icon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "main.ico"));
            notifyIcon.Visible = true;
            notifyIcon.Text = "Herramienta de Comunicación";

            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem salirMenuItem = new ToolStripMenuItem("Salir");
            salirMenuItem.Click += (s, e) =>
            {
                notifyIcon.Visible = false;
                Application.Exit();
            };
            contextMenu.Items.Add(salirMenuItem);
            notifyIcon.ContextMenuStrip = contextMenu;
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            if (yaInicializado) return;
            yaInicializado = true;

            string jwtToken = EnvReader.Get("JWT_TOKEN");
            string urlBase = EnvReader.Get("WEB_SERVICE_URL");
            int refreshSeconds = int.TryParse(EnvReader.Get("REFRESHTIME"), out int val) ? val : 60;

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            await SincronizarTodoAsync(client, urlBase);

            refreshTimer = new System.Timers.Timer(refreshSeconds * 1000);
            refreshTimer.Elapsed += async (s, args) =>
            {
                _ = Task.Run(() => SincronizarTodoAsync(client, urlBase));
            };
            refreshTimer.Start();

            var guiWrapper = new GuiWrapper();
            var imageLoader = new ImageLoader();
            var monitorService = new UserMonitorService(guiWrapper);
            var logger = new ActivityLogger();

            var imageMessageService = new ImageMessageDisplayService(imageLoader, guiWrapper, monitorService, logger);
            messageDisplayService = new MessageDisplayService(imageMessageService);

            mostrarTimer = new System.Timers.Timer(20_000);
            mostrarTimer.Elapsed += async (s, args) =>
            {
                if (messageDisplayService != null)
                    await MostrarMensajesDelDiaAsync(messageDisplayService);
            };
            mostrarTimer.Start();
        }

        private async Task SincronizarTodoAsync(HttpClient client, string urlBase)
        {
            List<string> userGroups = new();
            var userData = new UserDataGetter(client, urlBase);
            string userEmail = userData.GetWindowsUsername();
            Console.WriteLine($"[Usuario]: {userEmail}");

            try
            {
                await userData.RegisterUserIfNotExists(userEmail);
                userGroups = await userData.GetUserGroups(userEmail);
                Console.WriteLine($"[Grupos]: {string.Join(", ", userGroups)}");

                Database.TruncateGroups();
                foreach (var group in userGroups)
                    Database.InsertGroup(group);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error conexión WS - usando SQLite]: {ex.Message}");
                userGroups = Database.GetGroups();
                Console.WriteLine($"[Grupos desde SQLite]: {string.Join(", ", userGroups)}");
            }

            var messageGetter = new MessageGetter(client, urlBase);

            Console.WriteLine("[Limpieza SQLite]");
            messageGetter.LimpiarAgendasNoHoy();

            try
            {
                Console.WriteLine("[Sincronización con Web Service]");
                await messageGetter.SincronizarConWebService(userGroups);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Sincronización fallida]: {ex.Message}");
            }

            Console.WriteLine("[Cargando datos desde SQLite]");
            agenda = await messageGetter.BuildAgenda(userGroups);
            mensajes = await messageGetter.FetchMessages(agenda);

            Console.WriteLine("Agenda final:");
            foreach (var item in agenda)
            {
                Console.WriteLine($"message_id: {item.messageId}, schedule: {item.schedule:g}, showed: {item.showed}");
            }
        }

        private async Task MostrarMensajesDelDiaAsync(MessageDisplayService service)
        {
            try
            {
                Console.WriteLine("[Verificando mensajes para mostrar...]");

                var ahora = DateTime.Now;
                foreach (var item in agenda.Where(a => !a.showed))
                {
                    Console.WriteLine($"Evaluando item -> id: {item.messageId}, schedule: {item.schedule:g}, showed: {item.showed}");
                    Console.WriteLine($"Hora actual: {ahora}");

                    if (item.schedule.Year == ahora.Year &&
                        item.schedule.Month == ahora.Month &&
                        item.schedule.Day == ahora.Day &&
                        item.schedule.Hour == ahora.Hour &&
                        item.schedule.Minute == ahora.Minute)
                    {
                        Console.WriteLine($"Coincidencia exacta encontrada: {item.schedule:g}");

                        if (!mensajes.ContainsKey(item.messageId))
                        {
                            Console.WriteLine($"[Falta mensaje en memoria] ID: {item.messageId}");
                            continue;
                        }

                        dynamic rawMessage = mensajes[item.messageId];
                        string type = rawMessage.type;

                        Console.WriteLine($"Tipo de mensaje: {type}");

                        if (type == "image")
                        {
                            var mensaje = new ImageMessage
                            {
                                Type = rawMessage.type,
                                Link = rawMessage.link,
                                Duration = (int)rawMessage.duration,
                                Zone = (int)rawMessage.zone,
                                Content = (string)rawMessage.content.image,
                                Width = rawMessage.width != null ? (int)rawMessage.width : 400
                            };

                            await this.InvokeAsync(async () =>
                            {
                                string estado = await service.MostrarMensajeAsync(mensaje);
                                Console.WriteLine($"[Mostrado] Zona {mensaje.Zone}, Estado: {estado}");

                                Database.MarkAgendaAsShowed(item.messageId, item.schedule);
                                agenda = Database.GetAgenda();
                            });
                        }
                        else
                        {
                            Console.WriteLine($"[Tipo no soportado]: {type}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al mostrar mensaje: " + ex.Message);
            }
        }
    }

    public static class ControlExtensions
    {
        public static Task InvokeAsync(this Control control, Func<Task> func)
        {
            var tcs = new TaskCompletionSource<object>();
            control.Invoke(new Action(async () =>
            {
                try
                {
                    await func();
                    tcs.SetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }));
            return tcs.Task;
        }
    }
}
