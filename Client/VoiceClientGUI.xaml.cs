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
            InitializeComponent();
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

                    if(client.Connect(ncw.IPEndPoint)) 
                    {
                        AddActivity("Connected");
                    }
                    else MessageBox.Show("Could not connect to host.");
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
            Dispatcher.Invoke(() => { (this.UserAreaTree.Items[channel] as TreeViewItem).Items.Add(username); });
            // TODO: read from buffer and populate user/channel treeview
            //TreeView tv = this.UserAreaTree;

            //List<User> userList = new List<User>()
            //            {
            //                new User { Username = "Frank" },
            //                new User { Username = "Scott" },
            //                new User { Username = "Raef" }
            //            };

            //List<List<User>> channelList = new List<List<User>>() { userList };

            //foreach(List<User> uList in channelList)
            //{
            //    TreeViewItem channel = new TreeViewItem() { Header = "Daily Scrum" };
                
            //    channel.IsExpanded = true;
            //    foreach (User u in uList) channel.Items.Add(u.Username);
            //    tv.Items.Add(channel);
            //}
        }

        private void SomeUserDisconnected(string username, int channel)
        {
            Dispatcher.Invoke(() => { (this.UserAreaTree.Items[channel] as TreeViewItem).Items.Remove(username); });
        }

        private void HideContent()
        {
            this.UserAreaTree.IsEnabled = false;
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
