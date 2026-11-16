namespace MicroService.WebApi.Extensions.OpenApi
{
    /// <summary>
    /// Supplies an example value for the OpenAPI document.
    /// </summary>
    /// <typeparam name="T">The type the example stands for.</typeparam>
    public interface IExampleProvider<out T>
    {
        /// <summary>
        /// Builds the example value.
        /// </summary>
        /// <returns>The example.</returns>
        T GetExamples();
    }

    /// <summary>
    /// Shows the example from <see cref="ProviderType"/> for one response status code of an action.
    /// </summary>
    /// <param name="statusCode">The response status code.</param>
    /// <param name="providerType">A type that implements <see cref="IExampleProvider{T}"/>.</param>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class ResponseExampleAttribute(int statusCode, Type providerType) : Attribute
    {
        /// <summary>
        /// The response status code.
        /// </summary>
        public int StatusCode { get; } = statusCode;

        /// <summary>
        /// A type that implements <see cref="IExampleProvider{T}"/>.
        /// </summary>
        public Type ProviderType { get; } = providerType;
    }
}
