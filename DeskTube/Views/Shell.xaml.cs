using System;
using System.Windows;
using DeskTube.ViewModels;
using Infrastructure;
using Infrastructure.Interfaces;

namespace DeskTube.Views
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    public partial class Shell : IView
    {
        #region CONSTRUCTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="Shell" /> class.
        /// </summary>
        public Shell()
        {
            InitializeComponent();

            this.ShellViewModel = new ShellViewModel { View = this };
            this.SetDataContext(this.ShellViewModel);
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Gets or sets the shell view model.
        /// </summary>
        /// <value>
        /// The shell view model.
        /// </value>
        public ShellViewModel ShellViewModel { get; set; }

        #endregion

        #region EVENT HANDLERS

        /// <summary>
        /// Called when [minimize window].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void OnMinimizeWindow(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        /// <summary>
        /// Called when [close window].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void OnCloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Called when [window mouse left button down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs" /> instance containing the event data.</param>
        private void OnWindowMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        #endregion

        #region IView members

        /// <summary>
        /// Sets the data context.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        public void SetDataContext(ViewModelBase dataContext)
        {
            this.DataContext = dataContext;
        }

        #endregion
    }
}
