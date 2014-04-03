using UnityEngine;
using System.Collections;

public class EnemyScript : MonoBehaviour {

	//Any need for EnemyType? Only used as "backup" name, currently ><
	public enum EnemyType {Rat, Goblin, Skeleton, Ogre}
	//TODO complete enum enemytype

	public int damage;
	public int health;
	public EnemyType enemyType;
	public string enemyName;

	public void Die() {
		Destroy(gameObject);
	}

	public void Move(Vector3 movement) {
		transform.Translate(movement);
	}
}
