namespace Ekstep
{
    public class OE_INTERRUPT : GenieEvent
    {
        public enum Type
        {
            IDLE,
            SLEEP,
            CALL,
            SWITCH,
            LOCK,
            OTHER,
        }

        [EKS]
        Type type;
        [EKS]
        string id;
    }
}