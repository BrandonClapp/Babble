using System.Windows;
using System.Net;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System;
using System.Windows.Interop;

namespace Client
{
    public partial class VoiceClientGUI : Window
    {
        VoiceClient client = new VoiceClient();

        public VoiceClientGUI()
        {
            Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";lib");

            client.SomeUserConnected += SomeUserConnectedHandler;
            client.SomeUserDisconnected += SomeUserDisconnectedHandler;
            client.ChannelCreated += ChannelCreatedHandler;
            client.Connected += ConnectedHandler;
            client.Disconnected += DisconnectedHandler;
            
            InitializeComponent();
        }

        private void ConnectedHandler(bool successful)
        {
            Dispatcher.Invoke(() =>
            {
                if (successful) AddActivity("Connected");
                else MessageBox.Show("Could not connect to host.");
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
                    client.User.Username = ncw.Username;
                    client.User.Password = ncw.Password;
                    client.Owner = new WindowInteropHelper(this).Handle;
                    client.Connect(host, port);
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
            });
        }

        private void SomeUserConnectedHandler(string username, int channel)
        {
            Dispatcher.Invoke(() => {
                (this.UserAreaTree.Items[channel] as TreeViewItem).Items.Add(username);
            });
        }

        private void SomeUserDisconnectedHandler(string username, int channel)
        {
            Dispatcher.Invoke(() => { 
                (this.UserAreaTree.Items[channel] as TreeViewItem).Items.Remove(username); 
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
