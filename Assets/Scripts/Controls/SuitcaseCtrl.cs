using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using FGUnity.Utils;
using UnityEngine.EventSystems;
using Ekstep;

public class SuitcaseCtrl : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
{
    #region Inspector
    public Sprite[] stamps;
    #endregion

    #region Gui
    Transform _stampsHolder;
    public Transform getStampSlotByIdx (int idx) {
        return _stampsHolder.transform.Find("StampSlot" + idx.ToStringLookup());
    }

    Transform _closeBtn;
	Transform _undoBtn;
    Transform _gridBtn;

    Transform m_DragRegionMask;
    Transform m_StampRevealRoot;
    Transform m_Controls;
    #endregion

    #region Members

    public bool Open { get; private set; }

    bool m_UnlockingStamp;
    bool m_AllowDragNewStamp;
    StampCtrl m_SelectedStamp;
    bool m_AllowUndo = false;

    private RotateStampCtrl m_RotateStampControl;
    private ScaleStampCtrl m_ScaleStampCtrl;
    private MaskOptimizer m_Masks;
    private CoroutineHandle m_SlideRoutine;

    [HideInInspector]
    public delegate void OnExit();
    [HideInInspector]
    public OnExit onExited;
    #endregion

    #region Ctrl
    void Awake () {
        _stampsHolder = this.transform.Find ("StampsHolder/Mask");
        _closeBtn = this.transform.Find ("Close/closeButton");
		_undoBtn = this.transform.Find ("Grid/undoButton");
        _gridBtn = this.transform.Find("Grid/closeButton");
        m_StampRevealRoot = this.transform.Find("StampRevealHolder");
        m_Controls = this.transform.Find("stampControls");
        m_DragRegionMask = this.transform.Find("hitArea");

        m_Masks = gameObject.AddComponent<MaskOptimizer>();
    }

    void Start ()
    {
        _closeBtn.GetComponent<Button>().onClick.AddListener(CloseBtn_onClick);
        _gridBtn.GetComponent<Button>().onClick.AddListener(GridBtn_onClick);
		_undoBtn.GetComponent<Button>().onClick.AddListener(GridBtn_onClick);

        Transform rotateControls = m_Controls.FindChild("rotate");
        m_RotateStampControl = rotateControls.gameObject.AddComponent<RotateStampCtrl>();

        Transform scaleControls = m_Controls.FindChild("scale");
        m_ScaleStampCtrl = scaleControls.gameObject.AddComponent<ScaleStampCtrl>();
    }

    public void Init(bool unlockStamp)
    {
        m_UnlockingStamp = unlockStamp;
        m_AllowDragNewStamp = false;
        m_AllowUndo = false;
		SetGridButton();

        if (unlockStamp)
            this.WaitSecondsThen(0.25f, () => { m_AllowDragNewStamp = true; });

        // Load stamp data
        InitializeStampData();

        // fill with already unlocked stamps
        StampCtrl slot;
        int numStamps = (Session.Exists ? Session.instance.numStamps : SaveData.instance.Stamps);
        if (unlockStamp)
            --numStamps;
        for (int i = 0; i < m_StampData.Length; ++i)
        {
            slot = m_StampData[i];
            slot.transform.FindChild("Stamp").GetComponent<Image>().sprite = stamps[i];
            slot.transform.FindChild("Stamp").GetComponent<Image>().SetNativeSize();
            slot.Show(i < numStamps);
        }
		if(numStamps > 0){
			GetComponent<Animator>().SetTrigger("showGrid");
		}else{
			GetComponent<Animator>().SetTrigger("hideGrid");
		}
    }
    #endregion

    #region Methods
    public void Show (bool show, bool unlockStamp = false)
    {    
        this.GetComponent<Animator>().SetBool("showCollection", show);

        if (show)
            m_Masks.StartChecking();
        else
            m_Masks.StopChecking();

        if (show && !Open)
        {
            SoundManager.instance.PlayOneShot(SoundManager.instance.suitcaseOpen);
        }

        Open = show;

        // fill all stamps from beginning (can't skip adding if we're persisting this ctrl)
        Init(unlockStamp);

        // num stamps will increment after showing, hence the weirdness of indexing below
        /*if (show && unlockStamp) {
            // unlock next stamp in list
            if (stamps.Length > Session.instance.numStamps)
                getStampSlotByIdx(Session.instance.numStamps + 1).GetComponent<Image>().sprite = stamps[Session.instance.numStamps];
        }*/
        if (show && unlockStamp) {
            InitializeRevealAnimation();
        }
        else
        {
            m_StampRevealRoot.gameObject.SetActive(false);
            //m_StampRevealRoot.GetComponent<Animator>().SetBool("showReveal", false);
        }
    }
    #endregion

    #region Reveal Animation

