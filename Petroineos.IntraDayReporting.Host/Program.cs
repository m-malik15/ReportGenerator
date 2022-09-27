using Petroineos.IntraDayReporting.Domain.Infrastructure;
using Petroineos.IntraDayReporting.Domain.Interfaces;
using Petroineos.IntraDayReporting.Host;
using Petroineos.IntraDayReporting.Repository;
using Petroineos.IntraDayReporting.Service;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddLogging(builder => builder.AddLog4Net());
        services.AddHostedService<RecurringAggregatorJob>();
        services.AddTransient<ICsvGeneratorService, CsvGeneratorService>();

        services.AddSingleton<JobConfig>(jc =>
        {
            var configuration = jc.GetService<IConfiguration>();
            var jobConfig = new JobConfig();
            hostContext.Configuration.Bind(jobConfig);
            return jobConfig;
        });

        services.AddSingleton<IReportsRepo>(sp =>
        {
            return new NetworkFileShareReportsRepository(
                sp.GetService<JobConfig>().ReportsFolderPath,
                sp.GetService<ILogger<NetworkFileShareReportsRepository>>()
            );
        });

        services.AddTransient<ITradingService, TradingService>();
        services.AddTransient<IClockService, ClockService>();

    })
    .Build();

await host.RunAsync();

