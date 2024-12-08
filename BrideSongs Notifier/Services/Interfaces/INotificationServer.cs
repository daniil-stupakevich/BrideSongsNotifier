using BrideSongs.Models.Notifications;

namespace BrideSongs.Notifier.App.Services.Interfaces;
public interface INotificationServer
{
    public event Action<SongNotificationModel> OneSongRecieved;
    public event Action<IEnumerable<SongNotificationModel>> ManySongsRecieved;
    public event Action<Exception> ErrorHappend;

    public Task StartServer(string ipAdress, string port);
    public Task StopServer();
    void DisplayManySongsReceivedPopup(IEnumerable<SongNotificationModel> songs);
}
