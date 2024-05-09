﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. 

using System.Text.Json;
using Json.Schema;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Reader.ParseNodes;
using JsonSchema = Json.Schema.JsonSchema;

namespace Microsoft.OpenApi.Reader.V31
{
    /// <summary>
    /// Class containing logic to deserialize Open API V31 document into
    /// runtime Open API object model.
    /// </summary>
    internal static partial class OpenApiV31Deserializer
    {
        public static JsonSchema LoadSchema(ParseNode node, OpenApiDocument hostDocument = null)
        {
            Json.Schema.OpenApi.Vocabularies.Register();
            SchemaKeywordRegistry.Register<ExtensionsKeyword>();
            return JsonSerializer.Deserialize<JsonSchema>(node.JsonNode);
        }
    }

}
