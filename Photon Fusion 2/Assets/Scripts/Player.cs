using Fusion;
using UnityEngine;
using UnityEditor;

public class Player : NetworkBehaviour
{
    [SerializeField] float moveSpeed = 100f;
    [SerializeField] Ball ballPrefab;
    [Networked] TickTimer delay { get; set; }

    Vector3 forward;

    NetworkCharacterController networkCharacterController;

    private void Awake()
    {
        networkCharacterController = GetComponent<NetworkCharacterController>();
        forward = transform.forward;
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();
            networkCharacterController.Move(moveSpeed * data.direction * Runner.DeltaTime);

            if (data.direction.sqrMagnitude > 0)
                forward = data.direction;

            if (HasStateAuthority && delay.ExpiredOrNotRunning(Runner))
            {
                if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON0))
                {
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);

                    Runner.Spawn(
                        ballPrefab,
                        transform.position + forward,
                        Quaternion.LookRotation(forward),
                        Object.InputAuthority, (runner, o) =>
                        {
                            // Initialize the Ball before synchronizing it
                            o.GetComponent<Ball>().Init();
                        });
                }
            }
        }
    }
}
