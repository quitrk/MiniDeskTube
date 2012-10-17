using System.Windows.Controls;
using System.Windows.Input;

namespace DeskTube.Views
{
    /// <summary>
    /// Interaction logic for PlaylistsView.xaml
    /// </summary>
    public partial class PlaylistsView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlaylistsView" /> class.
        /// </summary>
        public PlaylistsView()
        {
            InitializeComponent();
        }

        #region EVENT HANDLERS

        /// <summary>
        /// Called when [list box item preview mouse right button down]. Disables right click item selection.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void OnListBoxItemPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        #endregion
    }
}
