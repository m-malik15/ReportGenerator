using Petroineos.IntraDayReporting.Host;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<RecurringAggregatorJob>();
    })
    .Build();

await host.RunAsync();
