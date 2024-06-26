namespace MonkeRotate.Tools
{
    public struct Counter
    {
        public uint value;

        public Counter(uint number)
        {
            value = number;
        }

        public static Counter operator ++(Counter num)
        {
            num.value = num.value < uint.MaxValue ? num.value + 1 : uint.MaxValue;
            return num;
        }

        public static Counter operator --(Counter num)
        {
            num.value = num.value > 0u ? num.value - 1 : 0;
            return num;
        }

        public static bool operator true(Counter trueFalse) => trueFalse.value > 0;
        public static bool operator ==(Counter lhs, bool trueFalse) => (lhs.value > 0u) == trueFalse;
        public static bool operator ==(Counter lhs, Counter rhs) => lhs.value == rhs.value;

        public static bool operator false(Counter trueFalse) => trueFalse.value == 0;
        public static bool operator !(Counter notTrueFalse) => notTrueFalse.value == 0;
        public static bool operator !=(Counter lhs, bool trueFalse) => (lhs.value > 0u) != trueFalse;
        public static bool operator !=(Counter lhs, Counter rhs) => lhs.value != rhs.value;
        
        public static bool operator >(Counter lhs, Counter rhs) => (lhs.value > rhs.value);
        public static bool operator <(Counter lhs, Counter rhs) => (lhs.value < rhs.value);
        
    }
}