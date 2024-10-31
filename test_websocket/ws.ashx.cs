using System;
using System.Collections.Generic;
using System.Web;

//nu-get console:
//Install-Package Microsoft.WebSockets
using Microsoft.Web.WebSockets;


namespace test_websocket
{
    /// <summary>
    /// Summary description for ws
    /// </summary>
    public class ws : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            if (context.IsWebSocketRequest)
            {
                context.AcceptWebSocketRequest(new MyWebSocket());
            }
            else
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write("Chỉ hỗ trợ WebSocket.");
                context.Response.End();
            }
        }

        public bool IsReusable => false;
    }

    public class MyWebSocket : WebSocketHandler
    {
        public override void OnOpen()
        {
            this.Send("Server đã mở kết nối");
        }

        public override void OnMessage(string message)
        {
            this.Send("Server nhận được tin nhắn: " + message);
        }

        public override void OnClose()
        {
            this.Send("Kết nối đã đóng.");
        }
    }
}