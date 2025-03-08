namespace Magic8
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private Application bot;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            bot = new();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
