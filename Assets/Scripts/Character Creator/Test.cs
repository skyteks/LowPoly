using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField]
    private GameObject characterRef;

    [SerializeField]
    private CharacterAttachment[] characterAttachmentPrefabs;

    private Dictionary<CharacterAttachmentPoint.AttachmentTypes, CharacterAttachmentPoint> pointDict;

    private void Start()
    {
        AddAttachments();
    }

    private void AddAttachments()
    {
        SortPointsIntoDict();

        foreach (CharacterAttachment attachmentPrefab in characterAttachmentPrefabs)
        {
            switch (attachmentPrefab.Type)
            {
                //case CharacterAttachmentPoint.AttachmentTypes.:
                default:
                    {
                        if (pointDict.TryGetValue(attachmentPrefab.Type, out CharacterAttachmentPoint point))
                        {
                            GameObject attachmentInstanceGO = Instantiate(attachmentPrefab.gameObject, point.transform);
                            CharacterAttachment attachmentInstance = attachmentInstanceGO.GetComponent<CharacterAttachment>();
                            ReplaceRenderer(attachmentInstance, point.Root);
                        }
                    }
                    break;
            }
        }
    }

    private static SkinnedMeshRenderer ReplaceRenderer(CharacterAttachment attachment, Transform rootBone)
    {
        MeshRenderer meshRender = attachment.GetComponent<MeshRenderer>();
        MeshFilter meshFilter = attachment.GetComponent<MeshFilter>();

        Material material = meshRender.sharedMaterial;
        Mesh mesh = meshFilter.mesh;

        Destroy(meshRender);
        Destroy(meshFilter);

        SkinnedMeshRenderer skinnedRender = attachment.gameObject.AddComponent<SkinnedMeshRenderer>();
        skinnedRender.sharedMaterial = material;
        skinnedRender.sharedMesh = mesh;
        skinnedRender.rootBone = rootBone;
        skinnedRender.localBounds = new Bounds(Vector3.zero, Vector3.one * 100f);
        return skinnedRender;
    }

    private void SortPointsIntoDict()
    {
        CharacterAttachmentPoint[] points = characterRef.GetComponentsInChildren<CharacterAttachmentPoint>();
        pointDict = new Dictionary<CharacterAttachmentPoint.AttachmentTypes, CharacterAttachmentPoint>();
        foreach (CharacterAttachmentPoint point in points)
        {
            pointDict.Add(point.Type, point);
        }
    }
}
