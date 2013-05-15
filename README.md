App42-Photo-Sharing
===================

App42 Client SDK sample for Window Phone application

# Runnnig Sample:

1. [Register] (https://apphq.shephertz.com/register) with App42 platform
2. Create an app once you are on Quickstart page after registeration.
3. Download the project from this repo and open it in Microsoft Visual Studio.
4. Open Util.cs in app and give the value of app42APIkey app42SecretKey in ServiceAPI.
5. You can also modify your appId variable in MainPage.cs to pass your own facebook app credentials.
6. Build and Run 

# Design Details:

1. Initilize Services

        ServiceAPI serviceAPI = new ServiceAPI("YOUR API KEY", "YOUR SECRET KEY");
        UploadService uploadService = serviceAPI.BuildUploadService();
        StorageService storageService = serviceAPI.BuildStorageService();
        ReviewService reviewService = serviceAPI.BuildReviewService();
        SocialService socialService = serviceAPI.BuildSocialService();





