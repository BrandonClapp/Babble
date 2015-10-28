using Babble.Core.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Client.ViewModels
{
    class ChannelTreeViewModel : ViewModelBase
    {
        public ChannelTreeViewModel(List<ChannelSession> channelSessions)
        {
            Channels = new ObservableCollection<ChannelViewModel>();

            foreach (var channelSession in channelSessions)
            {
                var channelVM = new ChannelViewModel(channelSession);
                Channels.Add(channelVM);
            }
        }

        public ObservableCollection<ChannelViewModel> Channels { get; private set; }
    }
}
