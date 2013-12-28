﻿/*
The MIT License (MIT)

Copyright (c) 2013 Banbury

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;

[ExecuteInEditMode]
public class Bone : MonoBehaviour {
    public float length = 1.0f;
    public bool snapToParent = true;
    public bool editMode = true;

    private Bone parent;

    public Vector2 Head {
        get {
            return gameObject.transform.position + gameObject.transform.up * length;
        }
    }

    [MenuItem("GameObject/Create Other/Bone")]
    public static void Create() {
        GameObject b = new GameObject("Bone");
        Undo.RegisterCreatedObjectUndo(b, "Add child bone");
        b.AddComponent<Bone>();

        if (Selection.activeGameObject != null) {
            GameObject sel = Selection.activeGameObject;
            b.transform.parent = sel.transform;

            if (sel.GetComponent<Bone>() != null) {
                Bone p = sel.GetComponent<Bone>();
                b.transform.position = p.Head;
            }
        }

        Selection.activeGameObject = b;
    }

    public static void Split() {
        if (Selection.activeGameObject != null) {
            Undo.IncrementCurrentGroup();
            string undo = "Split bone";

            GameObject old = Selection.activeGameObject;
            Undo.RegisterFullObjectHierarchyUndo(old);
            Bone b = old.GetComponent<Bone>();

            GameObject n1 = new GameObject(old.name + "1");
            Undo.RegisterCreatedObjectUndo(n1, undo);
            Bone b1 = n1.AddComponent<Bone>();
            b1.transform.parent = b.parent.transform;
            b1.snapToParent = b.snapToParent;
            b1.length = b.length / 2;
            b1.transform.localPosition = b.transform.localPosition;
            b1.transform.localRotation = b.transform.localRotation;

            GameObject n2 = new GameObject(old.name + "2");
            Undo.RegisterCreatedObjectUndo(n2, undo);
            Bone b2 = n2.AddComponent<Bone>();
            b2.length = b.length / 2;
            n2.transform.parent = n1.transform;
            b2.transform.localRotation = b.transform.localRotation;
            n2.transform.position = b1.Head;

            var children = (from Transform child in b.transform select child).ToArray<Transform>();
            b.transform.DetachChildren();
            foreach (Transform child in children) {
                Undo.SetTransformParent(child, n2.transform, undo);
                child.parent = n2.transform;
            }

            Undo.DestroyObjectImmediate(old);

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        }
    }

    public void AddIK() {
        Undo.AddComponent<InverseKinematics>(gameObject);
    }

    // Use this for initialization
	void Start () {
        if (gameObject.transform.parent != null)
            parent = gameObject.transform.parent.GetComponent<Bone>();
	}
	
	// Update is called once per frame
	void Update () {
        //transform.position = new Vector2(transform.position.x, transform.position.y);
        transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z);

        if (Application.isEditor && editMode && snapToParent && parent != null) {
            gameObject.transform.position = parent.Head;
        }

        if (!editMode) {

        }
	}

    void OnDrawGizmos() {
        if (gameObject.Equals(Selection.activeGameObject)) {
            Gizmos.color = Color.yellow;
        }
        else {
            if (editMode) {
                Gizmos.color = Color.gray;
            }
            else {
                Gizmos.color = Color.blue;
            }
        }

        int div = 5; 

        Vector3 v = Quaternion.AngleAxis(45, Vector3.forward) * (((Vector3)Head - gameObject.transform.position) / div);
        Gizmos.DrawLine(gameObject.transform.position, gameObject.transform.position + v);
        Gizmos.DrawLine(gameObject.transform.position + v, Head);

        v = Quaternion.AngleAxis(-45, Vector3.forward) * (((Vector3)Head - gameObject.transform.position) / div);
        Gizmos.DrawLine(gameObject.transform.position, gameObject.transform.position + v);
        Gizmos.DrawLine(gameObject.transform.position + v, Head);

        Gizmos.DrawLine(gameObject.transform.position, Head);
    }
}
