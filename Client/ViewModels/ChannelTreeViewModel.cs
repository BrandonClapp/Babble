using Babble.Core;
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

        private ChannelViewModel _SelectedChannel;
        public ChannelViewModel SelectedChannel
        {
            get { return _SelectedChannel; }
            set { _SelectedChannel = value;OnPropertyChanged(nameof(SelectedChannel)); }
        }
    }
}
