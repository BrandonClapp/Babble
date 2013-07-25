using System.Windows;
using System.Net;

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
                    if(user.Connect(ncw.IPEndPoint)) AddActivity("Connected");
                    else MessageBox.Show("Could not connect to host.");
                }
                catch
                {
                    AddActivity("Invalid IP or port.");
                }
            }
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
}
