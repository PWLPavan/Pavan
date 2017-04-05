using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using FGUnity.Utils;
using UnityEngine.EventSystems;

namespace Minigames
{
    public class AutoMinigameStart : MonoBehaviour
    {
        public MinigameCtrl Minigame;

        private void Start()
        {
            Minigame.Open();
        }
    }
}
