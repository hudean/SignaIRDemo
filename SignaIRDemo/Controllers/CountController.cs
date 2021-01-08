using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SignaIRDemo.Controllers
{
    [Route("/Count")]
    public class CountController : Controller
    {
        private readonly IHubContext<CountHub> _hubContext;
        public CountController(IHubContext<CountHub> hubContext)
        {
            _hubContext = hubContext;
        }
        [Route("/Count/Index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            await _hubContext.Clients.All.SendAsync("someFunc", new { random = "abcd" });

            Thread.Sleep(2000);
            return Accepted(1); //202: 请求已被接受并处理，但还没有处理完成
        }

    }
}
