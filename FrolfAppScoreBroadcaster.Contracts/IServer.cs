namespace FrolfAppScoreBroadcaster.Contracts
{
    public interface IServer
    {
        void Start();
        void Restart();
        void Shutdown();
    }
}
