using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using FGUnity.Utils;
using UnityEngine.UI;
using Ekstep;

namespace Minigames
{
    public class ExpressionChicken : MonoBehaviour
    {
        public DragObject Dragger;
        public ExpressionSlotFinder SlotFinder;
        public Text SignText;

        private ExpressionMinigame m_Minigame;

        private bool m_IsOperator;

        private void Awake()
        {
            Dragger.OnDragStart += Dragger_OnDragStart;
            Dragger.OnDragEnd += Dragger_OnDragEnd;

            m_IsOperator = GetComponent<ExpressionPiece>().Type == ExpressionPieceType.Operator;
        }

        void Dragger_OnDragEnd(DragObject arg1, DragEndState arg2)
        {
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, m_IsOperator ? "minigame.expression.opChicken" : "minigame.expression.valueChicken"));
            GetComponent<Animator>().SetBool("dragging", false);
            if (arg2 != DragEndState.ValidZone)
            {
                Dragger.SetOwner(SlotFinder.FindNearestSlot(Dragger));
                SoundManager.instance.PlayOneShot(SoundManager.instance.minigameDropEmpty);
            }
            else
            {
                if (Dragger.Owner.GetComponent<ExpressionDragLink>() != null)
                {
                    Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, m_IsOperator ? "minigame.expression.opNest" : "minigame.expression.valueNest"));
                    SoundManager.instance.PlayOneShot(SoundManager.instance.minigameDropNest);
                    Dragger.Owner.GetComponent<Animator>().SetTrigger("fill");
                }
                else
                {
                    SoundManager.instance.PlayOneShot(SoundManager.instance.minigameDropEmpty);
                }
            }

            m_Minigame.StopGlowing();
        }

        void Dragger_OnDragStart(DragObject obj)
        {
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, m_IsOperator ? "minigame.expression.opChicken" : "minigame.expression.valueChicken"));
            GetComponent<Animator>().SetBool("dragging", true);
            SoundManager.instance.PlayRandomOneShot(SoundManager.instance.chickenDrag);

            m_Minigame = Dragger.Controller.GetComponent<ExpressionMinigame>();

            if (m_IsOperator)
                m_Minigame.GlowOperatorNests();
            else
                m_Minigame.GlowChickenNests();
        }

        public void SetText(string inText)
        {
            SignText.text = inText;
        }

        public void SetValue(int inValue)
        {
            GetComponent<NumberPiece>().Value = inValue;
            SetText(inValue.ToStringLookup());
        }

        public void SetOperator(OperatorType inOperator)
        {
            GetComponent<OperatorPiece>().Operator = inOperator;
            SetText(inOperator == OperatorType.Add ? "+" : "-");
        }
    }
}
