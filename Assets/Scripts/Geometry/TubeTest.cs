using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Geometry
{
    public class TubeTest : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
            Polyline curve = new Polyline();
            curve.AddPoint(new Vector3(0, 0, 0));
            curve.AddPoint(new Vector3(1, 0, 0));
            curve.AddPoint(new Vector3(2, 0, 0));
            //curve.AddPoint(new Vector3(3, 0, 0));
            //curve.AddPoint(new Vector3(4, 0, 0));
            int radialSegments = 4;
            //radialSegments = (int)Mathf.Max(radialSegments, 2 * Mathf.Ceil(0.5f * Mathf.Log(baseRadius * 100, 2)) * radialSegments);
            Debug.Log("radial segments = " + radialSegments);
            Mesh tubeMesh = Tube.Build(
                curve,
                1,
                radialSegments: radialSegments,
                baseColor: Color.white,
                variableWidth: true
            );

            GetComponent<MeshFilter>().sharedMesh = tubeMesh;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}