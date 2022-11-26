using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialOverrider : MonoBehaviour
{
    // Start is called before the first frame update
    void OnEnable()
    {
        Renderer r = GetComponent<MeshRenderer>();
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        r.GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetColor("_BaseColor", Color.green);
        r.SetPropertyBlock(materialPropertyBlock);
    }

    private void OnDisable()
    {
        Renderer r = GetComponent<MeshRenderer>();
        r.SetPropertyBlock(null);
    }
}
