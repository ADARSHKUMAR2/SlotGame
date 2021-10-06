using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SlotGame
{
    public class ReelPanel : MonoBehaviour
    {
        [SerializeField] private List<Reel> _allReels;
        [SerializeField] private UiController _uiController;
        
        public List<int> stopPositions;
        public bool _stopAtChosenPos;
        public int totalReelsStopped { private set; get; }
        public void SpinReels(bool startSpin = true)
        {
            foreach (var reel in _allReels)
            {
                if (startSpin)
                {
                    totalReelsStopped = 0;
                    reel.StartSpin();
                }
                else
                {
                    _uiController.ChanePlayBtnInteraction(false);
                    
                    if(_stopAtChosenPos)
                        ForceStop(stopPositions,MakeBtnInteractable);
                    else
                        reel.StopSpin(MakeBtnInteractable);
                }
            }
        }

        private void MakeBtnInteractable()
        {
            _uiController.ChanePlayBtnInteraction(true);
        }

        private void ForceStop(List<int> pos,Action btnInteraction)
        {
            foreach (var reel in _allReels)
                reel.CheckForCustomPos(pos,btnInteraction);
        }

        public void UpdateReelsStoppedCount()
        {
            Debug.Log($"{totalReelsStopped}");
            totalReelsStopped++;
        }
    }

}

