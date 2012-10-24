using Infrastructure.Utilities;
using System;
using System.Windows;

namespace DeskTube.Views
{
    /// <summary>
    /// Interaction logic for BrowserView.xaml
    /// </summary>
    public partial class BrowserView : IDisposable
    {
        #region Backing fields

        /// <summary>
        /// Backing field for WindowHost
        /// </summary>
        private Window windowHost;

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserView" /> class.
        /// </summary>
        public BrowserView()
        {
            InitializeComponent();
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Gets or sets the browser overlay.
        /// </summary>
        /// <value>
        /// The browser overlay.
        /// </value>
        public WebBrowserOverlay BrowserOverlay { get; set; }

        /// <summary>
        /// Gets or sets the windowHost.
        /// </summary>
        /// <value>The windowHost.</value>
        /// <remarks></remarks>
        public Window WindowHost
        {
            get
            {
                return this.windowHost;
            }

            set
            {
                this.windowHost = value;
                this.BrowserOverlay = new WebBrowserOverlay(this.browserOverlayPlacementTarget, this.windowHost);
            }
        }

        #endregion

        #region IDisposable members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="all"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool all)
        {
            if (this.BrowserOverlay != null)
            {
                this.BrowserOverlay.Dispose();
                this.BrowserOverlay = null;
            }

            this.browser.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        #endregion
    }
}
