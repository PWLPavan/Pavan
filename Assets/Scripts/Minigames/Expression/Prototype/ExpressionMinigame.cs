using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.UI;
using FGUnity.Utils;
using Ekstep;

namespace Minigames
{
    /// <summary>
    /// Evaluates an expression from the given pieces.
    /// </summary>
    public class ExpressionMinigame : MinigameCtrl
    {
        public PilotCtrl Pilot;

        [Header("Expression")]
        public ExpressionMinigameSettings Level;
        public ExpressionSlotFinder SlotFinder;

        [Header("Layout")]
        public Transform ExpressionTitle;
        public Transform ExpressionRoot;
        public Transform ChickenRoot;

        public float ExpressionSpacing;
        public float ChickenSpacing;

        [Header("Expression Prefabs")]
        public GameObject ValueNestPrefab;
        public GameObject OperatorNestPrefab;
        public GameObject OperatorPrefab;

        [Header("Chicken Prefabs")]
        public GameObject ChickenNestPrefab;
        public GameObject ValueChickenPrefab;
        public GameObject OperatorChickenPrefab;

        [Header("Drag Feedback")]
        public float InvalidDragTargetFadeAlpha = 0.2f;
        public float InvalidDragTargetFadeTime = 0.2f;

        private Expression m_Expression;

        private CoroutineHandle m_CurrentRoutine;
        private List<Image> m_ChickenNestRenderers = new List<Image>();
        private List<Image> m_OperatorNestRenderers = new List<Image>();

        private float m_ChickenNestFade = 1.0f;
        private float m_OperatorNestFade = 1.0f;
        private CoroutineHandle m_ChickenFadeRoutine;
        private CoroutineHandle m_OperatorFadeRoutine;

        private void Awake()
        {
            m_Expression = GetComponent<Expression>();
            m_Expression.OnPieceChanged += m_Expression_OnPieceChanged;
        }

        public override void Clear()
        {
            base.Clear();

            SlotFinder.ClearSlots();
            foreach (Transform child in ChickenRoot)
                Destroy(child.gameObject);
            foreach (Transform child in ExpressionRoot)
                Destroy(child.gameObject);

            m_ChickenNestRenderers.Clear();
            m_OperatorNestRenderers.Clear();

            m_OperatorNestFade = m_ChickenNestFade = 1.0f;
        }

        public override void Open()
        {
            base.Open();

            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.START, "minigame.expression"));

            GetComponent<ExpressionMinigameGenerator>().GenerateLevel();

            foreach(Text text in ExpressionTitle.GetComponentsInChildren<Text>())
			    text.text = Level.Value.ToStringLookup();
            m_Expression.DesiredValue = Level.Value;
            m_Expression.SetNumPieces(Level.NumValueSlots * 2 - 1);

            SpawnNests();
            SpawnChickens();

            Timer.Timer.MaxTime = Level.TimeLimit;
            Timer.Timer.ResetTimer();
            Pilot.SetTrigger("Intro");

