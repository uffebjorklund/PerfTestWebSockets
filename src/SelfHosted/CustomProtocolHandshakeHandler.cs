

namespace SelfHosted
{
    using System.Text;
    using System.Threading.Tasks;
    using XSockets.Core.Common.Socket;
    using XSockets.Core.Common.Utility.Logging;
    using XSockets.Core.XSocket;
    using XSockets.Plugin.Framework;
    using XSockets.Plugin.Framework.Attributes;
    using XSockets.Protocol.Http;

    [Export(typeof(IProtocolHandshakeHandler), null, Rewritable.No)]
    public class CustomProtocolHandshakeHandler : ProtocolHandshakeHandler, IProtocolHandshakeHandler
    {
        private static readonly byte[] NOCONTENT = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n" +
            "Connection: close\r\n" +
            "Content-Length: 0\r\n\r\n");

        public override async Task HandShake(byte[] ar)
        {
            var rawHandshake = Encoding.UTF8.GetString(ar, 0, ar.Length);

            var identifiedProtocol = GetProtocolFromHandshake(ar);
            if (identifiedProtocol != null && identifiedProtocol is HyperTextTransferProtocol)
            {
                //Get the instance of the identified protocol
                var http = (HyperTextTransferProtocol)await identifiedProtocol.Resolve(ar, Transport, Configuration);

                var path = http.Handshake.Split(' ');
          
                if (path[0] == "GET" && path[1] == "/")
                {
                    Composable.GetExport<IXLogger>().Information("ProtocolHandshakeHandler - Handshake OK: {h}", rawHandshake);
                    var success = await Transport.Send(NOCONTENT);
                    await http.Disconnect(false);
                }
                else
                {
                    await base.HandShake(ar);
                }
                return;
            }
            else
            {
                await base.HandShake(ar);
            }
            
        }
    }
}
