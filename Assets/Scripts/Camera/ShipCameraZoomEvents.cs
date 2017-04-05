using UnityEngine;
using System;
using System.Collections;

public class ShipCameraZoomEvents : MonoBehaviour
{
    [Header("Fit to Horizontal Size")]
    public float MaxPlaneWidth = 1.0f;

    [Header("Close up")]
    public float AnswerSize = 4.0f;
    public ZoomAnimation AnswerIn;
    public ZoomAnimation AnswerInOnes;
    public ZoomAnimation AnswerOut;
     
    [Header("Far away")]
    public float SceneSize = 11.0f;
    public ZoomAnimation SceneOut;
    public ZoomAnimation SceneIn;
	public ZoomAnimation SceneOutTransition;

    [Serializable]
    public class ZoomAnimation
    {
        public AnimationCurve Curve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
        public float Time;
    }

    private void Awake()
    {
        CameraZoomController zoomer = GetComponent<CameraZoomController>();
        zoomer.FitToHorizontal(MaxPlaneWidth);
        Camera.main.orthographicSize = zoomer.RestingSize;
    }

    public void ZoomAnswerIn()
    {
        StartZoom(AnswerSize, AnswerIn);
    }

    public void ZoomAnswerInOnes()
    {
        StartZoom(AnswerSize, AnswerInOnes);
    }

    public void ZoomAnswerOut()
    {
        ReturnZoom(AnswerOut);
    }

    public void ZoomSceneOut()
    {
        StartZoom(SceneSize, SceneOut);
    }

	public void ZoomSceneOutTransition()
	{
		StartZoom(SceneSize, SceneOutTransition);
	}

    public void ZoomSceneIn()
    {
        ReturnZoom(SceneIn);
    }

    private void StartZoom(float inSize, ZoomAnimation inZoom)
    {
        CameraZoomController zoomer = GetComponent<CameraZoomController>();
        zoomer.Curve = inZoom.Curve;
        zoomer.ZoomToSize(inSize, inZoom.Time);
    }

    private void ReturnZoom(ZoomAnimation inZoom)
    {
        CameraZoomController zoomer = GetComponent<CameraZoomController>();
        zoomer.Curve = inZoom.Curve;
        zoomer.ReturnToResting(inZoom.Time);
    }
}
