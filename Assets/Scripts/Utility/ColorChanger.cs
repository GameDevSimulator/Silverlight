using UnityEngine;

namespace Assets.Scripts.Utility
{
    [ExecuteInEditMode]
    public class ColorChanger : MonoBehaviour
    {
        public Color ObjectColor;

        private Color currentColor;
        private Material materialColored;

        void Update()
        {
            if (ObjectColor != currentColor)
            {
                //helps stop memory leaks
                if (materialColored != null)
                    UnityEditor.AssetDatabase.DeleteAsset(UnityEditor.AssetDatabase.GetAssetPath(materialColored));

                //create a new material
                materialColored = new Material(Shader.Find("Diffuse"));
                materialColored.color = currentColor = ObjectColor;
                this.GetComponent<Renderer>().material = materialColored;
            }
        }
    }
}