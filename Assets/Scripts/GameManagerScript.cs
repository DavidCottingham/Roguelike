using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public enum Directions {North, East, South, West}

public class GameManagerScript : MonoBehaviour {

	private static PauseScript ps;
	private static bool paused = false;
	public static bool Paused {
		get { return paused; } }

	public GameObject wallPrefab; //assigned in inspector
	public GameObject wallsParent;

	public static float translateUnits = 2f;
	private int width = 50;
	private int height = 20;
	private BoardPieceObject[,] level;

	private Player player; //in start

	private int numEnemies = 4;
	private int numItems = 4;

	private int playerRadius = 2;

	void Start() {
		SetUpLevel();
		ps = GetComponent<PauseScript>() as PauseScript;
	}

	void SetUpLevel() {
		level = new BoardPieceObject[width, height];

		PopulateLevel<Player>(1);
		player = Player.player;
		
		//WALL perimeter
		//loop from -1 to max to adds corners
		//each loop creates walls on both sides at same time as move up to max
		GameObject tempWall;
		Vector3 pos;
		for (int i = -1; i <= width; ++i) {
			pos = new Vector3(i * translateUnits, -1 * translateUnits);
			tempWall = Instantiate(wallPrefab, pos, Quaternion.identity) as GameObject;
			//if (wallsParent == null) print("poop");
			tempWall.transform.parent = wallsParent.transform;
			pos = new Vector3(i * translateUnits, height * translateUnits);
			tempWall = Instantiate(wallPrefab, pos, Quaternion.identity) as GameObject;
			tempWall.transform.parent = wallsParent.transform;
		}
		for (int i = -1; i <= height; ++i) {
			pos = new Vector3(-1 * translateUnits, i * translateUnits);
			tempWall = Instantiate(wallPrefab, pos, Quaternion.identity) as GameObject;
			tempWall.transform.parent = wallsParent.transform;
			pos = new Vector3(width * translateUnits, i * translateUnits);
			tempWall = Instantiate(wallPrefab, pos, Quaternion.identity) as GameObject;
			tempWall.transform.parent = wallsParent.transform;
		}

		PopulateLevel<ExitPiece>(1);
		PopulateLevel<Enemy>(numEnemies);
		PopulateLevel<Item>(numItems);
	}

	void PopulateLevel<T>(int count) where T: BoardPieceObject {
		int spwnH, spwnV = 0;
		for (int i = 0; i < count; ++i) {
			spwnH = 0;
			spwnV = 0;
			while (true) {
				spwnH = Random.Range(0, width);
				spwnV = Random.Range(0, height);
				if (level[spwnH, spwnV] == null) { break; }
			}
			if (typeof(T) == typeof(Player) && Player.player != null) {
				player = Player.player;
				player.SetStartPos(spwnH, spwnV);
				level[spwnH, spwnV] = player;
			} else {
				System.Object[] args = new System.Object[] { spwnH, spwnV };
				level[spwnH, spwnV] = (T) System.Activator.CreateInstance(typeof(T), args); //Reflection to dynamically instantiate board pieces
			}
		}
	}

	void Update() {
		DebugGUI.Message("H: " + player.MapPosition.x + " | V: " + player.MapPosition.y);
		player.PlayerStats();
		//DebugGUI.Message(PrintLevel());

		if (!Paused) {
			if (Input.GetButtonDown("Right")) {
				PlayerMove(Directions.East);
			} else if (Input.GetButtonDown("Left")) {
				PlayerMove(Directions.West);
			} else if (Input.GetButtonDown("Up")) {
				PlayerMove(Directions.North);
			} else if (Input.GetButtonDown("Down")) {
				PlayerMove(Directions.South);
			}

			//DEBUG
			if (Input.GetKeyDown(KeyCode.I)) {
				player.ListInv();
			} else if (Input.GetKeyDown(KeyCode.U)) {
				player.PrintStats();
			} else if (Input.GetKeyDown(KeyCode.O)) {
				QueryPositions();
			}
		}

		if (Input.GetKeyDown("escape")) {
			if (!paused) PauseGame();
			else UnpauseGame();
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
			DebugGUI.Log(DebugGUI.Sides.LEFT, "You attacked " + e.ToString() + " for " + player.Damage + " damage");
			if (!enemyDeath) {
				CheckRadiusAroundPlayer(); //allow other enemies to move and attack
				return; //don't move to space
			} else { //enemy died
				level[(int) boardPiece.MapPosition.x, (int) boardPiece.MapPosition.y] = null; //clear map space
				DebugGUI.Log(DebugGUI.Sides.LEFT, e.ToString() + " died");
			}
		}

		//ITEM Check
		//if space has item, pick up item
		else if (boardPiece != null && boardPiece.GetType() == typeof(Item)) {
			player.PickUpItem((Item) boardPiece);
			level[(int) boardPiece.MapPosition.x, (int) boardPiece.MapPosition.y] = null;
		}

		//MOVE Player to (now) empty space
		MovePiece(dir, player);
		
		CheckRadiusAroundPlayer();
	}

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

