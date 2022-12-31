using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmosRig : MonoBehaviour
{
    [SerializeField] float sphereRadiusPerUnit = 0.11f;
    [SerializeField] float minSphereRadius = 0.01f;
    [SerializeField] float maxSphereRadius = 1f;
    [SerializeField] Color color = Color.white;

    private void OnDrawGizmos()
    {
        Gizmos.color = color;

        for(int i = 0; i < transform.childCount; i++)
        {
            DrawBone(transform.GetChild(i));
        }
    }

    private void DrawBone(Transform origin)
    {
        bool isRoot = !ConnectedToAny(origin) && origin.childCount > 0 && origin.parent == transform;
        if (isRoot)
        {
            Gizmos.matrix = Matrix4x4.TRS(origin.position, origin.rotation, Vector3.one);
            Vector3 destinyPos = Vector3.up * 0.3f;
            CalculateSizes(destinyPos, out float drawScale, out float boneWidth, out float boneLength);
            Gizmos.DrawSphere(Vector3.zero, drawScale);
            Gizmos.DrawSphere(destinyPos, drawScale);
            Vector3 centerPos = destinyPos * 0.5f;
            Gizmos.DrawCube(centerPos, new Vector3(boneWidth, boneLength, boneWidth));
        }
        for(int i = 0; i < origin.childCount; i++)
        {
            Transform destiny = origin.GetChild(i);
            if(!isRoot)
                DrawEdge(origin, destiny, i == 0);
            DrawBone(destiny);
        }
    }

    private void DrawEdge(Transform origin, Transform destiny, bool isFirst)
    {
        Gizmos.matrix = Matrix4x4.TRS(origin.position, origin.rotation, Vector3.one);

        Vector3 destinyPos = destiny.localPosition;
        bool isConnected = Vector3.Angle(Vector3.up, destinyPos) < 1f;
        destinyPos.Scale(origin.lossyScale);

        CalculateSizes(destinyPos, out float drawScale, out float boneWidth, out float boneLength);

        Vector3 centerPos = destinyPos * 0.5f;

        if (isConnected || isFirst)
        {
            Gizmos.DrawSphere(Vector3.zero, drawScale);
        }

        if (isConnected)
        {
            Gizmos.DrawCube(centerPos, new Vector3(boneWidth, boneLength, boneWidth));
        }
        else
        {
            Gizmos.DrawLine(Vector3.zero, destinyPos);
        }
        if (destiny.childCount == 0)
        {
            Gizmos.DrawSphere(destinyPos, drawScale);
        }
    }

    private void CalculateSizes(Vector3 destinyPos, out float drawScale, out float boneWidth, out float boneLength)
    {
        boneLength = destinyPos.magnitude;
        drawScale = Mathf.Clamp(sphereRadiusPerUnit * boneLength, minSphereRadius, maxSphereRadius);
        boneWidth = drawScale * 0.8f;
    }

    private bool ConnectedToAny(Transform origin)
    {
        for (int i = 0; i < origin.childCount; i++)
        {
            Transform destiny = origin.GetChild(i);
            bool isConnected = Vector3.Angle(origin.up, destiny.position-origin.position) < 1f;
            if (isConnected) return true;
        }
        return false;
    }
}
