using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTrigger : MonoBehaviour {

	private AudioSource audio;

	void Awake() {
		this.audio = GetComponent<AudioSource>();
	}

	public int damage;

	void Start() {
		damage = 20;
	}

	void OnTriggerEnter2D(Collider2D col) {
		if(col.gameObject.CompareTag("Enemy")) {
			if (RestartManager.Instance().isMusic()) {
				audio.PlayOneShot(audio.clip);
			}
			float dmg = Random.Range(Mathf.Max(damage - 10, 0), damage);
			col.gameObject.SendMessage("Damage", Mathf.Sign(col.gameObject.transform.position.x - this.transform.position.x) * dmg);	
		}
	}
}
