using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using FGUnity.Utils;
using Ekstep;

public class RotateStampCtrl : MonoBehaviour, IPointerDownHandler
{
    private StampCtrl m_Stamp;

    private float m_OldMouseAngle;
    private bool m_Rotating = false;
    private CoroutineHandle m_SoundRoutine;

    public void SetStamp(StampCtrl inStamp)
    {
        m_Stamp = inStamp;
        m_Rotating = false;
        transform.parent.GetComponent<Animator>().SetBool("showRotateAnim", true);

        if (m_Stamp != null)
        {
            UpdateAngle();
        }
    }

    private void Update()
    {
        if (m_Rotating && m_Stamp != null)
        {
            float mouseRotation = Mathf.Atan2(Input.mousePosition.y - transform.position.y, Input.mousePosition.x - transform.position.x);
            float deltaRotation = mouseRotation - m_OldMouseAngle;
            m_Stamp.SetRotation(m_Stamp.GetRotation() + deltaRotation * Mathf.Rad2Deg);
            m_OldMouseAngle = mouseRotation;

            UpdateAngle();

            if (!Input.GetMouseButton(0))
            {
                m_Rotating = false;
                m_SoundRoutine.Clear();
                transform.parent.GetComponent<Animator>().SetBool("showRotateAnim", true);
                Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "suitcase.stamp.rotate"));
            }
        }
    }

    private void UpdateAngle()
    {
        Vector3 eulerAngles = transform.localEulerAngles;
        eulerAngles.z = m_Stamp.GetRotation();
        transform.localEulerAngles = eulerAngles;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (m_Stamp != null)
        {
            m_OldMouseAngle = Mathf.Atan2(Input.mousePosition.y - transform.position.y, Input.mousePosition.x - transform.position.x);
            m_Rotating = true;
            transform.parent.GetComponent<Animator>().SetBool("showRotateAnim", false);
            m_SoundRoutine = this.SmartCoroutine(SoundRoutine());
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DRAG, "suitcase.stamp.rotate"));
        }
    }

    private IEnumerator SoundRoutine()
    {
        while (true)
        {
            SoundManager.instance.PlayRandomOneShot(SoundManager.instance.stampControlsTouch);
            yield return ScaleStampCtrl.RATCHET_SOUND_WAIT;

            float previousRotation = m_Stamp.GetRotation();
            float currentRotation = previousRotation;

            // Doesn't account for wrapping around (0 degrees vs 359)
            while (Mathf.Approximately(previousRotation, currentRotation))
            {
                yield return null;
                currentRotation = m_Stamp.GetRotation();
            }
        }
    }
}
