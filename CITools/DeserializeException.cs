namespace CITools
{
    internal class DeserializeException : NullReferenceException
    {
        public override string Message { get; }
        internal DeserializeException()
        {
            Message = "Error occurs while deserializing json";
        }
    }

}