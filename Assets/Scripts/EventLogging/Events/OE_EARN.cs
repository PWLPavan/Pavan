namespace Ekstep
{
    public class OE_EARN : GenieEvent
    {
        public enum Type
        {
            MONEY,
            GEMS,
            POINTS
        }

        [EKS] public Type type;
        [EKS] public int points;
    }
}