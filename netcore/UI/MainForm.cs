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

        private List<(int messageId, DateTime schedule)> agenda = new();
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

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                var userData = new UserDataGetter(client, urlBase);
                string userEmail = userData.GetWindowsUsername();

                await userData.RegisterUserIfNotExists(userEmail);
                List<string> userGroups = await userData.GetUserGroups(userEmail);

                var messageGetter = new MessageGetter(client, urlBase);
                agenda = await messageGetter.BuildAgenda(userGroups);
                mensajes = await messageGetter.FetchMessages(agenda);

                Console.WriteLine("Agenda final:");
                foreach (var item in agenda)
                {
                    Console.WriteLine($"message_id: {item.messageId}, schedule: {item.schedule:g}");
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
                string base64Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "tshirt_base64.txt");

                if (!File.Exists(base64Path))
                {
                    MessageBox.Show("No se encontró el archivo base64 para la imagen de prueba.");
                    return;
                }

                string base64 = File.ReadAllText(base64Path);

                var mensaje = new ImageMessage
                {
                    Type = "image",
                    Link = "https://www.youtube.com",
                    Duration = 10,
                    Zone = 1,
                    Content = base64,
                    Width = 100
                };

                string estado = await service.MostrarMensajeAsync(mensaje);
                Console.WriteLine($"Estado del mensaje en zona {mensaje.Zone}: {estado}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al mostrar mensaje: " + ex.Message);
            }
        }
    }
}
