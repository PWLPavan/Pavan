namespace Ekstep
{
    public class OE_MISC : GenieEvent
    {
        [EXT] public string type;
        [EXT] public string message;

        public OE_MISC(string type, string message)
        {
            this.type = type;
            this.message = message;
        }
    }
}