using UnityEngine;
using System.Collections;

public class ItemScript : MonoBehaviour {

	public ItemClass itemClass;
	public int mainStat = 1; //dmg if weapon or armor if armor
	public string itemName;

	public void RemoveFromScreen() {
		Destroy(gameObject);
	}
}
