using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQConnectionVerify;
using System.Text;

namespace RabbitMQConnectionVerify
{
    public interface IRabitMQProducer
    {
        void Publish<T>(T message);
    }


    public class RabbitMQProducer: IRabitMQProducer
    {
        private readonly DefaultObjectPool<IModel> objectPolicy;

        public RabbitMQProducer(IPooledObjectPolicy<IModel> objectPolicy)
        {
            this.objectPolicy = new 
                           DefaultObjectPool<IModel>(objectPolicy, Environment.ProcessorCount * 2);
        }
        public void Publish<T>(T message)
        {
            #region ConnectionFactory
            //var factory = new ConnectionFactory
            //{ HostName = "localhost" };
            //var connection=factory.CreateConnection();
            //var channel = connection.CreateModel();
            #endregion
            var channel = objectPolicy.Get();
            channel.QueueDeclare("product", exclusive: false);
            //Serialize the message
            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);
            //put the data on to the product queue
            channel.BasicPublish(exchange: "", routingKey: "product", body: body);
        }
    }
}

public static class RabbitServiceCollectionExtensions
{
    public static IServiceCollection AddRabbit(this IServiceCollection services)
    {
       

        services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
        services.AddSingleton<IPooledObjectPolicy<IModel>, RabbitModelPooledObjectPolicy>();

        services.AddSingleton<IRabitMQProducer, RabbitMQProducer>();

        return services;
    }
}

public class RabbitModelPooledObjectPolicy : IPooledObjectPolicy<IModel>
{

    private readonly IConnection _connection;

    public RabbitModelPooledObjectPolicy()
    {
        _connection = GetConnection();
    }

    private IConnection GetConnection()
    {
        var factory = new ConnectionFactory
        { HostName = "localhost" };
        return factory.CreateConnection();
    }

    public IModel Create()
    {
        return _connection.CreateModel();
    }

    public bool Return(IModel obj)
    {
        if (obj.IsOpen)
        {
            return true;
        }
        else
        {
            obj?.Dispose();
            return false;
        }
    }
}
