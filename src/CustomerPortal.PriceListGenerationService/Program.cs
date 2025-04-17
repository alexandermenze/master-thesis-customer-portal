using CustomerPortal.Extensions;
using CustomerPortal.PriceListGenerationService;
using Microsoft.Extensions.Configuration;
using Minio;
using StackExchange.Redis;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

var redisConfig = config.GetSection("Redis");

await using var redis = await ConnectionMultiplexer.ConnectAsync(
    redisConfig.GetValueOrThrow<string>("ConnectionString")
);

var minioConfig = config.GetSection("MinIO");

var minioBucket = minioConfig.GetValueOrThrow<string>("Bucket");

var minio = new MinioClient()
    .WithEndpoint(minioConfig.GetValueOrThrow<string>("Endpoint"))
    .WithCredentials(
        minioConfig.GetValueOrThrow<string>("AccessKey"),
        minioConfig.GetValueOrThrow<string>("SecretKey")
    )
    .Build();

await new App(
    new StreamDatabase(
        redis.GetDatabase(),
        redisConfig.GetValueOrThrow<string>("TaskStreamName"),
        redisConfig.GetValueOrThrow<string>("ConsumerGroupName"),
        redisConfig.GetValueOrThrow<string>("ResponseStreamName")
    ),
    minio,
    minioBucket
).Run(CancellationToken.None);
