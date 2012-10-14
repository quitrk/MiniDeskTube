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
using Google.GData.Client;
using Google.GData.YouTube;
using Google.YouTube;
using Infrastructure;
using Infrastructure.Utilities;
using Microsoft.Practices.Prism.Commands;
using mshtml;

namespace DeskTube.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        #region BACKING FIELDS

        /// <summary>
        /// Backing field for IsCommentsPopupOpen
        /// </summary>
        private bool isCommentsPopupOpen;

        /// <summary>
        /// Backing field for CurrentVideoComments property
        /// </summary>
        private ObservableCollection<Comment> currentVideoComments;

        /// <summary>
        /// Backing field for IsPaused
        /// </summary>
        private bool isPaused;

        /// <summary>
        /// Backing field for IsShuffle
        /// </summary>
        private bool isShuffle;

        /// <summary>
        /// Backing field for TotalSeconds property
        /// </summary>
        private int? totalSeconds;

        /// <summary>
        /// Backing field for TotalMinutes property
        /// </summary>
        private int? totalMinutes;

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

        /// <summary>
        /// Backing field for SelectedSubscription property
        /// </summary>
        private Subscription selectedSubscription;

        /// <summary>
        /// Backing field for IsSynced property.
        /// </summary>
        private bool isSynced = true;
        
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
        private ListCollectionView FilteredCurrentVideos;

        /// <summary>
        /// The random object used for shuffling videos
        /// </summary>
        private readonly Random random = new Random(2000);

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPageViewModel" /> class.
        /// </summary>
        public MainPageViewModel()
        {
            this.CurrentVideos = new ObservableCollection<Video>();
            this.CurrentVideoComments = new ObservableCollection<Comment>();

            this.SyncCommand = new DelegateCommand(this.HandleSyncCommand, () => !this.IsSynced);
            this.PauseCommand = new DelegateCommand(this.HandlePauseCommand);
            this.PlayCommand = new DelegateCommand(this.HandlePlayCommand);
            this.PlayNextVideoCommand = new DelegateCommand(this.HandlePlayNextVideoCommand);
            this.PlayPreviousVideoCommand = new DelegateCommand(this.HandlePlayPreviousVideoCommand);
            this.StopCommand = new DelegateCommand(this.HandleStopCommand);
            this.ViewCommentsCommand = new DelegateCommand(this.HandleViewCommentsCommand);
            this.SelectVideoCommand = new DelegateCommand<Video>(this.HandleSelectVideoCommand);
            this.RemoveVideoCommand = new DelegateCommand<Video>(this.HandleRemoveVideoCommand);
            this.AddVideoToPlaylistCommand = new DelegateCommand<Tuple<Playlist, Video>>(this.HandleAddVideoToPlaylistCommand);
            this.AddFavoriteVideoCommand = new DelegateCommand<Video>(this.HandleAddFavoriteVideoCommand);
            this.AddSubscriptionCommand = new DelegateCommand<Video>(this.HandleAddSubscriptionCommand);
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Gets or sets a value indicating whether this instance is synced.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is synced; otherwise, <c>false</c>.
        /// </value>
        public bool IsSynced
        {
            get
            {
                return this.isSynced;
            }

            set
            {
                this.isSynced = value;
                this.OnPropertyChanged(() => this.IsSynced);
                this.SyncCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can user add favorite video.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can user add favorite video; otherwise, <c>false</c>.
        /// </value>
        public bool CanAddFavoriteVideo
        {
            get
            {
                return !this.SelectedUserFeed.Equals("favorites");
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can user remove video.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can user remove video; otherwise, <c>false</c>.
        /// </value>
        public bool CanRemoveVideo
        {
            get { return this.SelectedUserFeed.Equals("favorites") || this.SelectedUserFeed.Equals("playlists"); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can subscribe.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can subscribe; otherwise, <c>false</c>.
        /// </value>
        public bool CanSubscribe
        {
            get { return !this.SelectedUserFeed.Equals("subscriptions"); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is user authenticated.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is user authenticated; otherwise, <c>false</c>.
        /// </value>
        public bool IsUserAuthenticated
        {
            get { return this.youtubeRequest != null && this.youtubeRequest.Settings.Credentials != null; }
        }

        /// <summary>
        /// Gets or sets the volume level.
        /// </summary>
        /// <value>
        /// The volume level.
        /// </value>
        public int VolumeLevel
        {
            get
            {
                return ApplicationVolume.GetVolume();
            }

            set
            {
                ApplicationVolume.SetVolume(value);
                this.OnPropertyChanged(() => this.VolumeLevel);
            }
        }

        /// <summary>
        /// Gets or sets the isCommentsPopupOpen.
        /// </summary>
        /// <value>The isCommentsPopupOpen.</value>
        /// <remarks></remarks>
        public bool IsCommentsPopupOpen
        {
            get
            {
                return this.isCommentsPopupOpen;
            }

            set
            {
                this.isCommentsPopupOpen = value;
                this.OnPropertyChanged(() => this.IsCommentsPopupOpen);
            }
        }

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
                return this.totalMinutes;
            }

            set
            {
                this.totalMinutes = value;
                this.OnPropertyChanged(() => this.TotalMinutes);
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
                return this.totalSeconds;
            }
            set
            {
                this.totalSeconds = value;
                this.OnPropertyChanged(() => this.TotalSeconds);
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
            get
            {
                return this.selectedUserFeed;
            }

            set
            {
                this.selectedUserFeed = value;

                this.OnPropertyChanged(() => this.SelectedUserFeed);
                this.OnPropertyChanged(() => this.ArePlaylistsVisible);
                this.OnPropertyChanged(() => this.AreSubscriptionsVisible);
                this.OnPropertyChanged(() => this.CanRemoveVideo);
                this.OnPropertyChanged(() => this.CanAddFavoriteVideo);
                this.OnPropertyChanged(() => this.CanSubscribe);

                switch (value)
                {
                    case "playlists":
                        this.LoadUserPlaylists();
                        break;
                    case "subscriptions":
                        this.LoadUserSubscriptions();
                        break;
                    case "favorites":
                        this.LoadUserFavorites();
                        break;
                    case "search":
                        break;
                }
            }
        }

        /// <summary>
        /// Gets the available playlists for add video.
        /// </summary>
        /// <value>
        /// The available playlists for add video.
        /// </value>
        public List<Playlist> AvailablePlaylistsForAddVideo
        {
            get { return this.Playlists != null ? new List<Playlist>(this.Playlists.Where(p => p != this.SelectedPlaylist)) : null; }
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
            get
            {
                return this.selectedPlaylist;
            }

            set
            {
                this.selectedPlaylist = value;
                this.OnPropertyChanged(() => this.SelectedPlaylist);
                this.OnPropertyChanged(() => this.AvailablePlaylistsForAddVideo);

                if (this.selectedPlaylist == null)
                {
                    return;
                }

                this.SelectedSubscription = null;
                this.Favorites = null;
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
            get { return this.SelectedUserFeed != null && this.SelectedUserFeed.Equals("playlists"); }
        }

        /// <summary>
        /// Gets the subscriptions.
        /// </summary>
        /// <value>
        /// The subscriptions.
        /// </value>
        public List<Subscription> Subscriptions { get; private set; }

        /// <summary>
        /// Gets or sets the selected subscription.
        /// </summary>
        /// <value>
        /// The selected subscription.
        /// </value>
        public Subscription SelectedSubscription
        {
            get
            {
                return this.selectedSubscription;
            }

            set
            {
                this.selectedSubscription = value;
                this.OnPropertyChanged(() => this.SelectedSubscription);

                if (this.selectedSubscription != null)
                {
                    this.SelectedPlaylist = null;
                    this.Favorites = null;
                    this.LoadSubscriptionVideos();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether [are subscriptions visible].
        /// </summary>
        /// <value>
        /// <c>true</c> if [are subscriptions visible]; otherwise, <c>false</c>.
        /// </value>
        public bool AreSubscriptionsVisible
        {
            get { return this.SelectedUserFeed != null && this.SelectedUserFeed.Equals("subscriptions"); }
        }

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
            get
            {
                return this.currentVideos;
            }

            set
            {
                this.currentVideos = value;
                this.OnPropertyChanged(() => this.CurrentVideos);
            }
        }

        /// <summary>
        /// Gets the current video comments.
        /// </summary>
        /// <value>
        /// The current video comments.
        /// </value>
        public ObservableCollection<Comment> CurrentVideoComments
        {
            get
            {
                return this.currentVideoComments;
            }
            set
            {
                this.currentVideoComments = value;
                this.OnPropertyChanged(() => this.CurrentVideoComments);
            }
        }

        /// <summary>
        /// Gets or sets the current video.
        /// </summary>
        /// <value>
        /// The current video.
        /// </value>
        public Video CurrentVideo
        {
            get
            {
                return this.currentVideo;
            }

            set
            {
                this.currentVideo = value;
                this.OnPropertyChanged(() => this.CurrentVideo);
            }
        }

        /// <summary>
        /// Gets or sets the isShuffle.
        /// </summary>
        /// <value>The isShuffle.</value>
        /// <remarks></remarks>
        public bool IsShuffle
        {
            get
            {
                return this.isShuffle;
            }

            set
            {
                this.isShuffle = value;
                this.OnPropertyChanged(() => this.IsShuffle);
            }
        }

        /// <summary>
        /// Gets or sets the isPaused.
        /// </summary>
        /// <value>The isPaused.</value>
        /// <remarks></remarks>
        public bool IsPaused
        {
            get
            {
                return this.isPaused;
            }

            set
            {
                this.isPaused = value;
                this.OnPropertyChanged(() => this.IsPaused);
            }
        }

        #endregion

        #region COMMANDS

        /// <summary>
        /// Gets or sets the sync command.
        /// </summary>
        /// <value>
        /// The sync command.
        /// </value>
        public DelegateCommand SyncCommand { get; set; }

        /// <summary>
        /// Gets or sets the view comments command.
        /// </summary>
        /// <value>
        /// The view comments command.
        /// </value>
        public DelegateCommand ViewCommentsCommand { get; set; }

        /// <summary>
        /// Gets or sets the stop command.
        /// </summary>
        /// <value>
        /// The stop command.
        /// </value>
        public DelegateCommand StopCommand { get; set; }

        /// <summary>
        /// Gets or sets the play command.
        /// </summary>
        /// <value>
        /// The play command.
        /// </value>
        public DelegateCommand PlayCommand { get; set; }

        /// <summary>
        /// Gets or sets the pause command.
        /// </summary>
        /// <value>
        /// The pause command.
        /// </value>
        public DelegateCommand PauseCommand { get; set; }

        /// <summary>
        /// Gets or sets the play previous song command.
        /// </summary>
        /// <value>
        /// The play previous song command.
        /// </value>
        public DelegateCommand PlayPreviousVideoCommand { get; set; }

        /// <summary>
        /// Gets or sets the play next song command.
        /// </summary>
        /// <value>
        /// The play next song command.
        /// </value>
        public DelegateCommand PlayNextVideoCommand { get; set; }

        /// <summary>
        /// Gets or sets the select video command.
        /// </summary>
        /// <value>
        /// The select video command.
        /// </value>
        public DelegateCommand<Video> SelectVideoCommand { get; set; }

        /// <summary>
        /// Gets or sets the remove video command.
        /// </summary>
        /// <value>
        /// The remove video command.
        /// </value>
        public DelegateCommand<Video> RemoveVideoCommand { get; set; }

        /// <summary>
        /// Gets or sets the add video to playlist command.
        /// </summary>
        /// <value>
        /// The add video to playlist command.
        /// </value>
        public DelegateCommand<Tuple<Playlist, Video>> AddVideoToPlaylistCommand { get; set; }

        /// <summary>
        /// Gets or sets the add favorite video command.
        /// </summary>
        /// <value>
        /// The add favorite video command.
        /// </value>
        public DelegateCommand<Video> AddFavoriteVideoCommand { get; set; }

        /// <summary>
        /// Gets or sets the add subscription command.
        /// </summary>
        /// <value>
        /// The add subscription command.
        /// </value>
        public DelegateCommand<Video> AddSubscriptionCommand { get; set; }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Populates the data.
        /// </summary>
        public void PopulateData(YouTubeRequest request)
        {
            this.youtubeRequest = request;

            this.OnPropertyChanged(() => this.IsUserAuthenticated);

            this.UserFeeds = this.IsUserAuthenticated ? new List<string>() { "playlists", "subscriptions", "favorites", "search" } : new List<string>() { "search" };
            this.SelectedUserFeed = this.UserFeeds.First();
        }

        /// <summary>
        /// Activates the overlay.
        /// </summary>
        public void ActivateOverlay()
        {
            if(this.CurrentVideo != null)
            {
                this.IsOverlayActive = true;
            }
        }

        /// <summary>
        /// Deactivates the overlay.
        /// </summary>
        public void DeactivateOverlay()
        {
            this.IsOverlayActive = false;
        }

        #endregion

        #region COMMAND HANDLERS
        
        /// <summary>
        /// Handles the sync command.
        /// </summary>
        private void HandleSyncCommand()
        {
            this.CurrentVideos.Clear();

            if (this.BrowserView != null)
            {
                this.BrowserView.Dispose();
            }

            this.IsBrowserVisible = false;

            this.ClearTimers();

            var currentUserFeed = this.SelectedUserFeed;
            
            if(currentUserFeed.Equals("playlists"))
            {
                var currentPlaylistId = this.SelectedPlaylist != null ? this.SelectedPlaylist.Id : null;
                
                this.Subscriptions = null;
                this.Playlists = null;
                this.Favorites = null;

                this.SelectedUserFeed = currentUserFeed;
                this.SelectedPlaylist = this.Playlists.FirstOrDefault(p => p.Id == currentPlaylistId);

            }
            else if(currentUserFeed.Equals("subscriptions"))
            {
                var currentSubscriptionId = this.SelectedSubscription != null ? this.SelectedSubscription.Id : null;

                this.Subscriptions = null;
                this.Playlists = null;
                this.Favorites = null;

                this.SelectedUserFeed = currentUserFeed;
                this.SelectedSubscription = this.Subscriptions.FirstOrDefault(s => s.Id == currentSubscriptionId);
                
                this.LoadUserPlaylists();
            }
            else if(currentUserFeed.Equals("favorites"))
            {
                this.Subscriptions = null;
                this.Playlists = null;
                this.Favorites = null;

                this.SelectedUserFeed = currentUserFeed;
                
                this.LoadUserPlaylists();
            }
            else if(currentUserFeed.Equals("search"))
            {
                
            }

            this.IsSynced = true;
        }

        /// <summary>
        /// Handles the view comments command.
        /// </summary>
        private void HandleViewCommentsCommand()
        {
            this.IsCommentsPopupOpen = true;
            this.youtubeRequest.Settings.AutoPaging = false;
            this.CurrentVideoComments = new ObservableCollection<Comment>(this.youtubeRequest.GetComments(this.CurrentVideo).Entries);
            this.youtubeRequest.Settings.AutoPaging = true;
        }

        /// <summary>
        /// Handles the pause command.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void HandlePauseCommand()
        {
            this.IsPaused = true;
            this.progressTimer.Stop();
            this.BrowserView = new BrowserView();
        }

        /// <summary>
        /// Handles the play command.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void HandlePlayCommand()
        {
            this.PlayVideo(this.currentVideo);
        }

        /// <summary>
        /// Handles the play next song command.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void HandlePlayNextVideoCommand()
        {
            if (!this.IsShuffle)
            {
                this.IsPaused = false;
                var nextVideoIndex = this.CurrentVideos.IndexOf(this.CurrentVideo) + 1;

                if (nextVideoIndex < this.CurrentVideos.Count)
                {
                    this.PlayVideo(this.CurrentVideos[nextVideoIndex]);
                }
                else
                {
                    this.PlayVideo(this.CurrentVideos.First());
                }
            }
            else
            {
                this.PlayRandomVideo();
            }
        }

        /// <summary>
        /// Handles the play previous song command.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void HandlePlayPreviousVideoCommand()
        {
            if (!this.IsShuffle)
            {
                this.IsPaused = false;
                var previousVideoIndex = this.CurrentVideos.IndexOf(this.CurrentVideo) - 1;

                if (previousVideoIndex >= 0)
                {
                    this.PlayVideo(this.CurrentVideos[previousVideoIndex]);
                }
            }
            else
            {
                this.PlayRandomVideo();
            }
        }

        /// <summary>
        /// Handles the stop command.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void HandleStopCommand()
        {
            this.IsPaused = true;
            this.ClearTimers();
            this.BrowserView = new BrowserView();
        }

        /// <summary>
        /// Handles the select video command.
        /// </summary>
        /// <param name="video">The video.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void HandleSelectVideoCommand(Video video)
        {
            this.PlayVideo(video);
        }

        /// <summary>
        /// Handles the remove video command.
        /// </summary>
        /// <param name="video">The video.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void HandleRemoveVideoCommand(Video video)
        {
            video.AtomEntry.EditUri = video.Self;
            this.youtubeRequest.Delete(video); 
            this.IsSynced = false;
        }

        /// <summary>
        /// Handles the add video to playlist command.
        /// </summary>
        /// <param name="tuple">The tuple.</param>
        private void HandleAddVideoToPlaylistCommand(Tuple<Playlist, Video> tuple)
        {
            var playlistMember = new PlayListMember { VideoId = tuple.Item2.VideoId };
            this.youtubeRequest.AddToPlaylist(tuple.Item1, playlistMember);
            this.IsSynced = false;
        }

        /// <summary>
        /// Handles the add favorite video command.
        /// </summary>
        /// <param name="video">The video.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void HandleAddFavoriteVideoCommand(Video video)
        {
            var videoEntry = (YouTubeEntry)this.youtubeRequest.Service.Get(video.Self);

            try
            {
                this.youtubeRequest.Service.Insert(new Uri("http://gdata.youtube.com/feeds/api/users/default/favorites"), videoEntry);
                this.IsSynced = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("The video is already in your favorites list.");
            }
        }

        /// <summary>
        /// Handles the add subscription command.
        /// </summary>
        /// <param name="video">The video.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void HandleAddSubscriptionCommand(Video video)
        {
            var subscription = new Subscription
                                   {
                                       Type = SubscriptionEntry.SubscriptionType.channel,
                                       UserName = video.Uploader
                                   };

            try
            {
                youtubeRequest.Insert(new Uri(YouTubeQuery.CreateSubscriptionUri(null)), subscription); 
                this.IsSynced = false;
            }
            catch (Exception)
            {
                MessageBox.Show("You are already subscribed to this channel.");
            }
        }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Selects the video.
        /// </summary>
        /// <param name="video">The video.</param>
        private void PlayVideo(Video video)
        {
            this.CurrentVideo = video;

            if (this.CurrentVideo != null)
            {
                if (!this.IsBrowserVisible)
                {
                    this.IsBrowserVisible = true;
                }

                if (!this.IsPaused)
                {
                    this.ClearTimers();
                }

                this.BrowserView = new BrowserView();
                this.GetBrowser().Navigate(this.GetEmbedUrlFromLink(this.currentVideo.WatchPage.ToString()));
                this.GetBrowser().Navigated += this.OnBrowserNavigated;

                this.TotalMinutes = new TimeSpan(0, 0, 0, int.Parse(this.CurrentVideo.Media.Duration.Seconds)).Minutes;
                this.TotalSeconds = new TimeSpan(0, 0, 0, int.Parse(this.CurrentVideo.Media.Duration.Seconds)).Seconds;
            }
            else
            {
                this.TotalMinutes = null;
                this.TotalSeconds = null;
            }
        }

        /// <summary>
        /// Plays the random video.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void PlayRandomVideo()
        {
            this.IsPaused = false;
            this.PlayVideo(this.CurrentVideos[this.random.Next(0, this.CurrentVideos.Count - 1)]);
        }

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
                embedUrl += "&hl=en&autoplay=1&controls=0&showinfo=0&iv_load_policy=3&disablekb=1&rel=0&start=" + this.CurrentMinute * 60 + this.CurrentSecond;
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
            this.SelectedPlaylist = null;
            this.SelectedSubscription = null;

            this.CurrentVideos.Clear();

            if (this.BrowserView != null)
            {
                this.BrowserView.Dispose();
            }

            this.IsBrowserVisible = false;

            this.ClearTimers();

            var favorites = this.youtubeRequest.GetFavoriteFeed("default");

            foreach (var favorite in favorites.Entries)
            {
                this.CurrentVideos.Add(favorite);
            }

            this.Favorites = new List<Video>(this.CurrentVideos);

            this.AddVideoFiltering();
        }

        /// <summary>
        /// Loads the user subscriptions.
        /// </summary>
        private void LoadUserSubscriptions()
        {
            if (this.Subscriptions != null)
            {
                return;
            }

            var enumerable = this.youtubeRequest.GetSubscriptionsFeed("default").Entries as Subscription[] ??
                             this.youtubeRequest.GetSubscriptionsFeed("default").Entries.ToArray();

            if (!enumerable.Any())
            {
                return;
            }

            this.Subscriptions = new List<Subscription>(enumerable);
            this.OnPropertyChanged(() => this.Subscriptions);
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

            var enumerable = this.youtubeRequest.GetPlaylistsFeed("default").Entries as Playlist[] ??
                             this.youtubeRequest.GetPlaylistsFeed("default").Entries.ToArray();

            if (!enumerable.Any())
            {
                return;
            }

            this.Playlists = new List<Playlist>(enumerable);
            this.OnPropertyChanged(() => this.Playlists);
            this.OnPropertyChanged(() => this.AvailablePlaylistsForAddVideo);
        }

        /// <summary>
        /// Loads the playlist videos.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void LoadPlaylistVideos()
        {
            this.CurrentVideos.Clear();

            if (this.BrowserView != null)
            {
                this.BrowserView.Dispose();
            }

            this.IsBrowserVisible = false;

            this.ClearTimers();

            var playListMembers = this.youtubeRequest.GetPlaylist(this.SelectedPlaylist).Entries;
            foreach (var playListMember in playListMembers)
            {
                this.CurrentVideos.Add(playListMember);
            }

            this.AddVideoFiltering();
        }

        /// <summary>
        /// Loads the subscription videos.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void LoadSubscriptionVideos()
        {
            this.CurrentVideos.Clear();

            if (this.BrowserView != null)
            {
                this.BrowserView.Dispose();
            }

            this.IsBrowserVisible = false;

            this.ClearTimers();

            var subscriptionMembers = this.youtubeRequest.GetVideoFeed(this.SelectedSubscription.UserName);

            foreach (var subscriptionMember in subscriptionMembers.Entries)
            {
                this.CurrentVideos.Add(subscriptionMember);
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
            return string.IsNullOrEmpty(this.SearchText) || video.Title.ToLower().Contains(this.SearchText.ToLower()) || video == this.CurrentVideo;
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

            this.IsPaused = false;
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

            if (this.IsShuffle)
            {
                this.PlayRandomVideo();
            }
            else
            {
                this.HandlePlayNextVideoCommand();
            }
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
        /// <param name="all"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool all)
        {
            this.currentVideo = null;
            this.CurrentVideos = null;
            this.selectedPlaylist = null;
            this.Playlists = null;
            this.UserFeeds = null;
            this.CurrentVideoComments = null;

            this.IsBrowserVisible = false;

            if (this.BrowserView != null)
            {
                this.BrowserView.Dispose();
            }

            this.ClearTimers();

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
