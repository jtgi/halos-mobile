using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using Facebook.MiniJSON;

public class HighScores : MonoBehaviour {

	public Button connectFbButton;
	public GameObject scoreRowPrefab;
	public GameObject connectFbBtn;
	private GameObject leaderboard;

	void Awake() {
		if(!FB.IsLoggedIn) {
			FB.Init(SetInit, OnHideUnity);  
		}
	}

	public void Init() {
		ToggleHighScoreView();
		FetchScores();
	}

	void ToggleHighScoreView() {
		Debug.Log ("ToggleHighScoreView");
		if(!FB.IsLoggedIn) {
			connectFbBtn.SetActive(true);
		} else {
			connectFbBtn.SetActive(false);
		}
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
		PlayerPrefs.SetString("fb_id", FB.UserId);
		Init();
	}

	public void SubmitHighScore() {
		string userId = PlayerPrefs.GetString("fb_id");
		int points = (int)PlayerPrefs.GetFloat("highScore");

		Debug.Log("fb token " + FB.AccessToken);

		if(userId.Equals(string.Empty)) {
			Debug.Log("Error retreiving user_id from playerprefs");
			return;
		}

		FB.API(string.Format ("/{0}/scores?score={1}", userId, points), Facebook.HttpMethod.POST, result => {
			if(result.Error != null) {
				Debug.Log("error setting new high score");
				Debug.Log(result.Error);
				return;
			} else {
				Debug.Log("Updated score.");
			}
		});
	}

	public void FetchScores() {
		if(FB.IsLoggedIn) {
			Debug.Log ("Fetching scores...");
			FB.API (string.Format ("/{0}/scores", FB.AppId), Facebook.HttpMethod.GET, FetchScoreCallback);
		}
	}

	void FetchScoreCallback(FBResult result)                                                                                              
	{                                                                                                                              
		if (result.Error != null)                                                                                                  
		{                                                                                                                          
			Debug.Log(result.Error);                                                                                                              
		}                                                                                                                          

		Debug.Log(result.Text);
		var scores = DeserializeScores(result.Text);
		RenderScores(scores);
	}

	void RenderScores(List<UserScore> userScores) {
		Debug.Log ("RenderScores");
		int i = 1;

		if(leaderboard != null) {
			Destroy(leaderboard);
		}

		leaderboard = Instantiate(Resources.Load ("Leaderboard"), new Vector3(184f, -94f, 0f), Quaternion.identity) as GameObject;
		leaderboard.transform.SetParent(this.transform, false);

		Transform scoreList = leaderboard.transform.FindChild("ScoreList");

		foreach(var userScore in userScores) {
			
			GameObject row = Instantiate(scoreRowPrefab, Vector3.zero, Quaternion.identity) as GameObject;

			row.transform.FindChild("Rank").GetComponent<Text>().text = i.ToString() + ".";
			row.transform.FindChild("ProfilePicture").GetComponent<Image>().sprite = userScore.ProfilePicture;
			row.transform.FindChild("Name").GetComponent<Text>().text = userScore.Name;
			row.transform.FindChild("Score").GetComponent<Text>().text = userScore.Score.ToString ();
			row.transform.SetParent(scoreList.transform, false);

			
			i++;
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
						LoadPictureAPI(string.Format ("/{0}/picture?redirect=0&type=square&width=225&height=225", user.Id), sprite => {
							user.ProfilePicture = sprite;
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
	delegate void LoadPictureCallback (Sprite sprite);

	IEnumerator LoadPictureEnumerator(string url, LoadPictureCallback callback)    
	{
//		WWW www = new WWW(url);
		yield return null;
//
//		Sprite sprite = new Sprite();
//		sprite = Sprite.Create(www.texture, new Rect(0, 0, 225, 225), new Vector2(0, 0), 0.1f);

		callback(null);
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
