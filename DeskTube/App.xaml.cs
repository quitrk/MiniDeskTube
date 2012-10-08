using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;

namespace DeskTube
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Exit" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.Windows.ExitEventArgs" /> that contains the event data.</param>
        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                //First get the 'user-scoped' storage information location reference in the assembly
                var isolatedStorage = IsolatedStorageFile.GetUserStoreForAssembly();

                //create a stream writer object to write content in the location
                var srWriter = new StreamWriter(new IsolatedStorageFileStream("isotest", FileMode.Create, isolatedStorage));

                for (var i = 0; i < Current.Properties.Count; i++)
                {
                    srWriter.WriteLine(Current.Properties[i]);
                }
                
                srWriter.Flush();
                srWriter.Close();
            }
            catch (System.Security.SecurityException sx)
            {
                MessageBox.Show(sx.Message);
                throw;
            }
        }
    }
}
