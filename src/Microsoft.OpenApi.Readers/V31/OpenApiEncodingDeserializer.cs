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
        private static readonly FixedFieldMap<OpenApiEncoding> _encodingFixedFields = new FixedFieldMap<OpenApiEncoding>
        {
            {
                "contentType", (o, n) =>
                {
                    o.ContentType = n.GetScalarValue();
                }
            },
            {
                "headers", (o, n) =>
                {
                    o.Headers = n.CreateMap(LoadHeader);
                }
            },
            {
                "style", (o, n) =>
                {
                    o.Style = n.GetScalarValue().GetEnumFromDisplayName<ParameterStyle>();
                }
            },
            {
                "explode", (o, n) =>
                {
                    o.Explode = bool.Parse(n.GetScalarValue());
                }
            },
            {
                "allowedReserved", (o, n) =>
                {
                    o.AllowReserved = bool.Parse(n.GetScalarValue());
                }
            },
        };

        private static readonly PatternFieldMap<OpenApiEncoding> _encodingPatternFields =
            new PatternFieldMap<OpenApiEncoding>
            {
                {s => s.StartsWith("x-"), (o, p, n) => o.AddExtension(p, LoadExtension(p,n))}
            };

        public static OpenApiEncoding LoadEncoding(ParseNode node)
        {
            var mapNode = node.CheckMapNode("encoding");

            var encoding = new OpenApiEncoding();
            foreach (var property in mapNode)
            {
                property.ParseField(encoding, _encodingFixedFields, _encodingPatternFields);
            }

            return encoding;
        }
    }
}