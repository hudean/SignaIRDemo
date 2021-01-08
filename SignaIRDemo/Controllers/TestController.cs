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

namespace SignaIRDemo.Controllers
{
    public class TestController : Controller
    {
        /***
        * 参考文章  SignaIR
        * https://www.cnblogs.com/cgzl/p/9509207.html
        * https://docs.microsoft.com/zh-cn/aspnet/core/signalr/introduction?view=aspnetcore-3.1
        * https://www.cnblogs.com/cgzl/p/9515516.html
        *
        *
        ***/
        private readonly MyServices services;
        private readonly HttpContextAccessor _httpContextAccessor;
       

        // HttpContextAccessor httpContextAccessor,
        public TestController(MyServices _services)
        {
            services = _services;
            //_httpContextAccessor = httpContextAccessor;
            
        }
        /// <summary>
        /// Polling
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }

        //public IActionResult Get(int id)
        //{
        //    #region Polling

        //    //int count = services.GetLastedCount();
        //    //if (count > 10)
        //    //{
        //    //    return Ok(new { id, count, finished = true });
        //    //}

        //    //if (count > 6)
        //    //{
        //    //    return Ok(new { id, count });
        //    //}
        //    //return NoContent();

        //    #endregion


        //    #region Long Polling

        //    int count;
        //    do
        //    {
        //        count = services.GetLastedCount();
        //        Thread.Sleep(1000);
        //    } while (count < 5);
        //    return Ok(new { id, count, finished = true });

        //    #endregion

        //}

        public async void Get(int id)
        {
            #region SSE(Server Sent Events)

            //注意SSE返回数据的只能是字符串, 而且以data:开头, 后边要跟着换行符号, 否则EventSource会失败.
            //Response.ContentType = "text/event-stream";
            //int count = 0;
            //do
            //{
            //    count = services.GetLastedCount();
            //    Thread.Sleep(1000);
            //    if (count % 3 == 0)
            //    {
            //        await HttpContext.Response.WriteAsync($"data: {count}\n\n");
            //        await HttpContext.Response.Body.FlushAsync();
            //    }

            //} while (count < 10);
            //Response.Body.Close();

            #endregion

            #region webSocket
            //这里需要注入HttpContextAccessor. 然后判断请求是否是WebSocket请求, 如果是的话, 客户端会收到回复, 这时Socket就升级完成了. 升级完返回一个webSocket对象, 然后我把events通过它发送出去. 随后我关闭了webSocket, 并指明了原因NormalClosure.
            //var context = _httpContextAccessor.HttpContext;
            var context = HttpContext;
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                await SendEvents(webSocket, id);
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", CancellationToken.None);
            }
            else
            {
                context.Response.StatusCode = 400;
            }

            #endregion

        }


        private async Task SendEvents(WebSocket webSocket, int id)
        {
            int count;
            //这里的重点就是webSocket对象的SendAsync方法. 我需要把数据转化成buffer进行传送. 数据类型是Text. 具体参数请查看文档.
            do
            {
                count = services.GetLastedCount();
                Thread.Sleep(1000);
                if (count % 3 != 0)
                {
                    continue;
                }
                var obj = new { id, count };
                var jsonStr = JsonConvert.SerializeObject(obj);
                await webSocket.SendAsync(buffer: new ArraySegment<byte>(
                    array: Encoding.ASCII.GetBytes(jsonStr),
                    offset: 0,
                    count: jsonStr.Length),
                    messageType: WebSocketMessageType.Text,
                    endOfMessage: true,
                    cancellationToken: CancellationToken.None);

            } while (count < 10);
        }
    }

   
}
