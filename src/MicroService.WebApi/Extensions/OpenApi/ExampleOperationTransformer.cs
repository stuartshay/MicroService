using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace MicroService.WebApi.Extensions.OpenApi
{
    /// <summary>
    /// Adds the examples from <see cref="IExampleProvider{T}"/> classes to the OpenAPI document:
    /// response examples named by <see cref="ResponseExampleAttribute"/>, and a request body
    /// example for every action whose body type has a provider in this assembly.
    /// </summary>
    public sealed class ExampleOperationTransformer : IOpenApiOperationTransformer
    {
        private static readonly IReadOnlyDictionary<Type, Type> RequestProviders = typeof(ExampleOperationTransformer).Assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .SelectMany(type => ProvidedTypes(type).Select(provided => (provided, type)))
            .Where(pair => pair.provided != typeof(object))
            .GroupBy(pair => pair.provided)
            .ToDictionary(group => group.Key, group => group.First().type);

        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExampleOperationTransformer"/> class.
        /// </summary>
        /// <param name="jsonOptions">The MVC JSON options, so examples are written as responses are.</param>
        public ExampleOperationTransformer(IOptions<Microsoft.AspNetCore.Mvc.JsonOptions> jsonOptions)
        {
            ArgumentNullException.ThrowIfNull(jsonOptions);
            _jsonOptions = jsonOptions.Value.JsonSerializerOptions;
        }

        /// <inheritdoc />
        public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(operation);
            ArgumentNullException.ThrowIfNull(context);

            var metadata = context.Description.ActionDescriptor.EndpointMetadata;
            foreach (var attribute in metadata.OfType<ResponseExampleAttribute>())
            {
                if (operation.Responses?.TryGetValue(attribute.StatusCode.ToString(System.Globalization.CultureInfo.InvariantCulture), out var response) == true
                    && response.Content is { } content)
                {
                    var example = Build(attribute.ProviderType);
                    foreach (var mediaType in content.Values.OfType<OpenApiMediaType>())
                    {
                        mediaType.Example = example?.DeepClone();
                    }
                }
            }

            var bodyType = context.Description.ParameterDescriptions
                .FirstOrDefault(parameter => parameter.Source == BindingSource.Body)?.Type;
            if (bodyType is not null
                && RequestProviders.TryGetValue(bodyType, out var providerType)
                && operation.RequestBody?.Content is { } requestContent)
            {
                var example = Build(providerType);
                foreach (var mediaType in requestContent.Values.OfType<OpenApiMediaType>())
                {
                    mediaType.Example = example?.DeepClone();
                }
            }

            return Task.CompletedTask;
        }

        private JsonNode? Build(Type providerType)
        {
            var provided = ProvidedTypes(providerType).FirstOrDefault()
                ?? throw new InvalidOperationException($"{providerType.Name} does not implement IExampleProvider<T>.");
            var provider = Activator.CreateInstance(providerType)!;
            var method = typeof(IExampleProvider<>).MakeGenericType(provided).GetMethod(nameof(IExampleProvider<object>.GetExamples))!;
            var value = method.Invoke(provider, null);
            return JsonSerializer.SerializeToNode(value, value?.GetType() ?? typeof(object), _jsonOptions);
        }

        private static IEnumerable<Type> ProvidedTypes(Type type) => type.GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IExampleProvider<>))
            .Select(i => i.GetGenericArguments()[0]);
    }
}
