using Microsoft.Web.WebSockets;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Web;
using System.Web.DynamicData;

namespace demo_ws
{
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
        private static int dem_ketnoi = 0;
        private static int dem_timer = 0;
        private static Timer _timer;
        private static Dictionary<int, float> _sensorData = new Dictionary<int, float>(); // Lưu trữ dữ liệu sensor
        private static readonly WebSocketCollection _clients = new WebSocketCollection(); // Dùng WebSocketCollection để quản lý kết nối

        
        static MyWebSocket()
        {
            dem_timer++;
            // Khởi tạo và bắt đầu Timer một lần
            _timer = new Timer(CheckDatabaseForChanges, null, 0, 1000); // Check mỗi 1 giây
        }

        public override void OnOpen()
        {
            dem_ketnoi++;
            _clients.Add(this); // Thêm kết nối mới vào WebSocketCollection
            this.Send("Server đã mở kết nối");
        }

        public override void OnMessage(string message)
        {
            this.Send("Server nhận được tin nhắn: " + message);
        }

        public override void OnClose()
        {
            dem_ketnoi--;
            _clients.Remove(this); // Loại bỏ kết nối khỏi WebSocketCollection
            this.Send("Kết nối đã đóng.");

            // Dừng Timer nếu không còn client nào kết nối
            if (_clients.Count == 0 && _timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        private static void CheckDatabaseForChanges(object state)
        {
            if (_clients.Count == 0)
            {
                //quit luôn, check làm gì, có ai mà gửi
                return;
            } 

            using (SqlConnection connection = new SqlConnection("Server=localhost,49259;Database=QLSV;User Id=sa;Password=123;"))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT sid, value FROM Sensor", connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int sensorId = reader.GetInt32(0);
                    float sensorValue = Convert.ToSingle(reader.GetValue(1));

                    if (_sensorData.ContainsKey(sensorId))
                    {
                        // Nếu giá trị sensor thay đổi, gửi cập nhật cho tất cả client
                        if (_sensorData[sensorId] != sensorValue)
                        {
                            _sensorData[sensorId] = sensorValue;
                            BroadcastSensorUpdate(sensorId, sensorValue);
                        }
                    }
                    else
                    {
                        // Thêm sensor mới vào dictionary và gửi giá trị cho tất cả client
                        _sensorData[sensorId] = sensorValue;
                        BroadcastSensorUpdate(sensorId, sensorValue);
                    }
                }
                reader.Close();
            }
        }

        private static void BroadcastSensorUpdate(int sensorId, float sensorValue)
        {
            // Gửi thông tin sensor tới tất cả client dưới dạng JSON
            string message = $"{{ \"sensorId\": {sensorId}, \"sensorValue\": {sensorValue}, \"dem\":{dem_timer},\"sl\":{dem_ketnoi} }}";
            _clients.Broadcast(message);
        }
    }
}
