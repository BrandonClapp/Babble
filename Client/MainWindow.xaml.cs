using System.Windows;
using System.Net;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Client
{
    public partial class MainWindow : Window
    {
        User user = new User();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void NewConnection_Click(object sender, RoutedEventArgs e)
        {
            // open new window for input information
            NewConnectionWindow ncw = new NewConnectionWindow();
            if (ncw.ShowDialog() == true) 
            {
                try
                {
                    Disconnect_Click(sender, e);
                    IPEndPoint endpoint = ncw.IPEndPoint;
                    AddActivity("Attempting to connect to " + endpoint.Address + ":" + endpoint.Port + ".");
                    if(user.Connect(ncw.IPEndPoint)) 
                    {
                        AddActivity("Connected");
                        ShowConnectedContent();
                    }
                    else MessageBox.Show("Could not connect to host.");
                }
                catch
                {
                    AddActivity("Invalid IP or port.");
                }
            }
        }

        private void ShowConnectedContent() // parameter - some kind of message form... buffer, json, dynamic object...
        {
            // TODO: read from buffer and populate user/channel treeview
            TreeView tv = this.UserAreaTree;

            List<User> userList = new List<User>()
                        {
                            new User { Username = "Frank" },
                            new User { Username = "Scott" },
                            new User { Username = "Raef" }
                        };

            List<List<User>> channelList = new List<List<User>>() { userList };

            foreach(List<User> uList in channelList)
            {
                TreeViewItem channel = new TreeViewItem() { Header = "Daily Scrum" };
                channel.IsExpanded = true;
                foreach (User u in uList) channel.Items.Add(u.Username);
                tv.Items.Add(channel);
            }
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
            this.user.Disconnect();
            AddActivity("Disconnected");
        }
    }

    public class Channel
    {
        public string Name { get; set; }
    }
}
