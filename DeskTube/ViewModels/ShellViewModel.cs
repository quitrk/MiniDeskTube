using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using DeskTube.Views;
using Google.YouTube;
using Infrastructure;
using Infrastructure.Utilities;
using Microsoft.Practices.Prism.Commands;

namespace DeskTube.ViewModels
{
    public class ShellViewModel : ViewModelBase
    {
        #region EVENTS

        #endregion

        #region BACKING FIELDS

        /// <summary>
        /// StartupPageViewModel backing field.
        /// </summary>
        private StartupPageViewModel startupPageViewModel;

        /// <summary>
        /// MainPageViewModel backing field.
        /// </summary>
        private MainPageViewModel mainPageViewModel;

        #endregion

        #region PRIVATE FIELDS

        #endregion

        #region CONSTRUCTOR

        public ShellViewModel()
        {
            this.LogoutCommand = new DelegateCommand(this.HandleLogoutCommand);
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Gets the startup page view model.
        /// </summary>
        /// <value>
        /// The startup page view model.
        /// </value>
        public StartupPageViewModel StartupPageViewModel
        {
            get
            {
                if (this.startupPageViewModel == null)
                {
                    this.startupPageViewModel = new StartupPageViewModel();
                    this.startupPageViewModel.ResolveView();
                    this.startupPageViewModel.StartupPageCompleted += this.OnStartupPageCompleted;
                    this.startupPageViewModel.PopulateData();
                }

                return this.startupPageViewModel;
            }
        }

        /// <summary>
        /// Gets the main page view model.
        /// </summary>
        /// <value>
        /// The main page view model.
        /// </value>
        public MainPageViewModel MainPageViewModel
        {
            get
            {
                return this.mainPageViewModel;
            }
        }

        #endregion

        #region COMMANDS

        /// <summary>
        /// Gets or sets the logout command.
        /// </summary>
        /// <value>
        /// The logout command.
        /// </value>
        public DelegateCommand LogoutCommand { get; set; }

        #endregion

        #region COMMAND HANDLERS

        /// <summary>
        /// Handles the logout command.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void HandleLogoutCommand()
        {
            if (this.mainPageViewModel != null)
            {
                this.mainPageViewModel.Dispose();
                this.mainPageViewModel = null;
            }

            ((Shell)this.View).LocationChanged -= OnShellLocationChanged;
            ((Shell)this.View).Activated -= this.OnShellActivated;
            ((Shell)this.View).Deactivated -= this.OnShellDeactived;

            var storyboard = ((Storyboard)((Shell)this.View).FindResource("ShowStartupPage"));
            storyboard.Begin();
        }

        #endregion

        #region EVENT HANDLERS

        /// <summary>
        /// Called when [startup page completed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void OnStartupPageCompleted(object sender, Tuple<bool, YouTubeRequest> e)
        {
            var youtubeRequest = e.Item2;

            this.mainPageViewModel = new MainPageViewModel();
            this.mainPageViewModel.ResolveView();
            this.mainPageViewModel.PopulateData(youtubeRequest);
            this.OnPropertyChanged(() => MainPageViewModel);
            
            ((Shell)this.View).LocationChanged += OnShellLocationChanged;
            ((Shell) this.View).Activated += this.OnShellActivated;
            ((Shell) this.View).Deactivated += this.OnShellDeactived;

            var storyboard = ((Storyboard)((Shell)this.View).FindResource("ShowMainPage"));
            storyboard.Begin();
        }

        /// <summary>
        /// Called when [shell activated].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void OnShellActivated(object sender, EventArgs e)
        {
            this.MainPageViewModel.ActivateOverlay();
        }

        /// <summary>
        /// Called when [shell deactived].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void OnShellDeactived(object sender, EventArgs e)
        {
            this.MainPageViewModel.DeactivateOverlay();
        }

        /// <summary>
        /// Called when [shell location changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void OnShellLocationChanged(object sender, EventArgs e)
        {
            this.MainPageViewModel.DeactivateOverlay();
            this.MainPageViewModel.ActivateOverlay();
        }

        #endregion

        #region ViewModelBase

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="all"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool all)
        {
            this.mainPageViewModel.Dispose();
            this.startupPageViewModel.Dispose();

            ((Shell)this.View).LocationChanged -= OnShellLocationChanged;
            ((Shell)this.View).Activated -= this.OnShellActivated;
            ((Shell)this.View).Deactivated -= this.OnShellDeactived;

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public sealed override void Dispose()
        {
            this.Dispose(true);
        }

        #endregion
    }
}
