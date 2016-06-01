# Poz1.AzureHyperLapse

Sample Xamarin.Forms App that uses Azure HyperLapse Cognitive Service.

Please remember to add your subscription key in the MainPage.cs (you can obtain one for free [here](https://www.microsoft.com/cognitive-services/en-us/subscriptions))

In this project you will find the [Azure Cognitive Services SDK](https://github.com/Microsoft/ProjectOxford-ClientSDK) in form of PCL instead of Nuget in order to be able to change the PCL profile.

At the moment the projects uploads a video, waits for it to be processed and saves it to the local storage.
I am working on a crossplatform VideoView to improve this sample.
