using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using com.shephertz.app42.paas.sdk.windows;
using Microsoft.Phone.Controls;
using System.Linq;
using com.shephertz.app42.paas.sdk.windows.storage;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Knock
{
    public class ReceivedFiles : App42Callback
    {
       // get current farme in app 
        DependencyObject currentPage  = ((App)Application.Current).RootFrame.Content as PhoneApplicationPage;
        
        ImageList imageList;
        public ReceivedFiles(ImageList _imageList)
        {
            imageList = _imageList;
        }

        // callback when server return an Exception on storage query .
        void App42Callback.OnException(App42Exception exception)
        {
            if (exception.GetAppErrorCode().Equals(2601))
            {
                currentPage.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("No one shared anything for you.");
                });
            }
            else
            {
                currentPage.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show(exception.Message.ToString());
                });
            }
           
        }

        // callback for storage query return received files. 
        void App42Callback.OnSuccess(object response)
        {
            if (response is Storage) 
            { 
                Storage storage = (Storage)response;
                IList<Storage.JSONDocument> JsonDocList = storage.GetJsonDocList();

                for (int i = 0; i < JsonDocList.Count; i++)
                {
                    JObject jsonObj = JObject.Parse(JsonDocList[i].GetJsonDoc());
                    string ownerName = (string)jsonObj["ownerName"];
                    string url = (string)jsonObj["imageUrl"];
                    string imageName = (string)jsonObj["imageName"];
                    ImagesItems images = new ImagesItems();
                    images.name = imageName;
                    images.userName = ownerName;
                    images.thumbnail = url;
                    imageList.Add(images);
                }
                
                currentPage.Dispatcher.BeginInvoke(() =>
                {
                    var frame = Application.Current.RootVisual as PhoneApplicationFrame;
                    var container = frame.Content as SharedFiles;
                    container.ImageListBox.ItemsSource = imageList;
                });
            }
        }

        //
        public void GetMyFiles() 
        {
            // check app has valid and authenticate user.
            using (DbStorage fbdb = new DbStorage(MainPage.strConnectionString))
            {
                IQueryable<Db> fbQuery = from db in fbdb.user select db;
                Db ac = fbQuery.FirstOrDefault();
                if (ac != null)
                {
                    // finding received images from NOSQL of App42.
                    Util.storageService.FindDocumentByKeyValue(Util.storageDbName, Util.storageCollName, "userId", ac.FbId, this);

                }

            }
         }
    }
}
