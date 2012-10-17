using System.Windows.Controls;
using System.Windows.Input;

namespace DeskTube.Views
{
    /// <summary>
    /// Interaction logic for SubscriptionsView.xaml
    /// </summary>
    public partial class SubscriptionsView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionsView" /> class.
        /// </summary>
        public SubscriptionsView()
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
