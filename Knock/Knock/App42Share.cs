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
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using com.shephertz.app42.paas.sdk.windows;
using com.shephertz.app42.paas.sdk.windows.upload;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Shell;
using com.shephertz.app42.paas.sdk.windows.storage;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using com.shephertz.app42.paas.sdk.windows.message;
using Microsoft.Phone.Controls;

namespace Knock
{
    public class App42Share : App42Callback
    {
        String name;
        String type;
        String url;
        String description;
        String uName;
        String uId ;
        Popup my_popup_cs = new Popup();
        DependencyObject currentPage;
        public ProgressIndicator indicator = new ProgressIndicator{IsVisible = true,IsIndeterminate = true,Text = "Uploading"};
        
        public App42Share(FriendsList fl)
        {
           uName = fl.userName.Text;
           uId = fl.userId.Text;
           currentPage = ((App)Application.Current).RootFrame.Content as PhoneApplicationPage;
        }

       private void btn_continue_Click(object sender, RoutedEventArgs e)
        {
            PhotoChooserTask objPhotoChooser = new PhotoChooserTask();
            objPhotoChooser.Show();
            objPhotoChooser.Completed += new EventHandler<PhotoResult>(PhotoChooserTask_Completed);
        }

        private void PhotoChooserTask_Completed(object sender, PhotoResult e)
        {
            // Share image with your friend
            UploadImageOnApp42CDN(e);
            SystemTray.SetProgressIndicator(currentPage, indicator);
                    
            if (my_popup_cs.IsOpen)
            {
                my_popup_cs.IsOpen = false;
            }
        } 

        // App42 File Upload with your friend
        private void UploadImageOnApp42CDN(PhotoResult imageIO) 
        {
            String dt = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            String imageName = uId + dt + ".png";
            Util.uploadService.UploadFileForUser(imageName, uId, imageIO.ChosenPhoto, "IMAGE", "CLICK", this);
        }


        private void AddMetaInfoWithApp42(JObject imageIfo) 
        {
            Util.storageService.InsertJSONDocument(Util.storageDbName, Util.storageCollName, imageIfo.ToString(), this);
        }

        // App42 Callback 
        void App42Callback.OnSuccess(object response)
        {
            if (response is Upload)
            {
                Upload upload = (Upload)response;
                IList<Upload.File> fileList = upload.GetFileList();
                for (int i = 0; i < fileList.Count; i++)
                {
                    name = fileList[i].GetName();
                    type = fileList[i].GetType();
                    url = fileList[i].GetUrl();
                    description = fileList[i].GetDescription();
                }
                currentPage.Dispatcher.BeginInvoke(() =>
                {
                    indicator.IsVisible = false;
                });
                using (DbStorage fbdb = new DbStorage(MainPage.strConnectionString))
                {
                    IQueryable<Db> fbQuery = from db in fbdb.user select db;
                    Db ac = fbQuery.FirstOrDefault();
                    if (ac != null)
                    {
                        JObject myJson = new JObject();

                        myJson.Add("ownerId", ac.FbId);
                        myJson.Add("ownerName", ac.Name);
                        myJson.Add("userId", uId);
                        myJson.Add("userName", uName);
                        myJson.Add("imageName", name);
                        myJson.Add("imageUrl", url);
                        myJson.Add("description", description);
                        AddMetaInfoWithApp42(myJson);
                    }

                }
            }
        }

        void App42Callback.OnException(App42Exception exception)
        {
            currentPage.Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(exception.Message);
            });
        }

        public void display_cspopup()
        {
            Border border = new Border();                                                     // to create green color border
            border.BorderBrush = new SolidColorBrush(Colors.White);
            border.BorderThickness = new Thickness(2);
            border.Margin = new Thickness(10, 10, 10, 10);

            StackPanel skt_pnl_outter = new StackPanel();                             // stack panel 
            skt_pnl_outter.Background = new SolidColorBrush(Colors.Black);
            skt_pnl_outter.Orientation = System.Windows.Controls.Orientation.Vertical;
            skt_pnl_outter.Height = skt_pnl_outter.Height * 5;


            TextBlock txt_blk1 = new TextBlock();                                         // Textblock 1
            txt_blk1.Text = "App42 Share";
            txt_blk1.TextAlignment = TextAlignment.Center;
            txt_blk1.FontSize = 40;
            txt_blk1.Margin = new Thickness(10, 0, 10, 0);
            txt_blk1.Foreground = new SolidColorBrush(Colors.White);

            TextBlock txt_blk2 = new TextBlock();                                      // Textblock 2
            txt_blk2.Text = "Tap Continue To Share With";
            txt_blk2.TextAlignment = TextAlignment.Center;
            txt_blk2.FontSize = 21;
            txt_blk2.Margin = new Thickness(10, 0, 10, 0);
            txt_blk2.Foreground = new SolidColorBrush(Colors.White);

            TextBlock txt_blk3 = new TextBlock();                                      // Textblock 2
            txt_blk3.Text = uName;
            txt_blk3.TextAlignment = TextAlignment.Center;
            txt_blk3.FontSize = 21;
            txt_blk3.Margin = new Thickness(10, 0, 10, 0);
            txt_blk3.Foreground = new SolidColorBrush(Colors.White);


            //Adding control to stack panel
            skt_pnl_outter.Children.Add(txt_blk1);
            skt_pnl_outter.Children.Add(txt_blk2);
            skt_pnl_outter.Children.Add(txt_blk3);

            StackPanel skt_pnl_inner = new StackPanel();
            skt_pnl_inner.Orientation = System.Windows.Controls.Orientation.Horizontal;

            Button btn_continue = new Button();                                         // Button continue
            btn_continue.Content = "Continue";
            btn_continue.Width = 215;
            btn_continue.Click += new RoutedEventHandler(btn_continue_Click);

            Button btn_cancel = new Button();                                           // Button cancel                                     
            btn_cancel.Content = "Cancel";
            btn_cancel.Width = 215;
            btn_cancel.Click += new RoutedEventHandler(btn_cancel_Click);

            skt_pnl_inner.Children.Add(btn_continue);
            skt_pnl_inner.Children.Add(btn_cancel);


            skt_pnl_outter.Children.Add(skt_pnl_inner);

            // Adding stackpanel  to border
            border.Child = skt_pnl_outter;

            // Adding border to pup-up
            my_popup_cs.Child = border;

            my_popup_cs.VerticalOffset = 400;
            my_popup_cs.HorizontalOffset = 10;

            my_popup_cs.IsOpen = true;
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            if (my_popup_cs.IsOpen)
            {
                my_popup_cs.IsOpen = false;
            }
        }

        public void close_popup()
        {
            if (my_popup_cs.IsOpen) { my_popup_cs.IsOpen = false; }

        }
    }
}
