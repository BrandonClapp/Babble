using Babble.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.ViewModels
{
    // base viewmodel for supporting property notification
    // so that when you change the property value on the back end, 
    // auto magically update the UI as well
    abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                var prop = PropertyChanged;
                prop.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    class ChannelTreeViewModel : ViewModelBase
    {
        public ChannelTreeViewModel(List<Channel> channels)
        {
            Channels = new ObservableCollection<ChannelViewModel>();

            foreach (var channel in channels)
            {
                var channelVM = new ChannelViewModel(channel);
                Channels.Add(channelVM);
            }
        }

        public ObservableCollection<ChannelViewModel> Channels { get; private set; }

        private ChannelViewModel _SelectedChannel;
        public ChannelViewModel SelectedChannel
        {
            get { return _SelectedChannel; }
            set { _SelectedChannel = value;OnPropertyChanged(nameof(SelectedChannel)); }
        }
    }

    class ChannelViewModel : ViewModelBase
    {
        private Channel channel;

        public ChannelViewModel(Channel channel)
        {
            this.channel = channel;
            Users = new ObservableCollection<UserInfoViewModel>();

            foreach (var user in channel.Users)
            {
                var userVM = new UserInfoViewModel(user);
                Users.Add(userVM);
            }
        }

        public int Id { get { return channel.Id; } }
        public string Name { get { return channel.Name; } }

        public ObservableCollection<UserInfoViewModel> Users { get; private set; }

    }

    class UserInfoViewModel : ViewModelBase
    {
        private UserInfo userInfo;

        public UserInfoViewModel(UserInfo userInfo)
        {
            this.userInfo = userInfo;
        }

        public string Username { get { return userInfo.Username; } }
        public int ChannelId { get { return userInfo.ChannelId; } }

        private static readonly TimeSpan TalkingStopDelay = new TimeSpan(0, 0, 0, 0, 300);
        private bool _IsTalking;
        public bool IsTalking
        {
            get { return _IsTalking; }
            set {
                _IsTalking = value;
                OnPropertyChanged(nameof(IsTalking));
            }
        }

        public DateTime BeginTalkingTime { get; set; } = DateTime.MinValue;

        public void UpdateTimedBaseProperty()
        {
            if (IsTalking && (DateTime.Now - BeginTalkingTime) > TalkingStopDelay)
            {
                IsTalking = false;
            }
        }
    }
}
