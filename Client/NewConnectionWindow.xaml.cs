using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net;
using System.Windows;
using System.Diagnostics;

namespace Client
{
    /// <summary>
    /// Interaction logic for NewConnectionWindow.xaml
    /// </summary>
    public partial class NewConnectionWindow : Window
    {
        public IPEndPoint IPEndPoint { get { return new IPEndPoint(IPAddress.Parse(HostTextBox.Text), int.Parse(PortTextBox.Text)); } }
        public string Username { get { return UsernameTextBox.Text; } }
        public string Password { get { return PasswordBox.Password; } }

        public NewConnectionWindow(Window window)
        {
            this.Owner = window;
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void LoginAsAnonymous_Click(object sender, RoutedEventArgs e)
        {
            this.UsernameTextBox.IsEnabled = !LoginAsAnonymousCheckBox.IsChecked.Value;
            this.PasswordBox.IsEnabled = !LoginAsAnonymousCheckBox.IsChecked.Value;
        }
    }
}
