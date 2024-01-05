using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public abstract class BasePlayer : MonoBehaviour
{
    public Properties stats;
    public AnimatorController AnimatorController;
    public AudioClip DieSFX;

    public abstract void InflictDamage();

    public abstract void TakeDamage();

}
