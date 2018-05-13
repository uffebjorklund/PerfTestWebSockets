using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Owin;

namespace OwinHosted
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;

    public class CustomWebSocket
    {

        private readonly Func<IDictionary<string, object>, Task> _next;
        private IDictionary<string, object> _env;

        public CustomWebSocket()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        protected Task Next(IDictionary<string, object> env)
        {
            if (env == null) throw new ArgumentNullException("environment");
            return _next(env);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="withInterceptors"></param>
        public CustomWebSocket(Func<IDictionary<string, object>, Task> next)
        {
            _next = next;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        public async Task Invoke(IDictionary<string, object> env)
        {
            _env = new Dictionary<string, object>();
            foreach (var k in env.Keys)
            {
                try
                {
                    _env.Add(k, env[k]);
                }
                catch
                {

                }
            }

            object supportsWebSocket;
            if (env.TryGetValue("websocket.Accept", out supportsWebSocket) && supportsWebSocket != null)
            {
                var requestHeaders = GetValue<IDictionary<string, string[]>>(env, "owin.RequestHeaders");
                Dictionary<string, object> acceptOptions = null;
                string[] subProtocols;
                if (requestHeaders.TryGetValue("Sec-WebSocket-Protocol", out subProtocols) && subProtocols.Length > 0)
                {
                    acceptOptions = new Dictionary<string, object>
                    {
                        {"websocket.SubProtocol", subProtocols[0].Split(',').First().Trim()}
                    };
                }
                var wsAccept =
                     (Action<IDictionary<string, object>, Func<IDictionary<string, object>, Task>>)supportsWebSocket;
                wsAccept(acceptOptions, Receive);
            }
            else
            {
                await _next.Invoke(env);
            }
        }

        private async Task Receive(IDictionary<string, object> wsEnv)
        {
            CancellationToken wsCallCancelled;
            WebSocketContext context;

            context = (WebSocketContext)wsEnv["System.Net.WebSockets.WebSocketContext"];

            wsCallCancelled = (CancellationToken)wsEnv["websocket.CallCancelled"];
            try
            {
                var b = new byte[1024];
                while (!context.WebSocket.CloseStatus.HasValue)
                {
                    var buff = new ArraySegment<byte>(b);
                    var receiveResult = await context.WebSocket.ReceiveAsync(buff,wsCallCancelled);

                    switch (receiveResult.MessageType)
                    {
                        case WebSocketMessageType.Binary:
                            break;
                        case WebSocketMessageType.Text:           
                            break;
                        case WebSocketMessageType.Close:
                            if (!context.WebSocket.CloseStatus.HasValue)
                                await context.WebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, wsCallCancelled);
                            context.WebSocket.Abort();

                            break;
                        default:
                            throw new WebSocketException("Unknown message type");
                    }
                }
            }
            catch
            {
                if (!context.WebSocket.CloseStatus.HasValue)
                {
                    context.WebSocket.Abort();
                }
            }
        }

        private static T GetValue<T>(IDictionary<string, object> env, string key)
        {
            object value;
            return env.TryGetValue(key, out value) && value is T ? (T)value : default(T);
        }
    }
}