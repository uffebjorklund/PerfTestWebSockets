using XSockets.Core.Common.Socket.Event.Interface;
using System.Threading.Tasks;
using XSockets.Plugin.Framework;
using XSockets.Core.Common.Utility.Logging;
using XSockets.Core.Common.Protocol;
using XSockets.Plugin.Framework.Attributes;
using XSockets.Protocol.Rfc6455;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using XSockets.Core.Common.Socket.Event.Arguments;
using XSockets.Core.XSocket.Model;
using System;

namespace SelfHosted
{
    [Export(typeof(IXSocketProtocol), Rewritable = Rewritable.No)]
    public class NativeWebSocketsProtocol : Rfc6455Protocol
    {
        /// <summary>
        /// The handshake should contain Sec-WebSocket-Version:\13
        /// The handshake should NOT contain any subprotocol
        /// </summary>
        /// <param name="handshake"></param>
        /// <returns></returns>
        public override async Task<bool> Match(IList<byte> handshake)
        {
            var s = Encoding.UTF8.GetString(handshake.ToArray());
            Composable.GetExport<IXLogger>().Information(s);
            return
                Regex.Match(s, @"(^Sec-WebSocket-Version:\s13)", RegexOptions.Multiline).Success
                &&
                Regex.Match(s, @"(^Sec-WebSocket-Protocol:\sNative)", RegexOptions.Multiline).Success;
        }

        /// <summary>
        /// Convert the incoming data to an IMessage of with correct type (text/binary)
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="messageType"></param>
        /// <returns></returns>
        public override IMessage OnIncomingFrame(IEnumerable<byte> payload, MessageType messageType)
        {
            Composable.GetExport<IXLogger>().Information("Native Websockets {p}", Encoding.UTF8.GetString(payload.ToArray()));
            if (messageType == MessageType.Binary)
            {
                return new Message(payload, "bar", "foo");
            }
            else
            {
                return new Message(Encoding.UTF8.GetString(payload.ToArray()), "bar", "foo");
            }
        }

        /// <summary>
        /// Create a RFC6455 dataframe from the IMessage and only pass out the payload
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public override byte[] OnOutgoingFrame(IMessage message)
        {
            return GetFrame(message).ToBytes();
        }

        /// <summary>
        /// Create text/binary frame
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private Rfc6455DataFrame GetFrame(IMessage message)
        {
            var rnd = new Random().Next(0, 34298);
            return new Rfc6455DataFrame
            {
                FrameType = (message.MessageType == MessageType.Text) ? FrameType.Text : FrameType.Binary,
                IsFinal = true,
                MaskKey = rnd,
                Payload = (message.MessageType == MessageType.Text) ? Encoding.UTF8.GetBytes(message.Data) : message.Blob.ToArray(),
            };
        }

        public override IXSocketProtocol NewInstance()
        {
            return new NativeWebSocketsProtocol();
        }
    }
}
