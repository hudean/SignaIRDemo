using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SignaIRDemoTwo
{
    //[Authorize]
    public class TestHub: Hub
    {
        private static int count;

        private int GetLastCount()
        {
           return count++;
        }

        public async Task GetLastedCount(string random)
        {
            int count;
            do
            {
                count = GetLastCount();
                Thread.Sleep(1000);
                await Clients.All.SendAsync("ReceiveMessage", count);
            } while (count < 10);
            Thread.Sleep(2000);
            await Clients.All.SendAsync("Finished");
        }

        //从Hub的Context属性, 可以获得用户的信息.
        public override async Task OnConnectedAsync()
        {
            //var userName = Context.User.Identity.Name;
            //return base.OnConnectedAsync();
            //ConnectionId就是连接到Hub的这个客户端的唯一标识.
            var connectionId = Context.ConnectionId;
            await Clients.Clients(connectionId).SendAsync("someFunc", new { });
            //await Clients.AllExcept(connectionId).SendAsync("someFunc");
            //await Groups.AddToGroupAsync(connectionId, "MyGroup");
            //await Groups.RemoveFromGroupAsync(connectionId, "MyGroup");
            //await Clients.Group("MyGroup").SendAsync("someFunc");
        }
    }
}
