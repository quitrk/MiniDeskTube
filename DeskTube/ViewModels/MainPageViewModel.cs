using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using System.Windows.Threading;
using DeskTube.Views;
using Google.YouTube;
using Infrastructure;
using Infrastructure.Utilities;
using mshtml;

namespace DeskTube.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        #region BACKING FIELDS

        /// <summary>
        /// Backing field for CurrentMinute
        /// </summary>
        private int currentMinute;

        /// <summary>
        /// Backing field for CurrentSecond
        /// </summary>
        private int currentSecond;

        /// <summary>
        /// Backing field for BrowserView
        /// </summary>
        private BrowserView browserView;

        /// <summary>
        /// Backing field for IsOverlayActive
        /// </summary>
        private bool isOverlayActive = true;

        /// <summary>
        /// The YouTubeRequest object
        /// </summary>
        private YouTubeRequest youtubeRequest;

        /// <summary>
        /// Backing field for SelectedUserFeed property
        /// </summary>
        private string selectedUserFeed;

        /// <summary>
        /// Backing field for SelectedUserPlaylist property
        /// </summary>
        private Playlist selectedPlaylist;

        /// <summary>
        /// Backing field for CurrentVideo property
        /// </summary>
        private Video currentVideo;

        /// <summary>
        /// Backing field for CurrentVideos property
        /// </summary>
        private ObservableCollection<Video> currentVideos;

        /// <summary>
        /// Backing field for SearchText property
        /// </summary>
        private string searchText;

        /// <summary>
        /// Backing field for IsBrowserVisible property
        /// </summary>
        private bool isBrowserVisible;

        #endregion

        #region PRIVATE FIELDS

        /// <summary>
        /// The video progress timer
        /// </summary>
        private DispatcherTimer progressTimer;

        /// <summary>
        /// Gets or sets the filtered current videos.
        /// </summary>
        /// <value>
        /// The filtered current videos.
        /// </value>
        public ListCollectionView FilteredCurrentVideos { get; set; }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPageViewModel" /> class.
        /// </summary>
        public MainPageViewModel()
        {
            this.currentVideos = new ObservableCollection<Video>();
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Gets the total minutes.
        /// </summary>
        /// <value>
        /// The total minutes.
        /// </value>
        public int? TotalMinutes
        {
            get
            {
                if (this.CurrentVideo != null)
                {
                    return new TimeSpan(0, 0, 0, int.Parse(this.CurrentVideo.Media.Duration.Seconds)).Minutes;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the total seconds.
        /// </summary>
        /// <value>
        /// The total seconds.
        /// </value>
        public int? TotalSeconds
        {
            get
            {
                if (CurrentVideo != null)
                {
                    return new TimeSpan(0, 0, 0, int.Parse(this.CurrentVideo.Media.Duration.Seconds)).Seconds;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets or sets the currentMinute.
        /// </summary>
        /// <value>The currentMinute.</value>
        /// <remarks></remarks>
        public int CurrentMinute
        {
            get
            {
                return this.currentMinute;
            }

            set
            {
                this.currentMinute = value;
                this.OnPropertyChanged(() => this.CurrentMinute);
            }
        }

        /// <summary>
        /// Gets or sets the currentSecond.
        /// </summary>
        /// <value>The currentSecond.</value>
        /// <remarks></remarks>
        public int CurrentSecond
        {
            get
            {
                return this.currentSecond;
            }

            set
            {
                if (value == 60)
                {
                    this.currentSecond = 0;
                    this.CurrentMinute++;
                }
                else
                {
                    this.currentSecond = value;
                }

                this.OnPropertyChanged(() => this.CurrentSecond);
            }
        }

        /// <summary>
        /// Gets or sets the browserView.
        /// </summary>
        /// <value>The browserView.</value>
        /// <remarks></remarks>
        public BrowserView BrowserView
        {
            get
            {
                return this.browserView;
            }

            set
            {
                if (this.BrowserView != null)
                {
                    this.BrowserView.Dispose();
                }

                this.browserView = value;
                this.OnPropertyChanged(() => this.BrowserView);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is browser visible.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is browser visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsBrowserVisible
        {
            get
            {
                return this.isBrowserVisible;
            }

            set
            {
                this.isBrowserVisible = value;
                this.OnPropertyChanged(() => this.IsBrowserVisible);

                if (value)
                {
                    this.ActivateOverlay();
                }
                else
                {
                    this.DeactivateOverlay();
                }
            }
        }

        /// <summary>
        /// Gets or sets the search text.
        /// </summary>
        /// <value>
        /// The search text.
        /// </value>
        public string SearchText
        {
            get { return this.searchText; }

            set
            {
                this.searchText = value;
                this.OnPropertyChanged(() => this.SearchText);
                this.FilteredCurrentVideos.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets the isOverlayActive.
        /// </summary>
        /// <value>The isOverlayActive.</value>
        /// <remarks></remarks>
        public bool IsOverlayActive
        {
            get
            {
                return this.isOverlayActive;
            }

            set
            {
                this.isOverlayActive = value;
                this.OnPropertyChanged(() => this.IsOverlayActive);
            }
        }

        /// <summary>
        /// Gets the user feeds.
        /// </summary>
        /// <value>
        /// The user feeds.
        /// </value>
        public List<string> UserFeeds { get; private set; }

        /// <summary>
        /// Gets or sets the selected user feed.
        /// </summary>
        /// <value>
        /// The selected user feed.
        /// </value>
        public string SelectedUserFeed
        {
            get { return this.selectedUserFeed; }

            set
            {
                this.selectedUserFeed = value;

                this.OnPropertyChanged(() => SelectedUserFeed);
                this.OnPropertyChanged(() => ArePlaylistsVisible);

                switch (value)
                {
                    case "playlists":
                        this.LoadUserPlaylists();
                        break;
                    case "subscribtions":
                        this.LoadUserSubscriptions();
                        break;
                    case "favorites":
                        this.LoadUserFavorites();
                        break;
                }
            }
        }

        /// <summary>
        /// Gets the playlists.
        /// </summary>
        /// <value>
        /// The playlists.
        /// </value>
        public List<Playlist> Playlists { get; private set; }

        /// <summary>
        /// Gets or sets the selected playlist.
        /// </summary>
        /// <value>
        /// The selected playlist.
        /// </value>
        public Playlist SelectedPlaylist
        {
            get { return this.selectedPlaylist; }

            set
            {
                this.selectedPlaylist = value;
                this.OnPropertyChanged(() => SelectedPlaylist);

                this.LoadPlaylistVideos();
            }
        }

        /// <summary>
        /// Gets a value indicating whether [are playlists visible].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [are playlists visible]; otherwise, <c>false</c>.
        /// </value>
        public bool ArePlaylistsVisible
        {
            get { return this.SelectedUserFeed.Equals("playlists"); }
        }

        /// <summary>
        /// Gets the subscriptions.
        /// </summary>
        /// <value>
        /// The subscriptions.
        /// </value>
        public List<Subscription> Subscriptions { get; private set; }

        /// <summary>
        /// Gets or sets the favorites.
        /// </summary>
        /// <value>
        /// The favorites.
        /// </value>
        public List<Video> Favorites { get; set; }

        /// <summary>
        /// Gets the selected playlist videos.
        /// </summary>
        /// <value>
        /// The selected playlist videos.
        /// </value>
        public ObservableCollection<Video> CurrentVideos
        {
            get { return this.currentVideos; }
        }

        /// <summary>
        /// Gets or sets the current video.
        /// </summary>
        /// <value>
        /// The current video.
        /// </value>
        public Video CurrentVideo
        {
            get { return this.currentVideo; }

            set
            {
                this.currentVideo = value;

                if (this.currentVideo != null)
                {
                    if (!this.IsBrowserVisible)
                    {
                        this.IsBrowserVisible = true;
                    }

                    this.ClearTimers();

                    this.BrowserView = new BrowserView();
                    this.GetBrowser().Navigate(this.GetEmbedUrlFromLink(this.currentVideo.WatchPage.ToString()));
                    this.GetBrowser().Navigated += this.OnBrowserNavigated;
                }

                this.OnPropertyChanged(() => this.CurrentVideo);
                this.OnPropertyChanged(() => this.TotalMinutes);
                this.OnPropertyChanged(() => this.TotalSeconds);
            }
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Populates the data.
        /// </summary>
        public void PopulateData(YouTubeRequest request)
        {
            this.youtubeRequest = request;

            if (this.youtubeRequest.Settings.Credentials != null)
            {
                this.UserFeeds = new List<string>() { "playlists", "subscribtions", "favorites" };
                this.OnPropertyChanged(() => UserFeeds);

                this.SelectedUserFeed = this.UserFeeds.First();
            }
        }

        /// <summary>
        /// Activates the overlay.
        /// </summary>
        public void ActivateOverlay()
        {
            this.IsOverlayActive = true;
        }

        /// <summary>
        /// Deactivates the overlay.
        /// </summary>
        public void DeactivateOverlay()
        {
            this.IsOverlayActive = false;
        }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Gets the embed URL from link.
        /// </summary>
        /// <param name="link">The link.</param>
        /// <returns></returns>
        private Uri GetEmbedUrlFromLink(string link)
        {
            try
            {
                link = link.Replace("&feature=youtube_gdata_player", "");
                string embedUrl = "http://www.";
                string startPart = link.Substring(link.IndexOf("you"));
                embedUrl += startPart.Substring(0, startPart.LastIndexOf(@"/"));
                embedUrl += "/v/";
                embedUrl += startPart.Substring(startPart.LastIndexOf("=") + 1);
                embedUrl += "&hl=en&autoplay=1&controls=0&showinfo=0&iv_load_policy=3&disablekb=1";
                return new Uri(embedUrl);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Loads the user favorites.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void LoadUserFavorites()
        {

        }

        /// <summary>
        /// Loads the user subscriptions.
        /// </summary>
        private void LoadUserSubscriptions()
        {

        }

        /// <summary>
        /// Loads the user playlists.
        /// </summary>
        private void LoadUserPlaylists()
        {
            if (this.Playlists != null)
            {
                return;
            }

            var enumerable = this.youtubeRequest.GetPlaylistsFeed("default").Entries as Playlist[] ?? this.youtubeRequest.GetPlaylistsFeed("default").Entries.ToArray();

            if (!enumerable.Any())
            {
                return;
            }

            this.Playlists = new List<Playlist>();

            foreach (var playlist in enumerable)
            {
                this.Playlists.Add(playlist);
            }
        }

        /// <summary>
        /// Loads the playlist videos.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void LoadPlaylistVideos()
        {
            this.CurrentVideos.Clear();
            this.IsBrowserVisible = false;

            if (this.BrowserView != null)
            {
                this.BrowserView.Dispose();
            }

            this.ClearTimers();

            var playListMembers = this.youtubeRequest.GetPlaylist(this.SelectedPlaylist).Entries;

            foreach (var playListMember in playListMembers)
            {
                this.CurrentVideos.Add(playListMember);
            }

            this.AddVideoFiltering();
        }

        /// <summary>
        /// Resets the timers.
        /// </summary>
        private void ClearTimers()
        {
            if (this.progressTimer == null)
            {
                return;
            }

            this.progressTimer.Tick -= this.OnProgressTimerTick;
            this.progressTimer = null;

            this.CurrentSecond = 0;
            this.CurrentMinute = 0;
        }

        /// <summary>
        /// Adds the video filtering.
        /// </summary>
        private void AddVideoFiltering()
        {
            this.FilteredCurrentVideos = CollectionViewSource.GetDefaultView(this.CurrentVideos) as ListCollectionView;
            this.FilteredCurrentVideos.Filter = this.ShouldVideoBeDisplayed;
        }

        /// <summary>
        /// Shoulds the video be displayed.
        /// </summary>
        /// <param name="videoObject">The video object.</param>
        /// <returns></returns>
        private bool ShouldVideoBeDisplayed(object videoObject)
        {
            var video = (Video)videoObject;
            return string.IsNullOrEmpty(this.SearchText) || video.Title.ToLower().Contains(this.SearchText.ToLower());
        }

        /// <summary>
        /// Gets the browser.
        /// </summary>
        /// <returns></returns>
        private WebBrowser GetBrowser()
        {
            return this.BrowserView.browser;
        }

        #endregion

        #region EVENT HANDLERS

        /// <summary>
        /// Called when [browser navigated].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="NavigationEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void OnBrowserNavigated(object sender, NavigationEventArgs e)
        {
            this.progressTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 1) };
            this.progressTimer.Tick += this.OnProgressTimerTick;
            this.progressTimer.Start();
        }

        /// <summary>
        /// Called when [progress timer tick].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void OnProgressTimerTick(object sender, EventArgs e)
        {
            this.CurrentSecond++;

            if (this.CurrentMinute != this.TotalMinutes || this.CurrentSecond != this.TotalSeconds)
            {
                return;
            }

            this.ClearTimers();
            this.CurrentVideo = this.CurrentVideos[this.CurrentVideos.IndexOf(this.CurrentVideo) + 1];
        }

        #endregion

        #region ViewModelBase overrides

        /// <summary>
        /// Resolves the view.
        /// </summary>
        public override void ResolveView()
        {
            this.View = new MainPageView();
            this.View.SetDataContext(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            this.currentVideo = null;
            this.currentVideos = null;
            this.selectedPlaylist = null;
            this.Playlists = null;
            this.UserFeeds = null;

            this.IsBrowserVisible = false;

            if (this.BrowserView != null)
            {
                this.BrowserView.Dispose();
            }

            this.ClearTimers();
            base.Dispose();
        }

        #endregion
    }
}
