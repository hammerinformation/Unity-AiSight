using System.Collections.Generic;
using UnityEngine;

internal class AiSight : MonoBehaviour {
    [SerializeField] protected List<AiDetectableTarget> detectableTargets;
    [SerializeField] protected List<AiDetectableTarget> seenTargets;

    [Header ("Values")]
    [SerializeField] protected Color debugColor = new Color (0, 255, 0, 255);
    [SerializeField] protected float maxDistance = 3000.0f;
    [SerializeField] protected float angle = 90.0f;
    [SerializeField] protected float sensingInterval = 0.1f;
    [SerializeField] protected bool loop = false;

    private static AiSight instance = null;
    internal static AiSight Instance => instance;

    private void Awake () {
        instance = this;
        detectableTargets = new List<AiDetectableTarget> (GameObject.FindObjectsOfType<AiDetectableTarget> ());
        Setup ();
        InvokeRepeating (nameof (Check), sensingInterval, sensingInterval);
    }

    private void OnDrawGizmos () {
        Gizmos.color = debugColor;
        foreach (var item in seenTargets) {

            Gizmos.DrawWireSphere (item.transform.position, 1f);
            Gizmos.DrawLine (transform.position, item.transform.position);
        }

        Gizmos.color = Color.red;
        Vector3 right = Quaternion.AngleAxis (angle, transform.up) * transform.forward;
        Gizmos.DrawRay (transform.position, right.normalized * maxDistance);
        Vector3 left = Quaternion.AngleAxis (-angle, transform.up) * transform.forward;
        Gizmos.DrawRay (transform.position, left.normalized * maxDistance);

    }

    protected virtual void Setup () {

        foreach (var item in detectableTargets) {
            if (!item.sight) { detectableTargets.Remove (item); }
        }
    }
    protected virtual void Check () {
        if (loop) {
            detectableTargets.Clear ();
            detectableTargets = new List<AiDetectableTarget> (GameObject.FindObjectsOfType<AiDetectableTarget> ());

        }

        foreach (var item in detectableTargets) {
            var dir = item.transform.position - this.transform.position;
            var distance = Vector3.Distance (this.transform.position, item.transform.position);
            var dotProduct = Vector3.Dot (this.transform.forward, dir.normalized);
            var t_angle = Vector3.Angle (this.transform.forward, dir.normalized);

            if (t_angle <= this.angle && dotProduct >= 0 && distance <= this.maxDistance) {

                RaycastHit raycastHit;
                if (Physics.Raycast (transform.position, dir * (maxDistance + 10), out raycastHit, maxDistance)) {
                    if (raycastHit.transform.gameObject.GetComponent<AiDetectableTarget> ()) {
                        if (!seenTargets.Contains (item)) {
                            seenTargets.Add (item);

                        }

                    } else {
                        seenTargets.Remove (item);
                    }
                }

            } else {
                if (seenTargets.Contains (item)) {
                    seenTargets.Remove (item);
                }
            }

        }

    }

    internal List<T> GetListByClass<T> () where T : MonoBehaviour {
        var list = new List<T> ();
        foreach (var item in seenTargets) {
            if (item.GetComponent<T> ()) {
                list.Add (item.GetComponent<T> ());
            }

        }
        return list;
    }

}