using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Helper class to display and log Debug output to the screen. Use <see cref="Message()"/>or <see cref="Log()"/>to display or log a message.
/// </summary>
public class DebugGUI : MonoBehaviour {

	//FUTURE move back to yOffset between lines (instead of going off messageHeight) so they can be closer together.
	///<summary>
	/// The target output area or log for the message you are sending
	/// </summary>
	public enum Sides { LEFT, RIGHT };

	///<summary>
	/// Coupling struct for the message and its corresponding side's area/log to be displayed on.
	/// </summary>
	private struct DebugMessage {
		public Sides side;
		public string message;

		public DebugMessage(Sides side, string message) {
			this.side = side;
			this.message = message;
		}
	}

	private static int maxMsgCountRight = 6;
	private static int maxMsgCountLeft = 6;

    private int msgLength = 250;
    private int msgHeight = 22;
	private static List<DebugMessage> messageList = new List<DebugMessage>();
	private static Queue<DebugMessage> bottomRightMsgLog = new Queue<DebugMessage>();
	private static Queue<DebugMessage> bottomLeftMsgLog = new Queue<DebugMessage>();

    void Start() {
        StartCoroutine(ClearMessages());
    }

	/// <summary>
	/// Output the message to the default location of top left corner.
	/// </summary>
	/// <seealso cref="Message(side, msgText)"/>
	/// <param name="msgText">Message text to be displayed.</param>
    public static void Message(string msgText) {
		Message(Sides.LEFT, msgText);
    }

	/// <summary>
	/// Output the message to the specified side of the top half of the screen.
	/// </summary>
	/// <param name="side">Output side.</param>
	/// <param name="msgText">Message text to be displayed.</param>
	public static void Message(Sides side, string msgText) {
		messageList.Add(new DebugMessage(side, msgText));
	}

	/// <summary>
	/// Log the message to the specified log / side.
	/// </summary>
	/// <seealso cref="Log(side, msgText, maxMsgCount)"/>
	/// <param name="side">Log Side.</param>
	/// <param name="msgText">Message text to be logged.</param>
	public static void Log(Sides side, string msgText) {
		if (side == Sides.RIGHT) {
			if (bottomRightMsgLog.Count < maxMsgCountRight) {
				bottomRightMsgLog.Enqueue(new DebugMessage(side, msgText));
			} else {
				while (bottomRightMsgLog.Count >= maxMsgCountRight) {
					bottomRightMsgLog.Dequeue();
				}
				bottomRightMsgLog.Enqueue(new DebugMessage(side, msgText));
			}
		} else {
			if (bottomLeftMsgLog.Count < maxMsgCountLeft) {
				bottomLeftMsgLog.Enqueue(new DebugMessage(side, msgText));
			} else {
				while (bottomLeftMsgLog.Count >= maxMsgCountLeft) {
					bottomLeftMsgLog.Dequeue();
				}
				bottomLeftMsgLog.Enqueue(new DebugMessage(side, msgText));
			}
		}
	}

	/// <summary>
	/// Log the message to the specified log / side and set the log max-length to the specified number.
	/// </summary>
	/// <param name="side">Log Side.</param>
	/// <param name="msgText">Message text to be logged.</param>
	/// <param name="maxLogLength">Max length of the log.</param>
	public static void Log(Sides side, string msgText, int maxLogLength) {
		if (side == Sides.RIGHT) {
			maxMsgCountRight = maxLogLength;
		} else {
			maxMsgCountLeft = maxLogLength;
		}
		Log(side, msgText);
	}

    void OnGUI() {
        if (messageList != null && messageList.Count > 0) {
			int loopCount_TR = 0; //top right output's count
			int loopCount_TL = 0; //top left output's count
			Rect area = new Rect();
			foreach(DebugMessage msg in messageList) {
				if (msg.side == Sides.LEFT) {
					area = new Rect(0, (loopCount_TL++ * msgHeight), msgLength, msgHeight);
				} else if (msg.side == Sides.RIGHT) {
					area = new Rect(Screen.width - msgLength, loopCount_TR++ * msgHeight, msgLength, msgHeight);
				}
				GUI.Label(area, msg.message);
			}
        }

		if (bottomRightMsgLog != null && bottomRightMsgLog.Count > 0) {
			int loopCount = 0;
			Rect area = new Rect();

			foreach(DebugMessage msg in bottomRightMsgLog) {
				area = new Rect(Screen.width - msgLength, (Screen.height - (maxMsgCountLeft * msgHeight)) + (loopCount++ * msgHeight), msgLength, msgHeight);
				GUI.Label(area, msg.message);
			}
		}

		if (bottomLeftMsgLog != null && bottomLeftMsgLog.Count > 0) {
			int loopCount = 0;
			Rect area = new Rect();
			
			foreach(DebugMessage msg in bottomLeftMsgLog) {
				area = new Rect(0, (Screen.height - (maxMsgCountLeft * msgHeight)) + (loopCount++ * msgHeight), msgLength, msgHeight);
				GUI.Label(area, msg.message);
			}
		}
    }

    IEnumerator ClearMessages() {
		while(true) {
			yield return new WaitForEndOfFrame();
			messageList.Clear();
		}
    }
}
