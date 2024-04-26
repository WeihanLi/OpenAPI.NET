using System.Text.Json;
using Json.Schema;
using Xunit;

namespace Microsoft.OpenApi.Tests.Validations
{
    public class JsonSchemaValidationTests
    {
        [Fact]
        public void ValidateJsonSchemaWorks()
        {
            // Arrange
            // Define the referenced schema
            //var referencedSchemaJson = @"{
            //    ""$id"": ""https://example.com/referencedSchema"",
            //    ""type"": ""object"",
            //    ""properties"": {
            //        ""name"": { ""type"": ""string"" }
            //    }
            //}";

            //// Define the main schema that references the other schema
            //var mainSchemaJson = @"{
            //    ""$id"": ""https://example.com/mainSchema"",
            //    ""type"": ""object"",
            //    ""properties"": {
            //        ""person"": { ""$ref"": ""https://example.com/referencedSchema"" }
            //    }
            //}";

            var doc = @"{
  ""$schema"": ""http://json-schema.org/draft-07/schema#"",
  ""type"": ""object"",
  ""title"": ""Root Schema"",
  ""required"": [
    ""referencedObject""
  ],
  ""properties"": {
    ""referencedObject"": {
      ""$ref"": ""#/components/schemas/ReferencedObject""
    }
  }
}";
            var referenced = @"{
""$schema"": ""http://json-schema.org/draft-07/schema#"",
""components"": {
    ""schemas"": {
      ""ReferencedObject"": {
        ""type"": ""object"",
        ""title"": ""Referenced Object"",
        ""properties"": {
          ""name"": {
            ""type"": ""string""
          },
          ""description"": {
            ""type"": ""string""
          }
        }
      }
    }
  }
}";

            // Act
            var referencedSchema = JsonSchema.FromText(referenced);
            var mainSchema = JsonSchema.FromText(doc);

            // Create a JSON object that conforms to the main schema
            var jsonObject = JsonDocument.Parse(@"{
                ""person"": {
                    ""name"": ""John Doe""
                }
            }").RootElement;

            // Validate the JSON object against the main schema
            var refUri = string.Concat(mainSchema.BaseUri, "#/components/schemas/ReferencedObject");
            mainSchema.BaseUri = new System.Uri(refUri);

            var options = new EvaluationOptions();
            options.SchemaRegistry.Register(referencedSchema);

            var validationResult = mainSchema.Evaluate(jsonObject, options);

            // Assert
            Assert.True(validationResult.IsValid);
        }
    }
}
