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
using Facebook;
using System.Collections.Generic;
using System.Linq;
using com.shephertz.app42.paas.sdk.windows;
using com.shephertz.app42.paas.sdk.windows.upload;
using com.shephertz.app42.paas.sdk.windows.storage;
using com.shephertz.app42.paas.sdk.windows.message;
using com.shephertz.app42.paas.sdk.windows.review;
using com.shephertz.app42.paas.sdk.windows.social;

namespace Knock
{
    public class Util
    {
        /// <summary>
        ///  App42 initialize with your API KEY and SECRET KEY.
        ///  If you don't have API KEY and SECRET KEY please register with us https://apphq.shephertz.com/register 
        ///  Build required services to your app. 
        ///  Get my facebook feed.
        /// </summary>
        String _accessToken;
        public static ServiceAPI serviceAPI = new ServiceAPI("YOUR API KEY", "YOUR SECRET KEY");
        public static UploadService uploadService = serviceAPI.BuildUploadService();
        public static StorageService storageService = serviceAPI.BuildStorageService();
        public static ReviewService reviewService = serviceAPI.BuildReviewService();
        public static SocialService socialService = serviceAPI.BuildSocialService();
        // storage db name
        public const String storageDbName = "Your db";
        // storage collection name.
        public const String storageCollName = "Your Collection";
        public Util(String accessToken)  
        {
            _accessToken = accessToken;  
        }  

        // My facebook feed.
        public void MyProfile()
        {
            var fb = new FacebookClient(_accessToken);
            fb.GetCompleted +=
                (o, ex) =>
                {

                    var feed = (IDictionary<String, object>)ex.GetResultData();
                    var me = feed["picture"] as JsonObject;
                    var pic = me["data"] as JsonObject;
                    String picture = pic["url"].ToString();
                    String name = feed["name"].ToString();
                    String id = feed["id"].ToString();
                    UpdateProfile(name, id, picture);
                    
                };

            var parameters = new Dictionary<String, object>();
            parameters["fields"] = "id,name,picture";
            fb.GetAsync("me", parameters);
        }


        // save my facebook feed to local db.
        private void UpdateProfile(String Name, String FbId, String Picture)
        {
            using (DbStorage fbdb = new DbStorage(MainPage.strConnectionString))
            {
                IQueryable<Db> fbQuery = from db in fbdb.user where db.AccessToken == _accessToken select db;
                Db ac = fbQuery.FirstOrDefault();
                if (ac != null)
                {
                    // save the current user info in local database.
                    ac.Name = Name;
                    ac.FbId = FbId;
                    ac.Picture = Picture;
                    fbdb.SubmitChanges();
                }

            }
        }
    
    }
}
