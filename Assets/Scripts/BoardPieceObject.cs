using UnityEngine;
using System.Collections;

public class BoardPieceObject {
	public Vector2 MapPosition { get; set; } //probably better way than vector2 property

	public virtual void Move(Directions dir) {
		//do nothing by default
	}
}