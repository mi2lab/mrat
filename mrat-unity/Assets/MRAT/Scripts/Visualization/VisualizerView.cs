using System.Collections.Generic;
using UnityEngine;

namespace MRAT
{

    public abstract class VisualizerView : MonoBehaviour
    {
        public abstract void VisualizeEvents(List<MratEventSimple> events);

        public abstract void ClearGraphics();
    }
}

