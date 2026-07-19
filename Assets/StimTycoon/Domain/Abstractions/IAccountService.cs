namespace StimTycoon.Abstractions
{
    /// <summary>
    /// Wraps Unity Authentication and platform identity providers behind one account boundary.
    /// </summary>
    public interface IAccountService
    {
        string PlayerAccountId { get; }
        bool IsAuthenticated { get; }
        void SignInAnonymously();
        void LinkAppleGameCenter();
    }
}