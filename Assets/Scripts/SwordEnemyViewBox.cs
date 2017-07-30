using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordEnemyViewBox : MonoBehaviour {

	void OnTriggerStay2D(Collider2D col) {
		if(col.gameObject.CompareTag("Player")) {
			gameObject.SendMessageUpwards("DetectPlayer", col.gameObject.transform.position);
		}
	}

	void OnTriggerExit2D(Collider2D col) {
		if(col.gameObject.CompareTag("Player")) {
			gameObject.SendMessageUpwards("LostPlayer");
		}
	}
}
