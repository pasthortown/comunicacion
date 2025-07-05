using System.Threading.Tasks;
using ImageActivityMonitor.Models;

namespace ImageActivityMonitor.App
{
    public interface BaseMessageDisplayService
    {
        string TypeHandled { get; }
        Task<string> MostrarMensajeAsync(MessageBase mensaje);
    }
}
