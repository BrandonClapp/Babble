using Babble.Core.Objects;
using System.Collections.ObjectModel;

namespace Client.ViewModels
{
    class ChannelViewModel : ViewModelBase
    {
        private ChannelSession channelSession;

        public ChannelViewModel(ChannelSession channelSession)
        {
            this.channelSession = channelSession;
            Users = new ObservableCollection<UserInfoViewModel>();

            foreach (var userSession in channelSession.UserSessions)
            {
                var userVM = new UserInfoViewModel(userSession);
                Users.Add(userVM);
            }
        }

        public int Id { get { return channelSession.Channel.Id; } }
        public string Name { get { return channelSession.Channel.Name; } set { channelSession.Channel.Name = value; OnPropertyChanged(nameof(Name)); } }

        public ObservableCollection<UserInfoViewModel> Users { get; private set; }

        private bool _IsSelected;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set { _IsSelected = value;OnPropertyChanged(nameof(IsSelected)); }
        }
    }
}
