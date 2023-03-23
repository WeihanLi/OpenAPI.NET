﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers.ParseNodes;

namespace Microsoft.OpenApi.Readers.V31
{
    /// <summary>
    /// Class containing logic to deserialize Open API V31 document into
    /// runtime Open API object model.
    /// </summary>
    internal static partial class OpenApiV31Deserializer
    {
        private static readonly FixedFieldMap<OpenApiPathItem> _pathItemFixedFields = new FixedFieldMap<OpenApiPathItem>
        {

            {
                "$ref", (o,n) => {
                    o.Reference = new OpenApiReference() { ExternalResource = n.GetScalarValue() };
                    o.UnresolvedReference =true;
                }
            },
            {
                "summary", (o, n) =>
                {
                    o.Summary = n.GetScalarValue();
                }
            },
            {
                "description", (o, n) =>
                {
                    o.Description = n.GetScalarValue();
                }
            },
            {"get", (o, n) => o.AddOperation(OperationType.Get, LoadOperation(n))},
            {"put", (o, n) => o.AddOperation(OperationType.Put, LoadOperation(n))},
            {"post", (o, n) => o.AddOperation(OperationType.Post, LoadOperation(n))},
            {"delete", (o, n) => o.AddOperation(OperationType.Delete, LoadOperation(n))},
            {"options", (o, n) => o.AddOperation(OperationType.Options, LoadOperation(n))},
            {"head", (o, n) => o.AddOperation(OperationType.Head, LoadOperation(n))},
            {"patch", (o, n) => o.AddOperation(OperationType.Patch, LoadOperation(n))},
            {"trace", (o, n) => o.AddOperation(OperationType.Trace, LoadOperation(n))},
            {"servers", (o, n) => o.Servers = n.CreateList(LoadServer)},
            {"parameters", (o, n) => o.Parameters = n.CreateList(LoadParameter)}
        };

        private static readonly PatternFieldMap<OpenApiPathItem> _pathItemPatternFields =
            new PatternFieldMap<OpenApiPathItem>
            {
                {s => s.StartsWith("x-"), (o, p, n) => o.AddExtension(p, LoadExtension(p,n))}
            };

        public static OpenApiPathItem LoadPathItem(ParseNode node)
        {
            var mapNode = node.CheckMapNode("PathItem");

            var pointer = mapNode.GetReferencePointer();

            if (pointer != null)
            {
                var description = node.Context.VersionService.GetReferenceScalarValues(mapNode, OpenApiConstants.Description);
                var summary = node.Context.VersionService.GetReferenceScalarValues(mapNode, OpenApiConstants.Summary);

                return new OpenApiPathItem()
                {
                    UnresolvedReference = true,
                    Reference = node.Context.VersionService.ConvertToOpenApiReference(pointer, ReferenceType.PathItem, summary, description)
                };
            }

            var pathItem = new OpenApiPathItem();

            ParseMap(mapNode, pathItem, _pathItemFixedFields, _pathItemPatternFields);

            return pathItem;
        }
    }
}