using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySwordDamage : MonoBehaviour {

	private AudioSource audio;

	void Awake() {
		this.audio = GetComponent<AudioSource>();
	}

	void OnTriggerEnter2D(Collider2D col) {
		if(col.gameObject.CompareTag("Player")) {
			if (RestartManager.Instance().isMusic()) {
				audio.PlayOneShot(audio.clip);
			}
			col.gameObject.SendMessage("Damage", Mathf.Sign(col.gameObject.transform.position.x - this.transform.position.x) * 5);	
		}
	}
}
