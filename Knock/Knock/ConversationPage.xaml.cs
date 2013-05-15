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
using System.Windows.Markup;
using WP7Contrib.View.Controls.Extensions;
using System.Diagnostics;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Notification;
using System.Windows.Navigation;
using System.Windows.Media.Imaging;
using com.shephertz.app42.paas.sdk.windows;
using com.shephertz.app42.paas.sdk.windows.message;
using com.shephertz.app42.paas.sdk.windows.storage;
using com.shephertz.app42.paas.sdk.windows.review;

namespace Knock
{
    public partial class ConversationPage : PhoneApplicationPage,App42Callback
    {
        String uri;
        String userName;
        String owner;
        String fileName;
        String me;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            uri = NavigationContext.QueryString["uri"];
            if (NavigationContext.QueryString.ContainsKey("user")) { userName = NavigationContext.QueryString["user"]; }
            if (userName == null) 
            {
                owner = NavigationContext.QueryString["owner"];
            }
            fileName = NavigationContext.QueryString["fileName"];
            GetMessages(fileName);
            LoadImage(uri);
           
        }
        private MessageCollection messages;
        private Storyboard scrollViewerStoryboard;
        private DoubleAnimation scrollViewerScrollToEndAnim;

        #region VerticalOffset DP

        /// <summary>
        /// VerticalOffset, a private DP used to animate the scrollviewer
        /// </summary>
        private DependencyProperty VerticalOffsetProperty = DependencyProperty.Register("VerticalOffset",
          typeof(double), typeof(ConversationPage), new PropertyMetadata(0.0, OnVerticalOffsetChanged));

        private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ConversationPage app = d as ConversationPage;
            app.OnVerticalOffsetChanged(e);
        }

        private void OnVerticalOffsetChanged(DependencyPropertyChangedEventArgs e)
        {
            ConversationScrollViewer.ScrollToVerticalOffset((double)e.NewValue);
        }

        #endregion

        // Constructor
        public ConversationPage()
        {
           InitializeComponent();
           messages = new MessageCollection();
           this.DataContext = messages;

            scrollViewerScrollToEndAnim = new DoubleAnimation()
            {
                Duration = TimeSpan.FromSeconds(1),
                EasingFunction = new SineEase()
            };
            Storyboard.SetTarget(scrollViewerScrollToEndAnim, this);
            Storyboard.SetTargetProperty(scrollViewerScrollToEndAnim, new PropertyPath(VerticalOffsetProperty));

            scrollViewerStoryboard = new Storyboard();
            scrollViewerStoryboard.Children.Add(scrollViewerScrollToEndAnim);
            this.Resources.Add("foo", scrollViewerStoryboard);
        }
       
        private void SendButton_Click(object sender, EventArgs e)
        {
            this.Focus();
            SendMessage(userName, InputText.Text);
            
        }

        private void TextInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ConversationContentContainer.ActualHeight < ConversationScrollViewer.ActualHeight)
            {
                PaddingRectangle.Show(() => ScrollConvesationToEnd());
            }
            else
            {
                ScrollConvesationToEnd();
            }

            ApplicationBar.IsVisible = true;
        }

        private void ScrollConvesationToEnd()
        {
            scrollViewerScrollToEndAnim.From = ConversationScrollViewer.VerticalOffset;
            scrollViewerScrollToEndAnim.To = ConversationContentContainer.ActualHeight;
            scrollViewerStoryboard.Begin();
        }

        private void TextInput_LostFocus(object sender, RoutedEventArgs e)
        {
            PaddingRectangle.Hide();
            ApplicationBar.IsVisible = false;
            ScrollConvesationToEnd();
        }

        private void LoadImage(String uri)
        {
            // Load a new image
            BitmapImage bitmapImage;
            bitmapImage = new BitmapImage(new Uri(uri, UriKind.RelativeOrAbsolute));
            image.Source = bitmapImage;
        }

        // Send comment 
        private void SendMessage(String userName,String message)
        {
            using (DbStorage fbdb = new DbStorage(MainPage.strConnectionString))
            {
                IQueryable<Db> fbQuery = from db in fbdb.user select db;
                Db ac = fbQuery.FirstOrDefault();
                if (ac != null)
                {
                    // Adding comment on image with your buddy.
                    Util.reviewService.AddComment(ac.Name,fileName,message,this);
                }

            }
             
        }

        private void GetMessages(String file) 
        {
            // Get all comment on image.
            Util.reviewService.GetCommentsByItem(fileName,this);
        }

        // callback when comments will be receive or add.
        void App42Callback.OnSuccess(object response)
        {
            using (DbStorage fbdb = new DbStorage(MainPage.strConnectionString))
            {
                IQueryable<Db> fbQuery = from db in fbdb.user select db;
                Db ac = fbQuery.FirstOrDefault();
                if (ac != null)
                {
                    me = ac.Name;
                }

            }

            if (response is IList<Review>)
            {
                IList<Review> reviewList = (IList<Review>)response;
                for (int i = 0; i < reviewList.Count; i++)
                {
                    String userId = reviewList[i].GetUserId();
                    String message = reviewList[i].GetComment();
                    DateTime time = reviewList[i].GetCreatedOn();
                    if (userId == me)
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {

                            messages.Add(new Message()
                            {

                                Side = MessageSide.Me,
                                Text = message,
                                Timestamp = time
                            });
                        });
                    }
                    else
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {

                            messages.Add(new Message()
                            {

                                Side = MessageSide.You,
                                Text = message,
                                Timestamp = time
                            });
                        });
                    }
                }
            }
            else if (response is Review)
            {
                Review review = (Review)response;
                String message = review.GetComment();
                DateTime time = review.GetCreatedOn();
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {

                    messages.Add(new Message()
                    {

                        Side = MessageSide.Me,
                        Text = message,
                        Timestamp = time
                    });
                });
            }

        }

        // callback when get an Exception.
        void App42Callback.OnException(App42Exception exception)
        {
            if (exception.GetAppErrorCode().Equals(3107))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Currently No Comments Found On This Image.");
                });
            } 
            else if (exception.GetAppErrorCode().Equals(3106))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Same Comment Could not be send.");
                });
            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show(exception.Message.ToString());
                });
            }
            
        }
     }
}