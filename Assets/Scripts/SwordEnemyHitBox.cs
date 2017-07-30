using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordEnemyHitBox : MonoBehaviour {

	void OnTriggerStay2D(Collider2D col) {
		if(col.gameObject.CompareTag("Player")) {
			gameObject.SendMessageUpwards("StartAttack");
		} else if (col.gameObject.CompareTag("Wall")) {
			gameObject.SendMessageUpwards("CloseToWall");
		}
	}
}
