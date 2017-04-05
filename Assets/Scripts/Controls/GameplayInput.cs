using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using FGUnity.Utils;

public class GameplayInput {

    //TODO: break queue into TENS_QUEUE, ONES_QUEUE
    public const string TENS_QUEUE = "tensQueue";
    public const string ONES_QUEUE = "onesQueue";
	public const string TENS_COLUMN = "tensColumn";
	public const string ONES_COLUMN = "onesColumn";
    public const string TENS_SUB = "tensSub";
    public const string ONES_SUB = "onesSub";
    public const string TENS_SUB_ADD = "tensSubAdd";
    public const string ONES_SUB_ADD = "onesSubAdd";
    public const string CONVERT_TO_TENS = "convertToTens";
	public const string CONVERT_TO_ONES = "convertToOnes";
	public const string SUBMIT = "submit";

    public const string COUNT_TENS = "countTens";
    public const string COUNT_ONES = "countOnes";

    public const string SUBMIT_NUMPAD = "submitNumberPad";
    public const string TOGGLE_NUMBER_PAD = "toggleNumberPad";
    public const string NUMBER_PAD_ARROWS = "numberPadArrows";

    public const string PILOT_TAP = "tapPilot";
    public const string EGG_TAP = "tapEggs";

    public const string PAUSE = "pause";
    public const string RESET = "reset";
    public const string SKIP = "skip";
    public const string PREV = "prev";

    Dictionary<string, GameObject> _objects = new Dictionary<string, GameObject>();

    private HashSet<string> _activatedInputs = new HashSet<string>();
    private HashSet<string> _temporaryPausedInputs = new HashSet<string>();

    // Used for stopping input until explicitly told to resume
    // ex. Number Pad blocking input from below.
    public void PauseInputs(params string[] inputsToPause)
    {
        foreach(string key in inputsToPause)
        {
            if (_activatedInputs.Contains(key))
                EnableInput(key, _objects[key], false, false);
            _temporaryPausedInputs.Add(key);
        }

        Logger.Log("PAUSING INPUTS");
    }

    // Resumes all paused input.
    public void ResumeInputs()
    {
        using(PooledList<string> keys = PooledList<string>.Create())
        {
            keys.AddRange(_temporaryPausedInputs);
            _temporaryPausedInputs.Clear();

            foreach (string key in keys)
            {
                if (_activatedInputs.Contains(key))
                    EnableInput(key, _objects[key], true, false);
            }
        }

        Logger.Log("RESUMING INPUTS");
    }

    // Disables every active input.
    public void FreezeAllInputs()
    {
        foreach(var key in _activatedInputs)
        {
            EnableInput(key, _objects[key], false, false);
        }
    }

    // Resumes every previously-enabled input.
    public void UnfreezeAllInputs()
    {
        foreach (var key in _activatedInputs)
        {
            if (_temporaryPausedInputs.Contains(key))
                continue;

            EnableInput(key, _objects[key], true, false);
        }
    }

	public void Add (string key, GameObject value)
    {
		_objects.Add(key, value);
	}

    public void DisableAllInput ()
    {
        EnableAllInput(false);
    }

	public void EnableAllInput (bool enable, params string[] exceptions) {
		foreach (KeyValuePair<string, GameObject> entry in _objects) {
			// if we find this value in exceptions array, skip enabling it
			if (FindInExceptionsArray(exceptions, entry.Key))
				continue;
			EnableInput(entry.Key, entry.Value, enable, true);
        }

        using (PooledStringBuilder stringBuilder = PooledStringBuilder.Create())
        {
            if (!enable)
                stringBuilder.Builder.Append("[DISABLE] ALL INPUT EXCEPT:");
            else
                stringBuilder.Builder.Append("[ENABLE] ALL INPUT EXCEPT:");

            // set all listed game objects in exceptions as !enable (the opposite)
            for (int i = 0; i < exceptions.Length; ++i)
            {
                stringBuilder.Builder.Append(" ").Append(exceptions[i]).Append(";");
                EnableInput(exceptions[i], _objects[exceptions[i]], !enable, true);
            }

            Logger.Log(stringBuilder.Builder.ToString());
        }
	}

    public void EnableCountingAndPause(bool enable, bool countOnes = true, bool countTens = true)
    {
        if (enable)
            Logger.Log("ENABLE PAUSING AND COUNTING INPUTS");
        else
            Logger.Log("DISABLE PAUSING AND COUNTING INPUTS");
        EnableInput(COUNT_ONES, enable && countOnes, true);
        EnableInput(COUNT_TENS, enable && countTens, true);
        EnableInput(PAUSE, enable, true);

        EnableInput(PILOT_TAP, enable, true);
        EnableInput(EGG_TAP, enable, true);
    }

    private void EnableInput(string key, bool enable, bool modifyList)
    {
        GameObject input;

        if (_objects.TryGetValue(key, out input))
        {
            EnableInput(key, input, enable, modifyList);
        }
    }

	void EnableInput (string key, GameObject value, bool enable, bool modifyList) {

        if (modifyList)
        {
            if (enable)
                _activatedInputs.Add(key);
            else
                _activatedInputs.Remove(key);
        }

        // If this is temporarily still disabled
        if (_temporaryPausedInputs.Contains(key))
            return;

		switch (key)
        {
        case ONES_QUEUE:
            value.GetComponent<QueueController>().EnableInput(enable, 1);
            break;

        case TENS_QUEUE:
            value.GetComponent<QueueController>().EnableInput(enable, 10);
            break;

        case TENS_COLUMN:
		case ONES_COLUMN:
			value.GetComponent<PlaceValueCtrl>().EnableInput(enable);
			break;

		case CONVERT_TO_TENS:
            value.GetComponent<ConvertNestCtrl>().EnableInput(enable);
            break;

		case CONVERT_TO_ONES:
            value.GetComponent<PlaceValueCtrl>().AllowDragConversion(enable);
			break;

        case COUNT_TENS:
        case COUNT_ONES:
            value.GetComponent<RealtimeCountCtrl>().SetEnabled(enable);
            break;
        
        case TENS_SUB:
        case ONES_SUB:
            value.GetComponent<DropZoneCtrl>().EnableDropping(enable);
            break;

        case TENS_SUB_ADD:
        case ONES_SUB_ADD:
            value.GetComponent<DropZoneCtrl>().EnableInput(enable);
            break;

        case NUMBER_PAD_ARROWS:
            {
                NumberPadCtrl numPad = value.GetComponent<NumberPadCtrl>();
                numPad.TensMinusButton.interactable = numPad.TensPlusButton.interactable
                    = numPad.OnesPlusButton.interactable = numPad.OnesMinusButton.interactable
                    = enable;
                break;
            }
        
        case SUBMIT:
        case PAUSE:
        case SKIP:
        case PREV:
        case TOGGLE_NUMBER_PAD:
        case SUBMIT_NUMPAD:
			value.GetComponent<Button>().interactable = enable;
			break;

        case PILOT_TAP:
            value.GetComponent<PilotSounds>().AllowTap = enable;
            break;

        case EGG_TAP:
            value.GetComponent<TapEggs>().AllowTap = enable;
            break;
        }
	}

	bool FindInExceptionsArray (string[] exceptions, string key) {
		for (int i = 0; i < exceptions.Length; ++i) {
			if (exceptions[i].Equals(key))
				return true;
		}
		return false;
	}
}
