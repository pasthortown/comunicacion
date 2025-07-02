using ImageActivityMonitor.App;
using ImageActivityMonitor.Infrastructure;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Security.Principal;
using Newtonsoft.Json;
using System;

namespace ImageActivityMonitor.UI
{
    public partial class MainForm : Form
    {
        private NotifyIcon notifyIcon;
        private static bool yaInicializado = false;

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

            // Crear menú contextual
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
            if (yaInicializado)
                return;
            yaInicializado = true;

            string userEmail = WindowsIdentity.GetCurrent().Name;
            if (userEmail.Contains("\\")) userEmail = userEmail.Split('\\')[1];

            string jwtToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyIjoiYWRtaW4ifQ.j7h-lJGsQ7X5u3H2Uj92BoWVfpYdS2DQvse7Z_DTPDI";
            string urlBase = "http://localhost:5050";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                try
                {
                    HttpResponseMessage response = await client.GetAsync($"{urlBase}/search/users/{userEmail}");

                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        var nuevoUsuario = new
                        {
                            email = userEmail
                        };

                        var json = JsonConvert.SerializeObject(nuevoUsuario);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        var postResponse = await client.PostAsync($"{urlBase}/users", content);

                        if (postResponse.IsSuccessStatusCode)
                        {
                            Console.WriteLine("✅ Usuario registrado correctamente");
                        }
                        else
                        {
                            Console.WriteLine("⚠️ Error al registrar usuario");
                        }
                    }
                    else
                    {
                        Console.WriteLine("✅ Usuario ya existe");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ Error al conectar al WebService: " + ex.Message);
                }
            }

            var guiWrapper = new GuiWrapper();
            var imageLoader = new ImageLoader();
            var monitorService = new UserMonitorService(guiWrapper);
            var logger = new ActivityLogger("activity.log");
            var displayService = new ImageDisplayService(imageLoader, guiWrapper, monitorService, logger);

            string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "tshirt.png");

            for (int zona = 0; zona < 9; zona++)
            {
                string estado = await displayService.MostrarImagenEnZonaAsync(imagePath, zona);
                Console.WriteLine($"[Zona {zona}] Estado: {estado}");
            }
        }
    }
}
