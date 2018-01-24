namespace Pixelbyte.Json
{
    public interface IJsonEncodeCallbacks
    {
        void OnPreJsonEncode();
        void OnPostJsonEncode();
    }
}
