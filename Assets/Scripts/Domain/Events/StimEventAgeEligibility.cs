namespace StimTycoon.Events
{
    public static class StimEventAgeEligibility
    {
        public static bool IsEligible(StimEvent evt, int age)
        {
            return evt != null && IsEligible(evt.ageRange, age);
        }

        public static bool IsEligible(AgeRange range, int age)
        {
            return range != null && age >= range.minAge && age <= range.maxAge;
        }
    }
}
