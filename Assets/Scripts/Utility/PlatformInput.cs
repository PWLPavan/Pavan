using UnityEngine;
using System.Collections.Generic;

/**
 * Input tools for cross-platform development. Ideal for uniting a mouse and touch environment.
 * Attach to a single object in each scene. SINGLETON.
 * NOTE: Don't forget to turn off Touch Input Module on your EventSystem.
 *
 * -Eric
 */
public class PlatformInput : MonoBehaviour
{
	private static Dictionary<int, InputEventState> inputStates = new Dictionary<int, InputEventState>();

	public enum InputEventState
	{
		UP, DOWN, RELEASED, PRESSED
	}

	void Awake()
	{
		Input.simulateMouseWithTouches = true;
	}
	
	void Start()
	{
		inputStates.Clear();
	}
	
	public static bool IsMobile()
	{
		#if UNITY_IPHONE
			return true;
		#elif UNITY_ANDROID
			return true;
		#else
		    return false;
        #endif
	}
	
	public static bool IsPointerOverUI()
	{
		if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
			return true;
		
		return false;
	}
	
	public static InputEventState GetButtonState(int button)
	{
		if (!inputStates.ContainsKey(button))
			return InputEventState.UP;
		return inputStates[button];
	}

	/*
	 * Is the button any one of the given states?
	 */
	public static bool IsButtonState(int button, params InputEventState[] states)
	{
		bool valid = false;
		var state = GetButtonState(button);

		foreach (var tState in states) {
			valid |= state == tState;
		}

		return valid;
	}

    public static Vector2 mouseInWorld { get; private set; }
	
	void Update()
	{
		for (int button = 0; button < 3; button++)
		{
			var prevState = GetButtonState(button);
			var md = Input.GetMouseButton(button);
			
			switch (prevState) {
			case InputEventState.DOWN:
				if (!md) inputStates[button] = InputEventState.RELEASED;
				break;
			case InputEventState.RELEASED:
				inputStates[button] = md ? InputEventState.PRESSED : InputEventState.UP;
				break;
			case InputEventState.UP:
				if (md) inputStates[button] = InputEventState.PRESSED;
				break;
			case InputEventState.PRESSED:
				inputStates[button] = md ? InputEventState.DOWN : InputEventState.RELEASED;
				break;
			}
		}

        mouseInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
	}
	
	public static void ForceInputState(int button, InputEventState state)
	{
		inputStates[button] = state;
	}
}
