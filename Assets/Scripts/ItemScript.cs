using UnityEngine;
using System.Collections;

public class ItemScript : MonoBehaviour {

	public ItemClass itemClass;
	[SerializeField] private int stat = 1;
	public int MainStat { get { return stat; } } //dmg if weapon or armor if armor
	public int minStat;
	public int maxStat;
	public string itemName;

	void Awake() {
		stat = Random.Range(minStat, maxStat + 1);
		print(stat);
	}

	public void RemoveFromScreen() {
		Destroy(gameObject);
	}
}
