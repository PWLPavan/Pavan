using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace FGUnity.Utils
{
    /// <summary>
    /// Allows saving and restoration of
    /// old transform parenting information.
    /// </summary>
    public class TransformParentMemory : MonoBehaviour
    {
        private Transform m_SavedParent;
        private int m_SavedSiblingIndex;
        private bool m_UseWorld;

        /// <summary>
        /// Saves the current parent and switches to
        /// another parent.
        /// </summary>
        public void ChangeTransform(Transform inNewParent, bool inbPreserveWorld)
        {
            m_SavedParent = transform.parent;
            m_SavedSiblingIndex = transform.GetSiblingIndex();
            m_UseWorld = inbPreserveWorld;

            transform.SetParent(inNewParent);
        }

        /// <summary>
        /// Restores the parent to the last saved parent.
        /// </summary>
        public void RestoreTransform()
        {
            transform.SetParent(m_SavedParent, m_UseWorld);
            transform.SetSiblingIndex(m_SavedSiblingIndex);
        }
    }
}
