namespace FGUnity.Utils
{
    /// <summary>
    /// Contains lookup tables, primarily for string values.
    /// This avoids unnecessary allocations.
    /// For example, int.ToString() creates a new string each time,
    /// but ToStringLookup() shares the same string between all
    /// instances of a given integer value (within a certain range).
    /// </summary>
    static public class LookupTables
    {
        static private string[] s_IntegerTable;
        private const int INTEGER_RANGE = 99;

        static LookupTables()
        {
            s_IntegerTable = new string[INTEGER_RANGE + 1];
            for (int i = 0; i <= INTEGER_RANGE; ++i)
                s_IntegerTable[i] = i.ToString();
        }

        static public string ToStringLookup(this int inValue)
        {
            if (inValue >= 0 && inValue <= INTEGER_RANGE)
                return s_IntegerTable[inValue];
            return inValue.ToString();
        }
    }
}
