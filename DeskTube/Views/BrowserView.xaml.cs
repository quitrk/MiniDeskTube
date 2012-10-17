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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Infrastructure.Utilities;
using mshtml;

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
            this.BrowserOverlay.WebBrowser.Dispose();
            this.BrowserOverlay.BrowserOverlayContainer.Dispose();;
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
