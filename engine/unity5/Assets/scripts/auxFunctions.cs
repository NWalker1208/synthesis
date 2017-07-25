using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using BulletSharp;
using BulletUnity;
using GopherAPI.STL;
using GopherAPI.Nodes;
using GopherAPI.Other;

public class AuxFunctions
{
    public delegate void HandleMesh(int id, Facet subMesh, Mesh mesh);

    public static void ReadMeshSet(Facet[] facets, HandleMesh handleMesh)
    {
        for (int j = 0; j < facets.Length; j++)
        {
            Facet sub = facets[j];
            //takes all of the required information from the API (the API information is within "sub" above)
            Vector3[] vertices = sub.Verteces == null ? null : AsVector3Array(sub.Verteces);

            Vector3[] normals = new Vector3[3];
            if(sub.Normal != null)
            {
                for(int i = 0; i < normals.Length; i++)
                {
                    normals[i] = AsVector3(sub.Normal);
                }
            }

            Mesh unityMesh = new Mesh();
            unityMesh.vertices = vertices;
            unityMesh.normals = normals;
            unityMesh.uv = new Vector2[vertices.Length];
            unityMesh.subMeshCount = facets.Length;
            for (int i = 0; i < facets.Length; i++)
            {
                int[] cpy = { i, i + 1, i + 2 };
                
                //Array.Reverse(cpy); <-- don't uncomment this.
                unityMesh.SetTriangles(cpy, i);
            }
            if (normals != null)
            {
                unityMesh.RecalculateNormals();
            }

            handleMesh(j, sub, unityMesh);
        }
    }

    public static void GetCombinedMesh(List<BXDAMesh.BXDASubMesh> meshes, HandleMesh handleMesh)
    {
        BXDAMesh.BXDASubMesh combinedMesh = new BXDAMesh.BXDASubMesh();
        combinedMesh.verts = new double[0];
        combinedMesh.norms = new double[0];
        combinedMesh.surfaces = new List<BXDAMesh.BXDASurface>();

        foreach (BXDAMesh.BXDASubMesh mesh in meshes)
        {
            double[] oldVertices = combinedMesh.verts;
            double[] newVertices = new double[oldVertices.Length + mesh.verts.Length];
            oldVertices.CopyTo(newVertices, 0);
            mesh.verts.CopyTo(newVertices, oldVertices.Length);

            combinedMesh.verts = newVertices;

            double[] oldNorms = combinedMesh.verts;
            double[] newNorms = new double[oldNorms.Length + mesh.norms.Length];
            oldNorms.CopyTo(newNorms, 0);
            mesh.norms.CopyTo(newNorms, oldNorms.Length);

            combinedMesh.norms = newNorms;

            combinedMesh.surfaces.AddRange(mesh.surfaces);
        }

        List<BXDAMesh.BXDASubMesh> combinedMeshes = new List<BXDAMesh.BXDASubMesh>();
        combinedMeshes.Add(combinedMesh);

        //ReadMeshSet(combinedMeshes, delegate (int id, BXDAMesh.BXDASubMesh subMesh, Mesh mesh)
        //{
        //    handleMesh(id, subMesh, mesh);
        //});
    }

    /// <summary>
    /// Generates a convex hull collision mesh from the given original mesh.
    /// </summary>
    /// <param name="original"></param>
    /// <returns></returns>
    public static Mesh GenerateCollisionMesh(Mesh original, Vector3 offset = default(Vector3))
    {
        ConvexHullShape tempShape = new ConvexHullShape(Array.ConvertAll(original.vertices, x => x.ToBullet()), original.vertices.Length);
        tempShape.Margin = 0f;

        ShapeHull shapeHull = new ShapeHull(tempShape);
        bool b = shapeHull.BuildHull(0f);

        Mesh collisionMesh = new Mesh();

        Vector3[] vertices = new Vector3[shapeHull.NumVertices];
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] = shapeHull.Vertices[i].ToUnity() - offset;

        int[] triangles = new int[shapeHull.NumIndices];
        for (int i = 0; i < triangles.Length; i++)
            triangles[i] = (int)shapeHull.Indices[i];

        collisionMesh.vertices = vertices;
        collisionMesh.triangles = triangles;
        collisionMesh.RecalculateNormals();
        collisionMesh.RecalculateBounds();

        // TODO: Find a way to implement embedded margins. See https://www.bulletphysics.org/Bullet/phpBB3/viewtopic.php?f=9&t=2358

