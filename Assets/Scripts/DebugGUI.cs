using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebugGUI : MonoBehaviour {

	//FUTURE moce back to yOffset between lines (instead of going off messageHeight) so they can be closer together.

	public enum Sides { LEFT, RIGHT };

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

    public static void Message(string msgText) {
		Message(Sides.LEFT, msgText);
    }

	public static void Message(Sides side, string msgText) {
		messageList.Add(new DebugMessage(side, msgText));
	}

	public static void AddToMessageLog(Sides side, string msgText) {
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

	public static void AddToMessageLog(Sides side, string msgText, int maxMsgCount) {
		if (side == Sides.RIGHT) {
			maxMsgCountRight = maxMsgCount;
		} else {
			maxMsgCountLeft = maxMsgCount;
		}
		AddToMessageLog(side, msgText);
	}

    void OnGUI() {
        if (messageList != null && messageList.Count > 0) {
			int loopCount_TR = 0;
			int loopCount_TL = 0;
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
