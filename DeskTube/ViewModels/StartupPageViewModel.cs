using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using DeskTube.Views;
using Google.YouTube;
using Infrastructure;
using Microsoft.Practices.Prism.Commands;

namespace DeskTube.ViewModels
{
    public sealed class StartupPageViewModel : ViewModelBase
    {
        #region EVENTS

        /// <summary>
        /// Occurs when [startup page completed].
        /// </summary>
        public event EventHandler<Tuple<bool, YouTubeRequest>> StartupPageCompleted = delegate { };

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
            try
            {
                var password = ((StartupPageView)this.View).PasswordBox.Password;
                this.settings = new YouTubeRequestSettings("DeskTube", ConfigurationManager.AppSettings["DeveloperKey"], this.Username, password) { AutoPaging = true };
                this.IsLoading = true;
                Task.Factory.StartNew(this.CheckLogin);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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

                ((DependencyObject)this.View).Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.IsLoading = false;
                    this.StartupPageCompleted(null, new Tuple<bool, YouTubeRequest>(true, request));

                    if (this.IsRememberMeChecked)
                    {
                        Application.Current.Properties[0] = this.IsRememberMeChecked;
                        Application.Current.Properties[1] = this.Username;
                        Application.Current.Properties[2] = ((StartupPageView)this.View).PasswordBox.Password;
                    }
                    else
                    {
                        Application.Current.Properties.Clear();
                    }
                }));
            }
            catch (Exception ex)
            {
                this.IsLoading = false;
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region PUBLIC METHODS

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
                var password = ((StartupPageView)this.View).PasswordBox.Password = srReader.ReadLine();

                srReader.Close();

                this.settings = new YouTubeRequestSettings("DeskTube", ConfigurationManager.AppSettings["DeveloperKey"], this.Username, password) { AutoPaging = true };

                this.IsLoading = true;
                Task.Factory.StartNew(this.CheckLogin);

            }
            catch (Exception ex)
            {
                this.IsLoading = false;
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

        #endregion
    }

    /// <summary>
    /// Used for crypting / decrypting
    /// </summary>
    internal class Cryptography
    {
        private static byte[] salt = Encoding.ASCII.GetBytes("o6806642kbM7c5");
        private static string sharedKey = "PotecaruTudor";

        /// <summary>
        /// Encrypt the given string using AES.  The string can be decrypted using
        /// DecryptStringAES().  The sharedKey parameters must match.
        /// </summary>
        /// <param name="plainText">The text to encrypt.</param>
        /// <returns></returns>
        public static string EncryptStringAES(string plainText)
        {
            string outStr = null;                       // Encrypted string to return
            RijndaelManaged aesAlg = null;              // RijndaelManaged object used to encrypt the data.

            try
            {
                // generate the key from the shared secret and the salt
                var key = new Rfc2898DeriveBytes(sharedKey, salt);

                // Create a RijndaelManaged object
                aesAlg = new RijndaelManaged();
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (var msEncrypt = new MemoryStream())
                {
                    // prepend the IV
                    msEncrypt.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(int));
                    msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                    }
                    outStr = Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            // Return the encrypted bytes from the memory stream.
            return outStr;
        }

        /// <summary>
        /// Decrypt the given string.  Assumes the string was encrypted using
        /// EncryptStringAES(), using an identical sharedKey.
        /// </summary>
        /// <param name="cipherText">The text to decrypt.</param>
        /// <returns></returns>
        public static string DecryptStringAES(string cipherText)
        {
            // Declare the RijndaelManaged object
            // used to decrypt the data.
            RijndaelManaged aesAlg = null;

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            try
            {
                // generate the key from the shared secret and the salt
                var key = new Rfc2898DeriveBytes(sharedKey, salt);

                // Create the streams used for decryption.                
                byte[] bytes = Convert.FromBase64String(cipherText);
                using (var msDecrypt = new MemoryStream(bytes))
                {
                    // Create a RijndaelManaged object
                    // with the specified key and IV.
                    aesAlg = new RijndaelManaged();
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                    // Get the initialization vector from the encrypted stream
                    aesAlg.IV = ReadByteArray(msDecrypt);
                    // Create a decrytor to perform the stream transform.
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            return plaintext;
        }

        /// <summary>
        /// Reads the byte array.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        /// <exception cref="System.SystemException"></exception>
        private static byte[] ReadByteArray(Stream s)
        {
            var rawLength = new byte[sizeof(int)];
            if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
            {
                throw new SystemException("Stream did not contain properly formatted byte array");
            }

            var buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new SystemException("Did not read byte array properly");
            }

            return buffer;
        }
    }
}
