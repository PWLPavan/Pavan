using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FGUnity.Utils;

public class QueueController : MonoBehaviour
{
    #region Prefabs
    public GameObject containerPrefab;

    public GameObject creatureOnesWhitePrefab;
    public GameObject creatureTensWhitePrefab;

    public GameObject creatureOnesBrownPrefab;
    public GameObject creatureTensBrownPrefab;

    public GameObject endlessOnesWhitePrefab;
    public GameObject endlessTensWhitePrefab;

    public GameObject endlessOnesBrownPrefab;
    public GameObject endlessTensBrownPrefab;
    #endregion

    #region Gui
    Transform spawn;
    Transform start;
    Transform end;
    #endregion

    #region Inspector
    public float spawnRate = .5f;       // seconds until the next spawns
    public int queueMax = 5;        // max number of Quantities allowed in queue

    public Vector3 onesQueuePosition = new Vector3(1.22f, -7.2f, 0.0f);
    public Vector3 tensQueuePosition = new Vector3(-3.6f, -7.21f, 0.0f);

    public Vector3 onesPlaneOnesQueuePosition = new Vector3(1.22f, -7.2f, 0.0f);

    public Vector3 onesSpawnPosition = new Vector3(3.6f, -3.6f, 0.0f);
    public Vector3 tensSpawnPosition = new Vector3(-2.4f, -3.6f, 0.0f);
    public Vector3 onesPlaneOnesSpawnPosition = new Vector3(3.6f, -3.6f, 0.0f);
    #endregion

    #region Members
    MyScreen mScreen;
    public MyScreen screen {
        get { return mScreen; }
        set { mScreen = value; }
    }

    [HideInInspector]
    public delegate void ContainerEndMove(QueueContainer container);
    [HideInInspector]
    public ContainerEndMove onContainerEndMove;

    GameObject _onesPrefabRef;
    GameObject _tensPrefabRef;

    GameObject _endlessOnesRef;
    GameObject _endlessTensRef;

    GameObject _endlessOnesWhite;
    GameObject _endlessTensWhite;

    GameObject _endlessOnesBrown;
    GameObject _endlessTensBrown;

    GameObject _infiniteOnesChickenSpawned;
    GameObject _infiniteTensNestSpawned;

    GameObject[] mQueue;

    //float mSpawnTimer = 0;
    int mNumInQueue = 0;    // track this ourselves

    float mQueueSpacing;

    [HideInInspector]
    public bool isRunning = false;
    [HideInInspector]
    public bool isTensDraggingEnabled = true;
    [HideInInspector]
    public bool isOnesDraggingEnabled = true;
    #endregion

    #region Controller
    void Awake() {
		_endlessOnesWhite = (GameObject)Instantiate(endlessOnesWhitePrefab, onesQueuePosition, Quaternion.identity);
		_endlessTensWhite = (GameObject)Instantiate(endlessTensWhitePrefab, tensQueuePosition, Quaternion.identity);
        _endlessTensWhite.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f); // horizontal flip tens col
        
