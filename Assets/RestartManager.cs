using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartManager : MonoBehaviour {

	private bool isRestarting = false;
	public void SetRestart() {
		this.isRestarting = true;
	}

	public bool IsRestarting() {
		return isRestarting;
	}

	private bool music = true;

	public bool isMusic() {
		return music;
	}

	public void ToggleMusic() {
		music = !music;
	}

	private static RestartManager _instance;
 
	 void Awake()
	{
		//Check if instance already exists
		if (_instance == null) {
                
			//if not, set instance to this
			_instance = this;
            
            //If instance already exists and it's not this:
		} else if (_instance != this) {
                
                //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
                Destroy(gameObject);    
		}   
		//Sets this to not be destroyed when reloading scene
		DontDestroyOnLoad(gameObject);
            
	}

	public static RestartManager Instance() {
		return _instance;
	}
}
