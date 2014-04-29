using UnityEngine;
using System.Collections;

public class EnemyScript : MonoBehaviour {

	public enum EnemyType {Melee}

	[SerializeField] private int damage;
	public int Damage { get { return damage; } }
	public int minDamage;
	public int maxDamage;
	[SerializeField] private int health;
	public int Health { get { return health; } }
	public int minHealth;
	public int maxHealth;
	//public EnemyType enemyType;
	public string enemyName;

	void Awake() {
		damage = Random.Range(minDamage, maxDamage + 1);
		health = Random.Range(minHealth, maxHealth + 1);
	}

	public void Die() {
		Destroy(gameObject);
	}

	public void Move(Vector3 movement) {
		transform.Translate(movement);
	}
}
