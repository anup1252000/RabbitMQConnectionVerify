using Microsoft.AspNetCore.Mvc;

namespace RabbitMQConnectionVerify.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IRabitMQProducer rabitMQProducer;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, 
            IRabitMQProducer rabitMQProducer)
        {
            _logger = logger;
            this.rabitMQProducer = rabitMQProducer;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost]
        public void AddProduct(Product product)
        {
            for (int i = 0; i < 100; i++)
            {
                product.ProductId = i;
                rabitMQProducer.Publish(product);
            }
           
        }
    }
}