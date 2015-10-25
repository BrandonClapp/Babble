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

        //http://stackoverflow.com/questions/592373/select-treeview-node-on-right-click-before-displaying-contextmenu
        // for right click select tree item
        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null)
            {
                item.Focus();
                e.Handled = true;
            }
        }

        private void JoinChannel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var channel = UserAreaTree.SelectedItem;
                vm.JoinChannelCommand.Execute(channel);
            }
        }
    }
}
