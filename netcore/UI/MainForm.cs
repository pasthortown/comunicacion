using ImageActivityMonitor.App;
using ImageActivityMonitor.Infrastructure;
using System.IO;
using System.Windows.Forms;
using System.Drawing;

namespace ImageActivityMonitor.UI
{
    public partial class MainForm : Form
    {
        private NotifyIcon notifyIcon;

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
