App42-Photo-Sharing
===================

App42 Client SDK sample for Window Phone application

# About App:

App42-Photo-Sharing app is based on manual photo sharing for Windows Phone via [App42CloudAPI] (http://api.shephertz.com/apis). Idea behind this to share photo with your friend
and put a comment in to it. This app has all friends from facebook, select a friend from list and share. This app nothing to share on your 
facebook wall or your friend facebook wall.  


# Runnnig Sample:

1. [Register] (https://apphq.shephertz.com/register) with App42 platform
2. Create an app once you are on Quickstart page after registeration.
3. Download the project from this repo and open it in Microsoft Visual Studio.
4. Open Util.cs in app and give the value of app42APIkey app42SecretKey in ServiceAPI.
5. Change storageDbName and storageCollName variable in Util.cs to your storage database and storage collection.
5. You can also modify your appId variable in MainPage.cs to pass your own facebook app credentials.
6. Build and Run 

#Design Details:

__Initialize Services :__

First you initialize the ServiceAPI with your APIKey and SecretKey from Running Sample step #1 and #2. And before this you can build UploadService from ServiceAPI object.
By using UploadService you can upload images and file to App42CDN, fetch from it.
Three other Services are use for this app. 

StorageService(NoSQL Storage) : database creation for your app on the cloud, Store app data in JSON objects at App42 Cloud
ReviewService : AddComment,Review and Rating on your app item.

SocialService : Social Connect to your App.

```
ServiceAPI serviceAPI = new ServiceAPI("YOUR API KEY", "YOUR SECRET KEY");
UploadService uploadService = serviceAPI.BuildUploadService();
StorageService storageService = serviceAPI.BuildStorageService();
ReviewService reviewService = serviceAPI.BuildReviewService();
SocialService socialService = serviceAPI.BuildSocialService();
```

__Get Facebook Friends :__

After initilize you get your facebook friends from social object by using GetFacebookFriendsOAuth. This method require facebook access token
and your callback class reference that implements App42Callback interface.

This methods return callback in App42Callback.OnSuccess.

```
socialService.GetFacebookFriendsOAuth(fbAccessToken,yourclass);
void App42Callback.OnSuccess(object response)
{
        Social social = (Social)response;
	IList<Social.Friends> fbList = social.GetFriendList();
	for (int i = 0; i < fbList.Count; i++)
	{
		String id = fbList[i].GetId();
		String name = fbList[i].GetName();
		String picture = fbList[i].GetPicture();      
	}
}    
```        
        
__Choose Photo From Gallery :__ 

When you get your friend list from facebook, you can share photos with your friend by using App42 API. Windows Phone provides PhotoChooserTask to select a Photo
from phone gallery and returns photo stream in PhotoResult object when PhotoChooserTask is Completed.

```
PhotoChooserTask objPhotoChooser = new PhotoChooserTask();
objPhotoChooser.Show();
objPhotoChooser.Completed += new EventHandler<PhotoResult>(PhotoChooserTask_Completed);   
```  

__Upload Photo To Your Friend :__

Upload Photo with user you must mention ptoto name, user name , photo stream, ptoto type and ptoto description.

```     
private void PhotoChooserTask_Completed(object sender, PhotoResult e)
{
	uploadService.UploadFileForUser(imageName, friendName, e.ChosenPhoto, "IMAGE", description, your callback class);
}
```  
  
__Add Additional Info For Image And Friend :__

Callback from server when photo wiill be successfully upload. In this app we are use storageService to save upload object to NOSQL
stoarge, to save additional information regarding photo like whose is the owner of photo with his facebook unique id, image url from upload object,
designated user of photo and his facebook unique id ect.

```
// Call When Upload File Successfully Uploaded
void App42Callback.OnSuccess(object response)
{
	Upload upload = (Upload)response;
	IList<Upload.File> fileList = upload.GetFileList();
	String name;
	String type;
	String url;
	String description
	for (int i = 0; i < fileList.Count; i++)
	{
		name = fileList[i].GetName();
		type = fileList[i].GetType();
		url = fileList[i].GetUrl();
		description = fileList[i].GetDescription();
	}
	
	// save upload object to NOSQL.
	
	JObject myJson = new JObject();
	myJson.Add("ownerId", ownerFacebookId);
	myJson.Add("ownerName", ownerName);
	myJson.Add("userId", userFacebookId);
	myJson.Add("userName", userName);
	myJson.Add("imageName", name);
	myJson.Add("imageUrl", url);
	myJson.Add("description", description);
	storageService.InsertJSONDocument(storageDbName, storageCollName, myJson.ToString(), this);
}
```

__Get Received Photos :__

```
storageService.FindDocumentByKeyValue(storageDbName, storageCollName, "userId", myFacebookId, this);
void App42Callback.OnSuccess(object response)
{
	Storage storage = (Storage)response;
	IList<Storage.JSONDocument> JsonDocList = storage.GetJsonDocList();
	for (int i = 0; i < JsonDocList.Count; i++)
	{
		JObject jsonObj = JObject.Parse(JsonDocList[i].GetJsonDoc());
		string ownerName = (string)jsonObj["ownerName"];
		string url = (string)jsonObj["imageUrl"];
		string imageName = (string)jsonObj["imageName"];
	}
}
```

__Get Shared Photos :__

```
storageService.FindDocumentByKeyValue(Util.storageDbName, Util.storageCollName, "ownerId", myfacebookId, this);
void App42Callback.OnSuccess(object response)
{
	Storage storage = (Storage)response;
	IList<Storage.JSONDocument> JsonDocList = storage.GetJsonDocList();
	for (int i = 0; i < JsonDocList.Count; i++)
	{
		JObject jsonObj = JObject.Parse(JsonDocList[i].GetJsonDoc());
		string userName = (string)jsonObj["userName"];
		string url = (string)jsonObj["imageUrl"];
		string imageName = (string)jsonObj["imageName"];
	}
}
```

__Add Comments To Photo :__

According to this app make sure your image name should be unique. Besause if you need to comment any photo there is a item id in this API 
```
reviewService.AddComment(myFacebookName,fileName,message,this);
```

__Get Comments To Photo :__

```
reviewService.GetCommentsByItem(fileName,this);
// callback when comments will be receive or add.
void App42Callback.OnSuccess(object response)
{
	// callback when getAllComments loded
	if (response is IList<Review>)
	{
		IList<Review> reviewList = (IList<Review>)response;
		for (int i = 0; i < reviewList.Count; i++)
		{
			String userId = reviewList[i].GetUserId();
			String message = reviewList[i].GetComment();
			DateTime time = reviewList[i].GetCreatedOn();
		}
	}
	// callback when comment add
	else if (response is Review)
	{
		Review review = (Review)response;
		String message = review.GetComment();
		DateTime time = review.GetCreatedOn();
	}
}
```

