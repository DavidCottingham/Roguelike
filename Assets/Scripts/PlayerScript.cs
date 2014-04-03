using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour {

	public void Move(Vector3 movement) {
		transform.Translate(movement);
	}
}
