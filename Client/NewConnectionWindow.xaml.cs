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

        public NewConnectionWindow()
        {
            InitializeComponent();
            this.Loaded += NewConnectionWindow_Loaded;
            this.WindowStartupLocation = WindowStartupLocation.Manual;
        }

        void NewConnectionWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            var mainWindow = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;
            this.Top = mainWindow.Top + (mainWindow.Height / 2) / 2;
            this.Left = mainWindow.Left + (mainWindow.Width / 2 ) / 2;
        }
        //MainWindow main = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;

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
