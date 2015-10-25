using Babble.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Client.ViewModels
{
    class VoiceClientViewModel : ViewModelBase
    {
        VoiceClient client = new VoiceClient();
        System.Windows.Threading.Dispatcher dispatcher = System.Windows.Application.Current.Dispatcher;

        public VoiceClientViewModel()
        {
            client.SomeUserConnected += SomeUserConnectedHandler;
            client.SomeUserDisconnected += SomeUserDisconnectedHandler;
            client.ChannelCreated += ChannelCreatedHandler;
            client.RefreshChannels += RefreshChannelsHandler;
            client.Connected += ConnectedHandler;
            client.Disconnected += DisconnectedHandler;

            ConnectCommand = new DelegateCommand(ConnectCommandHandler);
            DisconnectCommand = new DelegateCommand(DisconnectCommandHandler);
            JoinChannelCommand = new DelegateCommand(JoinChannelCommandHandler);
            CreateChannelCommand = new DelegateCommand(CreateChannelCommandHandler);


        }

        private ChannelTreeViewModel _ChannelTreeViewModel;
        public ChannelTreeViewModel ChannelTreeViewModel
        {
            get { return _ChannelTreeViewModel; }
            set { _ChannelTreeViewModel = value; OnPropertyChanged(nameof(ChannelTreeViewModel)); }
        }

        private string _Activity;
        public string Activity
        {
            get { return _Activity; }
            set { _Activity = value;OnPropertyChanged(nameof(Activity)); }
        }

        private bool _IsConnected;
        public bool IsConnected
        {
            get { return _IsConnected; }
            set { _IsConnected = value;OnPropertyChanged(nameof(IsConnected)); }
        }

        public ICommand ConnectCommand { get; private set; }
        private void ConnectCommandHandler(object state)
        {
            NewConnectionWindow ncw = new NewConnectionWindow(System.Windows.Application.Current.MainWindow);
            if (ncw.ShowDialog() == true)
            {
                try
                {
                    var host = ncw.Host;
                    var port = ncw.Port;
                    AddActivity("Attempting to connect to " + host + ":" + port + ".");
                    client.Connect(host, port, ncw.Username, ncw.Password);
                    IsConnected = true;
                }
                catch (Exception ex)
                {
                    AddActivity(ex.Message);
                    IsConnected = false;
                }
            }
        }

        public ICommand DisconnectCommand { get; private set; }
        private void DisconnectCommandHandler(object state)
        {
            this.client.Disconnect();
            ChannelTreeViewModel = null;
            AddActivity("Disconnected");
            IsConnected = false;
        }

        public ICommand JoinChannelCommand { get; private set; }
        private void JoinChannelCommandHandler(object state)
        {
        }

        public ICommand CreateChannelCommand { get; private set; }
        private void CreateChannelCommandHandler(object state)
        {
            NewChannelWindow window = new NewChannelWindow(System.Windows.Application.Current.MainWindow);
            if (window.ShowDialog() == true)
            {
                var channelName = window.ChannelNameTextBox.Text;
                client.WriteMessage(Message.Create(MessageType.RequestChannelCreate, new Channel() { Name = channelName }));
            }
        }

        private void ConnectedHandler(bool successful, string message)
        {
            if (successful)
            {
                AddActivity("Connected: Message From Host: " + message);
            }
            else
            {
                System.Windows.MessageBox.Show("Could not connect to host. Error: " + message);
            }
        }

        private void DisconnectedHandler()
        {
            DisconnectCommandHandler(null);
        }

        private void ChannelCreatedHandler(string name, int id)
        {
            dispatcher.Invoke(() =>
            {
                ChannelTreeViewModel.Channels.Add(new ChannelViewModel(new Channel() { Name = name, Id = id }));
                AddActivity("Channel " + id + " created");
            });
        }

        private void RefreshChannelsHandler(List<Channel> obj)
        {
            dispatcher.Invoke(() =>
            {
                ChannelTreeViewModel = new ChannelTreeViewModel(obj);
                AddActivity("Ain't nobody dope as me I'm dressed so fresh so clean");
            });
        }

        private void SomeUserConnectedHandler(string username, int channelId)
        {
            dispatcher.Invoke(() =>
            {
                var channel = ChannelTreeViewModel.Channels.FirstOrDefault(c => c.Id == channelId);
                if (channel != null)
                {
                    channel.Users.Add(new UserInfoViewModel(new UserInfo() { Username = username, ChannelId = channelId }));
                }
                AddActivity(string.Format("{0} Connected", username));
            });
        }

        private void SomeUserDisconnectedHandler(string username, int channelId)
        {
            dispatcher.Invoke(() =>
            {
                foreach (var channel in ChannelTreeViewModel.Channels)
                {
                    channel.Users.Remove(channel.Users.FirstOrDefault(u => u.Username == username));
                }
                AddActivity(string.Format("{0} Disconnected", username));
            });
        }

        public void AddActivity(string s)
        {
            Activity += Environment.NewLine + s;
        }
    }
}
