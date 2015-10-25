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

namespace Client
{
    public partial class VoiceClientGUI : Window
    {
        VoiceClient client = new VoiceClient();

        public VoiceClientGUI()
        {
            //Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";lib");

            client.SomeUserConnected += SomeUserConnectedHandler;
            client.SomeUserDisconnected += SomeUserDisconnectedHandler;
            client.ChannelCreated += ChannelCreatedHandler;
            client.RefreshChannels += RefreshChannelsHandler;
            client.Connected += ConnectedHandler;
            client.Disconnected += DisconnectedHandler;

            
            InitializeComponent();
        }

        

        private void ConnectedHandler(bool successful, string message)
        {
            Dispatcher.Invoke(() =>
            {
                if (successful)
                {
                    AddActivity("Connected: Message From Host: " + message);
                }
                else
                {
                    MessageBox.Show("Could not connect to host. Error: " + message);
                }
            });
        }

        private void DisconnectedHandler()
        {
            Dispatcher.Invoke(() => {
                this.UserAreaTree.Items.Clear();
            });
        }

        private void NewConnection_Click(object sender, RoutedEventArgs e)
        {
            NewConnectionWindow ncw = new NewConnectionWindow(this);
            if (ncw.ShowDialog() == true) 
            {
                try
                {
                    Disconnect_Click(sender, e);
                    var host = ncw.Host;
                    var port = ncw.Port;
                    AddActivity("Attempting to connect to " + host + ":" + port + ".");
                    client.Owner = new WindowInteropHelper(this).Handle;
                    client.Connect(host, port, ncw.Username, ncw.Password);
                }
                catch
                {
                    AddActivity("Invalid IP or port.");
                }
            }
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
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            this.client.Disconnect();
            AddActivity("Disconnected");
        }
    }
}
