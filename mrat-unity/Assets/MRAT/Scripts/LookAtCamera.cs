using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour {

    public Transform target;
    public float minScale = 0.05f;

    private Vector3 initialScale;

    void Start() {
        if (target == null)
            target = Camera.main.transform;
        initialScale = transform.localScale;

        UpdateScale();
        transform.LookAt(target);
    }

    // Update is called once per frame
    void Update() {
        transform.LookAt(target);
        UpdateScale();
    }

    void UpdateScale() {
        float factor = Vector3.Distance(transform.position, target.position);
        factor = Mathf.Max(factor, minScale);
        transform.localScale = initialScale * factor;
    }
}
