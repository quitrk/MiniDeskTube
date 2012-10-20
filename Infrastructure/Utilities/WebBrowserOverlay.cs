//// Thanks a lot http://blogs.msdn.com/b/changov/archive/2009/01/19/webbrowser-control-on-transparent-wpf-window.aspx
/// 
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Color = System.Drawing.Color;
using Point = System.Windows.Point;

namespace Infrastructure.Utilities
{
    /// <summary>
    /// Displays a WinForms.WebBrowser control over a given placement target element in a WPF Window.
    /// Applies the opacity of the Window to the WebBrowser control.
    /// </summary>
    public class WebBrowserOverlay : IDisposable
    {
        #region PRIVATE FIELDS

        private Window owner;
        private FrameworkElement placementTarget;
        private Color background = Color.FromArgb(255, 0, 0, 0);
        private DispatcherOperation _repositionCallback;
        private readonly WebBrowser webBrowser = new WebBrowser();

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="WebBrowserOverlay" /> class.
        /// </summary>
        /// <param name="placementTarget">The placement target.</param>
        /// <param name="window">The window.</param>
        public WebBrowserOverlay(FrameworkElement placementTarget, Window window)
        {
            this.placementTarget = placementTarget;

            this.owner = window;

            Debug.Assert(owner != null);

            BrowserOverlayContainer = new Form
                                          {
                                              BackColor = Color.Black,
                                              Opacity = 0.05,
                                              ShowInTaskbar = false,
                                              FormBorderStyle = FormBorderStyle.None
                                          };

            webBrowser.Dock = DockStyle.Fill;
            BrowserOverlayContainer.Controls.Add(webBrowser);

            this.owner.LocationChanged += OnOwnerLocationChanged;
            this.placementTarget.SizeChanged += OnPlacementTargetSizeChanged;

            if (this.owner.IsVisible)
                InitialShow();
            else
                this.owner.SourceInitialized += delegate
                {
                    InitialShow();
                };

            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(UIElement.OpacityProperty, typeof(Window));
            dpd.AddValueChanged(this.owner, delegate
            {
                BrowserOverlayContainer.Opacity = 0.05;
            });

            BrowserOverlayContainer.FormClosing += delegate { this.owner.Close(); };
        }
        
        #endregion

        #region PROPERTIES

        /// <summary>
        /// Gets the browser overlay container.
        /// </summary>
        /// <value>
        /// The browser overlay container.
        /// </value>
        public Form BrowserOverlayContainer { get; private set; }
        
        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Initials the show.
        /// </summary>
        private void InitialShow()
        {
            var owner = new NativeWindow();
            owner.AssignHandle(((HwndSource)HwndSource.FromVisual(this.owner)).Handle);
            BrowserOverlayContainer.Show(owner);
            owner.ReleaseHandle();
        }

        /// <summary>
        /// Called when [size location changed].
        /// </summary>
        private void HandleSizeLocationChanged()
        {
            // To reduce flicker when transparency is applied without DWM composition, 
            // do resizing at lower priority.
            if (_repositionCallback == null)
            {
                _repositionCallback = owner.Dispatcher.BeginInvoke(new Action(Reposition), DispatcherPriority.Input);
            }
        }

        /// <summary>
        /// Repositions this instance.
        /// </summary>
        private void Reposition()
        {
            _repositionCallback = null;

            var offset = placementTarget.TranslatePoint(new Point(), owner);
            var size = new Point(placementTarget.ActualWidth, placementTarget.ActualHeight);
            var hwndSource = (HwndSource)PresentationSource.FromVisual(owner);
            var ct = hwndSource.CompositionTarget;
            offset = ct.TransformToDevice.Transform(offset);
            size = ct.TransformToDevice.Transform(size);

            var screenLocation = new Win32.POINT(offset);
            Win32.ClientToScreen(hwndSource.Handle, ref screenLocation);
            var screenSize = new Win32.POINT(size);

            try
            {
                Win32.MoveWindow(BrowserOverlayContainer.Handle, screenLocation.X, screenLocation.Y, screenSize.X, screenSize.Y, true);
            }
            catch
            {

            }

            BrowserOverlayContainer.SetBounds(screenLocation.X, screenLocation.Y, screenSize.X, screenSize.Y);
            BrowserOverlayContainer.Update();
        }

        #endregion

        #region EVENT HANDLERS

        /// <summary>
        /// Called when [placement target size changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SizeChangedEventArgs" /> instance containing the event data.</param>
        private void OnPlacementTargetSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.HandleSizeLocationChanged();
        }

        /// <summary>
        /// Called when [owner location changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void OnOwnerLocationChanged(object sender, EventArgs e)
        {
            this.HandleSizeLocationChanged();
        }

        #endregion

        #region IDisposable members

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="all"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        public void Dispose(bool all)
        {
            this.owner.LocationChanged -= OnOwnerLocationChanged;
            this.placementTarget.SizeChanged -= OnPlacementTargetSizeChanged;

            this.BrowserOverlayContainer.Dispose();
            this.webBrowser.Dispose();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Dispose()
        {
           this.Dispose(true);
        }

        #endregion
    }
}
