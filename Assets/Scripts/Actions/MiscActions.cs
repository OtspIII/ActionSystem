using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunAction : ActionScript
{   
    //A stun action. You just spin and can’t do anything else until it ends.
    //How long it lasts gets set in its constructor.

    public float StartRot;
    
    public StunAction(ActorController who, float dur=1)
    {
        //Make sure I know who the actor actually is
        Who = who;
        //Its duration is set by the 'dur' parameter--defaults to 1 second
        Duration = dur;
        //High priority--you can't interrupt a stun by doing an attack
        Priority = 3;
        //Record where the target was looking when they first got stunned
        StartRot = who.transform.rotation.eulerAngles.z;
    }

    //The coroutine script that controls this
    public override IEnumerator Script()
    {
        //We're going to do one full spin
        //So calculate how fast we need to spin 360 degrees in the duration of the stun
        float speed = 360 / Duration;
        //Start at zero rotation
        float rot = 0;
        //Keep doing this until we've done a complete 360. . .
        while (rot < 360)
        {
            //Spin in real time
            rot += speed * Time.deltaTime;
            Who.transform.rotation = Quaternion.Euler(0,0,StartRot+rot);
            //And wait a frame to continue
            yield return null;
        }
        //Once we've fully spun, end the action
        End();
    }

    public override void OnEnd()
    {
        base.OnEnd();
        //Go back to their original rotation when the stun ends
        Who.transform.rotation = Quaternion.Euler(0,0,StartRot);
    }
}