    private void InitializeRevealAnimation()
    {
        int numStamps = (Session.Exists ? Session.instance.numStamps : SaveData.instance.Stamps);
        Transform revealHolder = m_StampRevealRoot.FindChild("StampReveal");
        m_StampRevealRoot.gameObject.SetActive(true);

        for (int i = 1; i <= 6; ++i)
        {
            Transform stamp = revealHolder.FindChild("Stamp" + i.ToStringLookup());
            Text numberText = revealHolder.FindChild("Numbers/number" + i.ToStringLookup()).GetComponent<Text>();
            int stampValue = i + numStamps - 4;
            int stampIndex = stampValue - 1;
            if (stampIndex < 0 || stampIndex >= stamps.Length)
            {
                stamp.gameObject.SetActive(false);
                numberText.gameObject.SetActive(false);
            }
            else
            {
                stamp.gameObject.SetActive(true);
                numberText.gameObject.SetActive(true);

                numberText.GetComponent<Text>().text = (stampValue * 10).ToStringLookup();

                Vector3 newRotation = m_StampData[stampIndex].GetDefaultRotation();

                if (i == 4)
                {
                    Transform newImage = stamp.FindChild("newStamp");
                    newImage.GetComponent<Image>().sprite = stamps[stampIndex];

                    m_StampRevealRoot.GetComponent<Animator>().SetBool("rotateStamp", !Mathf.Approximately(newRotation.z, 0));
                }
                else
                {
                    if (stampValue < numStamps)
                    {
                        stamp.GetComponent<Image>().sprite = stamps[stampIndex];
                        // TODO: Replace constants with better hierarchy for stamps on
                        // this screen.
                        newRotation.z += 1.760834f;
                        stamp.transform.localEulerAngles = newRotation;
                    }
                }
            }
        }

        Transform revealText = revealHolder.FindChild("revealText");
        revealText.GetComponent<Text>().text = (numStamps * 10).ToStringLookup();
        m_StampRevealRoot.GetComponent<Animator>().SetBool("showReveal", true);
    }

    public void SetRevealRotation()
    {
        int numStamps = (Session.Exists ? Session.instance.numStamps : SaveData.instance.Stamps);
        Transform revealHolder = m_StampRevealRoot.FindChild("StampReveal");
        Transform stamp = revealHolder.FindChild("Stamp4");

        Vector3 newRotation = m_StampData[numStamps - 1].GetDefaultRotation();
        newRotation.z += 1.760834f;
        stamp.transform.localEulerAngles = newRotation;
    }

    #endregion

