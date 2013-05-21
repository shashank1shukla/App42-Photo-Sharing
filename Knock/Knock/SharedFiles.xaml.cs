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
using System.Windows.Navigation;
using System.Collections;
using System.Collections.ObjectModel;
using com.shephertz.app42.paas.sdk.windows.upload;
using com.shephertz.app42.paas.sdk.windows;
using Microsoft.Phone.Shell;
using com.shephertz.app42.paas.sdk.windows.storage;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
namespace Knock
{
    public partial class SharedFiles : PhoneApplicationPage, App42Callback
    {
        /// <summary>
        ///  Use UploadFileForUser API to share image with your friend of App42 UploadService.
       /// </summary>
        static UploadService uploadObj = Util.uploadService;
        ImageList imageList = new ImageList();
        MyList myList = new MyList();
        public ProgressIndicator indicator = new ProgressIndicator

        {

            IsVisible = true,

            IsIndeterminate = true,

            Text = "Please wait."

        };
          
        public SharedFiles()
        {
            InitializeComponent();
            Recieved.Background = new SolidColorBrush(Colors.White);
            ImageDownloaded();
        }

        // Call when storage has no document for share or receive image.
        void App42Callback.OnException(App42Exception exception)
        {
            if (exception.GetAppErrorCode().Equals(2601))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("You did not shared anything.");
                });
            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show(exception.Message.ToString());
                });
            }
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                indicator.IsVisible = false;
            });
        }

        // call when storage query return shared files 
        void App42Callback.OnSuccess(object response)
        {
            
           if (response is Storage)
            {
                Storage storage = (Storage)response;
                IList<Storage.JSONDocument> JsonDocList = storage.GetJsonDocList();
                
                for (int i = 0; i < JsonDocList.Count; i++)
                {
                    JObject jsonObj = JObject.Parse(JsonDocList[i].GetJsonDoc());
                    string userName = (string)jsonObj["userName"];
                    string url = (string)jsonObj["imageUrl"];
                    string imageName = (string)jsonObj["imageName"];
                    My my = new My();
                    my.name = imageName;
                    my.userName = userName;
                    my.thumbnail = url;
                    // add images to list
                    myList.Add(my);
                }

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    indicator.IsVisible = false;
                });
            }
        }

        // fetch all shared image from App42 storage.
       void ImageDownloaded()
        {
            ReceivedFiles myfiles = new ReceivedFiles(imageList);
            myfiles.GetMyFiles();
             using (DbStorage fbdb = new DbStorage(MainPage.strConnectionString))
             {
                 IQueryable<Db> fbQuery = from db in fbdb.user select db;
                 Db ac = fbQuery.FirstOrDefault();
                 if (ac != null)
                 {
                     // finding Shared images from NOSQL of App42.
                     Util.storageService.FindDocumentByKeyValue(Util.storageDbName, Util.storageCollName, "ownerId", ac.FbId, this);

                 }

             }
             
             SystemTray.SetProgressIndicator(this, indicator);
          
        }

       // click on image 
        private void ImageListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string uri;
            string userName;
            string fileName;
            ListBox listBox = sender as ListBox;
            if (listBox.SelectedItem is ImagesItems)
            {
                ImagesItems iList = listBox.SelectedItem as ImagesItems;
                uri = iList.thumbnail;
                userName = iList.userName;
                fileName = iList.name;
                NavigationService.Navigate(new Uri("/ConversationPage.xaml?uri=" + uri + "&owner=" + userName + "&fileName=" + fileName, UriKind.Relative));
            }
            else if (listBox.SelectedItem is My)
            {
                My iList = listBox.SelectedItem as My;
                uri = iList.thumbnail;
                userName = iList.userName;
                fileName = iList.name;
                NavigationService.Navigate(new Uri("/ConversationPage.xaml?uri=" + uri + "&user=" + userName + "&fileName=" + fileName, UriKind.Relative));
            }
            
            
        }

        // to show shared files 
        private void Shared_Click(object sender, RoutedEventArgs e)
        {
            Shared.Background = new SolidColorBrush(Colors.White);
            Recieved.Background = new SolidColorBrush(Colors.Black);
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ImageListBox.ItemsSource = myList;
            });
        }
       
        // to show received files 
        private void Recieved_Click(object sender, RoutedEventArgs e)
        {
            Recieved.Background = new SolidColorBrush(Colors.White);
            Shared.Background = new SolidColorBrush(Colors.Black);
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ImageListBox.ItemsSource = imageList;
                indicator.IsVisible = false;
            });
          
        }

     
    }
}