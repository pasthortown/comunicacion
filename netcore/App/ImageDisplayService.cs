using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImageActivityMonitor.Infrastructure;

namespace ImageActivityMonitor.App
{
    public class ImageDisplayService
    {
        private readonly ImageLoader _imageLoader;
        private readonly GuiWrapper _guiWrapper;
        private readonly UserMonitorService _monitorService;
        private readonly ActivityLogger _logger;

        public ImageDisplayService(
            ImageLoader imageLoader,
            GuiWrapper guiWrapper,
            UserMonitorService monitorService,
            ActivityLogger logger)
        {
            _imageLoader = imageLoader ?? throw new ArgumentNullException(nameof(imageLoader));
            _guiWrapper = guiWrapper ?? throw new ArgumentNullException(nameof(guiWrapper));
            _monitorService = monitorService ?? throw new ArgumentNullException(nameof(monitorService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> MostrarImagenEnZonaAsync(string imagePath, int zona)
        {
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            var image = _imageLoader.LoadImage(imagePath, 100, out int imgWidth, out int imgHeight);

            var pos = _guiWrapper.CalcularPosicionPorZona(zona, screenWidth, screenHeight, imgWidth, imgHeight);

            var form = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                ShowInTaskbar = false,
                TopMost = true,
                BackColor = Color.White,
                TransparencyKey = Color.White,
                Bounds = new Rectangle(pos.X, pos.Y, imgWidth, imgHeight),
                Opacity = 0
            };

            var pictureBox = new PictureBox
            {
                Image = image,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            form.Controls.Add(pictureBox);
            form.Show();

            int fadein = 1000, visible = 5000, fadeout = 1000;
            int pasos = 30;

            for (int i = 0; i < pasos; i++)
            {
                form.Opacity = i / (double)pasos;
                await Task.Delay(fadein / pasos);
            }

            var monitoreo = _monitorService.MonitorearActividadAsync((fadein + visible + fadeout) / 1000);

            await Task.Delay(visible);

            for (int i = pasos; i >= 0; i--)
            {
                form.Opacity = i / (double)pasos;
                await Task.Delay(fadeout / pasos);
            }

            form.Close();

            string estado = await monitoreo ? "Activo" : "Inactivo";
            _logger.Log(zona, estado);

            return estado;
        }
    }
}
