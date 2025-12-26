using MessageBroker.RabbitMQ.Abstractions;
using MessageBroker.RabbitMQ.Consumers;
using MessageBroker.RabbitMQ.Options;
using MessageBroker.RabbitMQ.Publishers;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Registry;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace MessageBroker.RabbitMQ.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection
        AddRabbitMqPublisher(this IServiceCollection services,
                             string publisherKey,
                             Action<IServiceProvider, ConnectionOptions> connectionOptionsAction,
                             Action<IServiceProvider, ChannelOptions> channelOptionsAction,
                             Action<IServiceProvider, ExchangeOptions> exchangeOptionsAction,
                             Action<IServiceProvider, PublishOptions> publishOptionsAction)
    {

        var optionsKey = $"{publisherKey}-RabbitMQ-Options";

        services.AddKeyedSingleton<RabbitMqOptions>(optionsKey, (sp, key) =>
        {
            var options = new RabbitMqOptions();

            connectionOptionsAction(sp, options.Connection);
            channelOptionsAction(sp, options.Channel);
            exchangeOptionsAction(sp, options.Exchange);
            publishOptionsAction(sp, options.Publish);

            return options;
        });

        var pipelineKey = $"{publisherKey}-RabbitMQ-Pipeline";

        services.AddResiliencePipeline(pipelineKey, pipelineBuilder =>
        {
            pipelineBuilder
                .AddRetry(new RetryStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                    MaxRetryAttempts = 3,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true
                })
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    MinimumThroughput = 10,
                    BreakDuration = TimeSpan.FromSeconds(15),
                    ShouldHandle = new PredicateBuilder().Handle<BrokerUnreachableException>()
                })
                .AddTimeout(TimeSpan.FromSeconds(5));
        });

        services.AddSingleton<ISerializer, JsonMessageSerializer>();
        services.AddSingleton<IDeserialize, JsonMessageDeserialize>();

        var connectionFactoryKey = $"{publisherKey}-RabbitMQ-ConnectionFactory";

        services.AddKeyedSingleton<IConnectionFactory>(connectionFactoryKey, (sp, key) =>
        {
            var options = sp.GetRequiredKeyedService<RabbitMqOptions>(optionsKey);

            return new ConnectionFactory()
            {
                HostName = options.Connection.HostName,
                UserName = options.Connection.UserName,
                Password = options.Connection.Password,
                Uri = new Uri(options.Connection.ConnectionString)
            };
        });

        services.AddKeyedSingleton<IMessagePublisher>(publisherKey, (sp, key) =>
        {
            var serializer = sp.GetRequiredService<ISerializer>();
            var factory = sp.GetRequiredKeyedService<IConnectionFactory>(connectionFactoryKey);

            var pipelineProvider = sp.GetRequiredService<ResiliencePipelineProvider<string>>();

            var pipeline = pipelineProvider.GetPipeline(pipelineKey);

            var options = sp.GetRequiredKeyedService<RabbitMqOptions>(optionsKey);

            return new RabbitMqPublisher(factory, serializer, pipeline, options, publisherKey);
        });

        return services;
    }

    public static IServiceCollection
       AddRabbitMqConsumer<TMessage, TConsumer>(this IServiceCollection services,
                             string consumerKey,
                             Action<IServiceProvider, ConnectionOptions> connectionOptionsAction,
                             Action<IServiceProvider, ChannelOptions> channelOptionsAction,
                             Action<IServiceProvider, ExchangeOptions> exchangeOptionsAction,
                             Action<IServiceProvider, QueueOptions> queueOptionsAction,
                             Action<IServiceProvider, ConsumeOptions> consumeOptionsAction)
        where TMessage : class
        where TConsumer : class, IMessageHandler<TMessage>
    {
        var optionsKey = $"{consumerKey}-RabbitMQ-Options";

        services.AddKeyedSingleton<RabbitMqOptions>(optionsKey, (sp, key) =>
        {
            var options = new RabbitMqOptions();

            connectionOptionsAction(sp, options.Connection);
            channelOptionsAction(sp, options.Channel);
            exchangeOptionsAction(sp, options.Exchange);
            queueOptionsAction(sp, options.Queue);
            consumeOptionsAction(sp, options.Consume);

            return options;
        });

        var pipelineKey = $"{consumerKey}-RabbitMQ-Pipeline";

        services.AddResiliencePipeline(pipelineKey, pipelineBuilder =>
        {
            pipelineBuilder.AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                MaxRetryAttempts = 5,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = TimeSpan.FromSeconds(2)
            });
        });

        services.AddSingleton<ISerializer, JsonMessageSerializer>();
        services.AddSingleton<IDeserialize, JsonMessageDeserialize>();

        var connectionFactoryKey = $"{consumerKey}-RabbitMQ-ConnectionFactory";

        services.AddKeyedSingleton<IConnectionFactory>(connectionFactoryKey, (sp, key) =>
        {
            var options = sp.GetRequiredKeyedService<RabbitMqOptions>(optionsKey);

            return new ConnectionFactory()
            {
                HostName = options.Connection.HostName,
                UserName = options.Connection.UserName,
                Password = options.Connection.Password,
                Uri = new Uri(options.Connection.ConnectionString)
            };
        });

        var handlerKey = $"{consumerKey}-Handler";

        services.AddKeyedScoped<TConsumer>(handlerKey);

        services.AddKeyedSingleton<IMessageConsumer>(consumerKey, (sp, key) =>
        {
            var deserialize = sp.GetRequiredService<IDeserialize>();
            var factory = sp.GetRequiredKeyedService<IConnectionFactory>(connectionFactoryKey);

            var pipelineProvider = sp.GetRequiredService<ResiliencePipelineProvider<string>>();

            var pipeline = pipelineProvider.GetPipeline(pipelineKey);

            var options = sp.GetRequiredKeyedService<RabbitMqOptions>(optionsKey);

            return new RabbitMqConsumer<TMessage, TConsumer>(factory, deserialize, sp, pipeline, options, consumerKey);
        });

        services.AddHostedService(sp =>
        {
            var consumer = sp.GetRequiredKeyedService<IMessageConsumer>(consumerKey);

            return new ConsumerHostedService(consumer);
        });

        return services;
    }
}
