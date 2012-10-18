using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DeskTube.Views
{
    /// <summary>
    /// Interaction logic for UserFeedsView.xaml
    /// </summary>
    public partial class UserFeedsView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserFeedsView" /> class.
        /// </summary>
        public UserFeedsView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called when [user feed mouse down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void OnUserFeedMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = (ListBoxItem) sender;

            if(item.Content.Equals("playlists"))
            {
                item.ContextMenu = this.Resources["PlaylistsMenu"] as ContextMenu;

                item.ContextMenu.IsOpen = true;
                item.ContextMenu.PlacementTarget = item;
            }
            else if(item.Content.Equals("subscriptions"))
            {
                item.ContextMenu = this.Resources["SubscriptionsMenu"] as ContextMenu;

                item.ContextMenu.IsOpen = true;
                item.ContextMenu.PlacementTarget = item;
            }
        }
        
        /// <summary>
        /// Called when [list box item preview mouse right button down]. Disables right click item selection.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void OnListBoxItemPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = (ListBoxItem)sender;

            if (item.Content.Equals("playlists"))
            {
                item.ContextMenu = this.Resources["PlaylistsContextMenu"] as ContextMenu;

                item.ContextMenu.IsOpen = true;
                item.ContextMenu.PlacementTarget = item;
            }
            else if (item.Content.Equals("subscriptions"))
            {
                item.ContextMenu = this.Resources["SubscriptionsContextMenu"] as ContextMenu;

                item.ContextMenu.IsOpen = true;
                item.ContextMenu.PlacementTarget = item;
            }

            e.Handled = true;
        }
    }
}
