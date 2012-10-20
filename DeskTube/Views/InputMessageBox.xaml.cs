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

namespace DeskTube.Views
{
    /// <summary>
    /// Interaction logic for InputMessageBox.xaml
    /// </summary>
    public partial class InputMessageBox : Window
    {
        #region EVENTS

        /// <summary>
        /// Occurs when [on input completed].
        /// </summary>
        public event EventHandler<string> InputCompleted;

        /// <summary>
        /// Occurs when [on cancel].
        /// </summary>
        public event EventHandler InputCancelled;

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="InputMessageBox" /> class.
        /// </summary>
        public InputMessageBox()
        {
            InitializeComponent();

            Keyboard.Focus(this.titleInput);
            this.titleInput.Focus();
        }
        
        #endregion

        #region EVENT HANDLERS

        /// <summary>
        /// Called when [ok click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            this.InputCompleted(this, this.titleInput.Text);
        }

        /// <summary>
        /// Called when [cancel click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            this.InputCancelled(this, EventArgs.Empty);
        }

        #endregion

    }
}
