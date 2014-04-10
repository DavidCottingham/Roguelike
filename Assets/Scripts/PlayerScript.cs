using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerScript : MonoBehaviour {

	public List<Item> Inventory {get; set;}
	public Item Weapon {get; set;}
	public int Health {get; set;}

	void Start() {
		DontDestroyOnLoad(this);
	}

	public void Move(Vector3 movement) {
		transform.Translate(movement);
	}

	public void SetWorldStartPos(Vector2 pos) {
		transform.position = pos;
	}
}