        return collisionMesh;
    }

    public static void OrientRobot(List<GameObject> wheelcolliders, Transform parent)
    {
        Quaternion q = new Quaternion();
        List<Vector3> wheels = new List<Vector3>();

        foreach (GameObject collider in wheelcolliders)
            wheels.Add(collider.transform.position);

        if (wheels.Count > 2)
        {
            Vector3 a = wheels[0] - wheels[1];
            Vector3 b = a;

            for (int i = 2; Mathf.Abs(Vector3.Dot(a, b) / (a.magnitude * b.magnitude)) > .9f && i < wheels.Count; i++)
                b = wheels[0] - wheels[i];

            Vector3 norm = Vector3.Cross(a, b).normalized;
            Debug.DrawRay(wheels[0], norm);

            q.SetFromToRotation(norm, Vector3.up);
            parent.localRotation *= q;

            parent.position = new Vector3(parent.position.x, parent.position.y + .1f, parent.position.z);
        }
        //TODO THROW WHEEL EXCEPTION

    }
    public static Boolean rightRobot(List<GameObject> wheelcolliders, Transform parent)
    {
        Quaternion q = new Quaternion();
        List<Vector3> wheels = new List<Vector3>();

        foreach (GameObject collider in wheelcolliders)
            wheels.Add(collider.transform.position);

        Vector3 com = AuxFunctions.TotalCenterOfMass(parent.gameObject);
        Debug.Log(com.y < wheels[0].y);
        q.SetFromToRotation(parent.localToWorldMatrix * Vector3.up, parent.localToWorldMatrix * Vector3.down);
        if (com.y > wheels[0].y)
        {
            return false;
        }
        else
        {
            parent.localRotation *= q;
            return true;
        }
    }

    public static void IgnoreCollisionDetection(List<Collider> meshColliders)
    {
        for (int i = 0; i < meshColliders.Count; i++)
        {
            for (int j = i + 1; j < meshColliders.Count; j++)
            {
                try
                {
                    Physics.IgnoreCollision(meshColliders[i], meshColliders[j], true);
                }
                catch
                {
                }
            }
        }
    }

    /// <summary>
    /// Computes the total center of mass for all children of this game object.
    /// </summary>
    /// <param name="gameObj">The game object</param>
    /// <returns>The worldwide center of mass</returns>
    public static Vector3 TotalCenterOfMass(GameObject gameObj)
    {
        Vector3 centerOfMass = Vector3.zero;
        float sumOfAllWeights = 0f;

        Rigidbody[] rigidBodyArray = gameObj.GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rigidBase in rigidBodyArray)
        {
            centerOfMass += rigidBase.worldCenterOfMass * rigidBase.mass;
            sumOfAllWeights += rigidBase.mass;
        }
        centerOfMass /= sumOfAllWeights;
        return centerOfMass;
    }

    /// <summary>
    /// Mouses the in window.
    /// </summary>
    /// <returns><c>true</c>, if in window was moused, <c>false</c> otherwise.</returns>
    /// <param name="window">Window.</param>
    public static bool MouseInWindow(Rect window)
    {
        float mouseX = Input.mousePosition.x;
        float mouseY = Screen.height - Input.mousePosition.y; // Convert mouse coordinates to unity window positions coordinates
        return mouseX > window.x && mouseX < window.x + window.width && mouseY > window.y && mouseY < window.y + window.height;
    }

    public static GameObject FindObject(GameObject parent, string name)
    {
        Component[] trs = parent.GetComponentsInChildren(typeof(Transform), true);
        foreach (Transform t in trs)
        {
            if (t.name == name)
            {
                return t.gameObject;
            }
        }
        return new GameObject("COULDNOTFIND" + name);
    }

    public static GameObject FindObject(string name)
    {
        GameObject[] trs = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject t in trs)
        {
            if (t.name.Contains(name))
            {
                return t.gameObject;
            }
        }
        return new GameObject("COULDNOTFIND" + name);
    }

    public static Vector3 AsVector3(Vec3 v)
    {
        return new Vector3(v.X * 0.01f, v.Y * 0.01f, v.Z * 0.01f);
    }

    public static Vec3 AsVec3(Vector3 v)
    {
        return new Vec3(v.x / 0.01f, v.y / 0.01f, v.z / 0.01f);
    }

    public static Vector3[] AsVector3Array(Vec3[] vArray)
    {
        Vector3[] newArray = new Vector3[3];
        for(int i = 0; i < newArray.Length; i++)
        {
            newArray[i] = AsVector3(vArray[i]);
        }
        return newArray;
    }

    public static Material AsMaterial(Facet surf, bool emissive = false)
    {
        
        Color color = new Color32((byte)((surf.FacetColor.R >> 8) & 0xFF), (byte)((surf.FacetColor.G >> 8) & 0xFF), (byte)((surf.FacetColor.B >> 16) & 0xFF), (byte)((surf.FacetColor.A >> 24) & 0xFF));
        //if (surf.transparency != 0)
        //    color.a = surf.transparency;
        //else if (surf.translucency != 0)
        //    color.a = surf.translucency;
        if (color.a == 0)   // No perfectly transparent things plz.
            color.a = 1;
        Material result = new Material(Shader.Find(emissive ? "Standard" : (color.a != 1 ? "Transparent/" : "") +  "Specular"));
        result.SetColor("_Color", color);
        //if (surf.specular > 0)
        //{
        //    result.SetFloat("_Shininess", surf.specular);
        //    result.SetColor("_SpecColor", color);
        //}

        if (emissive)
        {
            result.EnableKeyword("_EMISSION");
            result.SetColor("_EmissionColor", Color.black);
        }

        return result;
    }
}