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
using System.Security.Principal;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageActivityMonitor.UI
{
    public partial class MainForm : Form
    {
        private NotifyIcon notifyIcon;
        private static bool yaInicializado = false;
        private List<(int messageId, DateTime schedule, bool showed)> agenda = new();
        private Dictionary<int, dynamic> mensajes = new();

        public MainForm()
        {
            InitializeComponent();

            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;
            this.Visible = false;
            this.Opacity = 0;

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

            List<string> userGroups = new();

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                var userData = new UserDataGetter(client, urlBase);
                string userEmail = userData.GetWindowsUsername();
                Console.WriteLine($"[Usuario]: {userEmail}");

                try
                {
                    await userData.RegisterUserIfNotExists(userEmail);
                    userGroups = await userData.GetUserGroups(userEmail);
                    Console.WriteLine($"[Grupos]: {string.Join(", ", userGroups)}");

                    // Guardar grupos en SQLite
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

            var guiWrapper = new GuiWrapper();
            var imageLoader = new ImageLoader();
            var monitorService = new UserMonitorService(guiWrapper);
            var logger = new ActivityLogger();

            var imageMessageService = new ImageMessageDisplayService(imageLoader, guiWrapper, monitorService, logger);
            var messageDisplayService = new MessageDisplayService(imageMessageService);

            await MostrarMensajesDelDiaAsync(messageDisplayService);
        }

        private async Task MostrarMensajesDelDiaAsync(MessageDisplayService service)
        {
            try
            {
                if (agenda.Count == 0)
                {
                    Console.WriteLine("[Sin mensajes en agenda]");
                    return;
                }

                var primerItem = agenda.FirstOrDefault();
                if (!mensajes.ContainsKey(primerItem.messageId))
                {
                    Console.WriteLine($"[Falta mensaje en SQLite] ID: {primerItem.messageId}");
                    return;
                }

                dynamic rawMessage = mensajes[primerItem.messageId];
                string type = rawMessage.type;

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

                    string estado = await service.MostrarMensajeAsync(mensaje);
                    Console.WriteLine($"[Mostrado] Zona {mensaje.Zone}, Estado: {estado}");
                }
                else
                {
                    Console.WriteLine($"[Tipo no soportado]: {type}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al mostrar mensaje: " + ex.Message);
            }
        }
    }
}
