using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using FGUnity.Utils;
using UnityEngine.UI;
using Ekstep;

namespace Minigames
{
    public class NestTest : MinigameCtrl
    {
        public DragHolder Holder;
        public DragHolder StartingNest;
        public Transform SortGroup;
        public GameObject ChickenPrefab;
        public Transform NumberRoot;
        public Transform Title;

        private List<DragObject> m_Objects = new List<DragObject>();

        protected override void Start() 
        {
            base.Start();

            Holder.OnObjectAdded += m_Holder_OnObjectAdded;
            Timer.Timer.OnFinish += Timer_OnFinish;

            this.SmartCoroutine(SortRoutine());
        }

        protected override void Timer_OnWarning(Timer arg1, float arg2)
        {
            base.Timer_OnWarning(arg1, arg2);

            Timer.GetComponent<Animator>().SetBool("hurry", true);
        }

        public override void Open()
        {
            base.Open();

            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.START, "minigame.make10"));

            Holder.DestroyObjects();
            StartingNest.DestroyObjects();
            SpawnAllChickens();
            Timer.Timer.ResetTimer();
            Timer.GetComponent<Animator>().SetTrigger("reset");
            this.WaitSecondsThen(2.5f, Begin);
        }

        public override void Begin()
        {
            base.Begin();

            Session.instance.MarkLevelStart();
            if (!SaveData.instance.SeenTens)
                Timer.Timer.StopTimer();
        }

        public override void Close()
        {
            base.Close();

            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.END, "minigame.make10"));
            
            GetComponent<Animator>().SetBool("isOn", false);

            this.WaitSecondsThen(1.0f, gameObject.SetActive, false);
        }

        public override void End()
        {
            if (WinState)
            {
                //CorrectAnswerDisplay.gameObject.SetActive(true);
                transform.FindChild("Holders").GetComponent<Animator>().SetTrigger("success");
                SoundManager.instance.PlayOneShot(SoundManager.instance.pilotHappy);
                Timer.GetComponent<Animator>().SetTrigger("won");
                SoundManager.instance.DuckMusic();

                if (!IsStandalone)
                    this.WaitSecondsThen(0.5f, AwardEgg, (Action)FinishMinigame);
                else
                    this.WaitSecondsThen(0.5f, FinishMinigame);

                Title.GetComponent<Animator>().SetTrigger("win");
            }
            else
            {
                //FailedAnswerDisplay.gameObject.SetActive(true);
                transform.FindChild("Holders").GetComponent<Animator>().SetTrigger("failed");
                Timer.GetComponent<Animator>().SetTrigger("lost");
                this.WaitSecondsThen(0.5f, FinishMinigame);
                Title.GetComponent<Animator>().SetTrigger("fail");
            }

            transform.FindChild("Holders").GetComponent<Animator>().SetBool("isFinished", true);
            for (int i = 0; i < SortGroup.childCount; ++i)
            {
                SortGroup.GetChild(i).GetComponent<Animator>().SetTrigger(WinState ? "correct" : "incorrect");
            }

            Controller.Touchable = false;
            Timer.Timer.StopTimer();

            Genie.I.LogEvent(new OE_ASSESS(this, Session.instance.timeTaken));

            base.End();
        }

        void m_Holder_OnObjectAdded(DragHolder arg1, DragObject arg2)
        {
            NumberRoot.FindChild("Seat" + (arg2.Index + 1).ToStringLookup()).GetComponent<Animator>().SetTrigger("count");

            if (arg1.Count == arg1.MaxAllowed)
            {
                Win();
            }
            else
            {
                float percentComplete = arg1.Count / (float)arg1.MaxAllowed;
                foreach (var chicken in m_Objects)
                    chicken.GetComponent<NestChicken>().SetPanicProgress(percentComplete);
            }
        }

        private void FinishMinigame()
        {
            if (!IsStandalone)
                Close();
            else
                this.WaitSecondsThen(0.8f, Application.LoadLevel, SceneMgr.GAME);
        }

        private void SpawnAllChickens()
        {
            for (int i = 0; i < m_Objects.Count; ++i)
            {
                GameObject obj = m_Objects[i].gameObject;
                UnityHelper.SafeDestroy(ref obj);
            }

            m_Objects.Clear();

            RectTransform holderTransform = StartingNest.GetComponent<RectTransform>();
            Rect holderRect = holderTransform.rect;

            for (int i = 0; i < 10; ++i)
            {
                Vector3 localPosition = holderTransform.localPosition;
                localPosition.y = RNG.Instance.NextFloat(holderRect.yMin, holderRect.yMax);
                localPosition.x = RNG.Instance.NextFloat(holderRect.xMin, holderRect.xMax);
                Vector3 worldPosition = holderTransform.TransformPoint(localPosition);
                GameObject newChicken = (GameObject)Instantiate(ChickenPrefab, Vector3.zero, Quaternion.identity);
                newChicken.transform.SetParent(SortGroup, false);
                newChicken.transform.position = worldPosition;
                newChicken.GetComponent<StartingSeat>().SetHolder(StartingNest);
                m_Objects.Add(newChicken.GetComponent<DragObject>());
            }
        }

        private IEnumerator SortRoutine()
        {
            while (true)
            {
                using (PooledList<Transform> children = PooledList<Transform>.Create())
                {
                    if (children.Capacity < SortGroup.childCount)
                        children.Capacity = SortGroup.childCount;
                    for (int i = 0; i < SortGroup.childCount; ++i)
                    {
                        children.Add(SortGroup.GetChild(i));
                    }

                    children.Sort(s_HeightSorter);

                    for (int i = 0; i < children.Count; ++i)
                    {
                        children[i].SetSiblingIndex(i);
                    }

                    if (Controller.DraggingObject != null && Controller.DraggingObject.transform.parent == SortGroup)
                    {
                        Controller.DraggingObject.transform.SetAsLastSibling();
                    }
                }

                yield return 0.05f;
            }
        }

        static private Comparison<Transform> s_HeightSorter = SortByHeight;

        static private int SortByHeight(Transform a, Transform b)
        {
            float sortDistance = b.position.y - a.position.y;
            if (Mathf.Approximately(sortDistance, 0))
                sortDistance = b.GetInstanceID() - a.GetInstanceID();
            return (int)Mathf.Sign(sortDistance);
        }
    }
}
