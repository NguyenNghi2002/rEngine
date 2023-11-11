namespace Engine
{
    public static class Insist
    {
        public static void IsFalse(bool condition, string trueMessage)
            => IsTrue(!condition,trueMessage);
        public static void IsTrue(bool condition,string falseMessage)
        {
            if (!condition) throw new Exception(falseMessage);
        }

        public static void IsNotNull<T>(T obj,string nullMessage ="") 
        {
            if (obj == null) throw new ArgumentNullException(nullMessage);
        }
    }
}