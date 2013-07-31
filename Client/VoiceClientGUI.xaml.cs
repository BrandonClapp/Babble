using System.Windows;
using System.Net;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Client
{
    public partial class VoiceClientGUI : Window
    {
        VoiceClient client = new VoiceClient();

        public VoiceClientGUI()
        {
            client.SomeUserConnected += SomeUserConnected;
            client.SomeUserDisconnected += SomeUserDisconnected;
            client.ChannelCreated += ChannelCreated;
            client.Connected += Connected;
            client.Disconnected += Disconnected;
            InitializeComponent();
        }

        private void Connected(bool successful)
        {
            Dispatcher.Invoke(() =>
            {
                if (successful) AddActivity("Connected");
                else MessageBox.Show("Could not connect to host.");
            });
        }

        private void Disconnected()
        {
            Dispatcher.Invoke(() => {
                this.UserAreaTree.Items.Clear();
            });
        }

        private void NewConnection_Click(object sender, RoutedEventArgs e)
        {
            // open new window for input information
            NewConnectionWindow ncw = new NewConnectionWindow(this);
            if (ncw.ShowDialog() == true) 
            {
                try
                {
                    Disconnect_Click(sender, e);
                    IPEndPoint endpoint = ncw.IPEndPoint;
                    AddActivity("Attempting to connect to " + endpoint.Address + ":" + endpoint.Port + ".");
                    client.User.Username = ncw.Username;
                    client.User.Password = ncw.Password;
                    client.Connect(ncw.IPEndPoint);
                }
                catch
                {
                    AddActivity("Invalid IP or port.");
                }
            }
        }

        private void ChannelCreated(string name, int id)
        {
            Dispatcher.Invoke(() => {
                this.UserAreaTree.Items.Insert(id, new TreeViewItem { IsExpanded = true, Header = name }); 
            });
        }

        private void SomeUserConnected(string username, int channel)
        {
            Dispatcher.Invoke(() => { 
                (this.UserAreaTree.Items[channel] as TreeViewItem).Items.Add(username);
            });
        }

        private void SomeUserDisconnected(string username, int channel)
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
