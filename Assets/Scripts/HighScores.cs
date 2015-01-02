using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using Facebook.MiniJSON;

public class HighScores : MonoBehaviour {

	public Button connectFbButton;

	private bool isFbConnected = false;
	
	void Awake() {
		FB.Init(SetInit, OnHideUnity);  
	}

	
	private void SetInit()                                                                       
	{                                                                                            
		Debug.Log("SetInit");                                                                  
		if (FB.IsLoggedIn)                                                                       
		{                                                                                        
			Debug.Log("Already logged in");                                                    
			OnLoggedIn();                                                                        
		}                                                                                        
	}    

	public void connectFacebook() {
		Debug.Log("connectFacebook...");
		FB.Login("email,publish_actions,user_friends", LoginCallback);                                                        
	}
	
	private void OnHideUnity(bool isGameShown)                                                   
	{                                                                                            
		Debug.Log("OnHideUnity");                                                              
		if (!isGameShown)                                                                        
		{                                                                                        
			// pause the game - we will need to hide                                             
			Time.timeScale = 0;                                                                  
		}                                                                                        
		else                                                                                     
		{                                                                                        
			// start the game back up - we're getting focus again                                
			Time.timeScale = 1;                                                                  
		}                                                                                        
	}
	
	void LoginCallback(FBResult result)                                                        
	{                                                                                          
		Debug.Log("LoginCallback");                                                          
		
		if (FB.IsLoggedIn)                                                                     
		{                                                                                      
			OnLoggedIn();                                                                      
		}                                                                                      
	}                                                                                          
	
	void OnLoggedIn()                                                                          
	{                                                                                          
		Debug.Log("Logged in. ID: " + FB.UserId);                                            
		fetchScores();
	}

	void fetchScores() {
		Debug.Log ("Fetching scores...");
		FB.API (string.Format ("/{0}/scores", FB.AppId), Facebook.HttpMethod.GET, FetchScoreCallback);
	}

	void FetchScoreCallback(FBResult result)                                                                                              
	{                                                                                                                              
		if (result.Error != null)                                                                                                  
		{                                                                                                                          
			Debug.Log(result.Error);                                                                                                              
		}                                                                                                                          

		Debug.Log(result.Text);
		var scores = DeserializeScores(result.Text);
		//deserialize payload here                                                                       
	}

	//{"data":[{"user":{"id":"10152511422056765","name":"John Giannakos"},"score":0,"application":{"name":"Halos","id":"1520323738236537"}}]}

	class UserScore {
		public string Id { get; set; }
		public string Name { get; set; }
		public int Score { get; set; }
		public Texture ProfilePicture { get; set; }

		public UserScore() {
			//nothing
		}

		public UserScore(string id, string name, int score) {
			Id = id;
			Name = name;
			Score = score;
		}

		public string toString() {
			return string.Format ("id: {0}, name: {1}, score: {2}", Id, Name, Score);
		}
	}

	List<UserScore> DeserializeScores(string scoreJson) {
		var dict = Json.Deserialize(scoreJson) as Dictionary<string, object>;

		object friendScoresObj;
		var friendScoresObjList = new List<object>();
		var userScores = new List<UserScore>();

		if(dict.TryGetValue("data", out friendScoresObj)) {
			friendScoresObjList = friendScoresObj as List<object>;
			if(friendScoresObjList.Count > 0) {
				foreach(var o in friendScoresObjList) {
					var userObj = (Dictionary<string, object>)(o);
					var fbUserObj = ((Dictionary<string, object>)(userObj["user"]));

					var user = new UserScore();
					user.Id = fbUserObj["id"] as string;
					user.Name = fbUserObj["name"] as string;
					user.Score = System.Convert.ToInt32(userObj["score"]);
					if(user.Score > 0) {
						LoadPictureAPI(string.Format ("/{0}/picture?redirect=0&type=normal&width=100&height=100", user.Id), texture => {
							user.ProfilePicture = texture;
						});
						userScores.Add(user);
					}
				}
			}
		} else {
			Debug.Log ("No data");
		}
	
		return userScores;
	}

	/* Load Profile Functions */
	delegate void LoadPictureCallback (Texture texture);

	IEnumerator LoadPictureEnumerator(string url, LoadPictureCallback callback)    
	{
		WWW www = new WWW(url);
		yield return www;
		callback(www.texture);
	}
	void LoadPictureAPI (string url, LoadPictureCallback callback)
	{
		FB.API(url,Facebook.HttpMethod.GET,result =>
		       {
			if (result.Error != null)
			{
				Debug.Log ("error getting photo");
				return;
			}
			
			var imageUrl = DeserializePictureURLString(result.Text);
			
			StartCoroutine(LoadPictureEnumerator(imageUrl,callback));
		});
	}

	void LoadPictureURL (string url, LoadPictureCallback callback)
	{
		StartCoroutine(LoadPictureEnumerator(url,callback));
	}

	public static string DeserializePictureURLString(string response)
	{
		return DeserializePictureURLObject(Json.Deserialize(response));
	}
	
	public static string DeserializePictureURLObject(object pictureObj)
	{
		var picture = (Dictionary<string, object>)(((Dictionary<string, object>)pictureObj)["data"]);
		object urlH = null;
		if (picture.TryGetValue("url", out urlH))
		{
			Debug.Log("url value " + urlH);
			return (string)urlH;
		}
		
		return null;
	}


}
