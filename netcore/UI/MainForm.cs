using ImageActivityMonitor.App;
using ImageActivityMonitor.Infrastructure;
using ImageActivityMonitor.Models;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Security.Principal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImageActivityMonitor.UI
{
    public partial class MainForm : Form
    {
        private NotifyIcon notifyIcon;
        private static bool yaInicializado = false;
        private List<string> userGroups = new();

        public MainForm()
        {
            InitializeComponent();

            // Ocultar el formulario completamente
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

            string userEmail = WindowsIdentity.GetCurrent().Name;
            if (userEmail.Contains("\\")) userEmail = userEmail.Split('\\')[1];

            string jwtToken = EnvReader.Get("JWT_TOKEN");
            string urlBase = EnvReader.Get("WEB_SERVICE_URL");

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                try
                {
                    // Verificar existencia del usuario
                    HttpResponseMessage response = await client.GetAsync($"{urlBase}/search/users/{userEmail}");

                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        var nuevoUsuario = new { email = userEmail };
                        var json = JsonConvert.SerializeObject(nuevoUsuario);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        var postResponse = await client.PostAsync($"{urlBase}/users", content);

                        if (postResponse.IsSuccessStatusCode)
                            Console.WriteLine("Usuario registrado correctamente");
                        else
                            Console.WriteLine("Error al registrar usuario");
                    }
                    else
                    {
                        Console.WriteLine("Usuario ya existe");

                        // Obtener grupos del usuario
                        HttpResponseMessage groupResponse = await client.GetAsync($"{urlBase}/search/usersgroup/{userEmail}");
                        if (groupResponse.IsSuccessStatusCode)
                        {
                            string responseContent = await groupResponse.Content.ReadAsStringAsync();
                            var parsed = JsonConvert.DeserializeObject<dynamic>(responseContent);
                            var items = parsed.response;

                            foreach (var item in items)
                            {
                                if (item.group != null)
                                    userGroups.Add((string)item.group.ToString());
                            }

                            Console.WriteLine("Grupos del usuario: " + string.Join(", ", userGroups));
                        }
                        else
                        {
                            Console.WriteLine("No se pudieron obtener los grupos del usuario");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al conectar al WebService: " + ex.Message);
                }
            }

            // Instanciar servicios
            var guiWrapper = new GuiWrapper();
            var imageLoader = new ImageLoader();
            var monitorService = new UserMonitorService(guiWrapper);
            var logger = new ActivityLogger();

            var imageMessageService = new ImageMessageDisplayService(imageLoader, guiWrapper, monitorService, logger);
            var messageDisplayService = new MessageDisplayService(imageMessageService);

            // Por ahora seguimos con una prueba simple
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
                    Link = "https://www.google.com",
                    Duration = 10,
                    Zone = 1,
                    Content = base64
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
