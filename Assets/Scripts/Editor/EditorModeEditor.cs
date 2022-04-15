using UnityEngine;

namespace WorldMapEditor
{
    public partial class HexMapEditor
    {
        public void OnEditorModeChanged()
        {
            IsBrush = mode == EditorMode.Brush;
            IsFeature = mode == EditorMode.Feature;
            IsPathfinding = mode == EditorMode.Pathfinding;
            IsFogOfWar = mode == EditorMode.FogOfWar;
            IsSettings = mode == EditorMode.Settings;

            if (IsFogOfWar )
                Shader.DisableKeyword("HEX_MAP_VISION");
            else
                Shader.EnableKeyword("HEX_MAP_VISION");

            //DisableHighlight();
            //cellShaderData.SetFogOfWar(ShowFogOfWar);
        }
    }
}