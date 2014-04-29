using UnityEngine;
using System.Collections;

public class PauseScript : MonoBehaviour {
	
	///<summary>
	/// Reasons pause screen might be called
	///</summary>
	public enum PauseReason { 
		/// <summary>Player paused the game</summary>
		Pause, 
		/// <summary>The player died</summary>
		PlayerDie };
	
	//DEBUG serialized for testing
	[SerializeField, Range(25,100)]
	private float menuSizePercent = 80f;
	
	/// <summary>
	/// You should set the <see cref="PauseReason"/>why you are pausing the game every time you pause.
	/// </summary>
	/// <value>The <paramref name="reason"/>the game was paused.</value>
	public PauseReason Reason { get; set; }
	
	private float scrnW;
	private float scrnH;
	
	//These variables are calculated in Start and depend upon the screen size and menuSizePercent to set the pause menu screen as a percentage of the whole screen
	private float offsetFromTop;
	private float offsetFromLeft;
	private float height;
	private float width;
	
	//DEBGUG serialized for testing
	[SerializeField] private float buttonWidth = 200f;
	[SerializeField] private float buttonHeight = 75f;
	[SerializeField] private float buttonSpacing = 20f;
	
	void OnEnable() {
		Time.timeScale = 0.0f;
	}
	
	void OnDisable() {
		Time.timeScale = 1.0f;
	}
	
	void Start() {
		scrnW = Screen.width;
		scrnH = Screen.height;
		
		//offset is the difference from the screen percentage (90% size screen is 10% here) divided in half
		offsetFromLeft = (scrnW * ((100f - menuSizePercent) / 100f)) / 2;
		offsetFromTop = (scrnH * ((100f - menuSizePercent) / 100f)) / 2;
		//menu dimension is the specified percentage of the whole screen. e.g. 90%
		height = scrnH * (menuSizePercent / 100f);
		width = scrnW * (menuSizePercent / 100f);
	}
	
	void OnGUI() {
		//TODO these 4 are only here to test menu size while game is running. should remove them from here (and let them stay in Start) before release. not efficient to calculate them every frame in final release
		offsetFromLeft = (scrnW * ((100f - menuSizePercent) / 100f) / 2);
		offsetFromTop = (scrnH * ((100f - menuSizePercent) / 100f) / 2);
		height = scrnH * (menuSizePercent / 100f);
		width = scrnW * (menuSizePercent / 100f);
		
		GUI.BeginGroup(new Rect(offsetFromLeft, offsetFromTop, width, height));
		GUI.Box(new Rect(0,0, width, height), "");
		
		//TODO string reason like "You have died" or "game paused"
		GUI.Label(new Rect(width / 2 - 20, 50, 100, 25), Reason.ToString());
		
		if (Reason == PauseReason.Pause) {
			if (GUI.Button(new Rect(width / 2 - buttonWidth / 2, buttonHeight * 2, buttonWidth, buttonHeight), "Resume")) {
				GameManagerScript.UnpauseGame();
			}
		}

		if (Reason == PauseReason.PlayerDie) {
			if (GUI.Button(new Rect(width / 2 - buttonWidth / 2, buttonHeight * 2, buttonWidth, buttonHeight), "Restart")) {
				GameManagerScript.UnpauseGame();
				GameManagerScript.RestartLevel();
			}
		}
		
		if (GUI.Button(new Rect(width / 2 - buttonWidth / 2, buttonHeight * 3 + (buttonSpacing), buttonWidth, buttonHeight), "Exit Game")) {
			GameManagerScript.EndGame();
		}
		
		GUI.EndGroup();
	}
}
