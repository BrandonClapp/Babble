using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for NewConnectionWindow.xaml
    /// </summary>
    public partial class NewConnectionWindow : Window
    {
        MainWindow main = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;

        public NewConnectionWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            main.AddActivity("Attempting to connect to " + HostTextBox.Text + ":" + PortTextBox.Text + ".");

            try
            {
                TcpClient client = new TcpClient(HostTextBox.Text, Convert.ToInt32(PortTextBox.Text));
                // play some cool robotic connected/success sound
                main.AddActivity("Connected");
            }
            catch
            {
                // play some cool robotic error sound
                MessageBox.Show("Could not connect to host.");
                main.AddActivity("Unable to connect to " + HostTextBox.Text + ":" + PortTextBox.Text + ".");
            }

            this.NewConnectionWindow1.Close();
        }
    }
}
