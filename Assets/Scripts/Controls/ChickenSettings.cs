using UnityEngine;
using System.Collections;

public class ChickenSettings : SingletonBehavior<ChickenSettings> {

    public float dropSpeed;  //30f

    public Vector3 dropScale;    //Vector3(0.66f, 2.5f, 1f);

    public float plopWait; // 2f

    public float toSubZoneSpeed; // 7f

    public float snapBackSpeed; // 9f

    public float seatbeltSpeed;

    public float leaveSpeed; // 1f

    public float scatterSpeed; // 3f
    
}
