using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Client.Views
{
    /// <summary>
    /// Interaction logic for NewChannelWindow.xaml
    /// </summary>
    public partial class NewChannelWindow : Window
    {
        public NewChannelWindow(Window window)
        {
            this.Owner = window;

            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ChannelNameTextBox.Text))
            {
                MessageBox.Show("Channel Name cannot be empty", "Error", MessageBoxButton.OK);
                return;
            }

            DialogResult = true;
        }
    }
}
