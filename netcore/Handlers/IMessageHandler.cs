using System.Threading.Tasks;
using ImageActivityMonitor.Models;

namespace ImageActivityMonitor.Handlers
{
    public interface IMessageHandler
    {
        Task<string> DisplayAsync(MessageBase message);
    }
}
