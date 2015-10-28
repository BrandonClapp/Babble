using Babble.Core.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Client.ViewModels
{
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
    }
}
