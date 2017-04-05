using UnityEngine;
using System.Collections;
using FGUnity.Utils;

public class TransitionBackgroundCtrl : MonoBehaviour
{
    public Animator TinyShipAnimator;
    public SpriteRenderer Background;
    public Transform Clouds;
    public float Duration = 1.0f;

    private Color m_TargetColor;
    private bool m_Prepped = false;

    private CoroutineHandle m_Routine;

    private void Awake()
    {
        SetVisible(false);
    }

    public void PrepTransition(Color inOldColor, Color inNewColor)
    {
        Background.color = inOldColor;
        SetVisible(true);
        Camera.main.GetComponent<Animator>().SetBool("isTransitioning", true);
        TinyShipAnimator.SetBool("isTransitioning", true);
        m_TargetColor = inNewColor;
        m_Prepped = true;
    }

    public void StartTransition()
    {
        if (m_Prepped)
        {
            m_Routine = this.SmartCoroutine(TweenRoutine());
        }
        else
        {
            Background.color = m_TargetColor;
            SetVisible(true);
            OnFinish();
        }
    }

    public void ForceFinish()
    {
        Background.color = m_TargetColor;
        m_Routine.Clear();
        OnFinish();
    }

    private IEnumerator TweenRoutine()
    {
        yield return Background.ColorTo(m_TargetColor, Duration);
        OnFinish();
    }

    private void OnFinish()
    {
        Camera.main.GetComponent<Animator>().SetBool("isTransitioning", false);
        TinyShipAnimator.SetBool("isTransitioning", false);
        m_Prepped = false;
    }

    public void SetVisible(bool inbVisible = true)
    {
        Background.enabled = inbVisible;
        Clouds.gameObject.SetActive(inbVisible);
    }
}
