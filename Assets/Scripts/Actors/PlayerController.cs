using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : ActorController
{
    //This is nice to have to make it easier to look up where the mouse is
    private Camera Cam;

    public override void OnAwake()
    {
        base.OnAwake();
        //Players walk at a speed of 5
        Speed = 5;
        //Record who the camera is in a local variable
        Cam = Camera.main;
    }


    public override void OnUpdate()
    {
        base.OnUpdate();
        //This is pretty basic movement code
        Vector2 vel = Vector2.zero;
        if (Input.GetKey(KeyCode.D)) vel.x = 1;
        if (Input.GetKey(KeyCode.A)) vel.x = -1;
        if (Input.GetKey(KeyCode.W)) vel.y = 1;
        if (Input.GetKey(KeyCode.S)) vel.y = -1;
        //We plug it into the DesiredMove variable, which our action later uses to know where to move
        DesiredMove = vel;

        //If my current action lets me turn. . .
        if (CurrentAction.RotateMult > 0)
        {
            //Turn towards the mouse
            LookAt(Cam.ScreenToWorldPoint(Input.mousePosition),0.1f / CurrentAction.RotateMult);
        }
        
        //If I click, swing a sword
        if(Input.GetKeyDown(KeyCode.Mouse0))
            DoAction(new SwingAction(this));
        
        
    }
}
