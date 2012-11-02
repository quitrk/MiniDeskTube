using System.Windows;

namespace DeskTube.Views
{
    /// <summary>
    /// Interaction logic for ShareOnFacebookWindow.xaml
    /// </summary>
    public partial class ShareOnFacebookWindow : Window
    {
        public ShareOnFacebookWindow()
        {
            InitializeComponent();

            this.browser.Navigated += browser_Navigated;
        }

        private void browser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if(e.Uri.ToString().Equals("http://www.facebook.com/"))
            {
                this.Close();
            }
        }

        /// <summary>
        /// Shares the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        public void Share(string path)
        {
            this.browser.Navigate(path);
        }
    }
}
