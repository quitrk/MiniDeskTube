using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Threading;
using DeskTube.Views;
using Gma.UserActivityMonitor;
using Google.GData.Client;
using Google.GData.YouTube;
using Google.YouTube;
using Infrastructure;
using Infrastructure.Utilities;
using Microsoft.Practices.Prism.Commands;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using WebBrowser = System.Windows.Controls.WebBrowser;

namespace DeskTube.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        #region BACKING FIELDS

        /// <summary>
        /// Backing field for IsPlaylistsPopupOpen
        /// </summary>
        private bool isPlaylistsPopupOpen;

        /// <summary>
        /// Backing field for IsSubscriptionsPopupOpen
        /// </summary>
        private bool isSubscriptionsPopupOpen;

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
        private bool isPaused = true;

        /// <summary>
        /// Backing field for IsShuffle
        /// </summary>
        private bool isShuffle;

        /// <summary>
        /// Backing field for Seconds property
        /// </summary>
        private int? seconds;

        /// <summary>
        /// Backing field for Minutes property
        /// </summary>
        private int? minutes;

        /// <summary>
        /// The total seconds
        /// </summary>
        private int? totalSeconds;

        /// <summary>
        /// Backing field for CurrentMinute
        /// </summary>
        private int currentMinute;

        /// <summary>
        /// Backing field for CurrentMinuteSecond
        /// </summary>
        private int currentMinuteSecond;

        /// <summary>
        /// The current second
        /// </summary>
        private int currentSecond;

        /// <summary>
        /// Backing field for BrowserView
        /// </summary>
        private BrowserView browserView;

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
        /// Backing field for FilterText property
        /// </summary>
        private string filterText;

        /// <summary>
        /// Backing field for SearchText
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
        /// The should listen for seeking
        /// </summary>
        private bool shouldListenForSeeking;

        /// <summary>
        /// The playlist title input
        /// </summary>
        private InputMessageBox newPlaylistTitleBox;

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
            this.EnqueuedVideos = new List<Video>();
            this.PreviousVideos = new List<Video>();
            this.CurrentVideos = new ObservableCollection<Video>();
            this.CurrentVideoComments = new ObservableCollection<Comment>();

            this.CreatePlaylistCommand = new DelegateCommand(this.HandleCreatePlaylistCommand);
            this.AddVideoCommentCommand = new DelegateCommand(this.HandleAddVideoCommentCommand);
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
            this.SearchCommand = new DelegateCommand<KeyEventArgs>(this.HandleSearchCommand);
            this.RemovePlaylistCommand = new DelegateCommand<Playlist>(this.HandleRemovePlaylistCommand);
            this.RemoveSubscriptionCommand = new DelegateCommand<Subscription>(this.HandleRemoveSubscriptionCommand);
            this.OpenSelectedUserFeedPopupCommand = new DelegateCommand(this.HandleOpenSelectedUserFeedPopupCommand);
            this.EnqueueVideoCommand = new DelegateCommand<Video>(this.HandleEnqueueVideoCommand);
            this.SeekToCommand = new DelegateCommand(this.HandleSeekToCommand);

            this.SelectPlaylistCommand = new DelegateCommand<Playlist>(this.HandleSelectPlaylistCommand);
            this.SelectSubscriptionCommand = new DelegateCommand<Subscription>(this.HandleSelectSubscriptionCommand);

            HookManager.KeyDown += this.OnHookManagerKeyDown;
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Gets or sets the new video comment.
        /// </summary>
        /// <value>
        /// The new video comment.
        /// </value>
        public string NewVideoComment { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is comments button visible.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is comments button visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsCommentsButtonVisible
        {
            get { return this.CurrentVideo != null; }
        }

        /// <summary>
        /// Gets or sets the windowHost.
        /// </summary>
        /// <value>The windowHost.</value>
        /// <remarks></remarks>
        public Window WindowHost { get; set; }

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
            get { return this.IsUserAuthenticated && (this.SelectedUserFeed == null || !this.SelectedUserFeed.Equals("favorites")); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can user remove video.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can user remove video; otherwise, <c>false</c>.
        /// </value>
        public bool CanRemoveVideo
        {
            get { return this.IsUserAuthenticated && this.SelectedUserFeed != null && (this.SelectedUserFeed.Equals("favorites") || this.SelectedUserFeed.Equals("playlists")); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can subscribe.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can subscribe; otherwise, <c>false</c>.
        /// </value>
        public bool CanSubscribe
        {
            get { return this.IsUserAuthenticated && (this.SelectedUserFeed == null || !this.SelectedUserFeed.Equals("subscriptions")); }
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
        public int? Minutes
        {
            get
            {
                return this.minutes;
            }

            set
            {
                this.minutes = value;
                this.OnPropertyChanged(() => this.Minutes);
            }
        }

        /// <summary>
        /// Gets the total seconds.
        /// </summary>
        /// <value>
        /// The total seconds.
        /// </value>
        public int? Seconds
        {
            get
            {
                return this.seconds;
            }

            set
            {
                this.seconds = value;
                this.OnPropertyChanged(() => this.Seconds);
            }
        }

        /// <summary>
        /// Gets or sets the total seconds.
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
        /// Gets or sets the currentMin.
        /// </summary>
        /// <value>The currentMin.</value>
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
        /// Gets or sets the CurrentMinuteSecond.
        /// </summary>
        /// <value>
        /// The CurrentMinuteSecond.
        /// </value>
        public int CurrentMinuteSecond
        {
            get
            {
                return this.currentMinuteSecond;
            }

            set
            {
                if (value == 60)
                {
                    this.currentMinuteSecond = 0;
                    this.CurrentMinute++;
                }
                else
                {
                    this.currentMinuteSecond = value;
                }

                this.OnPropertyChanged(() => this.CurrentMinuteSecond);
            }
        }

        /// <summary>
        /// Gets the current second.
        /// </summary>
        /// <value>
        /// The current second.
        /// </value>
        public int CurrentSecond
        {
            get
            {
                return this.currentSecond;
            }

            set
            {
                this.currentSecond = value;
                this.OnPropertyChanged(() => this.CurrentSecond);
                this.shouldListenForSeeking = true;
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
                this.browserView.WindowHost = this.WindowHost;
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

                if (this.BrowserView == null || this.BrowserView.BrowserOverlay == null)
                {
                    return;
                }

                this.BrowserView.BrowserOverlay.BrowserOverlayContainer.Opacity = this.isBrowserVisible ? 0.1 : 0;
            }
        }

        /// <summary>
        /// Gets or sets the searchText.
        /// </summary>
        /// <value>The searchText.</value>
        /// <remarks></remarks>
        public string SearchText
        {
            get
            {
                return this.searchText;
            }

            set
            {
                this.searchText = value;
                this.OnPropertyChanged(() => this.SearchText);
            }
        }

        /// <summary>
        /// Gets or sets the search text.
        /// </summary>
        /// <value>
        /// The search text.
        /// </value>
        public string FilterText
        {
            get { return this.filterText; }

            set
            {
                this.filterText = value;
                this.OnPropertyChanged(() => this.FilterText);
                this.FilteredCurrentVideos.Refresh();
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
                this.OnPropertyChanged(() => this.CanRemoveVideo);
                this.OnPropertyChanged(() => this.CanAddFavoriteVideo);
                this.OnPropertyChanged(() => this.CanSubscribe);

                if (this.selectedUserFeed == "favorites")
                {
                    this.LoadUserFavorites();
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

                this.PreviousVideos.Clear();
                this.EnqueuedVideos.Clear();
            }
        }

        /// <summary>
        /// Gets or sets the previous videos.
        /// </summary>
        /// <value>
        /// The previous videos.
        /// </value>
        public List<Video> PreviousVideos { get; set; }

        /// <summary>
        /// Gets or sets the enqueued videos.
        /// </summary>
        /// <value>
        /// The enqueued videos.
        /// </value>
        public List<Video> EnqueuedVideos { get; set; }

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
                this.OnPropertyChanged(() => this.IsCommentsButtonVisible);
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

        /// <summary>
        /// Gets or sets the isSubscriptionsPopupOpen.
        /// </summary>
        /// <value>The isSubscriptionsPopupOpen.</value>
        /// <remarks></remarks>
        public bool IsSubscriptionsPopupOpen
        {
            get
            {
                return this.isSubscriptionsPopupOpen;
            }

            set
            {
                this.isSubscriptionsPopupOpen = value;
                this.OnPropertyChanged(() => this.IsSubscriptionsPopupOpen);
            }
        }

        /// <summary>
        /// Gets or sets the isPlaylistsPopupOpen.
        /// </summary>
        /// <value>The isPlaylistsPopupOpen.</value>
        /// <remarks></remarks>
        public bool IsPlaylistsPopupOpen
        {
            get
            {
                return this.isPlaylistsPopupOpen;
            }

            set
            {
                this.isPlaylistsPopupOpen = value;
                this.OnPropertyChanged(() => this.IsPlaylistsPopupOpen);
            }
        }

        #endregion

        #region COMMANDS

        /// <summary>
        /// Gets or sets the seek to command.
        /// </summary>
        /// <value>
        /// The seek to command.
        /// </value>
        public DelegateCommand SeekToCommand { get; set; }

        /// <summary>
        /// Gets or sets the create playlist command.
        /// </summary>
        /// <value>
        /// The create playlist command.
        /// </value>
        public DelegateCommand CreatePlaylistCommand { get; set; }

        /// <summary>
        /// Gets or sets the add video comment command.
        /// </summary>
        /// <value>
        /// The add video comment command.
        /// </value>
        public DelegateCommand AddVideoCommentCommand { get; set; }

        /// <summary>
        /// Gets or sets the open selected user feed popup command.
        /// </summary>
        /// <value>
        /// The open selected user feed popup command.
        /// </value>
        public DelegateCommand OpenSelectedUserFeedPopupCommand { get; set; }

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
        /// Gets or sets the enqueue video command.
        /// </summary>
        /// <value>
        /// The enqueue video command.
        /// </value>
        public DelegateCommand<Video> EnqueueVideoCommand { get; set; }

        /// <summary>
        /// Gets or sets the add subscription command.
        /// </summary>
        /// <value>
        /// The add subscription command.
        /// </value>
        public DelegateCommand<Video> AddSubscriptionCommand { get; set; }

        /// <summary>
        /// Gets or sets the search command.
        /// </summary>
        /// <value>
        /// The search command.
        /// </value>
        public DelegateCommand<KeyEventArgs> SearchCommand { get; set; }

        /// <summary>
        /// Gets or sets the remove subscription command.
        /// </summary>
        /// <value>
        /// The remove subscription command.
        /// </value>
        public DelegateCommand<Subscription> RemoveSubscriptionCommand { get; set; }

        /// <summary>
        /// Gets or sets the remove playlist command.
        /// </summary>
        /// <value>
        /// The remove playlist command.
        /// </value>
        public DelegateCommand<Playlist> RemovePlaylistCommand { get; set; }

        /// <summary>
        /// Gets or sets the load playlist command.
        /// </summary>
        /// <value>
        /// The load playlist command.
        /// </value>
        public DelegateCommand<Playlist> SelectPlaylistCommand { get; set; }

        /// <summary>
        /// Gets or sets the set current subscription command.
        /// </summary>
        /// <value>
        /// The set current subscription command.
        /// </value>
        public DelegateCommand<Subscription> SelectSubscriptionCommand { get; set; }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Populates the data.
        /// </summary>
        public void PopulateData(YouTubeRequest request)
        {
            this.youtubeRequest = request;

            this.OnPropertyChanged(() => this.IsUserAuthenticated);

            this.UserFeeds = this.IsUserAuthenticated
                                 ? new List<string>() { "playlists", "subscriptions", "favorites" }
                                 : null;

            if (!this.IsUserAuthenticated)
            {
                return;
            }

            this.LoadUserPlaylists();
            this.LoadUserSubscriptions();

            if (this.Playlists != null && this.Playlists.Count == 1)
            {
                this.SelectedUserFeed = this.UserFeeds.First(uf => uf.Equals("playlists"));
                this.SelectedPlaylist = this.Playlists.First();
            }
        }

        #endregion

        #region COMMAND HANDLERS

        /// <summary>
        /// Handles the seek to command.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void HandleSeekToCommand()
        {
            if (this.shouldListenForSeeking)
            {
                this.shouldListenForSeeking = false;

                this.PlayVideo(this.CurrentVideo, this.CurrentSecond / 60, this.CurrentSecond % 60);
            }
        }

        /// <summary>
        /// Handles the create playlist command.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void HandleCreatePlaylistCommand()
        {
            this.newPlaylistTitleBox = new InputMessageBox();
            this.newPlaylistTitleBox.Show();
            this.newPlaylistTitleBox.InputCompleted += this.OnNewPlaylistTitleBoxInputCompleted;
            this.newPlaylistTitleBox.InputCancelled += this.OnNewPlaylistTitleBoxInputCancelled;
        }

        /// <summary>
        /// Handles the add video comment command.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void HandleAddVideoCommentCommand()
        {
            var comment = new Comment();
            comment.Content = this.NewVideoComment;

            this.youtubeRequest.AddComment(this.CurrentVideo, comment);
            this.NewVideoComment = string.Empty;
            this.OnPropertyChanged(() => this.NewVideoComment);
        }

        /// <summary>
        /// Handles the set current playlist command.
        /// </summary>
        /// <param name="playlist">The playlist.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void HandleSelectPlaylistCommand(Playlist playlist)
        {
            if (this.SelectedPlaylist == playlist)
            {
                return;
            }

            this.SelectedUserFeed = this.UserFeeds.First(uf => uf.Equals("playlists"));
            this.SelectedPlaylist = playlist;
        }

        /// <summary>
        /// Handles the set current subscription command.
        /// </summary>
        /// <param name="subscription">The subscription.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void HandleSelectSubscriptionCommand(Subscription subscription)
        {
            if (this.SelectedSubscription == subscription)
            {
                return;
            }

            this.SelectedUserFeed = this.UserFeeds.First(uf => uf.Equals("subscriptions"));
            this.SelectedSubscription = subscription;
        }

        /// <summary>
        /// Handles the open selected user feed popup command.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void HandleOpenSelectedUserFeedPopupCommand()
        {
            if (this.SelectedUserFeed.Equals("playlists"))
            {
                this.IsPlaylistsPopupOpen = true;
            }
            else if (this.SelectedUserFeed.Equals("subscriptions"))
            {
                this.IsSubscriptionsPopupOpen = true;
            }
        }

        /// <summary>
        /// Handles the sync command.
        /// </summary>
        private void HandleSyncCommand()
        {
            this.Sync();
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
            if (this.CurrentVideo != null)
            {
                this.PlayVideo(this.CurrentVideo, this.CurrentMinute, this.CurrentMinuteSecond);
            }
            else if (this.EnqueuedVideos.Any())
            {
                this.PlayVideo(this.EnqueuedVideos.First());
                this.EnqueuedVideos.RemoveAt(0);
            }
            else
            {
                if (this.IsShuffle)
                {
                    this.PlayRandomVideo();
                }
                else
                {
                    this.PlayVideo(this.CurrentVideos.First());
                }
            }
        }

        /// <summary>
        /// Handles the play next song command.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void HandlePlayNextVideoCommand()
        {
            this.PlayNextVideo();
        }

        /// <summary>
        /// Handles the play previous song command.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void HandlePlayPreviousVideoCommand()
        {
            if (!this.PreviousVideos.Any())
            {
                return;
            }

            this.PlayVideo(this.PreviousVideos.Last());
            this.PreviousVideos.Remove(this.PreviousVideos.Last());
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
            if (this.CurrentVideo != null)
            {
                this.PreviousVideos.Add(this.CurrentVideo);
            }

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

            if (this.CurrentVideo != null)
            {
                this.IsSynced = false;
            }
            else
            {
                this.Sync();
            }
        }

        /// <summary>
        /// Handles the add video to playlist command.
        /// </summary>
        /// <param name="tuple">The tuple.</param>
        private void HandleAddVideoToPlaylistCommand(Tuple<Playlist, Video> tuple)
        {
            var playlistMember = new PlayListMember { VideoId = tuple.Item2.VideoId };
            this.youtubeRequest.AddToPlaylist(tuple.Item1, playlistMember);

            if (this.CurrentVideo != null)
            {
                this.IsSynced = false;
            }
            else
            {
                this.Sync();
            }
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

                if (this.CurrentVideo != null)
                {
                    this.IsSynced = false;
                }
                else
                {
                    this.Sync();
                }
            }
            catch (Exception)
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

                if (this.CurrentVideo != null)
                {
                    this.IsSynced = false;
                }
                else
                {
                    this.Sync();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("You are already subscribed to this channel.");
            }
        }

        /// <summary>
        /// Handles the search command.
        /// </summary>
        /// <param name="eventArgs">The <see cref="KeyEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void HandleSearchCommand(KeyEventArgs eventArgs)
        {
            if (eventArgs.Key == Key.Enter && !string.IsNullOrEmpty(this.SearchText))
            {
                this.CurrentVideos = null;
                this.CurrentVideo = null;

                this.SelectedUserFeed = null;
                this.SelectedPlaylist = null;
                this.SelectedSubscription = null;
                this.Favorites = null;

                if (this.BrowserView != null)
                {
                    this.BrowserView.Dispose();
                }

                this.IsBrowserVisible = false;

                this.ClearTimers();

                this.youtubeRequest.Settings.AutoPaging = false;
                this.youtubeRequest.Settings.PageSize = 50;

                this.IsLoading = true;

                Task.Factory.StartNew(() =>
                                    {
                                        var query = new YouTubeQuery(YouTubeQuery.DefaultVideoUri)
                                                        {
                                                            OrderBy = "relevance",
                                                            Query = this.SearchText,
                                                            SafeSearch = YouTubeQuery.SafeSearchValues.None
                                                        };

                                        this.CurrentVideos = new ObservableCollection<Video>(youtubeRequest.Get<Video>(query).Entries);

                                        ((DependencyObject)this.View).Dispatcher.BeginInvoke(new Action(() =>
                                                                                        {
                                                                                            this.youtubeRequest.Settings.AutoPaging = true;
                                                                                            this.SearchText = null;
                                                                                            this.AddVideoFiltering();

                                                                                            this.IsLoading = false;
                                                                                        }));
                                    });
            }
        }

        /// <summary>
        /// Handles the remove playlist command.
        /// </summary>
        /// <param name="playlist">The playlist.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void HandleRemovePlaylistCommand(Playlist playlist)
        {
            try
            {
                this.youtubeRequest.Delete(playlist);

                if (this.CurrentVideo != null)
                {
                    this.IsSynced = false;
                }
                else
                {
                    this.Sync();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        /// <summary>
        /// Handles the remove subscription command.
        /// </summary>
        /// <param name="subscription">The subscription.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void HandleRemoveSubscriptionCommand(Subscription subscription)
        {
            try
            {
                this.youtubeRequest.Delete(subscription);

                if (this.CurrentVideo != null)
                {
                    this.IsSynced = false;
                }
                else
                {
                    this.Sync();
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Handles the enqueue video command.
        /// </summary>
        /// <param name="video">The video.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void HandleEnqueueVideoCommand(Video video)
        {
            this.EnqueuedVideos.Add(video);
        }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Closes the new playlist title box.
        /// </summary>
        private void CloseNewPlaylistTitleBox()
        {
            this.newPlaylistTitleBox.InputCompleted -= this.OnNewPlaylistTitleBoxInputCompleted;
            this.newPlaylistTitleBox.InputCancelled -= this.OnNewPlaylistTitleBoxInputCancelled;

            this.newPlaylistTitleBox.Close();
            this.newPlaylistTitleBox = null;
        }

        /// <summary>
        /// Syncs this instance.
        /// </summary>
        private void Sync()
        {
            this.CurrentVideos = null;
            this.CurrentVideo = null;

            if (this.BrowserView != null)
            {
                this.BrowserView.Dispose();
            }

            this.IsBrowserVisible = false;

            this.ClearTimers();

            var currentUserFeed = this.SelectedUserFeed;

            if (currentUserFeed == null)
            {
                this.IsSynced = true;
                return;
            }

            this.Subscriptions = null;
            this.Playlists = null;
            this.Favorites = null;

            this.SelectedUserFeed = currentUserFeed;
            this.LoadUserPlaylists();
            this.LoadUserSubscriptions();

            if (currentUserFeed.Equals("playlists"))
            {
                var currentPlaylistId = this.SelectedPlaylist != null ? this.SelectedPlaylist.Id : null;

                this.SelectedPlaylist = this.Playlists != null
                                            ? this.Playlists.FirstOrDefault(p => p.Id == currentPlaylistId)
                                            : null;
            }
            else if (currentUserFeed.Equals("subscriptions"))
            {
                var currentSubscriptionId = this.SelectedSubscription != null ? this.SelectedSubscription.Id : null;

                this.SelectedSubscription = this.Subscriptions != null
                                                ? this.Subscriptions.FirstOrDefault(s => s.Id == currentSubscriptionId)
                                                : null;
            }

            this.IsSynced = true;
        }

        /// <summary>
        /// Selects the video.
        /// </summary>
        /// <param name="video">The video.</param>
        /// <param name="currentMin">The current minute.</param>
        /// <param name="currentMinSec">The current minute second.</param>
        private void PlayVideo(Video video, int currentMin = 0, int currentMinSec = 0)
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

                this.CurrentMinute = currentMin;
                this.CurrentMinuteSecond = currentMinSec;
                this.CurrentSecond = this.CurrentMinute * 60 + this.CurrentMinuteSecond;

                this.BrowserView = new BrowserView();
                this.GetBrowser().Navigate(this.GetEmbedUrlFromLink(this.currentVideo.WatchPage.ToString()));
                this.GetBrowser().Navigated += this.OnBrowserNavigated;

                this.Minutes = new TimeSpan(0, 0, 0, int.Parse(this.CurrentVideo.Media.Duration.Seconds)).Minutes;
                this.Seconds = new TimeSpan(0, 0, 0, int.Parse(this.CurrentVideo.Media.Duration.Seconds)).Seconds;
                this.TotalSeconds = int.Parse(this.CurrentVideo.Media.Duration.Seconds);
            }
            else
            {
                this.Minutes = null;
                this.Seconds = null;
                this.TotalSeconds = null;
            }
        }

        /// <summary>
        /// Plays the next video.
        /// </summary>
        private void PlayNextVideo()
        {
            if (this.CurrentVideo != null)
            {
                this.PreviousVideos.Add(this.CurrentVideo);
            }

            if (this.EnqueuedVideos.Any())
            {
                this.PlayVideo(this.EnqueuedVideos.First());
                this.EnqueuedVideos.RemoveAt(0);
            }
            else if (!this.IsShuffle)
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
                embedUrl += "&hl=en&autoplay=1&controls=0&showinfo=0&iv_load_policy=3&disablekb=1&rel=0&start=" + (this.CurrentMinute * 60 + this.CurrentMinuteSecond);
                return new Uri(embedUrl);
            }
            catch (Exception)
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
            if (this.Favorites != null)
            {
                return;
            }

            this.SelectedPlaylist = null;
            this.SelectedSubscription = null;

            this.ClearCurrentUserFeed();

            this.IsLoading = true;
            Task.Factory.StartNew(() =>
                                {
                                    var favorites = this.youtubeRequest.GetFavoriteFeed("default");

                                    this.CurrentVideos = new ObservableCollection<Video>(favorites.Entries);
                                    this.Favorites = new List<Video>(this.CurrentVideos);

                                    ((DependencyObject)this.View).Dispatcher.BeginInvoke(new Action(() =>
                                                                                {
                                                                                    this.AddVideoFiltering();
                                                                                    this.IsLoading = false;
                                                                                }));
                                });
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

            try
            {
                var enumerable = this.youtubeRequest.GetSubscriptionsFeed("default").Entries as Subscription[] ??
                                 this.youtubeRequest.GetSubscriptionsFeed("default").Entries.ToArray();

                if (!enumerable.Any())
                {
                    return;
                }

                this.Subscriptions = new List<Subscription>(enumerable);
                this.OnPropertyChanged(() => this.Subscriptions);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
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

            try
            {
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
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        /// <summary>
        /// Loads the playlist videos.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void LoadPlaylistVideos()
        {
            this.ClearCurrentUserFeed();

            this.IsLoading = true;
            Task.Factory.StartNew(() =>
                                {
                                    var playListMembers = this.youtubeRequest.GetPlaylist(this.SelectedPlaylist).Entries;

                                    this.CurrentVideos = new ObservableCollection<Video>(playListMembers);

                                    ((DependencyObject)this.View).Dispatcher.BeginInvoke(new Action(() =>
                                                                                    {
                                                                                        this.AddVideoFiltering();
                                                                                        this.IsLoading = false;
                                                                                    }));
                                });
        }

        /// <summary>
        /// Loads the subscription videos.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void LoadSubscriptionVideos()
        {
            this.ClearCurrentUserFeed();

            this.IsLoading = true;
            Task.Factory.StartNew(() =>
                                {
                                    var subscriptionMembers = this.youtubeRequest.GetVideoFeed(this.SelectedSubscription.UserName);

                                    this.CurrentVideos = new ObservableCollection<Video>(subscriptionMembers.Entries);

                                    ((DependencyObject)this.View).Dispatcher.BeginInvoke(new Action(() =>
                                                                                    {
                                                                                        this.AddVideoFiltering();
                                                                                        this.IsLoading = false;
                                                                                    }));
                                });
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

            this.CurrentMinuteSecond = 0;
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
            var video = this.CurrentVideos.First(v => v.VideoId == ((Video)videoObject).VideoId);
            return string.IsNullOrEmpty(this.FilterText) || video.Title.ToLower().Contains(this.FilterText.ToLower()) || video == this.CurrentVideo;
        }

        /// <summary>
        /// Gets the browser.
        /// </summary>
        /// <returns></returns>
        private WebBrowser GetBrowser()
        {
            return this.BrowserView.browser;
        }

        /// <summary>
        /// Clears the current user feed.
        /// </summary>
        private void ClearCurrentUserFeed()
        {
            this.CurrentVideos = null;
            this.CurrentVideo = null;

            if (this.BrowserView != null)
            {
                this.BrowserView.Dispose();
            }

            this.IsBrowserVisible = false;

            this.ClearTimers();
        }

        /// <summary>
        /// Creates the playlist.
        /// </summary>
        /// <param name="playlistTitle">The playlist title.</param>
        private void CreatePlaylist(string playlistTitle)
        {
            var playlist = new Playlist { Title = playlistTitle };

            youtubeRequest.Insert(new Uri(YouTubeQuery.CreatePlaylistsUri(null)), playlist);

            if (this.CurrentVideo != null)
            {
                this.IsSynced = false;
            }
            else
            {
                this.Sync();
            }
        }

        #endregion

        #region EVENT HANDLERS

        /// <summary>
        /// Called when [hook manager key down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void OnHookManagerKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.MediaStop:
                    this.HandleStopCommand();
                    break;

                case Keys.MediaPlayPause:
                    if(this.IsPaused)
                    {
                        this.HandlePlayCommand();
                    }
                    else
                    {
                        this.HandlePauseCommand();
                    }
                    break;

                case Keys.MediaPreviousTrack:
                    this.HandlePlayPreviousVideoCommand();
                    break;

                case Keys.MediaNextTrack:
                    this.HandlePlayNextVideoCommand();
                    break;
            }
        }

        /// <summary>
        /// Called when [new playlist title box input completed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void OnNewPlaylistTitleBoxInputCompleted(object sender, string e)
        {
            this.CreatePlaylist(e);
            this.CloseNewPlaylistTitleBox();
        }

        /// <summary>
        /// Called when [new playlist title box input cancelled].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void OnNewPlaylistTitleBoxInputCancelled(object sender, EventArgs e)
        {
            this.CloseNewPlaylistTitleBox();
        }

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
            this.CurrentMinuteSecond++;
            this.CurrentSecond++;

            if (this.CurrentMinute != this.Minutes || this.CurrentMinuteSecond != this.Seconds)
            {
                return;
            }

            this.PlayNextVideo();
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
            HookManager.KeyDown -= this.OnHookManagerKeyDown;

            this.currentVideo = null;
            this.CurrentVideos = null;
            this.selectedPlaylist = null;
            this.Playlists = null;
            this.UserFeeds = null;
            this.CurrentVideoComments = null;
            this.CurrentVideo = null;

            this.IsBrowserVisible = false;

            if (this.BrowserView != null)
            {
                this.BrowserView.Dispose();
            }

            this.ClearTimers();

            this.View.Dispose();

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
