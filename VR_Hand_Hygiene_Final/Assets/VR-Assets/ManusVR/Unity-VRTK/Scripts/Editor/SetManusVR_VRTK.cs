using UnityEditor;

[InitializeOnLoad]
public class SetManusVR_VRTK : Editor
{
    const string DefinitionName = "ManusVR_VRTK";

    static SetManusVR_VRTK()
    {
        string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(
            BuildTargetGroup.Standalone);

        if (!symbols.Contains(DefinitionName))
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                BuildTargetGroup.Standalone, symbols + ";" + DefinitionName);
        }
    }
}
