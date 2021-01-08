using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SignaIRDemoTwo.Controllers
{
    public class DemoTestController : Controller
    {
        private readonly IHubContext<TestHub> _testHub;
        public DemoTestController(IHubContext<TestHub> testHub) 
        {
            _testHub = testHub;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region Polling

        /// <summary>
        ///  Polling
        /// </summary>
        /// <returns></returns>
        public IActionResult PollingTest()
        {
            return View();
        }


        /// <summary>
        ///  Polling
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public IActionResult PollingTest_GetCount(int id)
        {
            var count = GetLastedCount();
            if (id > 10)
            {
                return Ok(new { id, count, finished = true });

            }
            if (id > 6)
            {
                return Ok(new { id, count });
            }
            return NotFound();
        }

        #endregion

        #region Long Polling 

        public IActionResult LongPollingTest()
        {
            return View();
        }

        public IActionResult LongPollingTest_GetCount(int id)
        {
            //int count =0;
            //do
            //{
            //    count = GetLastedCount();
            //    Thread.Sleep(2000);

            //} while (count < 5);
            int count = GetLastedCount();
            Thread.Sleep(1000);
            if (count > 5)
            {
                return Ok(new { id, count, finished = true });
            }
            else
            {
                return Ok(new { id, count, finished = false });
            }
        }

        #endregion

        #region Server Sent Events (SSE)

        public IActionResult SSETest()
        {
            return View();
        }


        public async void SSEGet(int id)
        {
            Response.ContentType = "text/event-stream";
            int count;
            do 
            {
                count = GetLastedCount();
                Thread.Sleep(1000);
                if (count % 3 == 0)
                {
                    //注意SSE返回数据的只能是字符串, 而且以data:开头, 后边要跟着换行符号, 否则EventSource会失败.
                    await HttpContext.Response.WriteAsync($"data:{count}\n\n");
                    await HttpContext.Response.Body.FlushAsync();
                }

            } while(count<10);

            Response.Body.Close();
        }

        #endregion

        #region WebSocket

        public IActionResult WebSocketTest()
        {
            return View();
        }


        public async void WebSocketGet(int id)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await SendEvents(webSocket, id);
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", CancellationToken.None);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }


        private async Task SendEvents(WebSocket webSocket, int id)
        {
            int count;
            do
            {
                count = GetLastedCount();
                Thread.Sleep(1000);
                if (count % 3 != 0) continue;
                var obj = new { id, count };
                var jsonStr = JsonConvert.SerializeObject(obj);
                await webSocket.SendAsync(buffer: new ArraySegment<byte>(array:Encoding.UTF8.GetBytes(jsonStr),offset:0,count: jsonStr.Length),messageType:WebSocketMessageType.Text,endOfMessage:true,cancellationToken: CancellationToken.None);
            } while (count < 10);
        }


        #endregion

        #region SignalR

        public IActionResult SignalRTest()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignalRPost()
        {
            await _testHub.Clients.All.SendAsync("someFunc", new { random = "abcd" });

            Thread.Sleep(2000);
            return Accepted(1); //202: 请求已被接受并处理，但还没有处理完成
        }

        #endregion


        private static int _count;
        public int GetLastedCount()
        {
            _count++;
            return _count;
        }

    }
}
