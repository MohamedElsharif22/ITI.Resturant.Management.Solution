using ITI.Resturant.Management.Application.Services;
using ITI.Resturant.Management.Domain.Entities.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.Infrastructure.Services
{
    // Background hosted service that periodically advances orders from Pending -> Preparing -> Ready
    public class OrderProgressionHostedService : BackgroundService
    {
        private readonly ILogger<OrderProgressionHostedService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public OrderProgressionHostedService(
            ILogger<OrderProgressionHostedService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Order progression hosted service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var scope = _scopeFactory.CreateScope();
                    try
                    {
                        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

                        // Process pending orders
                        var pending = (await orderService.GetByStatusAsync(OrderStatus.Pending)).ToList();
                        foreach (var order in pending)
                        {
                            if (order.OrderDate.AddMinutes(2) <= DateTime.Now)
                            {
                                await orderService.UpdateStatusAsync(order.Id, OrderStatus.Preparing);
                                _logger.LogInformation("Order {OrderId} moved to Preparing", order.Id);
                            }
                        }

                        // Process preparing orders
                        var preparing = (await orderService.GetByStatusAsync(OrderStatus.Preparing)).ToList();
                        foreach (var order in preparing)
                        {
                            // If preparing for more than estimated prep time + buffer, mark as Ready
                            var estimatedDelivery = await orderService.EstimateDeliveryTimeAsync(order);
                            if (DateTime.Now >= estimatedDelivery.AddMinutes(-30)) // 30 min buffer for delivery time
                            {
                                await orderService.UpdateStatusAsync(order.Id, OrderStatus.Ready);
                                _logger.LogInformation("Order {OrderId} moved to Ready", order.Id);
                            }
                        }
                    }
                    finally
                    {
                        // Dispose scope asynchronously if possible, otherwise dispose synchronously
                        if (scope is IAsyncDisposable asyncDisposable)
                        {
                            await asyncDisposable.DisposeAsync();
                        }
                        else
                        {
                            scope.Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error progressing orders");
                }

                // Wait for 30 seconds before next check
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }

            _logger.LogInformation("Order progression hosted service stopped");
        }
    }
}
