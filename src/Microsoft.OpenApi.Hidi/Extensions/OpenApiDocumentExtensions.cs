// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.OpenApi.ApiManifest;
using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.Hidi.Extensions
{
    internal static class OpenApiDocumentExtensions
    {
        internal static ApiManifestDocument ToApiManifest(this OpenApiDocument document, string? apiDescriptionUrl)
        {
            var apiName = document.Info?.Title ?? "api-name";
            var publisherName = document.Info?.Contact?.Name ?? "publisher-name";
            var publisherEmail = document.Info?.Contact?.Email ?? "publisher-email@example.com";

            var apiManifest = new ApiManifestDocument(apiName)
            {
                Publisher = new(publisherName, publisherEmail),
                ApiDependencies = new() {
                    {
                        apiName, new() {
                            ApiDescriptionUrl = apiDescriptionUrl
                        }
                    }
                }
            };

            foreach (var path in document.Paths)
            {
                foreach (var operation in path.Value.Operations)
                {
                    var requestInfo = new RequestInfo
                    {
                        Method = operation.Key.ToString(),
                        UriTemplate = path.Key
                    };
                    apiManifest.ApiDependencies[apiName].Requests.Add(requestInfo);
                }
            }
            return apiManifest;
        }
    }
}
