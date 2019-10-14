using Microsoft.AspNetCore.SignalR.Client;
using SignalRJwtAndCookieAuthentication.Dtos;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

//https://docs.microsoft.com/en-us/aspnet/core/signalr/dotnet-client?view=aspnetcore-3.0&tabs=visual-studio
namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static HubConnection _connection;
        private static Settings _settings;

        // https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
        private RestService RestService { get; }

        private static Uri FormatBaseAddress(Uri uri)
        {
            if (uri == null || !uri.IsAbsoluteUri)
            {
                return uri;
            }

            var uriBuilder = new UriBuilder(uri.ToString().ToLowerInvariant());
            var path = uriBuilder.Path;

            if (path.EndsWith("/api/", StringComparison.Ordinal))
            {
                uriBuilder.Path = path.Substring(0, path.Length - 4);
            }
            else if (path.EndsWith("/api", StringComparison.Ordinal))
            {
                uriBuilder.Path = path.Substring(0, path.Length - 3);
            }
            else if (!path.EndsWith("/", StringComparison.Ordinal))
            {
                uriBuilder.Path += "/";
            }

            uriBuilder.Scheme = "https";

            if (uriBuilder.Port == 80)
            {
                uriBuilder.Port = 443;
            }

            return uriBuilder.Uri;
        }

        public MainWindow()
        {
            InitializeComponent();

            _settings = new Settings();
            var baseAddress = new Uri($"{_settings.BaseAddress}api/", UriKind.Absolute);
            RestService = new RestService(baseAddress);

            emailTextBox.Text = _settings.Email;
            passwordBox.Password = _settings.Password;
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            connectButton.IsEnabled = false;
            var email = emailTextBox.Text;
            var password = passwordBox.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                messagesList.Items.Add("Email or Password are blank.");
                return;
            }

            _connection = new HubConnectionBuilder()
            .WithUrl($"{_settings.BaseAddress}ChatHub?ClientType=PC", options =>
            {
                options.AccessTokenProvider = async () =>
                {
                    var tokenUserCommand = new TokenUserCommand
                    {
                        Email = emailTextBox.Text,
                        Password = passwordBox.Password
                    };

                    var token = await RestService.PostAsync<string>("account/token", tokenUserCommand);
                    return token;
                };

            })
            .Build();

            _connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await _connection.StartAsync();
            };

            _ = _connection.On<string>("ReceiveChatMessage", (userAndMessage) =>
            {
                Dispatcher.Invoke(() =>
                {
                    var newMessage = userAndMessage;
                    messagesList.Items.Add(newMessage);
                });
            });

            _connection.On<string>("SendLargeDataAsync", (user) =>
            {
                _ = Dispatcher.Invoke(async () =>
                {
                    try
                    {
                        // Get large json from https://next.json-generator.com
                        var currentDirectory = Directory.GetCurrentDirectory();
                        var jsonData = await File.ReadAllTextAsync(currentDirectory + "\\..\\..\\..\\test25.json");
                        //var jsonData = await File.ReadAllTextAsync(currentDirectory + "\\..\\..\\..\\test100.json");
                        var numberOfBytes = Encoding.Default.GetByteCount(jsonData);
                        var newMessage = $"Sending large data to User:{user}, Bytes: {numberOfBytes}";
                        messagesList.Items.Add(newMessage);

                        var submitLargeDataCommand = new SubmitLargeDataCommand
                        {
                            User = user,
                            JsonData = jsonData
                        };

                        await RestService.PostAsync("LargeData/SubmitLargeData", submitLargeDataCommand);
                    }
                    catch (Exception ex)
                    {
                        var message = ex.Message;
                        throw;
                    }
                });
            });

            try
            {
                await _connection.StartAsync();
                messagesList.Items.Add("Connection started");
                sendButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                messagesList.Items.Add(ex.Message);
                connectButton.IsEnabled = true;
            }
        }


        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_connection == null)
                {
                    messagesList.Items.Add("Not Connected.");
                    return;
                }

                await _connection.InvokeAsync("Send", messageTextBox.Text);
                messageTextBox.Text = string.Empty;
            }
            catch (Exception ex)
            {
                messagesList.Items.Add(ex.Message);
            }
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        public static async Task<bool> ConnectWithRetryAsync(HubConnection connection, CancellationToken token)
        {
            // Keep trying to until we can start or the token is canceled.
            while (true)
            {
                try
                {
                    await connection.StartAsync(token);
                    Debug.Assert(connection.State == HubConnectionState.Connected);
                    return true;
                }
                catch when (token.IsCancellationRequested)
                {
                    return false;
                }
                catch
                {
                    // Failed to connect, trying again in 5000 ms.
                    Debug.Assert(connection.State == HubConnectionState.Disconnected);
                    await Task.Delay(5000);
                }
            }
        }
    }
}
