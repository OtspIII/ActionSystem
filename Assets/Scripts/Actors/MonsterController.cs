using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : ActorController
{
    //Who I want to attack
    public ActorController Target;
    //How close I want to get before I attack
    public float AttackRange = 1.5f;
    //How close the target needs to be for me to aggro on them
    public float VisionRange = 4;

    public override void OnAwake()
    {
        base.OnAwake();
        //Monsters move at a speed of 3
        Speed = 3;
    }
    
    public override void OnStart()
    {
        base.OnStart();
        //If I don't have a target set that I want to attack, find the player and make it them
        if (Target == null) Target = GameObject.FindObjectOfType<PlayerController>();
    }

    //Override of DefaultAction to make it so that monster default actions are patroling
    public override ActionScript DefaultAction()
    {
        return new PatrolAction(this);
    }
}
