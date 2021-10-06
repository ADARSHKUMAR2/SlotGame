using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using SlotGame;
using UnityEngine;

public class Reel : MonoBehaviour, IReel
{
    [SerializeField] private List<Slot> _slots;
    [SerializeField] private List<string> reelStrip;
    [SerializeField] private Symbols _symbols;

    [SerializeField] private int _reelNumber;
    [SerializeField] private float _spinSpeed;

    private Coroutine _spinCoroutine;
    private Coroutine _stopCoroutine;
    private float _curSpeed;
    private bool shouldStop;
    private int curIndex;

    private List<Vector3> startPos;
    private List<int> customPos;
    [SerializeField] private float bounceOffset = 1f;
    private Action changeBtnInteractionAction;
    private void Start()
    {
        InitialiseSlots();
    }
    
    private void InitialiseSlots()
    {
        var stripLength = reelStrip.Count;
        for (int i = 0; i < _slots.Count; i++)
        {
            int value = i % stripLength;
            _slots[i].UpdateIndex(i);
            _slots[i].GetComponent<SpriteRenderer>().sprite = _symbols.SetSymbol(reelStrip[value]);
        }
    }

    public void StartSpin()
    {
        _curSpeed = _spinSpeed;
        _spinCoroutine = StartCoroutine(SpinReel());
    }

    private IEnumerator SpinReel()
    {
        foreach (var slot in _slots)
            slot.transform.Translate(Vector3.down * 0.1f);

        UpdateSlotPosition();
        yield return new WaitForSeconds(0.005f * _spinSpeed);
        _spinCoroutine = StartCoroutine(SpinReel());
    }

    private void UpdateSlotPosition()
    {
        foreach (var slot in _slots)
        {
            var pos = slot.transform.position;
            //To reset position
            if (pos.y < -4.5f)
            {
                pos.y = 3f; //moves the last slot pos to the top
                UpdateSlotIndex(slot);
            }
            slot.transform.position = pos;
        }
    }

    private void UpdateSlotIndex(Slot slot)
    {
        curIndex--;
        if (curIndex < 0)
            curIndex = reelStrip.Count - 1;
        slot.UpdateIndex(curIndex);
        slot.GetComponent<SpriteRenderer>().sprite = _symbols.SetSymbol(reelStrip[curIndex]);
    }

    private void LerpSlots()
    {
        foreach (var slot in _slots)
        {
            var pos = slot.transform.position;
            LeanTween.moveY(slot.gameObject, pos.y + bounceOffset, 0.4f).setEase(LeanTweenType.easeSpring);
        }

        InvokeBtnEnableAction();
    }

    private void InvokeBtnEnableAction()
    {
        var reelParent = GetComponentInParent<ReelPanel>();
        reelParent.UpdateReelsStoppedCount();
        if (reelParent.totalReelsStopped==5)
        {
            changeBtnInteractionAction?.Invoke();
            changeBtnInteractionAction = null;
        }   
        
    }
    
    #region RandomStop

    public void StopSpin([CanBeNull] Action btnInteraction)
    {
        StartCoroutine(SequentialStop());
        changeBtnInteractionAction = btnInteraction;
    }

    private IEnumerator SequentialStop()
    {
        yield return new WaitForSeconds(_reelNumber * 0.2f);
        InvokeRepeating(nameof(StopRandomly), 0.5f, 0.001f);
    }

    private void StopRandomly()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            if (Math.Abs(_slots[i].transform.position.y - (-1f)) < 0.0001f)
            {
                shouldStop = true;
                if (_spinCoroutine != null)
                    StopCoroutine(_spinCoroutine);
                
                CancelInvoke();
                
                if (shouldStop)
                    LerpSlots();
            }
        }

    }

    #endregion

    #region CustomStop

    public void CheckForCustomPos(List<int> pos,[CanBeNull] Action btnInteraction)
    {
        customPos = new List<int>();
        customPos = pos;
        changeBtnInteractionAction = btnInteraction;
        StartCoroutine(ForceStopReels());
    }

    private IEnumerator ForceStopReels()
    {
        var time = _reelNumber;
        yield return new WaitForSeconds(time);
        InvokeRepeating(nameof(ForceStop), 0.1f, 0.001f);
    }

    private void ForceStop()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            if (Math.Abs(_slots[i].transform.position.y - (-1f)) < 0.01f) //means 2nd pos
            {
                if (_reelNumber == i) //check reel
                {
                    if (_slots[i].index == (customPos[i] % _slots.Count))
                    {
                        if (_spinCoroutine != null)
                        {
                            StopCoroutine(_spinCoroutine);
                            LerpSlots();
                        }
                        
                        CancelInvoke();
                    }
                }
            }
        }
    }

    #endregion
}
