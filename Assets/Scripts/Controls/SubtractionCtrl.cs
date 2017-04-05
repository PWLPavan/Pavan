using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FGUnity.Utils;
using Ekstep;

public class SubtractionCtrl : MonoBehaviour {


    #region Prefabs
    public GameObject creaturePrefab;
    public GameObject whiteCreaturePrefab;

    #endregion


    #region Inspector
    #endregion


    #region Gui
    private Transform _suitcase;


    public DropZoneCtrl onesZone { get; private set; }
    public DropZoneCtrl tensZone { get; private set; }


    #endregion

    #region Members
    /*MyScreen _screen;
    public MyScreen screen {
        get { return _screen; }
        set { _screen = value; }
    }*/

    List<CreatureCtrl> _creatures;

    Vector3 _tweenTarget;
    float _tweenTime = 0f;
    public float grabSpeed = 1f;
    public bool isGrabbingSuitcases = false;

    public float scatterSpeed = 1f;
    int scatterIdx = 0;
    public bool isScattering = false;

    Vector3 scatterDist = new Vector3(20f, 20f, 20f);
    Vector3 anchorVec = Quaternion.Euler(0f, 0f, 180f) * Vector3.up;

    #endregion


    #region Ctrl
    void Awake () {
        _suitcase = this.transform.Find("suitcase");

        onesZone = this.transform.Find("chickenSlots").GetComponent<DropZoneCtrl>();
        tensZone = this.transform.Find("nestSlots").GetComponent<DropZoneCtrl>();

        _creatures = new List<CreatureCtrl>();
    }

    void Start () {

    }

    void Update () {
        UpdateSuitcaseGrab();
        UpdateCreatureScatter();
    }

    void UpdateSuitcaseGrab () {
        if (!isGrabbingSuitcases)
            return;

        // lerp 2 target
        _tweenTime += (Time.deltaTime * grabSpeed);
        for (int i = 0; i < _creatures.Count; ++i) {
            _creatures[i].transform.position = Vector3.Lerp(_creatures[i].prevLocalPosition, _tweenTarget, _tweenTime);
        }

        if (_tweenTime >= 1.0f) {
            // suitcases grabbed
            isGrabbingSuitcases = false;

			_suitcase.GetComponent<Animator>().SetBool("showSuitcases", false);
			_suitcase.GetComponent<Animator>().SetTrigger("flySuitcases");

            SoundManager.instance.PlayRandomOneShot(SoundManager.instance.chickenSubtractZone, 0.5f);

            /* When all sliding is finished:
             */
            
            // hide or destroy all nests

            Vector3 spawnLocation = _creatures.Count > 0 ? _creatures[0].transform.position : Vector3.zero;

            bool bIsWhite = false;
            bool addMoreChickens = (tensZone.creatures.Count > 0);
            for (int k = 0; k < tensZone.creatures.Count; ++k)
            {
                bIsWhite = bIsWhite || tensZone.creatures[k].color == CreatureCtrl.COLOR_WHITE;
                _creatures.Remove(tensZone.creatures[k]);
            }

            tensZone.Clear();
            onesZone.creatures.Clear(); // _onesSlots chickens are safe in _creatures List, they will be destroyed after scattering
            
            // If a tens nest was subtracted, spawn an extra 10 chickens at the
            //  center of the suitcases (not for each nest subtracted though)
            //  just to make it look like there are more chickens.
            if (addMoreChickens) {
                for (int i = 0; i < 10; ++i) {
                    GameObject creature = (GameObject)Instantiate(bIsWhite ? whiteCreaturePrefab : creaturePrefab, Vector3.zero, Quaternion.identity);
                    creature.transform.position = spawnLocation;
                    _creatures.Add(creature.GetComponent<CreatureCtrl>());
                }
            }
            
            scatterIdx = 0;
            _tweenTime = 0f;
            isScattering = true;
        }
    }

