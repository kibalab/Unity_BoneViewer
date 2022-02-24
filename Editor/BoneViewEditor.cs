using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase.Editor;
using VRC.SDKBase.Editor.BuildPipeline;


[CustomEditor(typeof(Transform))]
public class BoneViewEditor : Editor
{
    public Texture2D Logo;

    private static float size = 0.01f;
    private static float human_size = 0.02f;
    private static Color color = new Color(0.2f, 1, 1);
    private static Color human_color = new Color(1, 0.2f, 1);
    private static bool onlyHuman = false;

    public static VRCAvatarDescriptor selectedAvatar;
    public static bool isLocked = false;

    public static bool isFolded = true;


    void OnSceneGUI()
    {
        // get the chosen game object
        //tar.TryGetComponent<VRCAvatarDescriptor>(out var avatar);
        //var avatar = target as VRCAvatarDescriptor;
        VRCAvatarDescriptor avatar = null;
        if (Selection.activeGameObject && !isLocked) Selection.activeGameObject.TryGetComponent<VRCAvatarDescriptor>(out avatar);


        if (avatar == null)
            avatar = selectedAvatar;
        else
            selectedAvatar = avatar;


        if (selectedAvatar == null)
            return;

        var anim = avatar.GetComponent<Animator>();

        if (anim == null)
            return;

        var hips = anim.GetBoneTransform(HumanBodyBones.Hips);
        

        

        Event e = Event.current;
        if (e.control || e.alt)
        {
            if (!onlyHuman)
            {
                DrawBone(hips);
            }
            for (var i = 0; i < 55; i++)
            {
                var bone = anim.GetBoneTransform((HumanBodyBones)i);  // 0~55
                if (bone != null)
                    DrawPoint(bone, human_color, human_size);
            }
        }

        Handles.BeginGUI();
        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.Width(130));
        {
            GUILayout.Space(25);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                GUI.backgroundColor = new Color(1.0f, 1.0f, 1.0f, 0f); //0.5 is half opacity 
                GUILayout.Box(Logo, GUILayout.Width(200), GUILayout.Height(45));

                var style = new GUIStyle();
                style.richText = true;
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(isLocked ? EditorGUIUtility.IconContent("LockIcon-On") : EditorGUIUtility.IconContent("LockIcon"), GUILayout.Height(18)))
                {
                    isLocked = !isLocked;
                }
                EditorGUILayout.LabelField("  <size=15><b><color= white>" + avatar.gameObject.name + "</color></b></size>", style);
                EditorGUILayout.EndHorizontal();
                GUI.backgroundColor = Color.white;
                GUILayout.Space(10);
                if (GUILayout.Button("Upload Avatar"))
                {
                    UploadAvatar();
                }
                if (GUILayout.Button("Build Test"))
                {
                    BuildTestAvatar();
                }

                isFolded = EditorGUILayout.Foldout(isFolded, "Settings");

                if (isFolded)
                {

                    GUILayout.Space(25);
                    EditorGUILayout.LabelField("<size=11><b><color= white>Size</color></b></size>", style);
                    size = EditorGUILayout.Slider(size, 0.001f, 0.1f);
                    GUILayout.Space(25);
                    EditorGUILayout.LabelField("<size=11><b><color= white>Human Bone Size</color></b></size>", style);
                    human_size = EditorGUILayout.Slider(human_size, 0.001f, 0.1f);
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("<size=11><b><color= white>Color</color></b></size>", style);
                    color = EditorGUILayout.ColorField(color); 
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("<size=11><b><color= white>Human Bone Color</color></b></size>", style);
                    human_color = EditorGUILayout.ColorField(human_color);
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("<size=11><b><color= white>Only HumanBone</color></b></size>", style);
                    onlyHuman = EditorGUILayout.Toggle(onlyHuman);

                    GUILayout.Space(15);
                    EditorGUILayout.LabelField("<size=11><color=Gray>Version : 1.0</color></size>", style);
                }
                if (GUILayout.Button("K13A_Labs BOOTH"))
                {
                    Application.OpenURL("https://k13b.booth.pm/");
                }
                if (GUILayout.Button("Close Editor", GUILayout.Height(25)))
                {
                    avatar = null;
                    selectedAvatar = null;
                    Selection.objects = null;
                    isLocked = false;
                    isFolded = false;
                }
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(25);

            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("Ctrl : Show Bone with Name");
                EditorGUILayout.LabelField("Alt : Show Bone");
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();


