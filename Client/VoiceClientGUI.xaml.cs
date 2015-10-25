using System.Windows;
using System.Net;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System;
using System.Windows.Interop;
using Babble.Core;
using System.Linq;
using Client.ViewModels;

namespace Client
{
    public partial class VoiceClientGUI : Window
    {
        public VoiceClientGUI()
        {
            InitializeComponent();
        }

        private void ChannelCreatedHandler(string name, int id)
        {
            Dispatcher.Invoke(() => {
                this.UserAreaTree.Items.Insert(id, new TreeViewItem { IsExpanded = true, Header = name });
                AddActivity("Channel " + id + " created");
            });
        }

        private void RefreshChannelsHandler(List<Channel> obj)
        {
            Dispatcher.Invoke(() => {
                this.UserAreaTree.Items.Clear();
                foreach (var channel in obj)
                {
                    var channelTreeItem = new TreeViewItem();
                    channelTreeItem.IsExpanded = true;
                    channelTreeItem.Header = channel.Id + " : " + channel.Name;
                    channelTreeItem.Tag = channel.Id;
                    if (channel.Users != null && channel.Users.Any())
                    {
                        foreach (var user in channel.Users)
                        {
                            var userTreeItem = new TreeViewItem();
                            userTreeItem.Header = user.Username;
                            channelTreeItem.Items.Add(userTreeItem);
                        }
                    }

                    channelTreeItem.MouseDoubleClick += ChannelTreeItem_MouseDoubleClick;
                    this.UserAreaTree.Items.Add(channelTreeItem);
                }
                AddActivity("Ain't nobody dope as me I'm dressed so fresh so clean");
            });
        }

        private void ChannelTreeItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var channel = sender as TreeViewItem;
            var channelId = channel.Tag;
            AddActivity("Double clicked channel " + channelId);
            client.WriteMessage(Message.Create(MessageType.UserChangeChannelRequest, channelId));
        }

        private void SomeUserConnectedHandler(string username, int channel)
        {
            Dispatcher.Invoke(() => {
                (this.UserAreaTree.Items[channel] as TreeViewItem).Items.Add(username);
                AddActivity(string.Format("{0} Connected", username));
            });
        }

        private void SomeUserDisconnectedHandler(string username, int channel)
        {
            Dispatcher.Invoke(() => { 
                (this.UserAreaTree.Items[channel] as TreeViewItem).Items.Remove(username);
                AddActivity(string.Format("{0} Disconnected", username));

            });
        }

        public void AddActivity(string s)
        {
            ActivityTextBox.Text += "\n" + s;
            this.DataContext = new VoiceClientViewModel();
        }

        private void ActivityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            http://stackoverflow.com/questions/1895204/textbox-scrolltoend-doesnt-work-when-the-textbox-is-in-a-non-active-tab
            // microsoft scrolling issue that when you focus on the textbox, scroll up,
            // it's no longer scroll to end even when u call the function
            // use the workaround
            ActivityTextBox.CaretIndex = ActivityTextBox.Text.Length;
            ActivityTextBox.ScrollToEnd();
        }
    }
}
