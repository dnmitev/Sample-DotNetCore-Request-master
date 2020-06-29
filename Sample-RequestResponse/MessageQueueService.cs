using System;
using System.Threading;
using System.Threading.Tasks;

using GreenPipes;

using MassTransit;
using MassTransit.ActiveMqTransport;

using MessageContracts;

using Microsoft.Extensions.Hosting;

namespace Sample_RequestResponse
{
    public class MessageQueueService : BackgroundService
    {
        private readonly IBusControl _bus;

        public MessageQueueService()
        {
            _bus = Bus.Factory.CreateUsingActiveMq(cfg =>
                          {
                              cfg.Host("broker-amq-amqp.dnmitev.svc.cluster.local", h =>
                              {
                                  h.Username("admin");
                                  h.Password("admin");
                              });

                              cfg.ReceiveEndpoint("order-service", ep =>
                              {
                                  ep.Handler<SubmitOrder>(context => context.RespondAsync<OrderAccepted>(new
                                  {
                                      context.Message.OrderId
                                  }));
                                  //ec.Consumer<CreateOBOLInstanceCommandHandler>(provider);
                              });
                          });
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return _bus.StartAsync();
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.WhenAll(base.StopAsync(cancellationToken), _bus.StopAsync());
        }
    }
}
