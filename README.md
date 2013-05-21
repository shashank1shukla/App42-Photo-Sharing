App42-Photo-Sharing
===================

App42 Client SDK sample for Window Phone application

# About App:

This sample demonstrate the idea of one-to-one photo sharing between two people and posting comments on the shared photograph by both of them. One can pick the friend from his facebook friend list and can share his pic. It uses App42 Cloud API for back-end support and uses Storage, Social and File storage APIs to implement this.


# Running Sample:

1. <a href="https://apphq.shephertz.com/register" target="blank">Register</a> with App42 platform
2. Create an app once you are on Quickstart page after registration.
3. Download the project from this repo and open it in Microsoft Visual Studio.
4. Open Util.cs in app and give the value of APIKey and SECRETKey in ServiceAPI.
5. Change storageDbName and storageCollName variable in Util.cs to your storage database and storage collection.
5. You can also modify your appId variable in MainPage.cs to pass your own facebook app credentials.
6. Build and Run 

#Design Details:

__Initialize Services :__

Initialize  ServiceAPI instance with your APIKey and SecretKey recieved in step #2 above. Once it is initialized, you can build target service object by calling buildXXXXXService on ServiceAPI instance. Below is the snippet for the same.


```
ServiceAPI serviceAPI = new ServiceAPI("YOUR API KEY", "YOUR SECRET KEY");
UploadService uploadService = serviceAPI.BuildUploadService();
StorageService storageService = serviceAPI.BuildStorageService();
ReviewService reviewService = serviceAPI.BuildReviewService();
SocialService socialService = serviceAPI.BuildSocialService();
```

__Get Facebook Friends :__

Call GetFacebookFriendsOAuth method on social service once you have build it as shown above. This method require facebook access token
and your callback object reference that implements App42Callback interface.

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

Upload Photo with user you must mention photo name, user name , photo stream, ptoto type and photo description.

```     
private void PhotoChooserTask_Completed(object sender, PhotoResult e)
{
	uploadService.UploadFileForUser(imageName, friendName, e.ChosenPhoto, "IMAGE", description, your callback class);
}
```  
  
__Add Additional Info For Image And Friend :__

Callback from server when photo will be successfully upload. In this app we have used Storage Service to save additional information as JSON object using Storage Service.

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
	
	// save upload object to NoSQL.
	
	JObject myJson = new JObject();
	myJson.Add("ownerId", ownerFacebookId);
	myJson.Add("ownerName", ownerName);
	myJson.Add("userId", userFacebookId);
	myJson.Add("userName", userName);
	myJson.Add("imageName", name);
	myJson.Add("imageUrl", url);
	myJson.Add("description", description);
	//This will save all additional information in given dbName/collection
	storageService.InsertJSONDocument(storageDbName, storageCollName, myJson.ToString(), this);
}
```

__Get Received Photos :__

Now call storage service to fetch these information which were saved in above step.

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

Following storage query get relational information about photo. Here key has been passed as ownerId to identify shared images.

```
String keyName = "ownerId";
storageService.FindDocumentByKeyValue(Util.storageDbName, Util.storageCollName, keyName, myfacebookId, this);
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

AddComment method of review service has been used to put comments on photo.
```
reviewService.AddComment(myFacebookName,fileName,message,this);
```

__Get Comments To Photo :__

And finally will get all comments on photo by using GetCommentsByItem of review service.

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

