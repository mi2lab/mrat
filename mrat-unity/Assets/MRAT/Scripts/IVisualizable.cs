using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IVisualizable {
    Vector3 GetVisualizationPosition();
    Quaternion GetVisualizationRotation();
}
