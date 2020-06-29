using System;

using MassTransit;
using MassTransit.ActiveMqTransport;

using MessageContracts;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace WebApplication1
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var bus = Bus.Factory.CreateUsingActiveMq(cfg =>
            {
                var host = cfg.Host("broker-amq-amqp-dnmitev.apps.appcanvas.net",
                    h =>
                    {
                        h.Username("admin");
                        h.Password("admin");
                    });
            });

            services.AddSingleton<IPublishEndpoint>(bus);
            services.AddSingleton<ISendEndpointProvider>(bus);
            services.AddSingleton<IBus>(bus);

            var timeout = TimeSpan.FromSeconds(60);
            var serviceAddress = new Uri("activemq://broker-amq-amqp-dnmitev.apps.appcanvas.net/order-service");

            services.AddScoped<IRequestClient<SubmitOrder, OrderAccepted>>(x =>
                new MessageRequestClient<SubmitOrder, OrderAccepted>(x.GetRequiredService<IBus>(), serviceAddress, timeout, timeout));

            bus.Start();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseMvc();
            //            app.Run();
        }
    }
}