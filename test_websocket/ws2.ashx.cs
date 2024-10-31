using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Net.WebSockets;
using System.Text;
//using Microsoft.Web.WebSockets;
using System.Web.WebSockets;

namespace test_websocket
{
    public class ws2 : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            if (context.IsWebSocketRequest)
            {
                context.AcceptWebSocketRequest(HandleWebSocket);
            }
            else
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write("Chỉ hỗ trợ WebSocket.");
                context.Response.End();
            }
        }

        private async Task HandleWebSocket(AspNetWebSocketContext context)
        {
            var webSocket = context.WebSocket;
            var buffer = new ArraySegment<byte>(new byte[1024]);

            // Gửi thông báo khi kết nối WebSocket mở
            await SendMessage(webSocket, "Server đã mở kết nối");

            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Đóng kết nối", CancellationToken.None);
                    break;
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                    await SendMessage(webSocket, "Server nhận được tin nhắn: " + message);
                }
            }
        }

        private async Task SendMessage(WebSocket webSocket, string message)
        {
            var encodedMessage = Encoding.UTF8.GetBytes(message);
            var buffer = new ArraySegment<byte>(encodedMessage);

            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        public bool IsReusable => false;
    }
}
