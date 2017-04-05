using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FGUnity.Utils;

public class RealtimeCountCtrl : MonoBehaviour
{
    #region Inspector

    public float ChickenFadeTime = 1.0f;
    public float LastChickenFadeTime = 2.0f;
    public float TapReleaseThreshold = 0.5f;

    #endregion

    private PlaceValueCtrl m_PlaceValue;
    private Coroutine m_CountRoutine;
    private bool m_Touched = false;
    private Vector3 m_TouchPosition;
    private float m_TouchTime;

    private const float DRAG_THRESHOLD_SQR = 30 * 30;

    private void Awake()
    {
        m_PlaceValue = GetComponent<PlaceValueCtrl>();
        Assert.True(m_PlaceValue != null, "PlaceValueCtrl exists on GameObject.");
    }

    public bool Enabled { get; private set; }
    public bool Counting { get { return m_CountRoutine != null; } }

    private int m_CreatureIndex = 0;

    public void SetEnabled(bool inbEnabled)
    {
        Enabled = inbEnabled;
        if (!Enabled)
            CancelCount();
    }

    public void CancelCount()
    {
        if (m_CountRoutine != null)
        {
            OnFinished();
            m_PlaceValue.ClearRealtimeCount();
            StopCoroutine(m_CountRoutine);
            m_CountRoutine = null;
        }
    }

    private void OnMouseDown()
    {
        m_TouchPosition = Input.mousePosition;
        m_TouchTime = Time.realtimeSinceStartup;
    }

    private void OnMouseUpAsButton()
    {
        if (!Enabled)
            return;

        if ((m_TouchPosition - Input.mousePosition).sqrMagnitude > DRAG_THRESHOLD_SQR)
            return;

        if (Time.realtimeSinceStartup - m_TouchTime > TapReleaseThreshold)
            return;

        if (m_CountRoutine == null)
        {
            m_CountRoutine = StartCoroutine(CountRoutine());
        }
        else
        {
            m_Touched = true;
        }
    }

    private IEnumerator CountRoutine()
    {
        if (m_PlaceValue.numCreatures == 0)
            yield break;

        for (m_CreatureIndex = 0; m_CreatureIndex < m_PlaceValue.numCreatures; ++m_CreatureIndex)
        {
            m_PlaceValue.SetRealtimeCountColor(m_CreatureIndex, m_PlaceValue.DefaultSeatCountColor);
            m_PlaceValue.SetRealtimeCountNumber(m_CreatureIndex, (m_CreatureIndex + 1) * m_PlaceValue.value);
            m_PlaceValue.SetRealtimeCount(m_CreatureIndex);
            yield return null;

            float timeToWait = (m_CreatureIndex == m_PlaceValue.numCreatures - 1) ? LastChickenFadeTime : ChickenFadeTime;
            bool bProceed = false;

            while (timeToWait > 0 && !bProceed)
            {
                timeToWait -= Time.deltaTime;
                if (m_Touched)
                {
                    if (m_CreatureIndex == m_PlaceValue.numCreatures - 1)
                    {
                        m_PlaceValue.SetRealtimeCountColor(m_CreatureIndex, m_PlaceValue.DefaultSeatCountColor);
                        m_PlaceValue.SetRealtimeCountNumber(m_CreatureIndex, (m_CreatureIndex + 1) * m_PlaceValue.value);
                        m_PlaceValue.SetRealtimeCount(m_CreatureIndex);
                        timeToWait = LastChickenFadeTime;
                    }
                    else
                    {
                        bProceed = true;
                    }
                    m_Touched = false;
                }
                yield return null;
            }

            if (!bProceed)
                break;
        }

        OnFinished();
        m_PlaceValue.ClearRealtimeCount();
        m_CountRoutine = null;
    }

    private void OnFinished()
    {
        EnlearnInstance.I.LogActions(EnlearnInstance.Action.ColumnCount,
            "column", m_PlaceValue.value == 1 ? "ones" : "tens",
            "count", (m_CreatureIndex + 1).ToStringLookup(),
            "total", m_PlaceValue.numCreatures.ToStringLookup()
            );
    }
}
