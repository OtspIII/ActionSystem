using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxController : MonoBehaviour
{
    //Basically just exists to make it easy to find out which Actor got hit when a Hitbox and Hurtbox collide.
    
    //Link to this hitbox's actor
    public ActorController Who;

    void Awake()
    {
        //If you didn't set this by hand, try to find the ActorController script yourself
        if (Who == null) Who = gameObject.GetComponentInParent<ActorController>();
    }
}
