using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SignaIRDemo
{
    /// <summary>
    /// 
    /// </summary>
    //[Authorize]  //授权和验证
    public class CountHub : Hub
    {
        private readonly MyServices _myServices;
        public CountHub(MyServices myServices)
        {
            _myServices = myServices;
        }

        public async Task GetLastCountTest(string random)
        {
            //string userName = Context.User.Identity.Name;
            int count;
            do {
                count = _myServices.GetLastedCount();
                Thread.Sleep(1000);
                await Clients.All.SendAsync("ReceiveUpdate", count);


            } while (count<10);
            await Clients.All.SendAsync("Finished");
                 
        }

        /// <summary>
        /// 有新的连接建立了, 这个方法就会被执行.
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            //return base.OnConnectedAsync();
            //ConnectionId就是连接到Hub的这个客户端的唯一标识.
            //使用ConnectionId, 我们就可以取得这个客户端, 并调用其方法, 如Clients.Client(connectionId).xxx.
            //Hub的Clients属性表示客户端, 它有若干个方法可以选择客户端, 
            //刚才的Client(connectionId)就是使用connectionId找到这一个客户端. 
            //而AllExcept(connectionId)就是除了这个connectionId的客户端之外的所有客户端.
            //SignalR还有Group分组的概念, 而且操作简单, 这里用到的是Hub的Groups属性. 
            //向一个Group名添加第一个connectionId的时候, 分组就被建立. 移除分组内最后一个客户端的时候, 
            //分组就被删除了. 使用Clients.Group("组名")可以调用组内客户端的方法.
            var connectionId = Context.ConnectionId;
            await Clients.Client(connectionId).SendAsync("someFunc", new { });
            //await Clients.AllExcept(connectionId).SendAsync("someFunc");
            //await Groups.AddToGroupAsync(connectionId, "MyGroup");
            //await Groups.RemoveFromGroupAsync(connectionId, "MyGroup");
            //await Clients.Groups("MyGroup").SendAsync("someFunc");
        }

    }

    public class MyServices
    {
        private int _count = 0;
        public int GetLastedCount()
        {
            _count++;
            return _count;
        }
    }
}
