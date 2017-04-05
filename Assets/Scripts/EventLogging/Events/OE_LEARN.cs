namespace Ekstep
{
    public class OE_LEARN : GenieEvent
    {
        enum Method
        {
            PLAY,
            ANSWER,
            WRITE,
            SPEAK,
            OTHER,
        }
        class Topic
        {
            public string mc;
            // This should be removed in V2
            public string skill;
            public Method[] methods;
        }

        [EKS] Topic[] topics;

        public OE_LEARN()
        {
            this.topics = new Topic[0];
        }
    }
}