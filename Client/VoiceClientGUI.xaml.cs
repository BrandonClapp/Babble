using System.Windows;
using System.Net;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System;
using System.Windows.Interop;
using Babble.Core;
using System.Linq;
using Client.ViewModels;

namespace Client
{
    public partial class VoiceClientGUI : Window
    {
        public VoiceClientGUI()
        {
            InitializeComponent();

            this.DataContext = new VoiceClientViewModel();
        }

        private void ActivityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            http://stackoverflow.com/questions/1895204/textbox-scrolltoend-doesnt-work-when-the-textbox-is-in-a-non-active-tab
            // microsoft scrolling issue that when you focus on the textbox, scroll up,
            // it's no longer scroll to end even when u call the function
            // use the workaround
            ActivityTextBox.CaretIndex = ActivityTextBox.Text.Length;
            ActivityTextBox.ScrollToEnd();
        }
    }
}
