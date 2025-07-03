using System;
using System.Threading.Tasks;
using ImageActivityMonitor.Models;
using ImageActivityMonitor.Infrastructure;

namespace ImageActivityMonitor.App
{
    public class MessageDisplayService
    {
        private readonly ImageMessageDisplayService _imageMessageDisplayService;

        public MessageDisplayService(ImageMessageDisplayService imageMessageDisplayService)
        {
            _imageMessageDisplayService = imageMessageDisplayService;
        }

        public async Task<string> MostrarMensajeAsync(MessageBase mensaje)
        {
            switch (mensaje.Type.ToLower())
            {
                case "image":
                    return await _imageMessageDisplayService.MostrarMensajeAsync((ImageMessage)mensaje);
                default:
                    return "Tipo de mensaje no soportado aún";
            }
        }
    }
}