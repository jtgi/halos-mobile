﻿using UnityEngine;
using System.Collections;
using Facebook.MiniJSON;

public class MainMenu : MonoBehaviour {

	void Awake() {
		// Initialize FB SDK              
		enabled = false;                  
		FB.Init(SetInit, OnHideUnity);  
	}

	void OnGUI() {
//		if (!FB.IsLoggedIn)                                                                                              
//		{                                                                                                                
//			if (GUI.Button(LoginButtonRect, "", MenuSkin.GetStyle("button_login")))                                      
//			{                                                                                                            
//				FB.Login("email,publish_actions", LoginCallback);                                                        
//			}                                                                                                            
//		}    
	}

	public void PlayOnClick() {
		Debug.Log ("Clicked");
		Application.LoadLevel("Freefall");
	}

	private void SetInit()                                                                       
	{                                                                                            
		Debug.Log("SetInit");                                                                  
		enabled = true; // "enabled" is a property inherited from MonoBehaviour                  
		if (FB.IsLoggedIn)                                                                       
		{                                                                                        
			Debug.Log("Already logged in");                                                    
			OnLoggedIn();                                                                        
		}                                                                                        
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
	} 

}