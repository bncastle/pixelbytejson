namespace Pixelbyte.Json
{
    /// <summary>
    /// Implement on a class get a callback just before it is encoded to a JSON object
    /// </summary>
    public interface IJsonPreEncode { void OnJsonPreEncode(); }
}
