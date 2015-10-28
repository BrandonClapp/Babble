using Babble.Core.Objects;
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
        // periodic update GUI property that is timer based
        // so one of them is the talking status
        // if the user stopped talking for couple seconds, considered they're done, so update the UI
        System.Timers.Timer periodicUpdateTimer = new System.Timers.Timer(700);

        Dictionary<MessageType, Action<Message>> messageHandlers = new Dictionary<MessageType, Action<Message>>();
  
        public VoiceClientViewModel()
        {
            client.Connected += Client_Connected;
            client.Disconnected += Client_Disconnected;
            client.MessageReceived += Client_MessageReceived; ;

            periodicUpdateTimer.Elapsed += PeriodicUpdateTimer_Elapsed;

            ConnectCommand = new DelegateCommand(ConnectCommandHandler);
            DisconnectCommand = new DelegateCommand(DisconnectCommandHandler);
            JoinChannelCommand = new DelegateCommand(JoinChannelCommandHandler);
            CreateChannelCommand = new DelegateCommand(CreateChannelCommandHandler);
            RenameChannelCommand = new DelegateCommand(RenameChannelCommandHandler);
            DeleteChannelCommand = new DelegateCommand(DeleteChannelCommandHandler);
            SendChatMessageCommand = new DelegateCommand(SendChatMessageCommandHandler);

            messageHandlers.Add(MessageType.Chat, SomeUserChattingHandler);
            messageHandlers.Add(MessageType.Voice, SomeUserTalkingHandler);
            messageHandlers.Add(MessageType.UserConnected, SomeUserConnectedHandler);
            messageHandlers.Add(MessageType.UserDisconnected, SomeUserDisconnectedHandler);
            messageHandlers.Add(MessageType.GetAllChannelsResponse, RefreshChannelsHandler);
            messageHandlers.Add(MessageType.CreateChannelResponse, ChannelCreatedHandler);
            messageHandlers.Add(MessageType.RenameChannelResponse, ChannelRenamedHandler);
            messageHandlers.Add(MessageType.DeleteChannelResponse, ChannelDeletedHandler);
            messageHandlers.Add(MessageType.UserChangeChannelResponse, SomeUserChangedChannelHandler);
        }

        // EVENT handling here

        private void Client_Connected(bool successful, string responseMessage)
        {
            if (successful)
            {
                OnPropertyChanged(nameof(IsConnected));
                periodicUpdateTimer.Start();
                AddActivity("Connected: Message From Host: {0}", responseMessage);
            }
            else
            {
                System.Windows.MessageBox.Show("Could not connect to host. Error: {0}", responseMessage);
            }
        }

        private void Client_Disconnected()
        {
            dispatcher.Invoke(() =>
            {
                ChannelTreeViewModel = null;
                OnPropertyChanged(nameof(IsConnected));
                AddActivity("Disconnected");
                periodicUpdateTimer.Stop();
            });
        }

        private void Client_MessageReceived(Message message)
        {
            Action<Message> handler = null;
            messageHandlers.TryGetValue(message.Type, out handler);
            if (handler == null)
            {
                AddActivity("Error, unable to find a message handler for message type: {0}", message.Type);
                return;
            }

            dispatcher.Invoke(() =>
            {
                handler(message);
            });
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

        public UserSession UserSession { get { return client.UserSession; } }

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

        public bool IsConnected
        {
            get { return client.IsConnected; }
        }

        private string _ChatMessage;
        public string ChatMessage
        {
            get { return _ChatMessage; }
            set { _ChatMessage = value; OnPropertyChanged(nameof(ChatMessage)); }
        }

        public ICommand SendChatMessageCommand { get; set; }
        private void SendChatMessageCommandHandler(object state)
        {
            if (string.IsNullOrWhiteSpace(ChatMessage))
            {
                ChatMessage = string.Empty;
                return;
            }

            client.WriteMessage(Message.Create(MessageType.Chat, new ChatData() { UserSession = UserSession, Data = ChatMessage }));
            ChatMessage = string.Empty;
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
                    AddActivity("Attempting to connect to {0} : {1}", host, port);
                    client.Connect(host, port, ncw.Username, ncw.Password);
                }
                catch (Exception ex)
                {
                    AddActivity(ex.Message);
                }
            }
        }

        public ICommand DisconnectCommand { get; private set; }
        private void DisconnectCommandHandler(object state)
        {
            this.client.Disconnect();
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

        private void ChannelCreatedHandler(Message message)
        {
            var channelSession = message.GetData<ChannelSession>();
            ChannelTreeViewModel.Channels.Add(new ChannelViewModel(channelSession));
            AddActivity("Channel {0} : {1} created", channelSession.Channel.Id, channelSession.Channel.Name);
        }

        private void ChannelRenamedHandler(Message message)
        {
            var channel = message.GetData<Channel>();
            var channelVM = FindChannel(channel.Id);
            channelVM.Name = channel.Name;
            AddActivity("Channel {0} : {1} renamed", channel.Id, channel.Name);
        }

        private void ChannelDeletedHandler(Message message)
        {
            var channel = message.GetData<Channel>();
            var channelVM = FindChannel(channel.Id);
            ChannelTreeViewModel.Channels.Remove(channelVM);
            AddActivity("Channel {0} : {1} deleted", channel.Id, channel.Name);
        }

        private void RefreshChannelsHandler(Message message)
        {
            var channels = message.GetData<List<ChannelSession>>();
            ChannelTreeViewModel = new ChannelTreeViewModel(channels);
            AddActivity("Ain't nobody dope as me I'm dressed so fresh so clean");
        }

        private void SomeUserConnectedHandler(Message message)
        {
            var userSession = message.GetData<UserSession>();

            var channel = ChannelTreeViewModel.Channels.FirstOrDefault(c => c.Id == userSession.ChannelId);
            if (channel != null)
            {
                channel.Users.Add(new UserInfoViewModel(userSession));
            }
            AddActivity(string.Format("{0} Connected", userSession.UserInfo.Username));
        }

        private void SomeUserDisconnectedHandler(Message message)
        {
            var user = message.GetData<UserSession>();
            foreach (var channel in ChannelTreeViewModel.Channels)
            {
                channel.Users.Remove(channel.Users.FirstOrDefault(u => u.ConnectionId == user.ConnectionId));
            }
            AddActivity(string.Format("{0} Disconnected", user.UserInfo.Username));
        }

        private void SomeUserTalkingHandler(Message message)
        {
            var voiceData = message.GetData<VoiceData>();
            client.PlaySound(voiceData.GetDataInBytes());
            var userSession = voiceData.UserSession;
            var userVM = FindUser(userSession.ConnectionId);
            if (userVM == null)
            {
                return;
            }

            userVM.BeginTalkingTime = DateTime.Now;
            userVM.IsTalking = true;
        }

        private void SomeUserChattingHandler(Message message)
        {
            var chatData = message.GetData<ChatData>();
            AddActivity("{0}: {1}", chatData.UserSession.UserInfo.Username, chatData.Data);
        }

        private void SomeUserChangedChannelHandler(Message message)
        {
            var userSession = message.GetData<UserSession>();
            RemoveUserFromChannels(userSession);
            AddUserToChannel(userSession);
        }

        public void AddActivity(string s, params object[] args)
        {
            Activity += Environment.NewLine;
            Activity += string.Format(s, args);
        }

        private UserInfoViewModel FindUser(Guid connectionId)
        {
            var users = from c in ChannelTreeViewModel.Channels
                        from u in c.Users
                        where u.ConnectionId == connectionId
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

        private void AddUserToChannel(UserSession userSession)
        {
            var channel = ChannelTreeViewModel.Channels.FirstOrDefault(c => c.Id == userSession.ChannelId);
            if (channel == null)
            {
                AddActivity("Unable to find channel {0} to add the user to", channel.Id);
                return;
            }

            channel.Users.Add(new UserInfoViewModel(userSession));
        }

        private void RemoveUserFromChannels(UserSession userSession)
        {
            var user = FindUser(userSession.ConnectionId);
            if (user == null)
            {
                AddActivity("Unable to find user {0} from channel {1} in order to remove user", userSession.UserInfo.Username, userSession.ChannelId);
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
