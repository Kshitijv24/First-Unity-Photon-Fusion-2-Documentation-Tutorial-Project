using Fusion;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float timeBeforeDestroyingBall = 5f;

    [Networked] private TickTimer life { get; set; }

    public override void FixedUpdateNetwork()
    {
        if (life.Expired(Runner))
            Runner.Despawn(Object);
        else
            transform.position += moveSpeed * transform.forward * Runner.DeltaTime;
    }

    public void Init() => life = TickTimer.CreateFromSeconds(Runner, timeBeforeDestroyingBall);
}
