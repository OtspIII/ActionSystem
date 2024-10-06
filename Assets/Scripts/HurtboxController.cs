using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtboxController : MonoBehaviour
{
    //Manager for Hurtboxes.
    //Notices when triggers collide and calls HitBegins on the CurrentAction of the Hurtbox's owner,
    //  telling them who they hit.
    
    //Who owns this hurtbox
    public ActorController Who;
    
    void Awake()
    {
        //If you didn't set Who, set it automatically
        if (Who == null) Who = gameObject.GetComponentInParent<ActorController>();
    }

    //When I hit a hitbox. . .
    private void OnTriggerEnter2D(Collider2D other)
    {
        //If I hit myself, don't do anything
        if (other.gameObject == Who.gameObject) return;
        //Find out if what I hit has a Hitbox
        HitboxController hit = other.GetComponent<HitboxController>();
        //If not, don't do anything
        if (hit == null) return;
        //Make sure this hurtbox exists as part of an action--like an attack or something
        if (Who.CurrentAction == null)
        {
            //If not, throw an error and end early
            Debug.Log("ERROR: Hurtbox existed while actor wasn't acting");
            return;
        }
        //Tell the action that it hit something
        Who.CurrentAction.HitBegin(hit.Who,this);
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        //If I hit myself, don't do anything
        if (other.gameObject == Who.gameObject) return;
        //Find out if what I stopped hitting has a Hitbox
        HitboxController hit = other.GetComponent<HitboxController>();
        //If not, don't do anything
        if (hit == null) return;
        //Make sure this hurtbox exists as part of an action--like an attack or something
        if (Who.CurrentAction == null)
        {
            //If not, throw an error and end early
            Debug.Log("ERROR: Hurtbox existed while actor wasn't acting");
            return;
        }
        //Tell the action that it stopped hitting something
        Who.CurrentAction.HitEnd(hit.Who,this);
    }
    
}
