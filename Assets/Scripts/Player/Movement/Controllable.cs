using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Controllable : MonoBehaviour
{
    protected PlayerInputReader inputReader;

    protected virtual void Awake()
    {
        inputReader = GetComponentInParent<PlayerInputReader>();
        if (inputReader == null)
            inputReader = FindObjectOfType<PlayerInputReader>();
    }

    public abstract void Move(Vector2 input);
    public abstract void Rotate(Vector2 input);
    public abstract float GetSpeed();
    public abstract void SetActive(bool active);
}