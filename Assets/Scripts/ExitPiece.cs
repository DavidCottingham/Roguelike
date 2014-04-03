using UnityEngine;
using System.Collections;

public class ExitPiece : BoardPieceObject {
	public ExitPiece(int posH, int posV) {
		MapPosition = new Vector2(posH, posV);
		GameObject prefab = Resources.Load<GameObject>("Prefabs/Exit");
		GameManagerScript.Instantiate(prefab, MapPosition * GameManagerScript.translateUnits, Quaternion.identity);
	}
}
