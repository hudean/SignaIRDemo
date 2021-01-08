using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SignaIRDemo.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignaIRDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
          
            services.AddRazorPages().AddRazorRuntimeCompilation();
            services.AddDistributedMemoryCache();
            services.AddSession();
            services.AddSingleton(typeof(MyServices), typeof(MyServices));
            //����SignalR
            services.AddSignalR();
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            //���ú�ʹ�����WebSocket�м��
            //��������������ÿ��120���pingһ��. ���������ڽ��պͽ���frame�Ļ����С. ��ʵ������ֵ����Ĭ�ϵ�ֵ.
            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize=4*1024
            };
            app.UseWebSockets(webSocketOptions);

            //app.UseSignalR(routes => routes.MapHub<CountHub>("/count"));

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapHub<CountHub>("/count");
                //endpoints.MapHub<ChatHub>("/chathub");
            });
        }
    }
}
