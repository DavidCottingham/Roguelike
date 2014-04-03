using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Player : BoardPieceObject {

	private GameObject player; //loaded in constructor
	private PlayerScript ps;
	private List<Item> inventory;
	private Item weapon;

	public int Damage { get {
		if (weapon != null) return weapon.mainStat;
		else return baseDmg; }
	}

	private int health = 30;
	private int totalArmor = 0;
	private int baseDmg = 1;

	public Player(Vector2 startPos) {
		GameObject prefabTemp = Resources.Load<GameObject>("Prefabs/Player");
		player = (GameObject) GameManagerScript.Instantiate(prefabTemp, startPos * GameManagerScript.translateUnits, Quaternion.identity);
		ps = player.GetComponent<PlayerScript>();
		MapPosition = startPos;

		inventory = new List<Item>();
	}

	public override void Move(Directions dir) {
		int h = (int) MapPosition.x;
		int v = (int) MapPosition.y;
		if (dir == Directions.North) {
			ps.Move(new Vector3(0, GameManagerScript.translateUnits));
			MapPosition = new Vector2(h, ++v);
		} else if (dir == Directions.East) {
			ps.Move(new Vector3(GameManagerScript.translateUnits, 0));
			MapPosition = new Vector2(++h, v);
		} else if (dir == Directions.South) {
			ps.Move(new Vector3(0, -GameManagerScript.translateUnits));
			MapPosition = new Vector2(h, --v);
		} else if (dir == Directions.West) {
			ps.Move(new Vector3(-GameManagerScript.translateUnits, 0));
			MapPosition = new Vector2(--h, v);
		}
	}

	public void PickUpItem(Item item) {
		//TODO finish pickup item
		//if upgrade, add to equip, move old to inventory?
		DebugGUI.AddToMessageLog(DebugGUI.Sides.RIGHT, "You picked up " + item.ToString());
		if (item.itemClass == ItemClass.MeleeWeapon) {
			if (weapon == null || item.mainStat > weapon.mainStat) {
				weapon = item;
				DebugGUI.AddToMessageLog(DebugGUI.Sides.RIGHT, "You equipped " + item.ToString());
			} else { //weaker weapon
				inventory.Add(weapon);
			}
			if (weapon != null) inventory.Add(weapon);
		} else if (item.itemClass == ItemClass.ChestArmor || item.itemClass == ItemClass.Helmet) {
			inventory.Add(item);
			CalcArmor();
		} else {
			//else add whatever it is to inventory
			inventory.Add(item);
		}
		item.RemoveFromScreen();
	}

	public void ListInv() {
		if (weapon != null) {
			Debug.Log("*" + weapon.ToString());
		}
		if (inventory.Count > 0) {
			foreach (Item item in inventory) {
				Debug.Log(item.ToString());
			}
		}
	}

	public void PlayerStats() {
		DebugGUI.Message(DebugGUI.Sides.RIGHT, "Health: " + health);
		DebugGUI.Message(DebugGUI.Sides.RIGHT, "Damage: " + Damage + (weapon == null ? "" : (" from " + weapon.ToString())));
		DebugGUI.Message(DebugGUI.Sides.RIGHT, "Armor: " + totalArmor);
	}

	public void PrintStats() {
		Debug.Log("Health: " + health);
		Debug.Log("Damage: " + Damage + (weapon == null ? "" : (" from " + weapon.ToString())));
		Debug.Log("Armor: " + totalArmor);
	}

	//FUTURE equip items instead of just calcing armor. that way can tell player what was equipped
	private void CalcArmor() {
		int bestChest = 0;
		int bestHelm = 0;
		foreach (Item item in inventory) {
			if (item.itemClass == ItemClass.ChestArmor) {
				if (item.mainStat > bestChest) {
					bestChest = item.mainStat;
				}
			} else if (item.itemClass == ItemClass.Helmet) {
				if (item.mainStat > bestHelm) {
					bestHelm = item.mainStat;
				}
			}
		}
		totalArmor = bestHelm + bestChest;
	}

	public bool TakeDamage(int dmg, string enemyName) {
		//incoming damage mitigated by best armor in inventory
		if (totalArmor < dmg) {
			dmg -= totalArmor;
		} else dmg = 0;
		health -= dmg;
		DebugGUI.AddToMessageLog(DebugGUI.Sides.LEFT, enemyName + " has attacked you for " + dmg + " damage");
		if (health <= 0) {
			return true;
		} else {
			return false;
		}
	}
}
