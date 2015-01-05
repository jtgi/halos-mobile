using UnityEngine;
using System.Collections;

public class MissDetector : MonoBehaviour {

	public GameObject player;
	private PlayerCollider playerScript;
	private GameObject missDetector;
	private int playerHeightOffset = 20;

	void Update() {
		gameObject.transform.position = new Vector3(gameObject.transform.position.x, player.transform.position.y + playerHeightOffset, gameObject.transform.position.z);
	}

	void OnTriggerEnter(Collider col) {
		if(col.tag == "Ring") {
			DonutState donut = col.GetComponentInParent<DonutState>();
			player.GetComponent<PlayerCollider>().OnRingCollision(donut);
		}
	}

}
