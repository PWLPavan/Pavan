namespace FGUnity.Utils
{
    /// <summary>
    /// Performs bitwise operations on unsigned integers.
    /// </summary>
    static public class Bits
    {
        /// <summary>
        /// Total number of bits in an unsigned integer.
        /// </summary>
        public const int LENGTH = 32;

        /// <summary>
        /// Returns if the given uint has the given bit toggled on.
        /// </summary>
        static public bool Contains(uint inBitArray, byte inBitIndex)
        {
            Assert.True(inBitIndex < LENGTH, "Bits is within range.");
            return (inBitArray & (1U << inBitIndex)) > 0;
        }

        /// <summary>
        /// Toggles the given bit in the given uint to on.
        /// </summary>
        static public void Add(ref uint ioBitArray, byte inBitIndex)
        {
            Assert.True(inBitIndex < LENGTH, "Bits is within range.");
            ioBitArray |= (1U << inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given uint to off.
        /// </summary>
        static public void Remove(ref uint ioBitArray, byte inBitIndex)
        {
            Assert.True(inBitIndex < LENGTH, "Bits is within range.");
            ioBitArray &= ~(1U << inBitIndex);
        }

        /// <summary>
        /// Toggles the given bit in the given uint to the given state.
        /// </summary>
        static public void Set(ref uint ioBitArray, byte inBitIndex, bool inbState)
        {
            if (inbState)
                Add(ref ioBitArray, inBitIndex);
            else
                Remove(ref ioBitArray, inBitIndex);
        }
    }
}
