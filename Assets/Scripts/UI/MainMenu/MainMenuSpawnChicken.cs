using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Ekstep;
using FGUnity.Utils;

public class MainMenuSpawnChicken : MonoBehaviour {

    public GameObject menuChicken;

    private bool m_AllowTap = true;
    private GameObject newChicken;

    // Use this for initialization
    void Start () {
        StartCoroutine(randomizeChicken());
        GetComponent<Button>().onClick.AddListener(touchSpawnChicken);
    }

    void touchSpawnChicken()
    {
        if (m_AllowTap)
        {
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.TOUCH, "mainMenu.spawnChicken"));
            spawnNewChicken();
            m_AllowTap = false;
            this.WaitSecondsThen(Optimizer.instance.Low ? 0.5f : 0.1f, AllowTap);
        }
    }

    private void AllowTap()
    {
        m_AllowTap = true;
    }

    void spawnNewChicken(){
        newChicken = Instantiate(menuChicken);
        newChicken.transform.SetParent(transform.parent.Find("MainMenuChickens").transform);
    }

    private IEnumerator randomizeChicken()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(3, 5));
            spawnNewChicken();
        }
    }
}
