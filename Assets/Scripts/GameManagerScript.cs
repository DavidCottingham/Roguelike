using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public enum Directions {North, East, South, West}

public class GameManagerScript : MonoBehaviour {

	public GameObject wall; //assigned in inspector

	public static float translateUnits = 2f;
	private int width = 50;
	private int height = 20;
	private BoardPieceObject[,] level;

	private Player player; //in start

	private int numEnemies = 4;
	private int numItems = 4;

	private int playerRadius = 2;

	void Start() {
		level = new BoardPieceObject[width, height];

		//FUTURE don't make new player. persist it (use factory) so player keeps inventory and health
		player = new Player(new Vector2(6, 3)); //TEMP spawn Position
		level[6,3] = player;

		//WALL perimeter
		//add walls at 1- mins and 1+ maxes. add corners
		//FUTURE nest walls in GO
		for (int i = 0; i <= width; ++i) {
			Vector3 pos = new Vector3(i * translateUnits, -1 * translateUnits);
			Instantiate(wall, pos, Quaternion.identity);
			pos = new Vector3(i * translateUnits, height * translateUnits);
			Instantiate(wall, pos, Quaternion.identity);
		}
		for (int i = 0; i <= height; ++i) {
			Vector3 pos = new Vector3(-1 * translateUnits, i * translateUnits);
			Instantiate(wall, pos, Quaternion.identity);
			pos = new Vector3(width * translateUnits, i * translateUnits);
			Instantiate(wall, pos, Quaternion.identity);
		}
		//4 corners
		//FUTURE check if can do corners in loop
		Instantiate(wall, new Vector3(-1 * translateUnits, -1 * translateUnits), Quaternion.identity);
		Instantiate(wall, new Vector3(-1 * translateUnits, height * translateUnits), Quaternion.identity);
		Instantiate(wall, new Vector3(width * translateUnits, -1 * translateUnits), Quaternion.identity);
		Instantiate(wall, new Vector3(width * translateUnits, height * translateUnits), Quaternion.identity);
		
		int spwnH = 0;
		int spwnV = 0;
		while (true) {
			spwnH = Random.Range(0, width);
			spwnV = Random.Range(0, height);
			if (level[spwnH, spwnV] == null) { break; }
		}
		level[spwnH, spwnV] = new ExitPiece(spwnH, spwnV);

		//SPAWN ENEMIES in random positions
		for (int i = 0; i < numEnemies; ++i) {
			//emptySpace = false;
			spwnH = 0;
			spwnV = 0;
			while (true) {
				spwnH = Random.Range(0, width);
				spwnV = Random.Range(0, height);
				//TODO empty Space check to own method so can use for enemy movement too
				if (level[spwnH, spwnV] == null) { break; }
			}
			level[spwnH, spwnV] = new Enemy(spwnH, spwnV);
		}

		//SPAWN ITEMS in random positions
		for (int i = 0; i < numItems; ++i) {
			//emptySpace = false;
			spwnH = 0;
			spwnV = 0;
			while (true) {
				spwnH = Random.Range(0, width);
				spwnV = Random.Range(0, height);
				if (level[spwnH, spwnV] == null) { break; }
			}
			level[spwnH, spwnV] = new Item(spwnH, spwnV);
		}
	}

	void Update() {
		DebugGUI.Message("H: " + player.MapPosition.x + " | V: " + player.MapPosition.y);
		player.PlayerStats();
		//DebugGUI.Message(PrintLevel());

		if (Input.GetButtonDown("Right")) {
			PlayerMove(Directions.East);
		} else if (Input.GetButtonDown("Left")) {
			PlayerMove(Directions.West);
		} else if (Input.GetButtonDown("Up")) {
			PlayerMove(Directions.North);
		} else if (Input.GetButtonDown("Down")) {
			PlayerMove(Directions.South);
		}

		//TEMP
		if (Input.GetKeyDown(KeyCode.I)) {
			player.ListInv();
		} else if (Input.GetKeyDown(KeyCode.U)) {
			player.PrintStats();
		} else if (Input.GetKeyDown(KeyCode.O)) {
			QueryPositions();
		}
	}

