using Fusion;
using UnityEngine;
using UnityEditor;

public class Player : NetworkBehaviour
{
    public Material _material;

    [SerializeField] float moveSpeed = 100f;
    [SerializeField] Ball ballPrefab;
    [SerializeField] private PhysxBall _prefabPhysxBall;

    [Networked] public bool spawnedProjectile { get; set; }
    [Networked] TickTimer delay { get; set; }

    Vector3 forward;
    NetworkCharacterController networkCharacterController;
    ChangeDetector _changeDetector;

    private void Awake()
    {
        networkCharacterController = GetComponent<NetworkCharacterController>();
        _material = GetComponentInChildren<MeshRenderer>().material;
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
                    spawnedProjectile = !spawnedProjectile;
                }
                else if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON1))
                {
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    Runner.Spawn(_prefabPhysxBall,
                      transform.position + forward,
                      Quaternion.LookRotation(forward),
                      Object.InputAuthority,
                      (runner, o) =>
                      {
                          o.GetComponent<PhysxBall>().Init(10 * forward);
                      });
                    spawnedProjectile = !spawnedProjectile;
                }
            }
        }
    }

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    public override void Render()
    {
        foreach (string change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(spawnedProjectile):
                    _material.color = Color.white;
                    break;
            }
        }

        _material.color = Color.Lerp(_material.color, Color.blue, Time.deltaTime);
    }
}
