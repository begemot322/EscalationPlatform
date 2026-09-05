using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace EscalationPlatform.Bus.RabbitMq;

/// <summary>
/// Методы внедрения RabbitMQ
/// </summary>
public static class BusServiceExtensions
{
    /// <summary>
    /// Имя строки подключения к RabbitMQ
    /// </summary>
    public const string ConnectionStringName = "RabbitMq";

    /// <summary>
    /// Имя секции конфигурации шины
    /// </summary>
    public const string BusConfigurationSectionName = "Bus";

    /// <summary>
    /// Добавляет MassTransit с RabbitMQ в качестве брокера сообщений
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="connectionString">Строка подключения к RabbitMQ</param>
    /// <param name="configuration">Конфигурация MassTransit</param>
    /// <param name="jsonOptions">Настройки сериализации JSON</param>
    /// <param name="configureConsumers">Делегат для регистрации консьюмеров</param>
    /// <param name="configureRabbit">Делегат для настройки RabbitMQ</param>
    public static IServiceCollection AddRabbitMq(this IServiceCollection services,
        string? connectionString,
        BusConfiguration configuration,
        JsonSerializerOptions? jsonOptions = null,
        Action<IBusRegistrationConfigurator>? configureConsumers = null,
        Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator, BusConfiguration>? configureRabbit = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(connectionString);

        return services
            .AddMassTransit(mt =>
            {
                configureConsumers?.Invoke(mt);

                mt.UsingRabbitMq((context, rabbitMq) =>
                {
                    rabbitMq.Host(new Uri(connectionString));
                    rabbitMq.ConfigureJsonSerializerOptions(options =>
                    {
                        options.PropertyNamingPolicy =
                            jsonOptions?.PropertyNamingPolicy ?? JsonNamingPolicy.SnakeCaseLower;
                        options.DefaultIgnoreCondition =
                            jsonOptions?.DefaultIgnoreCondition ?? JsonIgnoreCondition.WhenWritingNull;
                        options.DictionaryKeyPolicy =
                            jsonOptions?.DictionaryKeyPolicy ?? JsonNamingPolicy.SnakeCaseLower;
                        options.WriteIndented = jsonOptions?.WriteIndented ?? false;
                        options.AllowOutOfOrderMetadataProperties =
                            jsonOptions?.AllowOutOfOrderMetadataProperties ?? true;

                        return options;
                    });

                    rabbitMq.AddRawJsonSerializer(RawSerializerOptions.All);
                    rabbitMq.DefaultContentType = new ContentType(MediaTypeNames.Application.Json);
                    
                    configureRabbit?.Invoke(context, rabbitMq, configuration);
                });
            });
    }

    /// <summary>
    /// Настраивает консьюмера для получения сообщений из RabbitMQ.
    /// </summary>
    /// <param name="cfg">Конфигуратор RabbitMQ</param>
    /// <param name="context">Контекст регистрации шины</param>
    /// <param name="exchangeName">Имя Exchange, к которому привязывается консьюмер</param>
    /// <param name="configuration">Кастомная конфигурация MassTransit</param>
    /// <param name="ignoredExceptions">Типы исключений, которые следует игнорировать при ретрай политики</param>
    /// <typeparam name="T">Тип консьюмера.</typeparam>>
    public static void ConfigureConsumer<T>(this IRabbitMqBusFactoryConfigurator cfg,
        IBusRegistrationContext context,
        string exchangeName,
        BusConfiguration configuration,
        Type[]? ignoredExceptions = null)
        where T : class, IConsumer
    {
        configuration.Topology.TryGetValue(exchangeName, out var busConfiguration);

        cfg.ReceiveEndpoint(busConfiguration?.QueueName ?? exchangeName, x =>
        {
            x.ConfigureConsumeTopology = false;
            x.PrefetchCount = busConfiguration?.PrefetchCount ?? 16;
            
            var targetExchange = busConfiguration?.ExchangeName ?? exchangeName;
            var endpointName = busConfiguration?.QueueName ?? exchangeName;

            // Если ExchangeName отличается от QueueName, то для очереди
            // всё равно будет создан свой exchange, а все привязанные
            // exchange будут присоединены к этому exchange.
            // https://github.com/MassTransit/MassTransit/discussions/2493#discussioncomment-693549
            if (!string.Equals(targetExchange, endpointName, StringComparison.OrdinalIgnoreCase))
            {
                x.Bind(targetExchange, bindConfig =>
                {
                    bindConfig.AutoDelete = false;
                });
            }
            x.ConfigureConsumer<T>(context);

            if (busConfiguration?.RetryPolicy is not { } retryPolicy)
                return;

            x.UseMessageRetry(z =>
            {
                z.Exponential(
                    retryLimit: retryPolicy.Limit,
                    minInterval: TimeSpan.FromSeconds(retryPolicy.MinInterval),
                    maxInterval: TimeSpan.FromSeconds(retryPolicy.MaxInterval),
                    intervalDelta: TimeSpan.FromSeconds(retryPolicy.IntervalDelta)
                );

                z.Ignore<JsonException>();

                if (ignoredExceptions == null) return;
                foreach (var exceptionType in ignoredExceptions)
                {
                    if (typeof(Exception).IsAssignableFrom(exceptionType))
                        z.Ignore(exceptionType);
                }
            });
        });
    }

    /// <summary>
    /// Настраивает публикацию сообщения для RabbitMQ
    /// </summary>
    /// <param name="cfg">Конфигуратор RabbitMQ </param>
    /// <param name="exchangeName">Имя Exchange, используемого для отправки сообщений</param>
    /// <param name="configuration">Кастомная конфигурация MassTransit</param>
    /// <typeparam name="T">Тип сообщения</typeparam>
    public static void ConfigureMessage<T>(this IRabbitMqBusFactoryConfigurator cfg, string exchangeName,
        BusConfiguration configuration)
        where T : class
    {
        configuration.Topology.TryGetValue(exchangeName, out var busConfiguration);

        cfg.Message<T>(m => m.SetEntityName(busConfiguration?.ExchangeName ?? exchangeName));
        cfg.Send<T>(m => m.UseSerializer(new ContentType(MediaTypeNames.Application.Json)));

        if (busConfiguration?.CreateQueue is not true)
            return;

        cfg.ReceiveEndpoint(busConfiguration.ExchangeName ?? exchangeName,
            e => { e.ConfigureConsumeTopology = false; });
    }
}