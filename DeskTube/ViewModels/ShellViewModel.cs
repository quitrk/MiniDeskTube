﻿using System;
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
using Google.GData.YouTube;
using Google.GData.Extensions;

namespace DeskTube.ViewModels
{
    public class ShellViewModel : ViewModelBase
    {
        #region EVENTS

        #endregion

        #region BACKING FIELDS

        /// <summary>
        /// Backing field for UserName
        /// </summary>
        private string userName;

        /// <summary>
        /// Backing field for UserThumbnail
        /// </summary>
        private string userThumbnail;

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
            this.SignInCommand = new DelegateCommand(this.HandleSignInCommand);
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
                    this.startupPageViewModel.UserLoggedIn += this.OnUserLoggedIn;
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

        /// <summary>
        /// Gets or sets the userThumbnail.
        /// </summary>
        /// <value>The userThumbnail.</value>
        /// <remarks></remarks>
        public string UserThumbnail
        {
            get
            {
                return this.userThumbnail;
            }

            set
            {
                this.userThumbnail = value;
                this.OnPropertyChanged(() => this.UserThumbnail);
            }
        }

        /// <summary>
        /// Gets or sets the userName.
        /// </summary>
        /// <value>The userName.</value>
        /// <remarks></remarks>
        public string UserName
        {
            get
            {
                return this.userName;
            }

            set
            {
                this.userName = value;
                this.OnPropertyChanged(() => this.UserName);
            }
        }

        #endregion

        #region COMMANDS

        /// <summary>
        /// Gets or sets the sign in command.
        /// </summary>
        /// <value>
        /// The sign in command.
        /// </value>
        public DelegateCommand SignInCommand { get; set; }

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
            this.UserThumbnail = null;
            this.UserName = null;

            if (this.MainPageViewModel != null)
            {
                this.MainPageViewModel.Dispose();
                this.mainPageViewModel = null;
                this.OnPropertyChanged(() => this.MainPageViewModel);
            }

            var storyboard = ((Storyboard)((Shell)this.View).FindResource("ShowStartupPage"));
            storyboard.Begin();
        }

        /// <summary>
        /// Handles the sign in command.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void HandleSignInCommand()
        {
            if (this.MainPageViewModel != null)
            {
                this.MainPageViewModel.Dispose();
                this.mainPageViewModel = null;
                this.OnPropertyChanged(() => this.MainPageViewModel);
            }

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
            this.mainPageViewModel.WindowHost = (Window)this.View;
            this.OnPropertyChanged(() => MainPageViewModel);

            var storyboard = ((Storyboard)((Shell)this.View).FindResource("ShowMainPage"));
            storyboard.Begin();
        }

        /// <summary>
        /// Called when [user logged in].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="request">The request.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void OnUserLoggedIn(object sender, YouTubeRequest request)
        {
            var profile = (ProfileEntry)request.Service.Get("http://gdata.youtube.com/feeds/api/users/default");
            var thumbnail = (from e in profile.ExtensionElements where e.XmlName == "thumbnail" select (XmlExtension)e).SingleOrDefault();

            if (thumbnail != null && thumbnail.Node.Attributes != null)
            {
                this.UserThumbnail = thumbnail.Node.Attributes["url"].Value;
            }

            this.UserName = request.Settings.Credentials.Username;
        }

        #endregion

        #region ViewModelBase

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="all"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool all)
        {
            this.startupPageViewModel.StartupPageCompleted -= this.OnStartupPageCompleted;
            this.startupPageViewModel.UserLoggedIn -= this.OnUserLoggedIn;

            this.mainPageViewModel.Dispose();
            this.startupPageViewModel.Dispose();

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