    #region Input
    void CloseBtn_onClick ()
    {
        if (m_UnlockingStamp || m_DraggingStamp)
            return;

        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "suitcase.close"));

        SoundManager.instance.PlayOneShot(SoundManager.instance.suitcaseClose);

        DropStamp();
        SaveStampData();

        Show (false);
        if (onExited != null)
            onExited();
    }

    void GridBtn_onClick()
    {
        if (m_UnlockingStamp || m_DraggingStamp)
            return;

        if (m_AllowUndo)
        {
            UndoStampAlign();
        }
        else
        {
            AlignStampsToGrid();
        }
    }
    #endregion

    #region Stamp Rearrangement

    private StampCtrl[] m_StampData;
    private bool m_DraggingStamp = false;
    private Vector3 m_OldMousePosition;

    private void InitializeStampData()
    {
        if (m_StampData == null)
        {
            m_StampData = new StampCtrl[stamps.Length];
            for (int i = 0; i < stamps.Length; ++i)
            {
                Transform node = getStampSlotByIdx(i + 1);
                StampCtrl ctrl = node.gameObject.GetComponent<StampCtrl>();
                if (ctrl == null)
                    ctrl = node.gameObject.AddComponent<StampCtrl>();
                ctrl.Initialize(this);
                m_StampData[i] = ctrl;
            }
        }

        LoadStampData();
    }

    private void LoadStampData()
    {
        JSONNode stampData = SaveData.instance.StampData;
        if (stampData == null)
        {
            SaveStampData();
        }
        else
        {
            JSONArray arrayOfStamps = stampData["stamps"].AsArray;
            for (int i = 0; i < arrayOfStamps.Count; ++i)
            {
                JSONNode node = arrayOfStamps[i];
                m_StampData[i].LoadJSON(node);
            }
        }
    }

    private void SaveStampData()
    {
        JSONClass shell = new JSONClass();

        JSONArray listOfStamps = new JSONArray();
        int numStamps = (Session.Exists ? Session.instance.numStamps : SaveData.instance.Stamps);
        for (int i = 0; i < numStamps && i < stamps.Length; ++i )
        {
            listOfStamps.Add(m_StampData[i].ToJSON());
        }
        shell["stamps"] = listOfStamps;

        SaveData.instance.StampData = shell;
    }

    private void AlignStampsToGrid()
    {
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "suitcase.sort"));
        DropStamp();
        foreach(var stamp in m_StampData)
        {
            stamp.PushState();
            stamp.ResetToDefault();
        }
        m_AllowUndo = true;
		SetUndoButton();
    }

    private void UndoStampAlign()
    {
        Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "suitcase.undoSort"));
        foreach (var stamp in m_StampData)
        {
            stamp.PopState();
        }
        m_AllowUndo = false;
		SetGridButton();
    }

    public void OnStampPicked(StampCtrl inStampCtrl)
    {
        if (m_SelectedStamp != inStampCtrl)
            DropStamp();

        if (inStampCtrl != null)
        {
            if (m_SelectedStamp != inStampCtrl)
            {
                SoundManager.instance.PlayRandomOneShot(SoundManager.instance.stampLift);
            }
            else
            {
                SoundManager.instance.PlayRandomOneShot(SoundManager.instance.stampSlide);
            }

            m_SelectedStamp = inStampCtrl;
            m_OldMousePosition = Input.mousePosition;
            HideControls();
            m_DraggingStamp = true;

            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DRAG, "suitcase.stamp"));

            m_AllowUndo = false;
			SetGridButton();
        }
    }

    public void DropStamp()
    {
        if (m_SelectedStamp != null)
        {
            SoundManager.instance.PlayRandomOneShot(SoundManager.instance.stampDrop);

            m_SelectedStamp.Drop();
            HideControls();
            m_SelectedStamp = null;
            m_DraggingStamp = false;
            m_SlideRoutine.Clear();
        }
    }

    private void HideControls()
    {
        m_Controls.GetComponent<Animator>().SetBool("showControls", false);
        m_Controls.GetComponent<Animator>().SetBool("showControlsEdge", false);
        m_Controls.GetComponent<Animator>().SetBool("showControlsBottom", false);

        m_RotateStampControl.SetStamp(null);
        m_ScaleStampCtrl.SetStamp(null);
    }

    private void ShowControls()
    {
        string control = "showControls";
        bool bEdge = DetectStampControlsNearEdge();
        bool bRoomAbove = DetectStampControlsRoomAbove();
        if (bEdge && bRoomAbove)
            control = "showControlsBottom";
        else if (bEdge)
            control = "showControlsEdge";

        m_Controls.GetComponent<Animator>().SetBool(control, true);
        m_Controls.GetComponent<Animator>().SetTrigger(control + "Trigger");
        m_Controls.position = m_SelectedStamp.transform.position;

        m_RotateStampControl.SetStamp(m_SelectedStamp);
        m_ScaleStampCtrl.SetStamp(m_SelectedStamp);

        SoundManager.instance.PlayOneShot(SoundManager.instance.stampControlsOn);
    }

    private bool DetectStampControlsNearEdge()
    {
        float stampX = m_SelectedStamp.transform.position.x;
        float edgeX = 0;
        float range = Screen.width / 4;
        if (Mathf.Abs(edgeX - stampX) < range)
            return true;
        return false;
    }

    private bool DetectStampControlsRoomAbove()
    {
        float stampY = m_SelectedStamp.transform.position.y;
        float edgeY = 0;
        float range = Screen.height / 2;
        if (Mathf.Abs(edgeY - stampY) < range)
            return false;
        return true;
    }

    private void Update()
    {
        if (m_SelectedStamp != null)
        {
            if (m_DraggingStamp)
            {
                if (Input.GetMouseButton(0))
                {
                    Vector3 newPosition = Input.mousePosition;
                    for (int i = 1; i <= 5; ++i)
                    {
                        Vector3 testPosition = Vector3.Lerp(m_OldMousePosition, newPosition, i / 5.0f);
                        if (m_DragRegionMask.HitTest(testPosition))
                            m_SelectedStamp.transform.position = testPosition;
                    }
                    m_OldMousePosition = newPosition;
                }
                else
                {
                    Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "suitcase.stamp"));
                    m_DraggingStamp = false;
                    ShowControls();
                }
            }
        }
    }

    #endregion

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!Open)
            return;

        if (m_UnlockingStamp && m_AllowDragNewStamp)
        {
            int nextStamp = Session.Exists ? Session.instance.numStamps : SaveData.instance.Stamps;
            m_UnlockingStamp = false;
            StampCtrl stampCtrl = m_StampData[nextStamp - 1];
            stampCtrl.ResetToDefault();
            stampCtrl.PickUp();
            m_StampRevealRoot.GetComponent<Animator>().SetBool("showReveal", false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!Open)
            return;

        if (!m_UnlockingStamp && !m_DraggingStamp)
        {
            DropStamp();
        }
    }

	public void SetGridButton(){
		_gridBtn.gameObject.SetActive(true);
		_undoBtn.gameObject.SetActive(false);
	}

	public void SetUndoButton(){
		_gridBtn.gameObject.SetActive(false);
		_undoBtn.gameObject.SetActive(true);
	}
}
