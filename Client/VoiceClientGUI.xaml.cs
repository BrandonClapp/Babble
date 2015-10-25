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
using System.Windows.Input;

namespace Client
{
    /// <summary>
    /// Provides access to UI elements and binding to the VoiceClientViewModel
    /// </summary>
    public partial class VoiceClientGUI : Window
    {
        VoiceClientViewModel vm = new VoiceClientViewModel();

        public VoiceClientGUI()
        {
            InitializeComponent();

            this.DataContext = vm;
        }

        private void ActivityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //http://stackoverflow.com/questions/1895204/textbox-scrolltoend-doesnt-work-when-the-textbox-is-in-a-non-active-tab
            // microsoft scrolling issue that when you focus on the textbox, scroll up,
            // it's no longer scroll to end even when u call the function
            // use the workaround
            ActivityTextBox.CaretIndex = ActivityTextBox.Text.Length;
            ActivityTextBox.ScrollToEnd();
        }

        private void JoinChannel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                vm.JoinChannelCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void TreeItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var uiElement = sender as FrameworkElement;
            if (uiElement == null) { return; }
            var channelVM = uiElement.DataContext as ChannelViewModel;
            if (channelVM != null)
            {
                channelVM.IsSelected = true;
                return;
            }
            var userVM = uiElement.DataContext as UserInfoViewModel;
            if (userVM != null)
            {
                userVM.IsSelected = true;
            }
        }
    }
}
