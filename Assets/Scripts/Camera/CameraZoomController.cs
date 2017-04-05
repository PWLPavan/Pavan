using UnityEngine;
using System.Collections;

public class CameraZoomController : MonoBehaviour
{
    public float RestingSize = 5.4f;
    public AnimationCurve Curve = AnimationCurve.EaseInOut(0.0f, 0, 1.0f, 1.0f);

    public void FitToSize(float inHorizontal, float inVertical)
    {
        RestingSize = CalculateNeededSize(inHorizontal, inVertical);
    }

    public void FitToHorizontal(float inHorizontal)
    {
        FitToSize(inHorizontal, Camera.main.orthographicSize);
    }

    private float CalculateNeededSize(float inHorizontal, float inVertical)
    {
        float sizeForVertical = inVertical;
        float sizeForHorizontal = inHorizontal * Screen.height / Screen.width;
        return Mathf.Max(sizeForHorizontal, sizeForVertical);
    }

    public void ZoomToSize(float inSize, float inTime)
    {
        InitializeTween(m_Camera.orthographicSize, inSize, inTime);
    }

    public void ReturnToResting(float inTime)
    {
        InitializeTween(m_Camera.orthographicSize, RestingSize, inTime);
    }

    private void InitializeTween(float inStart, float inEnd, float inTime)
    {
        m_Tweening = true;

        m_TweenTime = 0.0f;
        m_TweenLength = inTime;

        m_TweenStartValue = inStart;
        m_TweenDelta = inEnd - inStart;
    }

    private void Awake()
    {
        m_Camera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (!m_Tweening)
            return;

        m_TweenTime += Time.deltaTime;

        if (m_TweenTime >= m_TweenLength)
        {
            m_TweenTime = m_TweenLength;
            m_Tweening = false;
        }

        float percent = m_TweenTime / m_TweenLength;
        ApplySize(percent);
    }

    private void ApplySize(float inPercent)
    {
        Profiler.BeginSample("ApplyZoom");
        float curvedValue = Curve.Evaluate(inPercent);
        float desiredZoom = m_TweenStartValue + (curvedValue * m_TweenDelta);
        m_Camera.orthographicSize = desiredZoom;
        Profiler.EndSample();
    }

    private Camera m_Camera;

    private float m_TweenTime = 0.0f;
    private float m_TweenStartValue = 0.0f;
    private float m_TweenDelta = 0.0f;
    private float m_TweenLength = 0.0f;
    private bool m_Tweening = false;
}