	string PrintLevel() {
		StringBuilder sb = new StringBuilder();
		for (int i = height-1; i >= 0; --i) {
			for (int j = 0; j < width; ++j) {
				if (level[j,i] == null) {
					sb.Append("[ ]");
				} else if (level[j,i].GetType() == typeof(Enemy)) {
					sb.Append("[E]");
				} else if (level[j,i].GetType() == typeof(Item)){
					sb.Append("[I]");
				} else if (level[j,i].GetType() == typeof(Player)) {
					sb.Append("[P]");
				}
			}
			sb.Append("\n");
		}
		return sb.ToString();
	}

	void PlayerMove(Directions dir) {
		//check if on min edges
		int h = (int) player.MapPosition.x;
		int v = (int) player.MapPosition.y;
		if (dir == Directions.South && v <= 0) { return; }
		else if (dir == Directions.West && h <= 0) { return; }
		//check if on max edges
		else if (dir == Directions.North && v >= height-1) { return; }
		else if (dir == Directions.East && h >= width-1) { return; }

		//Pick space in Direction want to move
		BoardPieceObject boardPiece;
		if (dir == Directions.North) {
			boardPiece = level[h, v + 1];
		} else if (dir == Directions.East) {
			boardPiece = level[h + 1, v];
		} else if (dir == Directions.South) {
			boardPiece = level[h, v - 1];
		} else { //if (dir == Directions.West) {
			boardPiece = level[h - 1, v];
		}

		//EXIT check
		if (boardPiece != null && boardPiece.GetType() == typeof(ExitPiece)) {
			NextLevel();
		}

		//ENEMY Check
		if (boardPiece != null && boardPiece.GetType() == typeof(Enemy)) {
			Enemy e = (Enemy) boardPiece;
			//enemy attacks handled in radius check. can't have enemy attack in both or it would attack player twice. removed from here so don't have to keep track of which the player attacked
			//player do damage first; enemy take damage
			bool enemyDeath = e.TakeDamage(player.Damage);
			DebugGUI.AddToMessageLog(DebugGUI.Sides.LEFT, "You attacked " + e.ToString() + " for " + player.Damage + " damage");
			if (!enemyDeath) {
				//FUTURE change so that move player checks a boolean or something, rather than using return. this way, don't have to remember to call check radius here too
				CheckRadiusAroundPlayer(); //allow others to move and attack
				return; //don't move to space
			} else DebugGUI.AddToMessageLog(DebugGUI.Sides.LEFT, e.ToString() + " died");
		}

		//ITEM Check
		//if space has item, pick up item
		else if (boardPiece != null && boardPiece.GetType() == typeof(Item)) {
			player.PickUpItem((Item) boardPiece);
		}

		//MOVE Player to (now) empty space
		MovePiece(dir, player);
		
		CheckRadiusAroundPlayer();
	}

