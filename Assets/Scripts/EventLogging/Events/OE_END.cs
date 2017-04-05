namespace Ekstep
{
    public class OE_END : GenieEvent
    {
        public OE_END(float inDuration)
        {
            length = inDuration;
        }

        [EKS]
        [RenamedInVersion(2, "duration")]
        float length;
    }

}