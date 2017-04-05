using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using FGUnity.Utils;

public class StampCtrl : MonoBehaviour, IPointerDownHandler
{
    private struct Configuration
    {
        public Vector3 Position;
        public Vector3 Scale;
        public Vector3 Rotation;

        public void PopulateFromTransform(Transform inTransform, Transform inStampTransform)
        {
            Position = inTransform.localPosition;
            Scale = inTransform.localScale;
            Rotation = inStampTransform.localEulerAngles;
        }

        public void ApplyToTransform(Transform inTransform, Transform inStampTransform, Transform inShadowTransform)
        {
            inTransform.localPosition = Position;
            inTransform.localScale = Scale;

            inStampTransform.localEulerAngles = Rotation;
            inShadowTransform.localEulerAngles = Rotation;
        }
    }

    private Configuration m_DefaultConfig;
    private Configuration m_SavedConfig;

    private Transform m_ShadowTransform;
    private Transform m_StampTransform;

    private SuitcaseCtrl m_Suitcase;

    public void Initialize(SuitcaseCtrl inSuitcase)
    {
        m_ShadowTransform = transform.FindChild("Shadow");
        m_StampTransform = transform.FindChild("Stamp");

        m_DefaultConfig.PopulateFromTransform(transform, m_StampTransform);

        m_Suitcase = inSuitcase;
    }

    public void Show(bool inbShow)
    {
        gameObject.SetActive(inbShow);
    }

    public void ResetToDefault()
    {
        m_DefaultConfig.ApplyToTransform(transform, m_StampTransform, m_ShadowTransform);

        Show(gameObject.activeSelf);
    }

    public void PushState()
    {
        m_SavedConfig.PopulateFromTransform(transform, m_StampTransform);
    }

    public void PopState()
    {
        m_SavedConfig.ApplyToTransform(transform, m_StampTransform, m_ShadowTransform);
    }

    public Vector3 GetDefaultRotation()
    {
        return m_DefaultConfig.Rotation;
    }

    public float GetRotation()
    {
        return m_StampTransform.localEulerAngles.z;
    }

    public void SetRotation(float inRotation)
    {
        Vector3 eulerAngles = m_StampTransform.localEulerAngles;
        eulerAngles.z = inRotation;
        m_StampTransform.localEulerAngles = m_ShadowTransform.localEulerAngles = eulerAngles;
    }

    #region JSON

    public JSONNode ToJSON()
    {
        JSONClass json = new JSONClass();
        json["positionX"].AsFloat = transform.localPosition.x;
        json["positionY"].AsFloat = transform.localPosition.y;
        json["scale"].AsFloat = transform.localScale.x / m_DefaultConfig.Scale.x;
        json["rotation"].AsFloat = m_StampTransform.localEulerAngles.z;
        json["order"].AsInt = transform.GetSiblingIndex();
        return json;
    }

    public void LoadJSON(JSONNode inJSON)
    {
        transform.localPosition = new Vector3(inJSON["positionX"].AsFloat, inJSON["positionY"].AsFloat, m_DefaultConfig.Position.z);
        transform.localScale = m_DefaultConfig.Scale * inJSON["scale"].AsFloat;

        Vector3 newEulerAngles = m_DefaultConfig.Rotation;
        newEulerAngles.z = inJSON["rotation"].AsFloat;
        m_StampTransform.localEulerAngles = m_ShadowTransform.localEulerAngles = newEulerAngles;
        transform.SetSiblingIndex(inJSON["order"].AsInt);
    }

    #endregion

    #region Unity Events

    public void OnPointerDown(PointerEventData eventData)
    {
        PickUp();
    }

    public void PickUp()
    {
        Show(true);
        transform.SetAsLastSibling();
        GetComponent<Animator>().SetBool("isDragging", true);
        m_Suitcase.OnStampPicked(this);
    }

    public void Drop()
    {
        GetComponent<Animator>().SetBool("isDragging", false);
    }

    #endregion
}
