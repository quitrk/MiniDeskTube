﻿using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using DeskTube.Views;
using Google.YouTube;
using System.Linq;
using Infrastructure;
using Microsoft.Practices.Prism.Commands;
using Google.GData.YouTube;
using Google.GData.Extensions;

namespace DeskTube.ViewModels
{
    public sealed class StartupPageViewModel : ViewModelBase
    {
        #region EVENTS

        /// <summary>
        /// Occurs when [startup page completed].
        /// </summary>
        public event EventHandler<Tuple<bool, YouTubeRequest>> StartupPageCompleted = delegate { };

        /// <summary>
        /// Occurs when [user logged in].
        /// </summary>
        public event EventHandler<YouTubeRequest> UserLoggedIn = delegate { };

        #endregion

        #region BACKING FIELDS

        /// <summary>
        /// IsRememberMeChecked backing field
        /// </summary>
        private bool isRememberMeChecked;

        #endregion

        #region PRIVATE FIELDS

        /// <summary>
        /// YouTubeRequestSettings object
        /// </summary>
        private YouTubeRequestSettings settings;

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="StartupPageViewModel" /> class.
        /// </summary>
        public StartupPageViewModel()
        {
            this.GoToNewAccountPageCommand = new DelegateCommand(this.HandleGoToNewAccountPageCommand);
            this.LoginCommand = new DelegateCommand(this.HandleLoginCommand);
            this.LoginOnEnterCommand = new DelegateCommand<KeyEventArgs>(this.HandleLoginOnEnterCommand);
            this.SkipLoginCommand = new DelegateCommand(this.HandleSkipLoginCommand);
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>
        /// The username.
        /// </value>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the is remember me checked.
        /// </summary>
        /// <value>
        /// The is remember me checked.
        /// </value>
        public bool IsRememberMeChecked
        {
            get
            {
                return this.isRememberMeChecked;
            }

            set
            {
                this.isRememberMeChecked = value;

                if (!this.isRememberMeChecked)
                {
                    Application.Current.Properties.Clear();
                }

                this.OnPropertyChanged(() => this.IsRememberMeChecked);
            }
        }

        #endregion

        #region COMMANDS

        /// <summary>
        /// Gets or sets the go to new account page command.
        /// </summary>
        /// <value>
        /// The go to new account page command.
        /// </value>
        public DelegateCommand GoToNewAccountPageCommand { get; set; }

        /// <summary>
        /// Gets or sets the login command.
        /// </summary>
        /// <value>
        /// The login command.
        /// </value>
        public DelegateCommand LoginCommand { get; set; }

        /// <summary>
        /// Gets or sets the login on enter command.
        /// </summary>
        /// <value>
        /// The login on enter command.
        /// </value>
        public DelegateCommand<KeyEventArgs> LoginOnEnterCommand { get; set; }

        /// <summary>
        /// Gets or sets the skip login command.
        /// </summary>
        /// <value>
        /// The skip login command.
        /// </value>
        public DelegateCommand SkipLoginCommand { get; set; }

        #endregion

        #region COMMAND HANDLERS

        /// <summary>
        /// Handles the go to new account page command.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void HandleGoToNewAccountPageCommand()
        {
            Process.Start("https://accounts.google.com/SignUp");
        }

        /// <summary>
        /// Handles the skip login command.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void HandleSkipLoginCommand()
        {
            if (this.StartupPageCompleted != null)
            {
                this.settings = new YouTubeRequestSettings("DeskTube", ConfigurationManager.AppSettings["DeveloperKey"]) { AutoPaging = true };
                var request = new YouTubeRequest(this.settings);

                this.StartupPageCompleted(null, new Tuple<bool, YouTubeRequest>(false, request));
            }
        }

        /// <summary>
        /// Handles the login command.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void HandleLoginCommand()
        {
            this.Login();
        }

        /// <summary>
        /// Handles the login on enter command.
        /// </summary>
        /// <param name="eventArgs">The <see cref="KeyEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void HandleLoginOnEnterCommand(KeyEventArgs eventArgs)
        {
            if (eventArgs.Key == Key.Enter)
            {
                this.Login();
            }
        }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Checks the login.
        /// </summary>
        private void CheckLogin()
        {
            try
            {
                var request = new YouTubeRequest(this.settings);
                request.Service.QueryClientLoginToken();

                this.UserLoggedIn(null, request);

                ((DependencyObject)this.View).Dispatcher.BeginInvoke(new Action(() =>
                                                                    {
                                                                        this.StartupPageCompleted(null, new Tuple<bool, YouTubeRequest>(true, request));

                                                                        if (this.IsRememberMeChecked)
                                                                        {
                                                                            Application.Current.Properties[0] = this.IsRememberMeChecked;
                                                                            Application.Current.Properties[1] = this.Username;
                                                                            Application.Current.Properties[2] = ((StartupPageView)this.View).auth.Password;
                                                                        }
                                                                        else
                                                                        {
                                                                            Application.Current.Properties.Clear();
                                                                        }

                                                                        this.IsLoading = false;
                                                                    }));
            }
            catch (Exception ex)
            {
                this.IsLoading = false;
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Logins this instance.
        /// </summary>
        private void Login()
        {
            try
            {
                var password = ((StartupPageView)this.View).auth.Password;
                this.settings = new YouTubeRequestSettings("DeskTube", ConfigurationManager.AppSettings["DeveloperKey"],
                                                           this.Username, password) { AutoPaging = true };
                this.IsLoading = true;
                Task.Factory.StartNew(this.CheckLogin);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Clears the credentials.
        /// </summary>
        public void ClearCredentials()
        {
            this.Username = null;
            this.OnPropertyChanged(() => this.Username);

            ((StartupPageView)this.View).auth.Clear();

            this.IsRememberMeChecked = false;
        }

        /// <summary>
        /// Populates the data.
        /// </summary>
        public void PopulateData()
        {
            try
            {
                //First get the 'user-scoped' storage information location reference in the assembly
                var isolatedStorage = IsolatedStorageFile.GetUserStoreForAssembly();

                //create a stream reader object to read content from the created isolated location
                var srReader = new StreamReader(new IsolatedStorageFileStream("isotest", FileMode.OpenOrCreate, isolatedStorage));

                if (srReader.EndOfStream)
                {
                    srReader.Close();
                    return;
                }

                this.IsRememberMeChecked = bool.Parse(srReader.ReadLine());
                this.Username = srReader.ReadLine();
                var password = ((StartupPageView)this.View).auth.Password = srReader.ReadLine();

                srReader.Close();

                this.settings = new YouTubeRequestSettings("DeskTube", ConfigurationManager.AppSettings["DeveloperKey"], this.Username, password) { AutoPaging = true };

                this.IsLoading = true;
                Task.Factory.StartNew(this.CheckLogin);
            }
            catch (Exception ex)
            {
                this.IsLoading = false;
                this.ClearCredentials();
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region ViewModelBase overrides

        /// <summary>
        /// Resolves the view.
        /// </summary>
        public override void ResolveView()
        {
            this.View = new StartupPageView();
            this.View.SetDataContext(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="all"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool all)
        {
            this.StartupPageCompleted = null;

            this.GoToNewAccountPageCommand = null;
            this.LoginCommand = null;
            this.SkipLoginCommand = null;

            this.View.Dispose();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            this.Dispose(true);
        }

        #endregion
    }
}