        Handles.EndGUI();
    }
    
    public void DrawBone(Transform bone)
    {
        DrawPoint(bone, color, size);
        for (var i = 0; i< bone.childCount; i++)
        {
            var b = bone.GetChild(i);
            DrawBone(b);
        }
    }

    public void DrawPoint(Transform p, Color color, float size)
    {
        Vector3 point = p.position; //1
        Handles.color = color;
        if(Handles.Button(point, Quaternion.identity, size, 0.02f, Handles.SphereHandleCap))
        {
            Selection.activeTransform = p;
        }

        var guiLoc = HandleUtility.WorldToGUIPoint(point);  // 오브젝트의 월드좌표를 2D 좌표로 변환
        var rect = new Rect(guiLoc.x - 50.0f, guiLoc.y - 50, 100, 25);    // 라벨 위치 지정

        Handles.DrawLine(p.position, p.parent.position);

        if (Event.current.control)
        {
            Handles.BeginGUI();
            var oldbgcolor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            GUI.Box(rect, p.gameObject.name);
            GUI.backgroundColor = oldbgcolor;
            Handles.EndGUI();
        }
    }

    public override void OnInspectorGUI()
    {
        CreateEditor(target, typeof(AvatarDescriptorEditor3)).OnInspectorGUI();
        //base.OnInspectorGUI();
    }

    protected override void OnHeaderGUI()
    {
        base.OnHeaderGUI();
    }

    public void BuildTestAvatar()
    {
        if (VRC.Core.APIUser.CurrentUser.canPublishAvatars)
        {
            VRC_SdkBuilder.ExportAndTestAvatarBlueprint(((VRCAvatarDescriptor)target).gameObject);

            EditorUtility.DisplayDialog("VRChat SDK", "Test Avatar Built", "OK");
        }
        else
        {
            VRCSdkControlPanel.ShowContentPublishPermissionsDialog();
        }
    }

    public void UploadAvatar()
    {
        bool buildBlocked = !VRCBuildPipelineCallbacks.OnVRCSDKBuildRequested(VRCSDKRequestedBuildType.Avatar);
        if (!buildBlocked)
        {
            if (VRC.Core.APIUser.CurrentUser.canPublishAvatars)
            {
                EnvConfig.FogSettings originalFogSettings = EnvConfig.GetFogSettings();
                EnvConfig.SetFogSettings(
                    new EnvConfig.FogSettings(EnvConfig.FogSettings.FogStrippingMode.Custom, true, true, true));

#if UNITY_ANDROID
                        EditorPrefs.SetBool("VRC.SDKBase_StripAllShaders", true);
#else
                EditorPrefs.SetBool("VRC.SDKBase_StripAllShaders", false);
#endif

                VRC_SdkBuilder.shouldBuildUnityPackage = VRCSdkControlPanel.FutureProofPublishEnabled;
                VRC_SdkBuilder.ExportAndUploadAvatarBlueprint(((VRCAvatarDescriptor)target).gameObject);

                EnvConfig.SetFogSettings(originalFogSettings);

                // this seems to workaround a Unity bug that is clearing the formatting of two levels of Layout
                // when we call the upload functions
                return;
            }
            else
            {
                VRCSdkControlPanel.ShowContentPublishPermissionsDialog();
            }
        }
    }
}
