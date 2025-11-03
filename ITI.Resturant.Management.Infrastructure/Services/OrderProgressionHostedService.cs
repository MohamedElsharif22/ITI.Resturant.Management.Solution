using ITI.Resturant.Management.Application.Services;
using ITI.Resturant.Management.Domain.Entities.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace ITI.Resturant.Management.Infrastructure.Services
{
    public class OrderProgressionOptions
    {
        public int MaxConcurrentOrders { get; set; } = 50;
        public int RetryDelaySeconds { get; set; } = 30;
        public int MaxRetries { get; set; } = 3;
    }

    public class OrderProgressionHostedService : BackgroundService, IOrderProgressionService
    {
        private readonly ILogger<OrderProgressionHostedService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly Channel<int> _orderQueue;
        private readonly OrderProgressionOptions _options;

        public OrderProgressionHostedService(
            ILogger<OrderProgressionHostedService> logger,
            IServiceScopeFactory scopeFactory,
            IOptions<OrderProgressionOptions> options)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _options = options.Value;
            
            var channelOptions = new BoundedChannelOptions(_options.MaxConcurrentOrders)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            };
            _orderQueue = Channel.CreateBounded<int>(channelOptions);
        }

        public async Task QueueOrderProgressionAsync(int orderId)
        {
            try
            {
                await _orderQueue.Writer.WriteAsync(orderId);
                _logger.LogInformation("Order {OrderId} queued for progression", orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to queue order {OrderId} for progression", orderId);
                throw;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Order progression service starting");

            await foreach (var orderId in _orderQueue.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    await ProcessOrderAsync(orderId, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing order {OrderId}", orderId);
                }
            }
        }

        private async Task ProcessOrderAsync(int orderId, CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

            for (int attempt = 0; attempt <= _options.MaxRetries; attempt++)
            {
                try
                {
                    var order = await orderService.GetByIdAsync(orderId);
                    if (order == null)
                    {
                        _logger.LogWarning("Order {OrderId} not found", orderId);
                        return;
                    }

                    // Skip if already progressed or cancelled
                    if (order.Status >= OrderStatus.Ready || order.Status == OrderStatus.Cancelled)
                    {
                        _logger.LogInformation("Order {OrderId} already in final state {Status}", orderId, order.Status);
                        return;
                    }

                    // Progress order based on current status
                    var nextStatus = order.Status switch
                    {
                        OrderStatus.Pending => OrderStatus.Preparing,
                        OrderStatus.Preparing => OrderStatus.Ready,
                        _ => order.Status
                    };

                    if (nextStatus != order.Status)
                    {
                        // Calculate delay based on status
                        var delayMinutes = nextStatus == OrderStatus.Preparing ? 2 : 
                            await orderService.GetEstimatedPreparationTimeAsync(order);

                        await Task.Delay(TimeSpan.FromMinutes(delayMinutes), stoppingToken);
                        await orderService.UpdateStatusAsync(orderId, nextStatus);
                        
                        _logger.LogInformation("Order {OrderId} progressed to {Status}", orderId, nextStatus);

                        // Queue next progression if needed
                        if (nextStatus == OrderStatus.Preparing)
                        {
                            await QueueOrderProgressionAsync(orderId);
                        }
                    }

                    return;
                }
                catch (Exception ex) when (attempt < _options.MaxRetries)
                {
                    _logger.LogWarning(ex, 
                        "Failed to process order {OrderId} (attempt {Attempt}/{MaxRetries})", 
                        orderId, attempt + 1, _options.MaxRetries);
                    await Task.Delay(TimeSpan.FromSeconds(_options.RetryDelaySeconds), stoppingToken);
                }
            }
        }
    }
}
