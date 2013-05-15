using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Text;
using Facebook;
using Microsoft.Phone.Shell;
using com.shephertz.app42.paas.sdk.windows;
using com.shephertz.app42.paas.sdk.windows.upload;
using com.shephertz.app42.paas.sdk.windows.storage;
using com.shephertz.app42.paas.sdk.windows.message;
using com.shephertz.app42.paas.sdk.windows.social;

namespace Knock
{
    public partial class MainPage : PhoneApplicationPage, App42Callback
    {
        /// <summary>
        ///  Facebook authentication .
        ///  Connect to local database.
        ///  To initialize with App42 in Util.cs
        ///  Get facebook friends from App42 Social API.
        /// </summary>
        
        private string _accessToken;
        private WebBrowser _webBrowser;
        public const string strConnectionString = @"isostore:/FbDB.sdf";
       
        public  ProgressIndicator indicator = new ProgressIndicator

        {

            IsVisible = true,

            IsIndeterminate = true,

            Text = "Loading ...!"

        };
        public MainPage()
        {
            InitializeComponent();
            textBlock1.Visibility = Visibility.Collapsed;
            button1.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Get FacebookOAuthClient.
        /// </summary>
        void MainPage_Loaded()
        {
            using (DbStorage fbdb = new DbStorage(strConnectionString))
            {
                IQueryable<Db> fbQuery = from db in fbdb.user select db;
                Db ac = fbQuery.FirstOrDefault();
                if(ac == null){
                    string appId = "YOUR FACEBOOK APP ID";
                    string[] extendedPermissions = new[] { "publish_stream"};

                    var oauth = new FacebookOAuthClient { AppId = appId };
                    var parameters = new Dictionary<string, object>
                            {
                               { "response_type", "token" },
                                { "display", "touch" }
                            };
                    if (extendedPermissions != null && extendedPermissions.Length > 0)
                    {
                        var scope = new StringBuilder();
                        scope.Append(string.Join(",", extendedPermissions));
                        parameters["scope"] = scope.ToString();
                    }
                    var loginUrl = oauth.GetLoginUrl(parameters);
                    //Add webBrowser to the contentPanel
                    _webBrowser.Navigate(loginUrl);
                    ContentPanel.Children.Add(_webBrowser);
                    _webBrowser.Navigated += webBrowser_Navigated;
                    //Open the facebook login page into the browser
           
            }
        }
        }

        /// <summary>
        /// Get access token from facebook and save to local database.
        /// </summary>
        void webBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            FacebookOAuthResult result;
            //Try because there might be cases when user input wrong password
            if (FacebookOAuthResult.TryParse(e.Uri.AbsoluteUri, out result))
            {
                if (result.IsSuccess)
                {
                    _accessToken = result.AccessToken;
                    //AccessToken is used when you want to use API as a user
                    //This example is not using it at all just showing it in a messagebox

                    //Adding data to the local database
                    using (DbStorage fbdb = new DbStorage(strConnectionString))
                    {
                        IQueryable<Db> fbQuery = from db in fbdb.user select db;
                        Db ac = fbQuery.FirstOrDefault();
                        if (ac == null)
                        {
                            Db newUser = new Db
                            {
                                AccessToken = _accessToken,
                            };

                            fbdb.user.InsertOnSubmit(newUser);
                            fbdb.SubmitChanges();
                         }
                        
                    }
                    //Hide the browser controller
                    _webBrowser.Visibility = System.Windows.Visibility.Collapsed;
                    ContentPanel.Children.Remove(_webBrowser);
                    
                    SystemTray.SetProgressIndicator(this, indicator);
                    Util myProfile = new Util(_accessToken);
                    myProfile.myProfile();

                    // load friend list from App42
                    GetFacebookFriendsFromSocial(_accessToken);
                   
                }
                else
                {
                    var errorDescription = result.ErrorDescription;
                    var errorReason = result.ErrorReason;
                    MessageBox.Show(errorReason + " " + errorDescription);
                }
            }
        }

        // select friend from friend list..
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            FriendsList fl = listBox.SelectedItem as FriendsList;
            var Items = new App42Share(fl);
            //  open popup to share image with your selected friend.
            Items.display_cspopup();
        }

        // create database.
        private void CreateDbIfNotExists()
        {
            using (DbStorage fbDb = new DbStorage(strConnectionString))
            {
                if (fbDb.DatabaseExists() == false)
                {
                    fbDb.CreateDatabase();
                    image1.Visibility = Visibility.Collapsed;
                    LoginText.Visibility = Visibility.Collapsed;
                    textBlock1.Visibility = Visibility.Visible;
                    button1.Visibility = Visibility.Visible;
                    _webBrowser = new WebBrowser();
                    MainPage_Loaded();
                   // this.Loaded += new RoutedEventHandler(MainPage_Loaded);
                }
                else
                {
                    using (DbStorage fbdb = new DbStorage(strConnectionString))
                    {
                        IQueryable<Db> fbQuery = from db in fbdb.user select db;
                        Db ac = fbQuery.FirstOrDefault();
                        image1.Visibility = Visibility.Collapsed;
                        LoginText.Visibility = Visibility.Collapsed;
                        textBlock1.Visibility = Visibility.Visible;
                        button1.Visibility = Visibility.Visible;
                        SystemTray.SetProgressIndicator(this, indicator);
                        // load friend list from App42
                        GetFacebookFriendsFromSocial(ac.AccessToken.ToString());
                        
                    }
                }
            }
        }

        // Login with facebook. 
        private void Login(object sender, MouseEventArgs e)
        {
            // create database.
            CreateDbIfNotExists();
        }

        public void GetFacebookFriendsFromSocial(String fbAccessToken) 
        {
            // Get Facebook Friends From App42 Social Service
            Util.socialService.GetFacebookFriendsOAuth(fbAccessToken,this);
        
        }

        // App42 callback when Object is successfully loded.
        void App42Callback.OnSuccess(object response)
        {
            // App42Callback if social
            if (response is Social)
            {
                Social social = (Social)response;
                // Get All friends from social object.
                IList<Social.Friends> fbList = social.GetFriendList();
                for (int i = 0; i < fbList.Count; i++)
                {
                    String id = fbList[i].GetId(); ;
                    String name = fbList[i].GetName();
                    String picture = fbList[i].GetPicture();
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        FriendsList row = new FriendsList();
                        row.userName.Text = name;
                        row.userId.Text = id;
                        row.myimage.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(picture);
                        // friend added in to list.
                        L1.Items.Add(row);
                    });
                }
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    indicator.IsVisible = false;
                });
            }
        }

        // App42 callback when exception
        void App42Callback.OnException(App42Exception exception)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(exception.Message);
            });
        }

        // Open Gallery.
        private void ShowGallery(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri(string.Format("/SharedFiles.xaml" +
                                    "?Refresh=true&random={0}", Guid.NewGuid()), UriKind.Relative));
        }
    }
}