using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BulletUnity;
using BulletSharp;
using GopherAPI.Nodes;
using GopherAPI.STL;

public partial class RigidNode : GopherRobotNode
{
    /// <summary>
    /// Need to add mass and other properties
    /// </summary>
    /// <returns></returns>
    public bool CreateMesh()
    {
        STLMesh mesh = Mesh;
        
        List<GameObject> meshObjects = new List<GameObject>();
        
        AuxFunctions.ReadMeshSet(mesh.Facets, delegate (int id, Facet sub, Mesh meshu)
        {
            GameObject meshObject = new GameObject(MainObject.name + "_mesh");
            meshObjects.Add(meshObject);

            meshObject.AddComponent<MeshFilter>().mesh = meshu;
            meshObject.AddComponent<MeshRenderer>();

            Material[] materials = new Material[meshu.subMeshCount];
            for (int i = 0; i < materials.Length; i++)
                materials[i] = AuxFunctions.AsMaterial(sub, true);

            meshObject.GetComponent<MeshRenderer>().materials = materials;


            meshObject.transform.position = root.position;
            meshObject.transform.rotation = root.rotation;
            
            ComOffset = meshObject.transform.GetComponent<MeshFilter>().mesh.bounds.center;
        });

        Mesh[] colliders = new Mesh[mesh.Facets.Length];

        AuxFunctions.ReadMeshSet(mesh.Facets, delegate (int id, Facet sub, Mesh meshu)
        {
            colliders[id] = meshu;
        });


        MainObject.transform.position = root.position + ComOffset;
        MainObject.transform.rotation = root.rotation;

        foreach (GameObject meshObject in meshObjects)
            meshObject.transform.parent = MainObject.transform;

        if (this.IsDriveWheel())
        {
            CreateWheel();
        }
        else
        {
            BMultiHullShape hullShape = MainObject.AddComponent<BMultiHullShape>();

            foreach (Mesh collider in colliders)
            {
                ConvexHullShape hull = new ConvexHullShape(Array.ConvertAll(collider.vertices, x => x.ToBullet()), collider.vertices.Length);
                hull.Margin = 0f;
                hullShape.AddHullShape(hull, BulletSharp.Math.Matrix.Translation(-ComOffset.ToBullet()));
            }
        }

        //physicalProperties = mesh.physics;

        BRigidBody rigidBody = MainObject.AddComponent<BRigidBody>();
        //rigidBody.mass = mesh.physics.mass;
        //rigidBody.friction = 1f;

        if (IsDriveWheel())
            UpdateWheelRigidBody();

        foreach (BRigidBody rb in MainObject.transform.parent.GetComponentsInChildren<BRigidBody>())
        {
            rigidBody.GetCollisionObject().SetIgnoreCollisionCheck(rb.GetCollisionObject(), true);
        }
        
        if (IsDriveWheel())
            UpdateWheelMass(); // 'tis a wheel, so needs more mass for joints to work correctly.

        //#region Free mesh
        //foreach (var list in new List<BXDAMesh.BXDASubMesh>[] { mesh.meshes, mesh.colliders })
        //{
        //    foreach (BXDAMesh.BXDASubMesh sub in list)
        //    {
        //        sub.verts = null;
        //        sub.norms = null;
        //        foreach (BXDAMesh.BXDASurface surf in sub.surfaces)
        //        {
        //            surf.indicies = null;
        //        }
        //    }
        //    for (int i = 0; i < list.Count; i++)
        //    {
        //        list[i] = null;
        //    }
        //}
        //mesh = null;
        //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
        //#endregion

        return true;
    }
}
