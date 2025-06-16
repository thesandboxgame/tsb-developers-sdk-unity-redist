using Sandbox.Developers.Importers;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(SandboxAssetImporter))]
public class NPCLogic: MonoBehaviour
{
    enum NPCState
    {
        Walking,
        Idle
    };

    private NPCState state;
    private Vector3 target;
    private Vector3 direction;
    private SandboxAssetImporter importer;

    private float speed = 20f;

    public void Start()
    {
        importer = GetComponent<SandboxAssetImporter>();
        importer.PlayAnimation("Idle 01", true);
        state = NPCState.Idle;
    }

    public void Update()
    {
        if (state == NPCState.Idle && Random.Range(0f, 1f) < 0.005f) // 0.5% of probability to start walking each frame
        {
            state = NPCState.Walking;
            importer.PlayAnimation("Walk 01", true);
            target = new Vector3(Random.Range(-200f, 200f), 0, Random.Range(-20f, 100f));
            direction = (target - transform.position).normalized;
            transform.LookAt(transform.position + direction);
        }
        else if (state == NPCState.Walking)
        {
            transform.Translate(direction * Time.deltaTime * speed, Space.World);
            if ((transform.position - target).sqrMagnitude < 0.05f)
            {
                state = NPCState.Idle;
                importer.PlayAnimation("Idle 01", true);
            }
        }
    }
}