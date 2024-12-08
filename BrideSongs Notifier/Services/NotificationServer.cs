using BrideSongs.Models.Notifications;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
using BrideSongs.Notifier.App.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
namespace BrideSongs.Notifier.App.Services;

public class NotificationServer : INotificationServer
{
    private bool _isBusy;
    private readonly ILogger<NotificationServer> _logger;

    public NotificationServer(ILogger<NotificationServer> logger, IConfiguration configuration)
    {
        _logger = logger;
        configuration = configuration;
    }

    public event Action<SongNotificationModel> OneSongRecieved;
    public event Action<IEnumerable<SongNotificationModel>> ManySongsRecieved;
    public event Action<Exception> ErrorHappend;

    public async Task StopServer()
    {
        _isBusy = false;
    }

    public async Task StartServer(string listenIp, string port)
    {
        if (!_isBusy)
        {
            IPAddress ip = string.IsNullOrEmpty(listenIp) ? IPAddress.Any : IPAddress.Parse(listenIp);
            var parsedPort = !string.IsNullOrEmpty(port) ? int.Parse(port) : 8888;

            var tcpListener = new TcpListener(ip, parsedPort);
            try
            {
                tcpListener.Start();
                _logger?.LogInformation("Tcp listener has been started. Port: 8888");
                _isBusy = true;

                while (_isBusy)
                {
                    var tcpClient = await tcpListener.AcceptTcpClientAsync();

                    Task.Run(async () => await ProcessClientAsync(tcpClient));
                }
            }
            catch (Exception ex)
            {
                ErrorHappend?.Invoke(ex);
            }
            finally
            {
                _isBusy = false;
                tcpListener.Stop();
            }
        }
    }

    private async Task ProcessClientAsync(TcpClient tcpClient)
    {
        var stream = tcpClient.GetStream();
        var responseMessage = string.Empty;

        var response = new List<byte>();
        int bytesRead = 10;

        while (_isBusy)
        {
            while (_isBusy && (bytesRead = stream.ReadByte()) != NotificationsConstants.EndPackageChar)
            {
                if (response.Count > 70000)
                {
                    response.Clear();
                    break;
                }
                response.Add((byte)bytesRead);
            }

            var message = Encoding.UTF8.GetString(response.ToArray());
            var notificationMessage = JsonSerializer.Deserialize<NotificationMessage>(message);

            if (notificationMessage.Type == NotificationsConstants.GreetingsPrefix)
            {
                _logger?.LogInformation("Received greetings for connection request from client.");
                responseMessage = NotificationsConstants.GreetingsAnswerPrefix;
                responseMessage += NotificationsConstants.EndPackageChar;
                await stream.WriteAsync(Encoding.UTF8.GetBytes(responseMessage));
                break;
            }

            if (notificationMessage.Type == NotificationsConstants.OneSongPackagePrefix)
            {
                _logger?.LogInformation("Received song information from client.");
                var song = JsonSerializer.Deserialize<SongNotificationModel>(notificationMessage.JsonPayload);
                OneSongRecieved?.Invoke(song);
                response.Clear();
                break;
            }

            if (notificationMessage.Type == NotificationsConstants.ManySongsPackagePrefix)
            {
                _logger?.LogInformation("Received Favorited list information from client.");
                var songs = JsonSerializer.Deserialize<IEnumerable<SongNotificationModel>>(notificationMessage.JsonPayload);
                ManySongsRecieved?.Invoke(songs);
                response.Clear();
                break;
            }

            if (notificationMessage.Type == NotificationsConstants.EndPrefix)
            {
                _logger?.LogInformation("End package.");
                response.Clear();
                break;
            }
        }
        tcpClient.Close();
    }

    public void DisplayManySongsReceivedPopup(IEnumerable<SongNotificationModel> songs) 
    {
        ManySongsRecieved?.Invoke(songs);
    }
}
