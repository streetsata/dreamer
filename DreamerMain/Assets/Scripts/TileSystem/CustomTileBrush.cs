using UnityEngine;

namespace UnityEditor
{
    [CustomGridBrush(true, false, false, "Custom Tile Brush")]
    public class CustomTileBrush : GridBrush
    {
        public TileType tileType;
        public override void Paint(GridLayout grid, GameObject brushTarget, Vector3Int position)
        {
            GameObject parent = GameObject.FindGameObjectWithTag("TileSpot");
            if (parent != null)
            {
                try
                {
                    Map map = GameObject.FindGameObjectWithTag("Map").GetComponent<Map>();
                    Transform transform = GetObjectInCell(parent.transform, position);
                    GameObject instance = transform != null ? transform.gameObject : null;
                    if (instance == null)
                    {
                        instance = new GameObject("T");
                        instance.AddComponent<TileBlock>().SetTileBlock(map, tileType, position.x, position.y);
                        Undo.RegisterCreatedObjectUndo(instance, "Paint Prefabs");
                    }
                    else instance.GetComponent<TileBlock>().SetTileBlock(map, tileType, position.x, position.y);
                    instance.transform.SetParent(parent.transform);
                    base.Paint(grid, brushTarget, position);
                }
                catch { Debug.LogError("Map not found and block not created! Maybe she's not on scene, or she has wrong tag."); }
            }
            else Debug.LogError("TileSpot not found and block not created! Maybe she's not on scene, or she has wrong tag.");
        }

        public override void Erase(GridLayout grid, GameObject brushTarget, Vector3Int position)
        {
            try
            {
                Transform erased = GetObjectInCell(GameObject.FindGameObjectWithTag("TileSpot").transform, position);
                if (erased != null) Undo.DestroyObjectImmediate(erased.gameObject);
                base.Erase(grid, brushTarget, position);
            }
            catch { Debug.LogError("TileSpot not found and block not еrase! Maybe she's not on scene, or she has wrong tag."); }
        }

        private static Transform GetObjectInCell(Transform parent, Vector3Int position)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                TileBlock child = parent.GetChild(i).GetComponent<TileBlock>();
                if (child.x == position.x && child.y == position.y)
                    return child.gameObject.transform;
            }
            return null;
        }

        [MenuItem("Assets/Create/Custom Tile Brush")]
        public static void CreateBrush()
        {
            string path = EditorUtility.SaveFilePanelInProject("Save Custom Tile Brush", "New Custom Tile Brush", "Asset", "Save Custom Tile Brush", "Assets");
            if (path == "") return;
            AssetDatabase.CreateAsset(CreateInstance<CustomTileBrush>(), path);
        }
    }

    [CustomEditor(typeof(CustomTileBrush))]
    public class CustomTileBrushEditor : GridBrushEditor
    {
        public override void OnPaintSceneGUI(GridLayout grid, GameObject brushTarget, BoundsInt position, GridBrushBase.Tool tool, bool executing)
        {
            base.OnPaintSceneGUI(grid, brushTarget, position, tool, executing);

            CustomTileBrush lineBrush = (CustomTileBrush)brush;
            Handles.color = lineBrush.tileType >= TileType.OneWayStart ? Color.blue : Color.green;
            Vector3[] vector3s = TileFormPoints.GetFormPoint(lineBrush.tileType, new Vector3(position.center.x * Map.tileSize, position.center.y * Map.tileSize));
            if(vector3s != null)
                for (int i = 0; i < vector3s.Length; i++)
                    Handles.DrawLine(vector3s[i], vector3s[i == vector3s.Length - 1 ? 0 : i + 1]);
        }
    }
}