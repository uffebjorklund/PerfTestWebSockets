using XSockets.Core.XSocket;
using XSockets.Core.XSocket.Helpers;
using XSockets.Core.Common.Socket.Event.Interface;
using System.Threading.Tasks;
using XSockets.Core.Common.Socket.Attributes;

namespace SelfHosted.XSocketsNET
{
    /// <summary>
    /// Implement/Override your custom actionmethods, events etc in this real-time MVC controller
    /// </summary>
    public class Foo : XSocketController
    {
        /// <summary>
        /// This will broadcast any message to all clients
        /// connected to this controller.
        /// To use Pub/Sub replace InvokeToAll with PublishToAll
        /// </summary>
        /// <param name="message"></param>
        public override async Task OnMessage(IMessage message)
        {
            await this.InvokeToAll(message);
        }

        [HttpGet()]
        public string Bar()
        {
            return "Foo.Bar";
        }
    }
}
