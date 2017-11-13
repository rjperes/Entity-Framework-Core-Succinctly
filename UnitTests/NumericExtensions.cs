namespace UnitTests
{
    public static class NumericExtensions
    {
        public static bool IsEven(this decimal? number)
        {
            if (number == null)
            {
                return false;
            }

            return (number % 2) == 0;
        }
    }
}
