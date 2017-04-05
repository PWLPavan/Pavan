namespace Ekstep
{
    public class OE_ITEM_RESPONSE : GenieEvent
    {
        public enum Type
        {
            CHOOSE,
            DRAG,
            SELECT,
            MATCH,
            INPUT,
            SPEAK,
            WRITE
        }

        public override int MinVersion
        {
            get { return 2; }
        }

        [EKS] public string itemid;
        [EKS] public Type type;
        [EKS] public string[] res;
    }
}