using UnityEngine;
using System.Collections;

public class Enemy : BoardPieceObject {

	private int currHealth;
	private int dmg;
	public int Damage { get { return dmg; } }

	private GameObject genericEnemyPrefab;
	private EnemyScript es;

	private string[] prefabs = new string[] {"Prefabs/Enemies/Rat", "Prefabs/Enemies/Goblin", "Prefabs/Enemies/Ogre", "Prefabs/Enemies/Skeleton"};

	public Enemy(int posX, int posY) {
		//Load random prefab (enemy type)
		string prefabChoice = prefabs[Random.Range(0,prefabs.Length)];
		GameObject prefabTemp = Resources.Load<GameObject>(prefabChoice);
		//start pos = level pos * translateUnits
		genericEnemyPrefab = (GameObject) GameManagerScript.Instantiate(prefabTemp, new Vector2(posX * GameManagerScript.translateUnits, posY * GameManagerScript.translateUnits), Quaternion.identity);
		es = genericEnemyPrefab.GetComponent<EnemyScript>();
		MapPosition = new Vector2(posX, posY);

		currHealth = es.health;
		dmg = es.damage;
	}

	/*
	 * returns true if enemy dies from damage
	 */
	public bool TakeDamage(int dmg) {
		currHealth -= dmg;
		if (currHealth <= 0) {
			es.Die(); //call from here?
			return true;
		} else {
			return false;
		}
	}

	public override void Move(Directions dir) {
		int h = (int) MapPosition.x;
		int v = (int) MapPosition.y;
		if (dir == Directions.North) {
			es.Move(new Vector3(0, GameManagerScript.translateUnits));
			MapPosition = new Vector2(h, ++v);
		} else if (dir == Directions.East) {
			es.Move(new Vector3(GameManagerScript.translateUnits, 0));
			MapPosition = new Vector2(++h, v);
		} else if (dir == Directions.South) {
			es.Move(new Vector3(0, -GameManagerScript.translateUnits));
			MapPosition = new Vector2(h, --v);
		} else if (dir == Directions.West) {
			es.Move(new Vector3(-GameManagerScript.translateUnits, 0));
			MapPosition = new Vector2(--h, v);
		}
	}

	public override string ToString() {
		if (es.enemyName != "")	return es.enemyName;
		else return es.enemyType.ToString();
	}
}
