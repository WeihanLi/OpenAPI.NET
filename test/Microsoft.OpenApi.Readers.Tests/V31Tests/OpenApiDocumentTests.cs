﻿using System.Collections.Generic;
using System.Globalization;
using System.IO;
using FluentAssertions;
using Json.Schema;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using Xunit;

namespace Microsoft.OpenApi.Readers.Tests.V31Tests
{
    public class OpenApiDocumentTests
    {
        private const string SampleFolderPath = "V31Tests/Samples/OpenApiDocument/";

        public T Clone<T>(T element) where T : IOpenApiSerializable
        {
            using var stream = new MemoryStream();
            IOpenApiWriter writer;
            var streamWriter = new FormattingStreamWriter(stream, CultureInfo.InvariantCulture);
            writer = new OpenApiJsonWriter(streamWriter, new OpenApiJsonWriterSettings()
            {
                InlineLocalReferences = true
            });
            element.SerializeAsV31(writer);
            writer.Flush();
            stream.Position = 0;

            using var streamReader = new StreamReader(stream);
            var result = streamReader.ReadToEnd();
            return new OpenApiStringReader().ReadFragment<T>(result, OpenApiSpecVersion.OpenApi3_1, out OpenApiDiagnostic diagnostic4);
        }
        
        [Fact]
        public void ParseDocumentWithWebhooksShouldSucceed()
        {
            // Arrange and Act
            using var stream = Resources.GetStream(Path.Combine(SampleFolderPath, "documentWithWebhooks.yaml"));
            var actual = new OpenApiStreamReader().Read(stream, out var diagnostic);

            var petSchema = new JsonSchemaBuilder()
                        .Type(SchemaValueType.Object)
                        .Required("name")
                        .Properties(
                            ("id", new JsonSchemaBuilder()
                                .Type(SchemaValueType.Integer)
                                .Format("int64")),
                            ("name", new JsonSchemaBuilder()
                                .Type(SchemaValueType.String)
                            ),
                            ("tag", new JsonSchemaBuilder().Type(SchemaValueType.String))
                        )
                        .Ref("#/components/schemas/newPet");

            var newPetSchema = new JsonSchemaBuilder()
                        .Type(SchemaValueType.Object)
                        .Required("name")
                        .Properties(
                            ("id", new JsonSchemaBuilder()
                                .Type(SchemaValueType.Integer)
                                .Format("int64")),
                            ("name", new JsonSchemaBuilder()
                                .Type(SchemaValueType.String)
                            ),
                            ("tag", new JsonSchemaBuilder().Type(SchemaValueType.String))
                        )
                        .Ref("#/components/schemas/newPet");
            
            var components = new OpenApiComponents
            {
                Schemas31 =
                {
                    ["pet"] = petSchema,
                    ["newPet"] = newPetSchema                    
                }
            };

            var expected = new OpenApiDocument
            {
                Info = new OpenApiInfo
                {
                    Version = "1.0.0",
                    Title = "Webhook Example"
                },
                Webhooks = new Dictionary<string, OpenApiPathItem>
                {
                    ["/pets"] = new OpenApiPathItem
                    {
                        Operations = new Dictionary<OperationType, OpenApiOperation>
                        {
                            [OperationType.Get] = new OpenApiOperation
                            {
                                Description = "Returns all pets from the system that the user has access to",
                                OperationId = "findPets",
                                Parameters = new List<OpenApiParameter>
                                    {
                                        new OpenApiParameter
                                        {
                                            Name = "tags",
                                            In = ParameterLocation.Query,
                                            Description = "tags to filter by",
                                            Required = false,
                                            Schema31 = new JsonSchemaBuilder()
                                            .Type(SchemaValueType.Array)
                                            .Items(new JsonSchemaBuilder()
                                                .Type(SchemaValueType.String)
                                            )                                            
                                        },
                                        new OpenApiParameter
                                        {
                                            Name = "limit",
                                            In = ParameterLocation.Query,
                                            Description = "maximum number of results to return",
                                            Required = false,
                                            Schema31 = new JsonSchemaBuilder()
                                            .Type(SchemaValueType.Integer).Format("int32")
                                        }
                                    },
                                Responses = new OpenApiResponses
                                {
                                    ["200"] = new OpenApiResponse
                                    {
                                        Description = "pet response",
                                        Content = new Dictionary<string, OpenApiMediaType>
                                        {
                                            ["application/json"] = new OpenApiMediaType
                                            {
                                                Schema31 = new JsonSchemaBuilder()
                                                    .Type(SchemaValueType.Array)
                                                    .Items(new JsonSchemaBuilder()
                                                        .Ref("#/components/schemas/pet"))

                                            },
                                            ["application/xml"] = new OpenApiMediaType
                                            {
                                                Schema31 = new JsonSchemaBuilder()
                                                    .Type(SchemaValueType.Array)
                                                    .Items(new JsonSchemaBuilder()
                                                        .Ref("#/components/schemas/pet"))
                                            }
                                        }
                                    }
                                }
                            },
                            [OperationType.Post] = new OpenApiOperation
                            {
                                RequestBody = new OpenApiRequestBody
                                {
                                    Description = "Information about a new pet in the system",
                                    Required = true,
                                    Content = new Dictionary<string, OpenApiMediaType>
                                    {
                                        ["application/json"] = new OpenApiMediaType
                                        {
                                            Schema31 = newPetSchema
                                        }
                                    }
                                },
                                Responses = new OpenApiResponses
                                {
                                    ["200"] = new OpenApiResponse
                                    {
                                        Description = "Return a 200 status to indicate that the data was received successfully",
                                        Content = new Dictionary<string, OpenApiMediaType>
                                        {
                                            ["application/json"] = new OpenApiMediaType
                                            {
                                                Schema31 = petSchema
                                            },
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                Components = components
            };

            // Assert
            //diagnostic.Should().BeEquivalentTo(new OpenApiDiagnostic() { SpecificationVersion = OpenApiSpecVersion.OpenApi3_1 });
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ParseDocumentsWithReusablePathItemInWebhooksSucceeds()
        {
            // Arrange && Act
            using var stream = Resources.GetStream("V31Tests/Samples/OpenApiDocument/documentWithReusablePaths.yaml");
            var actual = new OpenApiStreamReader().Read(stream, out var context);

            var components = new OpenApiComponents
            {
                Schemas31 = new Dictionary<string, JsonSchema>
                {
                    ["pet"] = new JsonSchemaBuilder()
                                .Type(SchemaValueType.Object)
                                .Required("id", "name")
                                .Properties(
                                    ("id", new JsonSchemaBuilder().Type(SchemaValueType.Integer).Format("int64")),
                                    ("name", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                                    ("tag", new JsonSchemaBuilder().Type(SchemaValueType.String)))
                                .Ref("pet"),
                    ["newPet"] = new JsonSchemaBuilder()
                                .Type(SchemaValueType.Object)
                                .Required("name")
                                .Properties(
                                    ("id", new JsonSchemaBuilder().Type(SchemaValueType.Integer).Format("int64")),
                                    ("name", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                                    ("tag", new JsonSchemaBuilder().Type(SchemaValueType.String)))
                                .Ref("newPet")
                }
            };

            // Create a clone of the schema to avoid modifying things in components.
            var petSchema = components.Schemas31["pet"];

            //petSchema.Reference = new OpenApiReference
            //{
            //    Id = "pet",
            //    Type = ReferenceType.Schema,
            //    HostDocument = actual
            //};

            var newPetSchema = components.Schemas31["newPet"];

            //newPetSchema.Reference = new OpenApiReference
            //{
            //    Id = "newPet",
            //    Type = ReferenceType.Schema,
            //    HostDocument = actual
            //};
            components.PathItems = new Dictionary<string, OpenApiPathItem>
            {
                ["/pets"] = new OpenApiPathItem
                {
                    Operations = new Dictionary<OperationType, OpenApiOperation>
                    {
                        [OperationType.Get] = new OpenApiOperation
                        {
                            Description = "Returns all pets from the system that the user has access to",
                            OperationId = "findPets",
                            Parameters = new List<OpenApiParameter>
                                {
                                    new OpenApiParameter
                                    {
                                        Name = "tags",
                                        In = ParameterLocation.Query,
                                        Description = "tags to filter by",
                                        Required = false,
                                        Schema31 = new JsonSchemaBuilder()
                                            .Type(SchemaValueType.Array)
                                            .Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
                                    },
                                    new OpenApiParameter
                                    {
                                        Name = "limit",
                                        In = ParameterLocation.Query,
                                        Description = "maximum number of results to return",
                                        Required = false,
                                        Schema31 = new JsonSchemaBuilder()
                                                    .Type(SchemaValueType.Integer).Format("int32")
                                    }
                                },
                            Responses = new OpenApiResponses
                            {
                                ["200"] = new OpenApiResponse
                                {
                                    Description = "pet response",
                                    Content = new Dictionary<string, OpenApiMediaType>
                                    {
                                        ["application/json"] = new OpenApiMediaType
                                        {
                                            Schema31 = new JsonSchemaBuilder()
                                                .Type(SchemaValueType.Array)
                                                .Items(petSchema)
                                        },
                                        ["application/xml"] = new OpenApiMediaType
                                        {
                                            Schema31 = new JsonSchemaBuilder()
                                                .Type(SchemaValueType.Array)
                                                .Items(petSchema)
                                        }
                                    }
                                }
                            }
                        },
                        [OperationType.Post] = new OpenApiOperation
                        {
                            RequestBody = new OpenApiRequestBody
                            {
                                Description = "Information about a new pet in the system",
                                Required = true,
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema31 = newPetSchema
                                    }
                                }
                            },
                            Responses = new OpenApiResponses
                            {
                                ["200"] = new OpenApiResponse
                                {
                                    Description = "Return a 200 status to indicate that the data was received successfully",
                                    Content = new Dictionary<string, OpenApiMediaType>
                                    {
                                        ["application/json"] = new OpenApiMediaType
                                        {
                                            Schema31 = petSchema
                                        },
                                    }
                                }
                            }
                        }
                    },
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.PathItem,
                        Id = "/pets",
                        HostDocument = actual
                    }
                }
            };

            var expected = new OpenApiDocument
            {
                Info = new OpenApiInfo
                {
                    Title = "Webhook Example",
                    Version = "1.0.0"
                },
                JsonSchemaDialect = "http://json-schema.org/draft-07/schema#",
                Webhooks = components.PathItems,
                Components = components
            };

            // Assert
            actual.Should().BeEquivalentTo(expected);
            context.Should().BeEquivalentTo(
    new OpenApiDiagnostic() { SpecificationVersion = OpenApiSpecVersion.OpenApi3_1 });

        }

        [Fact]
        public void ParseDocumentWithDescriptionInDollarRefsShouldSucceed()
        {
            // Arrange
            using var stream = Resources.GetStream(Path.Combine(SampleFolderPath, "documentWithSummaryAndDescriptionInReference.yaml"));

            // Act
            var actual = new OpenApiStreamReader().Read(stream, out var diagnostic);
            var schema = actual.Paths["/pets"].Operations[OperationType.Get].Responses["200"].Content["application/json"].Schema31;
            var header = actual.Components.Responses["Test"].Headers["X-Test"];

            // Assert
            Assert.True(header.Description == "A referenced X-Test header"); /*response header #ref's description overrides the header's description*/
            //Assert.True(schema.UnresolvedReference == false && schema.Type == "object"); /*schema reference is resolved*/
            Assert.Equal("A pet in a petstore", schema.GetDescription()); /*The reference object's description overrides that of the referenced component*/
        }
    }
}