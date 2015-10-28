using Babble.Core.Objects;
using System;

namespace Client.ViewModels
{
    class UserInfoViewModel : ViewModelBase
    {
        UserSession userSession;

        public UserInfoViewModel(UserSession userSession)
        {
            this.userSession = userSession;
        }

        public int Id { get { return userSession.UserInfo.Id; } }
        public Guid ConnectionId { get { return userSession.ConnectionId; } }
        public string Username { get { return userSession.UserInfo.Username; } }
        public int ChannelId { get { return userSession.ChannelId; } }

        private static readonly TimeSpan TalkingStopDelay = new TimeSpan(0, 0, 0, 0, 300);
        private bool _IsTalking;
        public bool IsTalking
        {
            get { return _IsTalking; }
            set
            {
                _IsTalking = value;
                OnPropertyChanged(nameof(IsTalking));
            }
        }

        private bool _IsSelected;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set { _IsSelected = value; OnPropertyChanged(nameof(IsSelected)); }
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
