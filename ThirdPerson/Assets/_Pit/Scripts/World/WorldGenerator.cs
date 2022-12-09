using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [SerializeField] GameObject tilePrototype;
    [SerializeField] int sizeXZ = 20;

    public void Generate()
    {
        Clean();
        for(int x = 0; x < sizeXZ; x++)
        {
            for(int z = 0; z < sizeXZ; z++)
            {
                Vector3 tilePosition = LocalToTileSpace(new Vector3(x, 0f, z));
                CreateTile(tilePosition, tilePrototype);
            }
        }
    }

    private void Clean()
    {
        int count = transform.childCount;
        for (int i = 0; i < count; i++)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    private void CreateTile(Vector3 tilePosition, GameObject prototype)
    {
        GameObject tile = GameObject.Instantiate(prototype, transform);
        tile.transform.localPosition = tilePosition;
    }

    private Vector3 LocalToTileSpace(Vector3 localPosition)
    {
        return localPosition - new Vector3(sizeXZ * 0.5f-0.5f, 0f, sizeXZ * 0.5f - 0.5f);
    }

    private Vector3 TileToLocalSpace(Vector3 tilePosition)
    {
        return tilePosition + new Vector3(sizeXZ * 0.5f+0.5f, 0f, sizeXZ * 0.5f+0.5f);
    }
}
