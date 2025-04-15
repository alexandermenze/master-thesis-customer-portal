using System.Text;
using System.Text.Json;
using CustomerPortal.Extensions;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
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
var minioSubPath = minioConfig.GetValueOrThrow<string>("SubPath");

var minio = new MinioClient()
    .WithEndpoint(minioConfig.GetValueOrThrow<string>("Endpoint"))
    .WithCredentials(
        minioConfig.GetValueOrThrow<string>("AccessKey"),
        minioConfig.GetValueOrThrow<string>("SecretKey")
    )
    .Build();

var bytes = new byte[1024];
Random.Shared.NextBytes(bytes);
var text = Convert.ToBase64String(bytes);
var data = new MemoryStream(Encoding.UTF8.GetBytes(text));

var result = await minio.PutObjectAsync(
    new PutObjectArgs()
        .WithStreamData(data)
        .WithObject($"{minioSubPath}/some-file.txt")
        .WithObjectSize(data.Length)
        .WithBucket(minioBucket)
);

Console.WriteLine(JsonSerializer.Serialize(result));