	//TODO finish check radius around player
	void CheckRadiusAroundPlayer() {		
		int h = (int) player.MapPosition.x;
		int v = (int) player.MapPosition.y;

		List<Enemy> enemies = new List<Enemy>();

		//j is vertical (y) position of Enemy, relative to player
		//i is horzontal (x) position of Enemy, relative to player
		//This can be confusing (for me!) because you are moving the player. so when you move the player below the enemy, one may think,
			//"I just moved below, y should be negative", but this is incorrect. player is below, enemy is above! Y will be positive when enemy is above player.
		for (int i = -playerRadius; i <= playerRadius; ++i) {
			for (int j = -playerRadius; j <= playerRadius; ++j) {
				int enemyH = i+h;
				int enemyV = j+v;
				if (enemyH < 0 || enemyH >= width || enemyV < 0 || enemyV >= height) { continue; } //if supposed enemy position is out of bounds, short loop
				if (level[enemyH, enemyV] == null || level[enemyH, enemyV].GetType() != typeof(Enemy)) { continue; } //if not an enemy, short loop
				Enemy e = (Enemy) level[enemyH, enemyV];

				enemies.Add(e);
			}
		}

		foreach (Enemy e in enemies) {
			int enemyH = (int) e.MapPosition.x;
			int enemyV = (int) e.MapPosition.y;
			int diffH = enemyH - h;
			int diffV = enemyV - v;

			//TODO check if space occupied (by another enemy) before moving to it?

			//Check if adjacent. If so, don't move.
			if ((diffH == 0 && Mathf.Abs(diffV) == 1) || (diffV == 0 && Mathf.Abs(diffH) == 1)) {
				continue;
			}

			//enemy movement loop
			//difference between playerPos and enemyPos is already known: absolute values of diffs!
			if (Mathf.Abs(diffH) > Mathf.Abs(diffV)) {
				//further horizontally - move that direction
				//diffH can't be 0 AND closer than diffV, so don't worry about that check
				if (diffH < 0) { //if diffH is negative, enemy is left of player; move right/East
					MovePiece(Directions.East, e);
					//print("Moving from " + enemyH + ", " + enemyV + " East");
				} else { //if diffH is positive, enemy is right of player; move left/West
					MovePiece(Directions.West, e);
					//print("Moving from " + enemyH + ", " + enemyV + " West");
				}
			} else if (Mathf.Abs(diffV) > Mathf.Abs(diffH)) {
				//further vertically - move that direction
				if (diffV < 0) { //if diffV is negative, enemy is below player; move up/North
					MovePiece(Directions.North, e);
					//print("Moving from " + enemyH + ", " + enemyV + " North");
				} else { //if diffV is positive, enemy is above player; move down/South
					MovePiece(Directions.South, e);
					//print("Moving from " + enemyH + ", " + enemyV + " South");
				}
			} else {
				//if diffs are same, randomly choose north/south (diffH) or East/West (diffV)
				int axis = Random.Range(0, 2);
				switch (axis) {
				case 0: //horizontal
					//print("Chose Horizontal");
					if (diffH < 0) { //if diffH is negative, enemy is left of player; move right/East
						MovePiece(Directions.East, e);
						//print("Moving from " + enemyH + ", " + enemyV + " East");
					} else { //if diffH is positive, enemy is right of player; move left/West
						MovePiece(Directions.West, e);
						//print("Moving from " + enemyH + ", " + enemyV + " West");
					}
					break;
				case 1: //vertical
					//print("Chose Vertical");
					if (diffV < 0) { //if diffV is negative, enemy is below player; move up/North
						MovePiece(Directions.North, e);
						//print("Moving from " + enemyH + ", " + enemyV + " North");
					} else { //if diffV is positive, enemy is above player; move down/South
						MovePiece(Directions.South, e);
						//print("Moving from " + enemyH + ", " + enemyV + " South");
					}
					break;
				}
			}
		}

		//enemy combat loop
		//now check adjacenecy again so can attack. do this after everyone moved, so newly-adjacent enemies can get an attack in. if not, enemies would chase player as he moved and only got to attack if he stopped and engaged them. too easy!
		foreach (Enemy e in enemies) {
			int enemyH = (int) e.MapPosition.x;
			int enemyV = (int) e.MapPosition.y;
			int diffH = enemyH - h;
			int diffV = enemyV - v;
			
			if ((diffH == 0 && Mathf.Abs(diffV) == 1) || (diffV == 0 && Mathf.Abs(diffH) == 1)) { 
				//adjacent; enemy attacks
				//enemy do damage; player take damage;
				bool playerDeath = player.TakeDamage(e.Damage, e.ToString());
				if (playerDeath) PlayerDied();
			}
		}
	}

	void MovePiece(Directions dir, BoardPieceObject piece) {
		int h = (int) piece.MapPosition.x;
		int v = (int) piece.MapPosition.y;
		switch (dir) {
		case Directions.North:
			if (v + 1 >= height) { return; }
			level[h, v + 1] = piece;
			break;
		case Directions.East:
			if (h + 1 >= width) { return; }
			level[h + 1, v] = piece;
			break;
		case Directions.South:
			if (v - 1 < 0) { return; }
			level[h, v - 1] = piece;
			break;
		case Directions.West:
			if (h - 1 < 0) { return; }
			level[h - 1, v] = piece;
			break;
		}
		level[h, v] = null;
		piece.Move(dir);
	}

	void PlayerDied() {
		//TODO end level properly: death screen
		DebugGUI.AddToMessageLog(DebugGUI.Sides.LEFT, "You have died!");
		NextLevel();
	}

	void QueryPositions() {
		for (int i = 0; i < height; ++i) {
			for (int j = 0; j < width; ++j) {
				if (level[j,i] != null) {
					Vector2 pos = level[j,i].MapPosition;
					print(level[j,i].ToString() + " pos: " + pos.x + ", " + pos.y);
				}
			}
		}
	}

	void NextLevel() {
		Application.LoadLevel(0);
	}
}