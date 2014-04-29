using UnityEngine;
using System.Collections;

public enum ItemClass {MeleeWeapon, Helmet, ChestArmor}

public class Item : BoardPieceObject {

	public ItemClass itemClass;
	public int mainStat;

	private GameObject genericItemPrefab;
	private ItemScript itemScript;
	private string[] prefabs = new string[] {"Prefabs/Items/Leather Cap", "Prefabs/Items/Leather Chest Armor", "Prefabs/Items/Steel Chest Armor", "Prefabs/Items/Steel Helmet", "Prefabs/Items/Steel Sword", "Prefabs/Items/Wooden Sword"};
	
	public Item(int posH, int posV) {
		string prefabChoice = prefabs[Random.Range(0,prefabs.Length)];
		GameObject prefabTemp = Resources.Load<GameObject>(prefabChoice);
		Vector3 startPos = new Vector3(posH * GameManagerScript.translateUnits, posV * GameManagerScript.translateUnits);
		genericItemPrefab = (GameObject) GameManagerScript.Instantiate(prefabTemp, startPos, Quaternion.identity);
		itemScript = genericItemPrefab.GetComponent<ItemScript>();
		MapPosition = new Vector2(posH, posV);

		itemClass = itemScript.itemClass;
		mainStat = itemScript.MainStat;
	}

	public void RemoveFromScreen() {
		itemScript.RemoveFromScreen();	
	}

	public override string ToString() {
		string name;
		if (itemScript.itemName != "") name = itemScript.itemName;
		else name = itemClass.ToString();
		return name + " (+" + mainStat + ")";
	}
}
