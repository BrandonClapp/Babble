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

namespace Client
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        private MyConfig config;

        public ConfigWindow(Window window, MyConfig config)
        {
            Owner = window;
            this.config = config;
            InitializeComponent();
            talkKeyText.Text = config.TalkKey.ToString();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            Key key;
            if (Enum.TryParse<Key>(talkKeyText.Text, out key))
            {
                config.TalkKey = key;
                config.Save();
            }

            DialogResult = true;
        }

        private void setDefaultTalkKeyButton_Click(object sender, RoutedEventArgs e)
        {
            talkKeyText.Text = MyConfig.DefaultTalkKey.ToString();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (setTalkKeyButton.IsChecked == true)
            {
                talkKeyText.Text = e.Key.ToString();
                e.Handled = true;
            }
        }

        private void setTalkKeyButton_Checked(object sender, RoutedEventArgs e)
        {
            if (setTalkKeyButton.IsChecked == true)
            {
                talkKeyHint.Visibility = Visibility.Visible;
            }
            else
            {
                talkKeyHint.Visibility = Visibility.Hidden;
            }
        }
    }
}
