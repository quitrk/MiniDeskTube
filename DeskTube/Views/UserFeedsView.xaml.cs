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
        #region CONSTRUCTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="UserFeedsView" /> class.
        /// </summary>
        public UserFeedsView()
        {
            InitializeComponent();
        }

        #endregion

        #region EVENT HANDLERS

        /// <summary>
        /// Called when [user feed mouse down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void OnUserFeedMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = (ListBoxItem)sender;

            if (item.Content.Equals("playlists"))
            {
                item.ContextMenu = this.Resources["PlaylistsMenu"] as ContextMenu;

                item.ContextMenu.IsOpen = true;
                item.ContextMenu.PlacementTarget = item;

                e.Handled = true;
            }
            else if (item.Content.Equals("subscriptions"))
            {
                item.ContextMenu = this.Resources["SubscriptionsMenu"] as ContextMenu;

                item.ContextMenu.IsOpen = true;
                item.ContextMenu.PlacementTarget = item;

                e.Handled = true;
            }
        }

        /// <summary>
        /// Called when [settings mouse down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void OnManageMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.manageBtn.ContextMenu = this.Resources["ManageMenu"] as ContextMenu;

            this.manageBtn.ContextMenu.IsOpen = true;
            this.manageBtn.ContextMenu.PlacementTarget = this.manageBtn;

            e.Handled = true;
        }

        #endregion
    }
}
