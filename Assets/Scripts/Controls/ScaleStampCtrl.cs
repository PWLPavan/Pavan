using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using FGUnity.Utils;
using Ekstep;

public class ScaleStampCtrl : MonoBehaviour, IPointerDownHandler
{
    public const float RATCHET_SOUND_WAIT = 0.05f;

    private StampCtrl m_Stamp;

    private Transform m_ScaleButton;
    private CoroutineHandle m_SoundRoutine;

    private const float MIN_SCALE = 0.44f;
    private const float MAX_SCALE = 0.8f;

    private bool m_Scaling = false;

    private float m_Height;

    private void Awake()
    {
        m_ScaleButton = this.transform.FindChild("scaleBtn");
        m_Height = ((RectTransform)this.transform.FindChild("scaleBG")).sizeDelta.y * 0.8f;
    }

    public void SetStamp(StampCtrl inStamp)
    {
        m_Stamp = inStamp;
        m_Scaling = false;
        transform.parent.GetComponent<Animator>().SetBool("showScaleAnim", true);

        if (m_Stamp != null)
        {
            UpdatePosition();
        }
    }

    private void Update()
    {
        if (m_Scaling && m_Stamp != null)
        {
            RectTransform rectTransform = (RectTransform)transform;
            Vector2 localPos = rectTransform.InverseTransformPoint(Input.mousePosition);

            float percentAmount = Mathf.InverseLerp(-m_Height / 2, m_Height / 2, localPos.y);

            SetScale(Mathf.Lerp(MIN_SCALE, MAX_SCALE, percentAmount));
            UpdatePosition();

            if (!Input.GetMouseButton(0))
            {
                m_Scaling = false;
                m_SoundRoutine.Clear();
                transform.parent.GetComponent<Animator>().SetBool("showScaleAnim", true);
                Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "suitcase.stamp.scale"));
            }
        }
    }

    private void SetScale(float scale)
    {
        Vector3 localScale = m_Stamp.transform.localScale;
        localScale.x = localScale.y = scale;
        m_Stamp.transform.localScale = localScale;
    }

    private void UpdatePosition()
    {
        Vector3 localPos = m_ScaleButton.transform.localPosition;
        float percent = Mathf.InverseLerp(MIN_SCALE, MAX_SCALE, m_Stamp.transform.localScale.x);
        localPos.y = (percent - 0.5f) * m_Height;
        m_ScaleButton.transform.localPosition = localPos;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (m_Stamp != null)
        {
            m_Scaling = true;
            transform.parent.GetComponent<Animator>().SetBool("showScaleAnim", false);
            m_SoundRoutine = this.SmartCoroutine(SoundRoutine());
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DRAG, "suitcase.stamp.scale"));
        }
    }

    private IEnumerator SoundRoutine()
    {
        while (true)
        {
            SoundManager.instance.PlayRandomOneShot(SoundManager.instance.stampControlsTouch);
            yield return RATCHET_SOUND_WAIT;

            float previousScale = m_Stamp.transform.localScale.x;
            float currentScale = previousScale;
            while(Mathf.Approximately(previousScale, currentScale))
            {
                yield return null;
                currentScale = m_Stamp.transform.localScale.x;
            }
        }
    }
}
