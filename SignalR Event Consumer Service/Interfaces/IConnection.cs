namespace SignalR_Event_Consumer_Service.Interfaces
{
    public interface IConnection
    {
        Task StartConnection();
        Task StopConnection();
        void SubscribeToEvents();
    }
}
