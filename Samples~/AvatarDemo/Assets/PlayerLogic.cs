using Sandbox.Developers.Importers;
using UnityEngine;


[RequireComponent(typeof(SandboxAsset))]
public class PlayerLogic: MonoBehaviour
{
    private enum PlayerState {
        Idle,
        Walking,
        Running,
        Dancing
    }

    private PlayerState state;
    private Vector3 target;
    private Vector3 direction;
    private SandboxAsset asset;

    public void Start()
    {
        asset = GetComponent<SandboxAsset>();
        asset.PlayAnimation("Idle 01", true);
        state = PlayerState.Idle;
    }

    private const float WALK_SPEED = 30f;
    private const float RUN_SPEED = 100f;
        
    public void Update()
    {
        Vector3 direction = Vector3.zero;

        bool shouldRun = false;
        bool shouldDance = false;
        
        // Handle input
        if (Input.GetKey(KeyCode.UpArrow)) direction += Vector3.forward;
        if (Input.GetKey(KeyCode.DownArrow)) direction += Vector3.back;
        if (Input.GetKey(KeyCode.LeftArrow)) direction += Vector3.left;
        if (Input.GetKey(KeyCode.RightArrow)) direction += Vector3.right;
        if (Input.GetKey(KeyCode.LeftShift)) shouldRun = true;
        if (Input.GetKey(KeyCode.Space))
        {
            shouldDance = true;
            direction = Vector3.zero;
        }

        // Handle states
        if (direction != Vector3.zero)
        {
            transform.position += (shouldRun ? RUN_SPEED : WALK_SPEED) * Time.deltaTime * direction.normalized;
            transform.LookAt(transform.position + direction);
            
            if (shouldRun && state != PlayerState.Running)
            {
                asset.PlayAnimation("Run 01", true);
                state = PlayerState.Running;
            }
            else if (!shouldRun && state != PlayerState.Walking)
            {
                asset.PlayAnimation("Walk 01", true);
                state = PlayerState.Walking;
            }
        }
        else
        {
            if (shouldDance && state != PlayerState.Dancing)
            {
                state = PlayerState.Dancing;
                asset.PlayAnimation("Dance SnoopDogg 01", true);
            }

            if (state != PlayerState.Dancing)
            {
                if (state != PlayerState.Idle)
                    asset.PlayAnimation("Idle 01", true);
                state = PlayerState.Idle;
            }
        }
    }
}