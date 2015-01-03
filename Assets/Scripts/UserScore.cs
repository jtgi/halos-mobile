using UnityEngine;
using System.Collections;

class UserScore {
	public string Id { get; set; }
	public string Name { get; set; }
	public int Score { get; set; }
	public Sprite ProfilePicture { get; set; }
	
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

