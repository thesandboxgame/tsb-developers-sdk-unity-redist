
using UnityEngine;

public class CameraManager: MonoBehaviour
{
    private GameObject target;
    private bool follow;
    
    public void Start()
    {
        follow = false;
    }

    /// <summary>
    /// Set target to follow
    /// </summary>
    /// <param name="target">gameobject to follow</param>
    public void SetTarget(GameObject target)
    {
        this.target = target;
        follow = true;
    }

    public void ReleaseTarget()
    {
        follow = false;
    }

    public void Update()
    {
        if (follow)
        {
            transform.position = target.transform.position + new Vector3(0, 20f, -100f);
        }
    }
}