namespace Pixelbyte.Json
{
    /// <summary>
    /// Implement on a class to get a callback after it is decoded to a JSON object
    /// </summary>
    public interface IJsonDecoded { void OnJsonDecoded(); }
}