        _endlessOnesBrown = (GameObject)Instantiate(endlessOnesBrownPrefab, onesQueuePosition, Quaternion.identity);
        _endlessTensBrown = (GameObject)Instantiate(endlessTensBrownPrefab, tensQueuePosition, Quaternion.identity);
        _endlessTensBrown.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f); // horizontal flip tens col

        _onesPrefabRef = creatureOnesWhitePrefab;
        _tensPrefabRef = creatureTensWhitePrefab;
        _endlessOnesRef = _endlessOnesWhite;
        _endlessTensRef = _endlessTensWhite;
    }

    void Start() {
        // pre populate the queue
        mQueue = new GameObject[queueMax];
        
        spawn = this.transform.Find("Spawn");
        start = this.transform.Find("Start");
        end = this.transform.Find("End");

        mQueueSpacing = (end.transform.position.x - 
                         start.transform.position.x) / (queueMax - 1);   // -2 cause queue already contains start and end wells
    }

    public void EnableInput(bool enabled, int queueValue) {
        // enable dragging
        if (queueValue == 1) {
            isOnesDraggingEnabled = enabled;
            if (_infiniteOnesChickenSpawned)
                _infiniteOnesChickenSpawned.GetComponent<QueueContainer>().dragGroup.GetComponent<DragGroup>().isDraggingEnabled = enabled;
                
        } else if (queueValue == 10) {
            isTensDraggingEnabled = enabled;
            if (_infiniteTensNestSpawned)
                _infiniteTensNestSpawned.GetComponent<QueueContainer>().dragGroup.GetComponent<DragGroup>().isDraggingEnabled = enabled;
                
        }
    }

    void EnableQueue (GameObject infiniteDragChicken, GameObject endlessQueue, System.Action callback, bool enabled) {
        if (infiniteDragChicken)
            infiniteDragChicken.GetComponent<QueueContainer>().dragGroup.GetComponent<DragGroup>().isDraggingEnabled = enabled;

        if (enabled) {
            //mEndlessOnes.GetComponent<Animator>().SetBool("endlessEnable", true);
            //TODO: make sure to set to false afterwards - joe    // what does this mean??
            if (endlessQueue.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Default") ||
                endlessQueue.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.QueueEndlessDisable")) {
                endlessQueue.GetComponent<Animator>().SetBool("endlessEnable", true);
                endlessQueue.GetComponent<MecanimEventHandler>().RegisterOnStateEnd(Animator.StringToHash("Base Layer.QueueEndlessEnable"), callback);
            }
        } else {
            //TODO: check anim states
            //mEndlessOnes.GetComponent<Animator>().SetTrigger("endlessDisable");
            if (endlessQueue.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Default")) {
                endlessQueue.GetComponent<Animator>().SetTrigger("endlessDisable");
            }

        }
    }

    void EndlessOnes_OnEnabled () {
        _endlessOnesWhite.GetComponent<MecanimEventHandler>().UnRegisterOnStateEnd(Animator.StringToHash("Base Layer.QueueEndlessEnable"), EndlessOnes_OnEnabled);
        _endlessOnesBrown.GetComponent<MecanimEventHandler>().UnRegisterOnStateEnd(Animator.StringToHash("Base Layer.QueueEndlessEnable"), EndlessOnes_OnEnabled);
    }

    void EndlessTens_OnEnabled (){
        _endlessTensWhite.GetComponent<MecanimEventHandler>().UnRegisterOnStateEnd(Animator.StringToHash("Base Layer.QueueEndlessEnable"), EndlessTens_OnEnabled);
        _endlessTensBrown.GetComponent<MecanimEventHandler>().UnRegisterOnStateEnd(Animator.StringToHash("Base Layer.QueueEndlessEnable"), EndlessOnes_OnEnabled);
    }
    #endregion

    #region Queue Management
    void AddToQueue (GameObject objToAdd) {
		// loop backwards through queue and push quantities to the farthest free idx
		for (int i = mQueue.Length - 1; i >= 0; i--) {
			if (mQueue[i] == null)
				continue;
			
			int targetIdx = FurthestContinuousFreeSpace(i);
			if (targetIdx == -1)
				continue;
			
			AssignQuantityIdxTarget(i, targetIdx);
		}
		
		// add and move
		AssignQuantityIdxTarget(objToAdd, FurthestContinuousFreeSpace(0));
		mNumInQueue++;
	}

	public void RemoveFromQueue (QueueContainer obj, bool destroy = true) {
		for (int i = 0; i < mQueue.Length; ++i) {
			if (mQueue[i] && mQueue[i].GetComponent<QueueContainer>() == obj) {
				Destroy(mQueue[i]); //TODO: keep an eye on this
				mQueue[i] = null;
				mNumInQueue--;
				break;
			}
		}
        
		if (destroy)
			Destroy (obj.gameObject);
	}

	public void DestroyQueue () {
		//TODO: refactor to handle at any level of deletion
		for (int i = 0; i < mQueue.Length; ++i) {
			if (mQueue[i] == null)
				continue;
			
			// destroy creatures in drag group
			CreatureCtrl[] creatures = mQueue[i].GetComponent<QueueContainer>().dragGroup.GetComponentsInChildren<CreatureCtrl>();
			for (int j = 0; j < creatures.Length; ++j) {
				Destroy(creatures[j].gameObject);
			}

			// destroy drag group
			Destroy(mQueue[i].GetComponent<QueueContainer>().dragGroup.gameObject);

			// destroy queue container
			RemoveFromQueue(mQueue[i].GetComponent<QueueContainer>());
		}

		if (_infiniteOnesChickenSpawned) {
			CreatureCtrl[] creatures = _infiniteOnesChickenSpawned.GetComponent<QueueContainer>().dragGroup.GetComponentsInChildren<CreatureCtrl>();
			for (int j = 0; j < creatures.Length; ++j) {
				Destroy(creatures[j].gameObject);
			}
			Destroy(_infiniteOnesChickenSpawned.GetComponent<QueueContainer>().dragGroup.gameObject);
			RemoveFromQueue(_infiniteOnesChickenSpawned.GetComponent<QueueContainer>());
			_infiniteOnesChickenSpawned = null;
		}
        
        if (_infiniteTensNestSpawned) {
			CreatureCtrl[] creatures = _infiniteTensNestSpawned.GetComponent<QueueContainer>().dragGroup.GetComponentsInChildren<CreatureCtrl>();
			for (int j = 0; j < creatures.Length; ++j) {
				Destroy(creatures[j].gameObject);
			}
			Destroy(_infiniteTensNestSpawned.GetComponent<QueueContainer>().dragGroup.gameObject);
			RemoveFromQueue(_infiniteTensNestSpawned.GetComponent<QueueContainer>());
			_infiniteTensNestSpawned = null;
		}
    }
	#endregion

	#region Methods
	void Container_onEndMove (QueueContainer container) {
		container.onContainerEndMove = null;
		onContainerEndMove(container);
	}

	void FillDragGroup (int numCreatures, GameObject prefab, Transform parent) {
		// find the right mounts
		Transform mount;
		int mountIdx = 1;
		string mountPrefix = "One";
		if (prefab == creatureTensWhitePrefab || prefab == creatureTensBrownPrefab) 
			mountPrefix = "Ten";

        string color = CreatureCtrl.COLOR_WHITE;
        if (prefab == creatureTensBrownPrefab || prefab == creatureOnesBrownPrefab)
            color = CreatureCtrl.COLOR_BROWN;

		for (int i = 0; i < numCreatures; ++i) {
			GameObject creature = (GameObject)Instantiate(prefab,
			                                              Vector3.zero,
			                                              Quaternion.identity);
            mount = parent.transform.Find((mountPrefix + "Mount" + mountIdx.ToStringLookup()));
			// find mount and position
			creature.transform.localPosition = mount.transform.localPosition;
			creature.transform.SetParent(parent.transform, false);
			creature.GetComponent<CreatureCtrl>().inner.GetComponent<SpriteRenderer>().sortingOrder = mount.GetComponent<SpriteRenderer>().sortingOrder;
			creature.GetComponent<CreatureCtrl>().SetBool("inQueue", true);
            creature.GetComponent<CreatureCtrl>().color = color;

            mountIdx++;
		}
	}

	int FurthestContinuousFreeSpace (int startIdx) {
		// start on the next index
		int furthestRight = -1;
		for (int i = startIdx + 1; i < mQueue.Length; ++i) {
			if (mQueue[i] == null)
				furthestRight = i;
			else
				break;
		}
		return furthestRight;
	}

	void AssignQuantityIdxTarget (int currentIdx, int targetIdx) {
		Vector3 pos = start.transform.localPosition;
		pos.x += (mQueueSpacing * targetIdx);
		mQueue[currentIdx].GetComponent<QueueContainer>().StartMove(pos);
		
		mQueue[targetIdx] = mQueue[currentIdx];
		mQueue[currentIdx] = null;
	}

	void AssignQuantityIdxTarget (GameObject objToAdd, int targetIdx) {
		Vector3 pos = start.transform.localPosition;
		pos.x += (mQueueSpacing * targetIdx);
		objToAdd.GetComponent<QueueContainer>().StartMove(pos);
		mQueue[targetIdx] = objToAdd;
	}
	#endregion

	#region Endless Queue Management
	public void Init () {
        // determine if chickens will be brown or white
        _onesPrefabRef = creatureOnesWhitePrefab;
        _tensPrefabRef = creatureTensWhitePrefab;
        _endlessOnesRef = _endlessOnesWhite;
        _endlessTensRef = _endlessTensWhite;
        if (Session.instance.currentLevel.useBrownQueue) {
            _onesPrefabRef = creatureOnesBrownPrefab;
            _tensPrefabRef = creatureTensBrownPrefab;
            _endlessOnesRef = _endlessOnesBrown;
            _endlessTensRef = _endlessTensBrown;
        }

        // show endless queue
        // animate on
        EndlessEnter();

        // spawn quantities
        _endlessOnesRef.transform.position = (Session.instance.currentLevel.tensColumnEnabled) ? onesQueuePosition : onesPlaneOnesQueuePosition;
        if (Session.instance.currentLevel.onesQueueEnabled)
            _infiniteOnesChickenSpawned = ForceColumnSpawn(1, 1, _onesPrefabRef, _endlessOnesRef, (Session.instance.currentLevel.tensColumnEnabled) ? onesSpawnPosition : onesPlaneOnesSpawnPosition);
        if (Session.instance.currentLevel.tensColumnEnabled && Session.instance.currentLevel.tensQueueEnabled) {
            _infiniteTensNestSpawned = ForceColumnSpawn(1, 10, _tensPrefabRef, _endlessTensRef, tensSpawnPosition);
        }
    }

	GameObject ForceColumnSpawn (int count, int value, GameObject prefab, GameObject parent, Vector3 pos) {
		// spawn a new QueueContainer (child: DragGroup)
		GameObject obj = (GameObject)Instantiate(containerPrefab,
		                                         /*Vector3.zero*/pos,
		                                         Quaternion.identity);
		obj.GetComponent<QueueContainer>().controller = this;
		obj.GetComponent<QueueContainer>().onContainerEndMove = Container_onEndMove;
		// create a number of creatures and add them to the dragGroup
		if (obj.GetComponent<QueueContainer>().dragGroup) {
			Transform p = obj.GetComponent<QueueContainer>().dragGroup;
			FillDragGroup(count, prefab, p);
			// let the dragGroup know the whole value
			p.GetComponent<DragGroup>().SetValue(value);
			p.GetComponent<DragGroup>().SetCreaturesAlpha(0.0f);
            p.GetComponent<DragGroup>().isDraggingEnabled = (value == 1 ? isOnesDraggingEnabled : isTensDraggingEnabled);
		}
		obj.GetComponent<QueueContainer>().StartMove(pos);
		return obj;
	}

	public void EndlessDragging (int value) {
        if (value == 1) {
            _endlessOnesRef.GetComponent<Animator>().SetTrigger("endlessDragging");
        } else if (value == 10) {
            _endlessTensRef.GetComponent<Animator>().SetTrigger("endlessDragging");
        }
	}

	public void EndlessReturn (int value) {
        if (value == 1) {
            _endlessOnesRef.GetComponent<Animator>().SetTrigger("endlessReturn");
        } else if (value == 10) {
            _endlessTensRef.GetComponent<Animator>().SetTrigger("endlessReturn");
        }
    }

	public void EndlessDropped (int value) {
        if (value == 1) {
            if (_infiniteOnesChickenSpawned)   // reset dropped chicken's draggability
                _infiniteOnesChickenSpawned.GetComponent<QueueContainer>().dragGroup.GetComponent<DragGroup>().isDraggingEnabled = true;
            
            _infiniteOnesChickenSpawned = ForceColumnSpawn(1, 1, _onesPrefabRef, _endlessOnesRef, (Session.instance.currentLevel.tensColumnEnabled) ? onesSpawnPosition : onesPlaneOnesSpawnPosition);
            _endlessOnesRef.GetComponent<Animator>().SetTrigger("endlessDropped");
            _infiniteOnesChickenSpawned.GetComponent<QueueContainer>().dragGroup.GetComponent<DragGroup>().isDraggingEnabled = isOnesDraggingEnabled;
        } else if (value == 10) {
			if (_infiniteTensNestSpawned)   // reset dropped chicken's draggability
                _infiniteTensNestSpawned.GetComponent<QueueContainer>().dragGroup.GetComponent<DragGroup>().isDraggingEnabled = true;
            
            _infiniteTensNestSpawned = ForceColumnSpawn(1, 10, _tensPrefabRef, _endlessTensRef, tensSpawnPosition);
            _endlessTensRef.GetComponent<Animator>().SetTrigger("endlessDropped");
            _infiniteTensNestSpawned.GetComponent<QueueContainer>().dragGroup.GetComponent<DragGroup>().isDraggingEnabled = isTensDraggingEnabled;
        }
	}
    
	public void EndlessExit (bool ones = true, bool tens = true) {
        if (ones) {
            if (!_endlessOnesRef.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.QueueEndlessExit") &&
                !_endlessOnesRef.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.QueueEndlessHide"))
                _endlessOnesRef.GetComponent<Animator>().SetTrigger("endlessExit");
        }
        if (tens) {
            if (!_endlessTensRef.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.QueueEndlessExit") &&
                !_endlessTensRef.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.QueueEndlessHide"))
                _endlessTensRef.GetComponent<Animator>().SetTrigger("endlessExit");
        }
	}

	public void EndlessEnter (bool ones = true, bool tens = true)
    {
        bool bShowOnes = ones && Session.instance.currentLevel.onesQueueEnabled;
        bool bShowTens = tens && Session.instance.currentLevel.tensQueueEnabled && Session.instance.currentLevel.tensColumnEnabled;

        if (bShowOnes)
        {
            if (!_endlessOnesRef.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Default"))
            {
                _endlessOnesRef.GetComponent<Animator>().SetTrigger("endlessEnter");
                _endlessOnesRef.GetComponent<Animator>().ResetTrigger("endlessExit");
            }
        }
        if (bShowTens)
        {
            if (!_endlessTensRef.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Default"))
            {
                _endlessTensRef.GetComponent<Animator>().SetTrigger("endlessEnter");
                _endlessTensRef.GetComponent<Animator>().ResetTrigger("endlessExit");
            }
        }

        // hide other queues if they're showing (skip btn, etc)
        if (tens)
        {
            bool bHideWhite = (!bShowTens || _endlessTensRef == _endlessTensBrown);
            if (bHideWhite &&
                !_endlessTensWhite.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.QueueEndlessExit") &&
                !_endlessTensWhite.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.QueueEndlessHide"))
            {
                _endlessTensWhite.GetComponent<Animator>().SetTrigger("endlessExit");
            }

            bool bHideBrown = (!bShowTens || _endlessTensRef == _endlessTensWhite);

            if (bHideBrown &&
                  !_endlessTensBrown.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.QueueEndlessExit") &&
                  !_endlessTensBrown.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.QueueEndlessHide"))
            {
                _endlessTensBrown.GetComponent<Animator>().SetTrigger("endlessExit");
            }
        }
        if (ones)
        {
            bool bHideWhite = (!bShowOnes || _endlessOnesRef == _endlessOnesBrown);

            if (bHideWhite &&
                !_endlessOnesWhite.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.QueueEndlessExit") &&
                !_endlessOnesWhite.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.QueueEndlessHide"))
            {
                _endlessOnesWhite.GetComponent<Animator>().SetTrigger("endlessExit");
            }

            bool bHideBrown = (!bShowOnes || _endlessOnesRef == _endlessOnesWhite);

            if (bHideBrown &&
                  !_endlessOnesBrown.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.QueueEndlessExit") &&
                  !_endlessOnesBrown.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.QueueEndlessHide"))
            {
                _endlessOnesBrown.GetComponent<Animator>().SetTrigger("endlessExit");
            }
        }
    }
	#endregion

}