			//Check if adjacent. If so, don't move.
			if ((diffH == 0 && Mathf.Abs(diffV) == 1) || (diffV == 0 && Mathf.Abs(diffH) == 1)) {
				continue;
			}

			//enemy movement loop
			//difference between playerPos and enemyPos is already known: absolute values of diffs!
			if (Mathf.Abs(diffH) > Mathf.Abs(diffV)) {
				//further horizontally - move that direction
				//diffH can't be 0 AND closer than diffV, so don't worry about that check
				EnemyMoveHorizontal(diffH, e);
			} else if (Mathf.Abs(diffV) > Mathf.Abs(diffH)) {
				//further vertically - move that direction
				EnemyMoveVertical(diffV, e);
			} else {
				//if diffs are same, randomly choose north/south (diffH) or East/West (diffV)
				int axis = Random.Range(0, 2);
				//ignore empty if block warning. Work is done in bool check; I want it empty
				#pragma warning disable 642
				if (axis == 0) { //arbitrarily chosen to be horizontal
					if (!EnemyMoveHorizontal(diffH, e)); //try horizontal first. if could not move ...
					else EnemyMoveVertical(diffV, e); // ...try the vertical
				} else if (!EnemyMoveVertical(diffV, e)); //try vertical first. if not ...
				else EnemyMoveHorizontal(diffH, e); // ... try horizontal
				#pragma warning restore 642
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

	bool EnemyMoveHorizontal(int check, Enemy e) {
		if (check < 0) { //if diffH is negative, enemy is left of player; move right/East
			//print("Moving from " + enemyH + ", " + enemyV + " East");
			return MovePiece(Directions.East, e);
		} else { //if diffH is positive, enemy is right of player; move left/West
			//print("Moving from " + enemyH + ", " + enemyV + " West");
			return MovePiece(Directions.West, e);
		}
	}

	bool EnemyMoveVertical(int check, Enemy e) {
		if (check < 0) { //if diffV is negative, enemy is below player; move up/North
			//print("Moving from " + enemyH + ", " + enemyV + " North");
			return MovePiece(Directions.North, e);
		} else { //if diffV is positive, enemy is above player; move down/South
			//print("Moving from " + enemyH + ", " + enemyV + " South");
			return MovePiece(Directions.South, e);
		}
	}

	bool MovePiece(Directions dir, BoardPieceObject piece) {
		int h = (int) piece.MapPosition.x;
		int v = (int) piece.MapPosition.y;
		switch (dir) {
		case Directions.North:
			if (v + 1 >= height) { return false; } //if attempted space out of bounds, abort move attempt
			if (level[h, v + 1] != null) { return false; } //if space occupied, abort move attempt
			level[h, v + 1] = piece;
			break;
		case Directions.East:
			if (h + 1 >= width) { return false; } //if attempted space out of bounds, abort move attempt
			if (level[h + 1, v] != null) { return false; } //if space occupied, abort move attempt
			level[h + 1, v] = piece;
			break;
		case Directions.South:
			if (v - 1 < 0) { return false; } //if attempted space out of bounds, abort move attempt
			if (level[h, v - 1] != null) { return false; } //if space occupied, abort move attempt
			level[h, v - 1] = piece;
			break;
		case Directions.West:
			if (h - 1 < 0) { return false; } //if attempted space out of bounds, abort move attempt
			if (level[h - 1, v] != null) { return false; } //if space occupied, abort move attempt
			level[h - 1, v] = piece;
			break;
		}
		level[h, v] = null;
		piece.Move(dir);
		return true;
	}

	void PlayerDied() {
		DebugGUI.Log(DebugGUI.Sides.LEFT, "You have died!");
		player.Reset();
		//NextLevel();
		PauseGame(PauseScript.PauseReason.PlayerDie);
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

	void PauseGame() {
		PauseGame(PauseScript.PauseReason.Pause);
	}

	void PauseGame(PauseScript.PauseReason reason) {
		paused = true;
		ps.enabled = true;
		ps.Reason = reason;
	}

	public static void UnpauseGame() {
		ps.enabled = false;
		paused = false;
	}

	public static void EndGame() {
		Application.Quit();
	}

	public static void RestartLevel() {
		Application.LoadLevel(0);
	}
}