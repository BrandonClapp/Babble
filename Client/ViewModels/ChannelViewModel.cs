﻿using Babble.Core;
using System.Collections.ObjectModel;

namespace Client.ViewModels
{
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
        public string Name { get { return channel.Name; } set { channel.Name = value; OnPropertyChanged(nameof(Name)); } }

        public ObservableCollection<UserInfoViewModel> Users { get; private set; }

        private bool _IsSelected;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set { _IsSelected = value;OnPropertyChanged(nameof(IsSelected)); }
        }
    }
}
