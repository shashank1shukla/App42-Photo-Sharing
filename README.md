App42-Photo-Sharing
===================

App42 Client SDK sample for Window Phone application

# Runnnig Sample:

1. [Register] (https://apphq.shephertz.com/register) with App42 platform
2. Create an app once you are on Quickstart page after registeration.
3. Download the project from this repo and open it in Microsoft Visual Studio.
4. Open Util.cs in app and give the value of app42APIkey app42SecretKey in ServiceAPI.
5. Change storageDbName and storageCollName variable in Util.cs to your storage database and storage collection.
5. You can also modify your appId variable in MainPage.cs to pass your own facebook app credentials.
6. Build and Run 

#Design Details:

__Initilize Services :__

```
ServiceAPI serviceAPI = new ServiceAPI("YOUR API KEY", "YOUR SECRET KEY");
UploadService uploadService = serviceAPI.BuildUploadService();
StorageService storageService = serviceAPI.BuildStorageService();
ReviewService reviewService = serviceAPI.BuildReviewService();
SocialService socialService = serviceAPI.BuildSocialService();
```

__Get Facebook Friends :__

```
socialService.GetFacebookFriendsOAuth(fbAccessToken,this);
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

```
PhotoChooserTask objPhotoChooser = new PhotoChooserTask();
objPhotoChooser.Show();
objPhotoChooser.Completed += new EventHandler<PhotoResult>(PhotoChooserTask_Completed);   
```  

__Upload Photo To Your Friend :__

```     
private void PhotoChooserTask_Completed(object sender, PhotoResult e)
{
	uploadService.UploadFileForUser(imageName, friendName, e.ChosenPhoto, "IMAGE", description, this);
}
```  
  
__Add Additional Info For Image And Friend :__

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

