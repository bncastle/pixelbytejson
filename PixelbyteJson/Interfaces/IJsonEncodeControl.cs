namespace Pixelbyte.Json
{

    public interface IJsonEncodeControl
    {
        /// <summary>
        /// If found in an object, this method
        /// is called to get all values that are to be JSON encoded
        /// Anything that is not returned in the EncodeData is not encoded
        /// </summary>
        /// <param name="info"></param>
        void GetSerializedData(EncodeInfo info);
    }
}
