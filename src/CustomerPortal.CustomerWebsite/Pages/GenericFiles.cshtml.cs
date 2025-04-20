using System.Collections.Immutable;
using CustomerPortal.CustomerWebsite.Configurations;
using CustomerPortal.CustomerWebsite.Models;
using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel.Args;

namespace CustomerPortal.CustomerWebsite.Pages;

public class GenericFiles(
    ILogger<GenericFiles> logger,
    IHttpClientFactory httpClientFactory,
    IMinioClient minio,
    MinioAppConfig minioConfig
) : UserPageModel(logger, httpClientFactory)
{
    public ImmutableArray<string> Files = [];

    public async Task<IActionResult> OnGet()
    {
        var currentUser = await GetCurrentUser();

        if (currentUser?.CustomerNo is null)
            return RedirectToPage("/Login");

        var filePrefix = $"{currentUser.CustomerNo}/{minioConfig.GenericFilesPath}/";

        var args = new ListObjectsArgs()
            .WithBucket(minioConfig.BucketName)
            .WithPrefix(filePrefix)
            .WithRecursive(false);

        var files = await minio
            .ListObjectsEnumAsync(args)
            .Where(o => o.IsDir is false)
            .Select(obj => obj.Key)
            .ToListAsync();

        var fileNames = files.Select(Path.GetFileName).Where(f => f is not null).Select(f => f!);

        Files = [.. fileNames];

        return Page();
    }
}
