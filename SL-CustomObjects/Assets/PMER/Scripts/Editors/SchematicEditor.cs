namespace PMER.Scripts.Editors
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(Schematic))]
    public class SchematicEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            Schematic schematic = (Schematic)target;

            GUILayout.Label($"<color=white>Number of blocks: <b>{schematic.GetComponentsInChildren<SchematicBlock>().Length - 1}</b></color>", SchematicManager.UnityRichTextStyle);

            if (GUILayout.Button("Apply Rotation to Empty Objects"))
            {
                int i = ApplyTransformProperty(schematic, true, false);
                Debug.Log(
                    i > 0
                        ? $"<color=#00FF00>Successfully applied rotation to <b>{i}</b> empties!</color>"
                        : "<color=#FFFF00>No empties have been found to which rotation could be applied to.</color>");

                return;
            }

            if (GUILayout.Button("Apply Scale to Empty Objects"))
            {
                int i = ApplyTransformProperty(schematic, false, true);
                Debug.Log(
                    i > 0
                        ? $"<color=#00FF00>Successfully applied scale to <b>{i}</b> empties!</color>"
                        : "<color=#FFFF00>No empties have been found to which scale could be applied to.</color>");

                return;
            }

            if (GUILayout.Button("Compile"))
                schematic.CompileSchematic();
        }

        private static int ApplyTransformProperty(Schematic schematic, bool rotation, bool scale)
        {
            int i = 0;
            foreach (Transform empty in schematic.GetComponentsInChildren<Transform>())
            {
                if (empty.GetComponents<Component>().Length > 1)
                    continue;

                if (empty.TryGetComponent(out SchematicBlock _))
                    continue;

                if (empty.transform.localScale == Vector3.one && empty.transform.localEulerAngles == Vector3.zero)
                    continue;

                i++;

                PrimitiveComponent[] primitives = empty.GetComponentsInChildren<PrimitiveComponent>();
                Dictionary<Transform, Transform> transformToParent = new();

                foreach (PrimitiveComponent primitiveComponent in primitives)
                {
                    Transform primitiveTransform = primitiveComponent.transform;
                    transformToParent.Add(primitiveTransform, primitiveTransform.parent);
                    primitiveComponent.transform.parent = null;
                }

                if (rotation)
                    empty.transform.localEulerAngles = Vector3.zero;

                if (scale)
                    empty.transform.localScale = Vector3.one;

                foreach (PrimitiveComponent primitiveComponent in primitives)
                {
                    Transform primitiveTransform = primitiveComponent.transform;
                    primitiveTransform.parent = transformToParent[primitiveTransform];
                }
            }

            return i;
        }
    }
}