            this.WaitSecondsThen(2.5f, Begin);
        }

        public override void Begin()
        {
            base.Begin();

            Session.instance.MarkLevelStart();
        }

        public override void Close()
        {
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.END, "minigame.expression"));

            this.WaitSecondsThen(1f, gameObject.SetActive, false);
			GetComponent<Animator>().SetBool ("isOn", false);
            base.Close();
        }

        private void SpawnNests()
        {
            for (int i = 0; i < m_Expression.NumPieces; ++i)
            {
                float offset = i - ((m_Expression.NumPieces - 1) / 2.0f);

                // Nest piece
                if ((i % 2) == 0)
                {
                    ExpressionDragLink nest = ((GameObject)Instantiate(ValueNestPrefab, new Vector3(offset * ExpressionSpacing, 0, 0), Quaternion.identity)).GetComponent<ExpressionDragLink>();
                    nest.transform.SetParent(ExpressionRoot, false);
                    nest.Expression = m_Expression;
                    nest.PieceIndex = i;
                    m_ChickenNestRenderers.Add(nest.GetComponentInChildren<Image>());
                }
                // Operator piece
                else
                {
                    if (Level.FixedOperator)
                    {
                        OperatorPiece piece = ((GameObject)Instantiate(OperatorPrefab, new Vector3(offset * ExpressionSpacing, 0, 0), Quaternion.identity)).GetComponent<OperatorPiece>();
                        piece.transform.SetParent(ExpressionRoot, false);
                        piece.Operator = Level.Operator;
                        piece.GetComponent<Animator>().SetBool("isSub", Level.Operator == OperatorType.Subtract);
                        m_Expression[i] = piece;
                    }
                    else
                    {
                        ExpressionDragLink nest = ((GameObject)Instantiate(OperatorNestPrefab, new Vector3(offset * ExpressionSpacing, 0, 0), Quaternion.identity)).GetComponent<ExpressionDragLink>();
                        nest.transform.SetParent(ExpressionRoot, false);
                        nest.Expression = m_Expression;
                        nest.PieceIndex = i;
                        m_OperatorNestRenderers.Add(nest.GetComponentInChildren<Image>());
                    }
                }
            }
        }

        private void SpawnChickens()
        {
            using (PooledList<DragHolder> chickenNests = PooledList<DragHolder>.Create())
            {
                int maxChickens = Level.Chickens.Length;
                if (!Level.FixedOperator)
                    maxChickens += 2;
                for (int i = 0; i < maxChickens; ++i)
                {
                    float offset = i - ((maxChickens - 1) / 2.0f);

                    // Spawn the nest
                    DragHolder chickenNest = ((GameObject)Instantiate(ChickenNestPrefab, new Vector3(offset * ChickenSpacing, 0, 0), Quaternion.identity)).GetComponent<DragHolder>();
                    chickenNest.transform.SetParent(ChickenRoot, false);
                    chickenNests.Add(chickenNest);
                    SlotFinder.AddSlot(chickenNest);

                    if (i < Level.Chickens.Length)
                    {
                        // Spawn the chicken
                        ExpressionChicken chicken = ((GameObject)Instantiate(ValueChickenPrefab, new Vector3(offset * ChickenSpacing, 0, 0), Quaternion.identity)).GetComponent<ExpressionChicken>();
                        chicken.transform.SetParent(ChickenRoot, false);
                        chicken.GetComponent<StartingSeat>().SetHolder(chickenNest);
                        chicken.SetValue(Level.Chickens[i]);
                        chicken.SlotFinder = SlotFinder;
                    }
                    else
                    {
                        OperatorType operatorType = i == Level.Chickens.Length ? OperatorType.Add : OperatorType.Subtract;

                        // Spawn the operator chicken
                        ExpressionChicken chicken = ((GameObject)Instantiate(OperatorChickenPrefab, new Vector3(offset * ChickenSpacing, 0, 0), Quaternion.identity)).GetComponent<ExpressionChicken>();
                        chicken.transform.SetParent(ChickenRoot, false);
                        chicken.GetComponent<StartingSeat>().SetHolder(chickenNest);
                        chicken.SetOperator(operatorType);
                        chicken.SlotFinder = SlotFinder;
                    }
                }

                foreach (var nest in chickenNests)
                    nest.transform.SetAsFirstSibling();
            }
        }

        void m_Expression_OnPieceChanged(Expression arg1, int arg2)
        {
            if (!Running)
                return;

            m_CurrentRoutine.Stop();
            m_CurrentRoutine = this.WaitOneFrameThen(Evaluate, arg1);
        }

        void Evaluate(Expression inExpression)
        {
            int calculatedValue;
            WinState = inExpression.Evaluate(out calculatedValue);
            if (WinState)
            {
                End();
            }
            else
            {
                if (inExpression.IsValidFormat())
                {
                    Timer.Timer.RemoveTime(5f);
                    Pilot.SetTrigger("Incorrect");
                }
            }
        }

        protected override void Timer_OnWarning(Timer arg1, float arg2)
        {
            base.Timer_OnWarning(arg1, arg2);

            //Pilot.SetBool("Nervous", true);
			Timer.GetComponent<Animator>().SetBool("hurry", true);
        }

        public override void End()
        {
            SoundManager.instance.DuckMusic();

            m_ChickenFadeRoutine.Clear();
            m_OperatorFadeRoutine.Clear();
            SetChickenNestFade(1.0f);
            SetOperatorNestFade(1.0f);

            if (WinState)
            {
                Pilot.SetTrigger("Correct");
                //CorrectAnswerDisplay.gameObject.SetActive(true);
                SoundManager.instance.PlayOneShot(SoundManager.instance.pilotHappy);
				Timer.GetComponent<Animator>().SetTrigger("won");
				ExpressionTitle.GetComponent<Animator>().SetTrigger("win");

                if (!IsStandalone)
                    this.WaitSecondsThen(0.5f, AwardEgg, (Action)FinishMinigame);
                else
                    this.WaitSecondsThen(0.5f, FinishMinigame);

                foreach (var animator in ChickenRoot.GetComponentsInChildren<Animator>())
                    animator.SetTrigger("win");
            }
            else
            {
                Pilot.SetTrigger("Failure");
                //FailedAnswerDisplay.gameObject.SetActive(true);
                SoundManager.instance.PlayOneShot(SoundManager.instance.pilotAngry);
				Timer.GetComponent<Animator>().SetTrigger("lost");
				ExpressionTitle.GetComponent<Animator>().SetTrigger("fail");

                this.WaitSecondsThen(0.5f, FinishMinigame);

                foreach (var animator in ChickenRoot.GetComponentsInChildren<Animator>())
                    animator.SetTrigger("lose");

                foreach (var animator in ExpressionRoot.GetComponentsInChildren<Animator>())
                    animator.SetTrigger("lose");
            }

            Genie.I.LogEvent(new OE_ASSESS(this, Session.instance.timeTaken));

            base.End();
        }

        private void FinishMinigame()
        {
            if (!IsStandalone)
                Close();
            else
                this.WaitSecondsThen(0.8f, Application.LoadLevel, SceneMgr.GAME);
        }

        public void StopGlowing()
        {
            SetGlowState(m_OperatorNestRenderers, false);
            SetGlowState(m_ChickenNestRenderers, false);
        }

        public void GlowChickenNests()
        {
            SetGlowState(m_OperatorNestRenderers, false);
            SetGlowState(m_ChickenNestRenderers, true);
            //FadeChickensTo(1.0f);
            //FadeOperatorsTo(InvalidDragTargetFadeAlpha);
			SendToFront(m_ChickenNestRenderers);
        }

        public void GlowOperatorNests()
        {
            SetGlowState(m_OperatorNestRenderers, true);
            SetGlowState(m_ChickenNestRenderers, false);
            //FadeChickensTo(InvalidDragTargetFadeAlpha);
            //FadeOperatorsTo(1.0f);
			SendToFront(m_OperatorNestRenderers);
        }

        private void SendToFront(List<Image> inNests)
        {
            foreach (var img in inNests)
            {
                img.transform.parent.SetAsLastSibling();
            }
        }

        private void SetGlowState(List<Image> inNests, bool inbGlow)
        {
            foreach (var img in inNests)
            {
                img.transform.parent.GetComponent<Animator>().SetBool("glowing", inbGlow);
            }
        }

        private void SetOperatorNestFade(float inFade)
        {
            m_OperatorNestFade = inFade;
            Color fadeColor = new Color(1, 1, 1, m_OperatorNestFade);
            for(int i = 0; i < m_OperatorNestRenderers.Count; ++i)
            {
                m_OperatorNestRenderers[i].color = fadeColor;
            }
        }

        private void SetChickenNestFade(float inFade)
        {
            m_ChickenNestFade = inFade;
            Color fadeColor = new Color(1, 1, 1, m_ChickenNestFade);
            for (int i = 0; i < m_ChickenNestRenderers.Count; ++i)
            {
                m_ChickenNestRenderers[i].color = fadeColor;
            }
        }

        private void FadeOperatorsTo(float inValue)
        {
            m_OperatorFadeRoutine.Clear();
            m_OperatorFadeRoutine = this.SmartCoroutine(Tween.ValueTo(m_OperatorNestFade, inValue, this.InvalidDragTargetFadeTime, SetOperatorNestFade));
        }

        private void FadeChickensTo(float inValue)
        {
            m_ChickenFadeRoutine.Clear();
            m_ChickenFadeRoutine = this.SmartCoroutine(Tween.ValueTo(m_ChickenNestFade, inValue, this.InvalidDragTargetFadeTime, SetChickenNestFade));
        }
    }
}
