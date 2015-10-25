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
        System.Timers.Timer periodicUpdateTimer = new System.Timers.Timer(700); // periodic update GUI property that is timer based
        // so one of them is the talking status
        // if the user stopped talking for couple seconds, considered they're done, so update the UI

        public VoiceClientViewModel()
        {
            client.SomeUserConnected += SomeUserConnectedHandler;
            client.SomeUserDisconnected += SomeUserDisconnectedHandler;
            client.SomeUserTalking += SomeUserTalkingHandler;
            client.SomeUserChangedChannel += SomeUserChangedChannelHandler;
            client.ChannelCreated += ChannelCreatedHandler;
            client.ChannelRenamed += ChannelRenamedHandler;
            client.ChannelDeleted += ChannelDeletedHandler;
            client.RefreshChannels += RefreshChannelsHandler;
            client.Connected += ConnectedHandler;
            client.Disconnected += DisconnectedHandler;

            ConnectCommand = new DelegateCommand(ConnectCommandHandler);
            DisconnectCommand = new DelegateCommand(DisconnectCommandHandler);
            JoinChannelCommand = new DelegateCommand(JoinChannelCommandHandler);
            CreateChannelCommand = new DelegateCommand(CreateChannelCommandHandler);
            RenameChannelCommand = new DelegateCommand(RenameChannelCommandHandler);
            DeleteChannelCommand = new DelegateCommand(DeleteChannelCommandHandler);

            periodicUpdateTimer.Elapsed += PeriodicUpdateTimer_Elapsed;
        }

        private void PeriodicUpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (ChannelTreeViewModel == null || ChannelTreeViewModel.Channels == null)
            {
                return;
            }

            var users = GetAllUsers();
            
            foreach (var u in users)
            {
                u.UpdateTimedBaseProperty();
            }
        }

        public UserInfo UserInfo { get { return client.UserInfo; } }

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
                    periodicUpdateTimer.Start();
                }
                catch (Exception ex)
                {
                    AddActivity(ex.Message);
                    IsConnected = false;
                    periodicUpdateTimer.Stop();
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
            periodicUpdateTimer.Stop();

        }

        public ICommand JoinChannelCommand { get; private set; }
        private void JoinChannelCommandHandler(object state)
        {
            var channel = GetSelectedChannel();
            client.WriteMessage(Message.Create(MessageType.UserChangeChannelRequest, channel.Id));
        }

        public ICommand CreateChannelCommand { get; private set; }
        private void CreateChannelCommandHandler(object state)
        {
            NewChannelWindow window = new NewChannelWindow(System.Windows.Application.Current.MainWindow);
            if (window.ShowDialog() == true)
            {
                var channelName = window.ChannelNameTextBox.Text;
                client.WriteMessage(Message.Create(MessageType.CreateChannelRequest, new Channel() { Name = channelName }));
            }
        }

        public ICommand RenameChannelCommand { get; private set; }
        private void RenameChannelCommandHandler(object state)
        {
            var channel = GetSelectedChannel();

            RenameChannelWindow window = new RenameChannelWindow(System.Windows.Application.Current.MainWindow);
            window.ChannelNameTextBox.Text = channel.Name;
            if (window.ShowDialog() == true)
            {
                var newChannelName = window.ChannelNameTextBox.Text;
                client.WriteMessage(Message.Create(MessageType.RenameChannelRequest, new Channel() { Id = channel.Id, Name = newChannelName }));

            }
        }

        public ICommand DeleteChannelCommand { get; private set; }
        private void DeleteChannelCommandHandler(object state)
        {
            var confirmResult = System.Windows.MessageBox.Show(
                "Are you really really sure??",
                "Confirm",
                System.Windows.MessageBoxButton.YesNoCancel,
                System.Windows.MessageBoxImage.Warning);
            if (confirmResult != System.Windows.MessageBoxResult.Yes)
            {
                return;
            }
            var channel = GetSelectedChannel();
            client.WriteMessage(Message.Create(MessageType.DeleteChannelRequest, new Channel() { Id = channel.Id }));
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

        private void ChannelCreatedHandler(Channel channel)
        {
            dispatcher.Invoke(() =>
            {
                ChannelTreeViewModel.Channels.Add(new ChannelViewModel(channel));
                AddActivity($"Channel {channel.Id} : {channel.Name} created");
            });
        }

        private void ChannelRenamedHandler(Channel channel)
        {
            dispatcher.Invoke(() =>
            {
                var channelVM = FindChannel(channel.Id);
                channelVM.Name = channel.Name;
                AddActivity($"Channel {channel.Id} : {channel.Name} renamed");
            });
        }

        private void ChannelDeletedHandler(Channel channel)
        {
            dispatcher.Invoke(() =>
            {
                var channelVM = FindChannel(channel.Id);
                ChannelTreeViewModel.Channels.Remove(channelVM);
                AddActivity($"Channel {channel.Id} : {channel.Name} deleted");
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

        private void SomeUserConnectedHandler(UserInfo userInfo)
        {
            dispatcher.Invoke(() =>
            {
                var channel = ChannelTreeViewModel.Channels.FirstOrDefault(c => c.Id == userInfo.ChannelId);
                if (channel != null)
                {
                    channel.Users.Add(new UserInfoViewModel(userInfo));
                }
                AddActivity(string.Format("{0} Connected", userInfo.Username));
            });
        }

        private void SomeUserDisconnectedHandler(UserInfo userInfo)
        {
            dispatcher.Invoke(() =>
            {
                foreach (var channel in ChannelTreeViewModel.Channels)
                {
                    channel.Users.Remove(channel.Users.FirstOrDefault(u => u.Id == userInfo.Id));
                }
                AddActivity(string.Format("{0} Disconnected", userInfo.Username));
            });
        }

        private void SomeUserTalkingHandler(UserInfo userInfo)
        {
            var user = FindUser(userInfo.Id);
            if (user == null)
            {
                return;
            }

            user.BeginTalkingTime = DateTime.Now;
            user.IsTalking = true;
        }

        private void SomeUserChangedChannelHandler(UserInfo userInfo)
        {
            dispatcher.Invoke(() =>
            {
                RemoveUserFromChannels(userInfo);
                AddUserToChannel(userInfo);
            });
        }

        public void AddActivity(string s)
        {
            Activity += Environment.NewLine + s;
        }

        private UserInfoViewModel FindUser(Guid id)
        {
            var users = from c in ChannelTreeViewModel.Channels
                        from u in c.Users
                        where u.Id == id
                        select u;

            return users.FirstOrDefault();
        }

        private List<UserInfoViewModel> GetAllUsers()
        {
            var users = from c in ChannelTreeViewModel.Channels
                        from u in c.Users
                        select u;
            return users.ToList();
        }

        private void AddUserToChannel(UserInfo userInfo)
        {
            var channel = ChannelTreeViewModel.Channels.FirstOrDefault(c => c.Id == userInfo.ChannelId);
            if (channel == null)
            {
                AddActivity($"Unable to find channel {channel.Id} to add the user to");
                return;
            }

            channel.Users.Add(new UserInfoViewModel(userInfo));
        }

        private void RemoveUserFromChannels(UserInfo userInfo)
        {
            var user = FindUser(userInfo.Id);
            if (user == null)
            {
                AddActivity($"Unable to find user {user.Username} from channel {user.ChannelId} in order to remove user");
                return;
            }

            foreach (var channel in ChannelTreeViewModel.Channels)
            {
                channel.Users.Remove(user);
            }
        }

        private ChannelViewModel GetSelectedChannel()
        {
            return ChannelTreeViewModel.Channels.FirstOrDefault(c => c.IsSelected);
        }

        private ChannelViewModel FindChannel(int id)
        {
            return ChannelTreeViewModel.Channels.FirstOrDefault(c => c.Id == id);
        }
    }
}
