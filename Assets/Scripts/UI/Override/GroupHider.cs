using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using FGUnity.Utils;

public class GroupHider : MonoBehaviour
{
    public void HideAll()
    {
        HideFrom(transform);
    }

    public void HideFrom(Transform inTransform)
    {
        CleanList();
        using (PooledList<Graphic> graphics = PooledList<Graphic>.Create())
        {
            inTransform.GetComponentsInChildren<Graphic>(graphics);
            foreach (var graphic in graphics)
            {
                if (graphic.enabled)
                {
                    graphic.enabled = false;
                    m_GraphicsToRestore.Add(graphic);
                }
            }
        }
    }

    public void ShowAll()
    {
        CleanList();
        foreach (var graphic in m_GraphicsToRestore)
        {
            graphic.enabled = true;
        }

        m_GraphicsToRestore.Clear();
    }

    private void CleanList()
    {
        for(int i = 0; i < m_GraphicsToRestore.Count; ++i)
        {
            if (m_GraphicsToRestore[i] == null)
                m_GraphicsToRestore.RemoveAt(i--);
        }
    }

    private List<Graphic> m_GraphicsToRestore = new List<Graphic>();
}