    void UpdateCreatureScatter () {
        if (!isScattering)
            return;
        
        _tweenTime += (Time.deltaTime * scatterSpeed);
        if (_tweenTime >= 0.1f) {
            //TODO: set isLeaving on all chickens to true, and slide them in a random
            //       direction downwards (between 100° -170°)
            for (int j = scatterIdx; j < _creatures.Count; ++j) {
                _creatures[j].inner.GetComponent<Animator>().SetBool("isLeaving", true);
				_creatures[j].inner.GetComponent<Animator>().SetBool("isWalking", true);
                _creatures[j].prevLocalPosition = _creatures[j].transform.position;

				bool direction = RNG.Instance.NextBool();

				Vector3 scatterPos = Quaternion.Euler(0f, 0f, direction ? Random.Range(70f, 80f) : Random.Range(-70f, -80f)) * anchorVec;
                scatterPos.Scale(scatterDist);
				_creatures[j].StartMove(scatterPos, true, ChickenSettings.instance.scatterSpeed * RNG.Instance.NextFloat(0.2f, 1.1f));

				if (_creatures[j].transform.position.x - scatterPos.x < 0) {
					_creatures[j].transform.localScale = new Vector3(-1, 1, 1);
				}
            }

            // scattered!
            isScattering = false;
        } else {
            if (_creatures.Count > scatterIdx && Random.value < 0.5f) {
                _creatures[scatterIdx].inner.GetComponent<Animator>().SetBool("isLeaving", true);
				_creatures[scatterIdx].inner.GetComponent<Animator>().SetBool("isWalking", true);
                _creatures[scatterIdx].prevLocalPosition = _creatures[scatterIdx].transform.position;

				bool direction = RNG.Instance.NextBool();

				Vector3 scatterPos = Quaternion.Euler(0f, 0f, direction ? Random.Range(70f, 80f) : Random.Range(-70f, -80f)) * anchorVec;
                scatterPos.Scale(scatterDist);
                _creatures[scatterIdx].StartMove(scatterPos, true, ChickenSettings.instance.scatterSpeed * RNG.Instance.NextFloat(0.2f, 1.1f));

                if (_creatures[scatterIdx].transform.position.x - scatterPos.x < 0) {
                    _creatures[scatterIdx].transform.localScale = new Vector3(-1, 1, 1);
                }

                scatterIdx++;
            }
        }
    }

    #endregion



    #region Methods
    public void Clear () {
        if (!this.isActiveAndEnabled)
            return;

        onesZone.Clear();
        tensZone.Clear();

        for (int i = 0; i < _creatures.Count; ++i) {
            Destroy(_creatures[i].gameObject);
        }
        _creatures.Clear();

        scatterIdx = 0;
        isScattering = false;
        isGrabbingSuitcases = false;

        //_suitcase.GetComponent<Animator>().SetBool("showSuitcases", true);
    }

    public void AddDragGroup (int zone, DragGroup group) {
        if (zone == 1) {

            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.OTHER, "gameplay.onesSubtract"));

            onesZone.AddDragGroup(group);
        } else if (zone == 10) {

            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.OTHER, "gameplay.tensSubtract"));

            tensZone.AddDragGroup(group);
        }
    }
    
    public void SetHighlight (int zone, bool enabled) {
        if (zone == 1) {

            onesZone.SetHighlight(enabled);
        } else if (zone == 10) {

            tensZone.SetHighlight(enabled);
        }
    }

    public void SetTarmacColor(Color inColor)
    {
        var spriteRenderer = transform.Find("BG").GetComponent<SpriteRenderer>();
        spriteRenderer.color = inColor;
    }

    public void ClearHighlight() {
        SetHighlight(1, false);
        SetHighlight(10, false);
    }

    public void ShowCorrectFeedback () {
        // trigger correctSeat on all chickens & nests in drop zone

        if ((onesZone.creatures.Count + tensZone.creatures.Count) == 0)
            return;

        onesZone.ShowCorrectFeedback();
        tensZone.ShowCorrectFeedback();
        
        // slide all chickens & nests to the center of the suitcases

        _creatures.AddRange(onesZone.creatures);
        _creatures.AddRange(tensZone.creatures);

        _tweenTime = 0f;
        _tweenTarget = _suitcase.transform.position;
        for (int i = 0; i < _creatures.Count; ++i) {
            _creatures[i].prevLocalPosition = _creatures[i].transform.position;
        }
        isGrabbingSuitcases = true;

        // on the suitcase animator, set showSuitcases to true
        _suitcase.GetComponent<Animator>().SetBool("showSuitcases", true);
		//_suitcase.GetComponent<Animator>().SetBool("showSuitcases", false);
		//_suitcase.GetComponent<Animator>().SetTrigger("flySuitcases");
    }

    #endregion

}
