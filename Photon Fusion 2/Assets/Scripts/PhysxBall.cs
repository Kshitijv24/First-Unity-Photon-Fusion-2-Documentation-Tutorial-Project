using UnityEngine;
using Fusion;

public class PhysxBall : NetworkBehaviour
{
    [Networked] private TickTimer life { get; set; }

    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float timeBeforeDestroyingBall = 5f;

    public void Init(Vector3 forward)
    {
        life = TickTimer.CreateFromSeconds(Runner, timeBeforeDestroyingBall);
        GetComponent<Rigidbody>().velocity = forward * moveSpeed;
    }

    public override void FixedUpdateNetwork()
    {
        if (life.Expired(Runner))
            Runner.Despawn(Object);
    }
}