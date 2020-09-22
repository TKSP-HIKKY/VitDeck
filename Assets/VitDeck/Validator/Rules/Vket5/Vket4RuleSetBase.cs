using UnityEngine;
using VitDeck.Language;
using VketTools.Utilities;
using VRCSDK2;

namespace VitDeck.Validator
{

    /// <summary>
    /// Vket4の基本ルールセット。
    /// </summary>
    /// <remarks>
    /// 変数をabstractまたはvirtualプロパティで宣言し、継承先で上書きすることでワールドによる制限の違いを表現する。
    /// </remarks>
    public abstract class Vket4RuleSetBase : IRuleSet
    {
        public abstract string RuleSetName
        {
            get;
        }

        protected readonly long MegaByte = 1048576;

        public IValidationTargetFinder TargetFinder { get { return new Vket4TargetFinder(); } }

        public static RequestComponent requestComponent;

        private readonly IObjectDetector officialPrefabsDetector;

        public Vket4RuleSetBase() : base()
        {
            officialPrefabsDetector = new PrefabPartsDetector(
                Vket4OfficialAssetData.AudioSourcePrefabGUIDs,
                Vket4OfficialAssetData.AvatarPedestalPrefabGUIDs,
                Vket4OfficialAssetData.ChairPrefabGUIDs,
                Vket4OfficialAssetData.PickupObjectSyncPrefabGUIDs,
                Vket4OfficialAssetData.CanvasPrefabGUIDs,
                Vket4OfficialAssetData.PointLightProbeGUIDs);
        }

        public IRule[] GetRules()
        {
            // デフォルトで使っていたAttribute式は宣言時にconst以外のメンバーが利用できない。
            // 継承したプロパティを参照して挙動を変えることが出来ない為、直接リストを返す方式に変更した。
            var requested = requestComponent ?? new DummyRequestComponent();
            var pickupObjectSyncUsesLimit = requested.PickupObjectSync > -1 ? requested.PickupObjectSync : PickupObjectSyncUsesLimit;
            var vrcTriggerUsesLimit = requested.VrcTrigger > -1 ? requested.VrcTrigger : VRCTriggerCountLimit;
            var componentIgnorer = new Vket4ComponentIgnorer(requested);
            var propertyIgnorer = new Vket4PropertyIgnorer(requested);

            return new IRule[]
            {

                new UnityVersionRule(LocalizedMessage.Get("Vket4RuleSetBase.UnityVersionRule.Title", "2018.4.20f1"), "2018.4.20f1"),

                new A02_VRCSDKVersionRule(LocalizedMessage.Get("Vket4RuleSetBase.VRCSDKVersionRule.Title"),
                    new VRCSDKVersion("2020.01.14.10.39"),
                    "https://files.vrchat.cloud/sdk/VRCSDK2-2020.01.14.10.39.unitypackage"),

                new A04_ExistInSubmitFolderRule(LocalizedMessage.Get("Vket4RuleSetBase.ExistInSubmitFolderRule.Title"), Vket4OfficialAssetData.GUIDs),

                new AssetGuidBlacklistRule(LocalizedMessage.Get("Vket4RuleSetBase.OfficialAssetDontContainRule.Title"), Vket4OfficialAssetData.GUIDs),

                new AssetNamingRule(LocalizedMessage.Get("Vket4RuleSetBase.NameOfFileAndFolderRule.Title"), @"[a-zA-Z0-9 _\.\-\(\)]+"),

                new AssetPathLengthRule(LocalizedMessage.Get("Vket4RuleSetBase.FilePathLengthLimitRule.Title", 184), 184),

                new AssetExtentionBlacklistRule(LocalizedMessage.Get("Vket4RuleSetBase.MeshFileTypeBlacklistRule.Title"),
                                                new string[]{".ma", ".mb", "max", "c4d", ".blend"}
                ),

                new ContainMatOrTexInAssetRule(LocalizedMessage.Get("Vket4RuleSetBase.ContainMatOrTexInAssetRule.Title")),

                new FolderSizeRule(LocalizedMessage.Get("Vket4RuleSetBase.FolderSizeRule.Title"), FolderSizeLimit),

                new C02_ExhibitStructureRule(LocalizedMessage.Get("Vket4RuleSetBase.ExhibitStructureRule.Title")),

                new C02_StaticFlagRule(LocalizedMessage.Get("Vket4RuleSetBase.StaticFlagsRule.Title")),

                new BoothBoundsRule(LocalizedMessage.Get("Vket4RuleSetBase.BoothBoundsRule.Title"),
                    size: BoothSizeLimit,
                    margin: 0.01f),

                new D04_AssetTypeLimitRule(
                    LocalizedMessage.Get("Vket4RuleSetBase.MaterialLimitRule.Title", MaterialUsesLimit),
                    typeof(Material),
                    MaterialUsesLimit,
                    Vket4OfficialAssetData.MaterialGUIDs),

                new D08_LightmapSizeLimitRule(
                    LocalizedMessage.Get("Vket4RuleSetBase.LightMapsLimitRule.Title", LightmapCountLimit, 512),
                    lightmapCountLimit: LightmapCountLimit,
                    lightmapResolutionLimit: 512),

                new E05_GlobalIlluminationBakedRule(LocalizedMessage.Get("Vket4RuleSetBase.GlobalIlluminationBakedRule.Title")),

                new UsableComponentListRule(LocalizedMessage.Get("Vket4RuleSetBase.UsableComponentListRule.Title"),
                    GetComponentReferences(),
                    ignorePrefabGUIDs: Vket4OfficialAssetData.GUIDs,
                    customIgnore: componentIgnorer,
                    unregisteredComponent: ValidationLevel.NEGOTIABLE),

                new SkinnedMeshRendererRule(LocalizedMessage.Get("Vket4RuleSetBase.SkinnedMeshRendererRule.Title")),

                new MeshRendererRule(LocalizedMessage.Get("Vket4RuleSetBase.MeshRendererRule.Title")),

                new ReflectionProbeRule(LocalizedMessage.Get("Vket4RuleSetBase.ReflectionProbeRule.Title")),

                new VRCTriggerConfigRule(LocalizedMessage.Get("Vket4RuleSetBase.VRCTriggerConfigRule.Title"),
                            new VRC_EventHandler.VrcBroadcastType []{
                                VRC_EventHandler.VrcBroadcastType.Local },
                            new VRC_Trigger.TriggerType[] {
                                VRC_Trigger.TriggerType.Custom,
                                VRC_Trigger.TriggerType.OnInteract,
                                VRC_Trigger.TriggerType.OnEnterTrigger,
                                VRC_Trigger.TriggerType.OnExitTrigger,
                                VRC_Trigger.TriggerType.OnPickup,
                                VRC_Trigger.TriggerType.OnDrop,
                                VRC_Trigger.TriggerType.OnPickupUseDown,
                                VRC_Trigger.TriggerType.OnPickupUseUp   },
                            new VRC_EventHandler.VrcEventType[] {
                                VRC_EventHandler.VrcEventType.ActivateCustomTrigger,
                                VRC_EventHandler.VrcEventType.AudioTrigger,
                                VRC_EventHandler.VrcEventType.PlayAnimation,
                                VRC_EventHandler.VrcEventType.SetComponentActive,
                                VRC_EventHandler.VrcEventType.SetGameObjectActive,
                                VRC_EventHandler.VrcEventType.AnimationBool,
                                VRC_EventHandler.VrcEventType.AnimationFloat,
                                VRC_EventHandler.VrcEventType.AnimationInt,
                                VRC_EventHandler.VrcEventType.AnimationIntAdd,
                                VRC_EventHandler.VrcEventType.AnimationIntDivide,
                                VRC_EventHandler.VrcEventType.AnimationIntMultiply,
                                VRC_EventHandler.VrcEventType.AnimationIntSubtract,
                                VRC_EventHandler.VrcEventType.AnimationTrigger},
                            Vket4OfficialAssetData.GUIDs,
                            propertyIgnorer),

                new UseMeshColliderRule(LocalizedMessage.Get("Vket4RuleSetBase.UseMeshColliderRule.Title")),

                new VRCTriggerCountLimitRule(LocalizedMessage.Get("Vket4RuleSetBase.VRCTriggerCountLimitRule.Title", vrcTriggerUsesLimit), vrcTriggerUsesLimit),

                new LightCountLimitRule(LocalizedMessage.Get("Vket4RuleSetBase.DirectionalLightLimitRule.Title"), UnityEngine.LightType.Directional, 0),

                new LightConfigRule(LocalizedMessage.Get("Vket4RuleSetBase.PointLightConfigRule.Title"), UnityEngine.LightType.Point, ApprovedPointLightConfig),

                new LightConfigRule(LocalizedMessage.Get("Vket4RuleSetBase.SpotLightConfigRule.Title"), UnityEngine.LightType.Spot, ApprovedSpotLightConfig),

                new LightConfigRule(LocalizedMessage.Get("Vket4RuleSetBase.AreaLightConfigRule.Title"), UnityEngine.LightType.Area, ApprovedAreaLightConfig),

                new LightCountLimitRule(
                    LocalizedMessage.Get("Vket4RuleSetBase.AreaLightLimitRule.Title", AreaLightUsesLimit),
                    UnityEngine.LightType.Area,
                    AreaLightUsesLimit),

                new UseLightModeRule(LocalizedMessage.Get("Vket4RuleSetBase.PointLightModeRule.Title"), UnityEngine.LightType.Point, unusablePointLightModes),

                new UseLightModeRule(LocalizedMessage.Get("Vket4RuleSetBase.SpotLightModeRule.Title"), UnityEngine.LightType.Spot, unusableSpotLightModes),

                requested.DynamicCollider ? DummyRule.Get(LocalizedMessage.Get("Vket4RuleSetBase.AnimationMakesMoveCollidersRule.Title")) :
                new AnimationMakesMoveCollidersRule(LocalizedMessage.Get("Vket4RuleSetBase.AnimationMakesMoveCollidersRule.Title")),

                new F01_AnimationClipRule(LocalizedMessage.Get("Vket4RuleSetBase.F01_AnimationClipRule.Title")),

                new F01_AnimationComponentRule(LocalizedMessage.Get("Vket4RuleSetBase.F01_AnimationComponentRule.Title"), officialPrefabsDetector),

                new F01_AnimatorComponentRule(LocalizedMessage.Get("Vket4RuleSetBase.F01_AnimatorComponentRule.Title"),
                    new System.Type[]{
                        typeof(VRC_Pickup),
                        typeof(VRC_ObjectSync)
                    },officialPrefabsDetector),

                new F01_CanvasRenderModeRule(LocalizedMessage.Get("Vket4RuleSetBase.CanvasRenderModeRule.Title")),

                new F02_PickupObjectSyncPrefabRule(LocalizedMessage.Get("Vket4RuleSetBase.PickupObjectSyncRule.Title"), Vket4OfficialAssetData.PickupObjectSyncPrefabGUIDs),

                new F02_AvatarPedestalPrefabRule(LocalizedMessage.Get("Vket4RuleSetBase.AvatarPedestalPrefabRule.Title"), Vket4OfficialAssetData.AvatarPedestalPrefabGUIDs),

                new F02_AudioSourcePrefabRule(LocalizedMessage.Get("Vket4RuleSetBase.AudioSourcePrefabRule.Title"),  Vket4OfficialAssetData.AudioSourcePrefabGUIDs),

                new F02_RigidbodyRule(LocalizedMessage.Get("Vket4RuleSetBase.F02_RigidbodyRule.Title"), propertyIgnorer),

                new F02_PrefabLimitRule(
                    LocalizedMessage.Get("Vket4RuleSetBase.ChairPrefabLimitRule.Title", ChairPrefabUsesLimit),
                    Vket4OfficialAssetData.ChairPrefabGUIDs,
                    ChairPrefabUsesLimit),

                new F02_PrefabLimitRule(
                    LocalizedMessage.Get("Vket4RuleSetBase.UnusabePrefabRule.Title", ChairPrefabUsesLimit),
                    Vket4OfficialAssetData.VRCSDKPrefabGUIDs,
                    0),

                new F02_PrefabLimitRule(
                    LocalizedMessage.Get("Vket4RuleSetBase.PickupObjectSyncPrefabLimitRule.Title", pickupObjectSyncUsesLimit),
                    Vket4OfficialAssetData.PickupObjectSyncPrefabGUIDs,
                    pickupObjectSyncUsesLimit,
                    negotiable: true),

                new F02_VideoPlayerComponentRule(LocalizedMessage.Get("Vket4RuleSetBase.VideoPlayerComponentRule.Title")),

                new F01_AnimatorComponentMaxCountRule(LocalizedMessage.Get("Vket4RuleSetBase.AnimatorComponentMaxCountRule.Title"), limit: 50)
            };
        }

        protected abstract long FolderSizeLimit { get; }

        protected abstract Vector3 BoothSizeLimit { get; }

        protected abstract int VRCTriggerCountLimit { get; }

        protected abstract int MaterialUsesLimit { get; }

        protected abstract int LightmapCountLimit { get; }

        private ComponentReference[] GetComponentReferences()
        {
            return new ComponentReference[] {
                new ComponentReference("VRC_Trigger", new string[]{"VRCSDK2.VRC_Trigger", "VRCSDK2.VRC_EventHandler"}, AdvancedObjectValidationLevel),
                new ComponentReference("VRC_Object Sync", new string[]{"VRCSDK2.VRC_ObjectSync"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_Pickup", new string[]{"VRCSDK2.VRC_Pickup"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_Audio Bank", new string[]{"VRCSDK2.VRC_AudioBank"}, ValidationLevel.NEGOTIABLE),
                new ComponentReference("VRC_Avatar Pedestal", new string[]{"VRCSDK2.VRC_AvatarPedestal"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_Ui Shape", new string[]{"VRCSDK2.VRC_UiShape"}, AdvancedObjectValidationLevel),
                new ComponentReference("Rigidbody", new string[]{"UnityEngine.Rigidbody"}, ValidationLevel.DISALLOW),
                new ComponentReference("Cloth", new string[]{"UnityEngine.Cloth"}, ValidationLevel.NEGOTIABLE),
                new ComponentReference("Collider", new string[]{"UnityEngine.SphereCollider", "UnityEngine.BoxCollider", "UnityEngine.SphereCollider", "UnityEngine.CapsuleCollider", "UnityEngine.MeshCollider", "UnityEngine.WheelCollider"}, ValidationLevel.ALLOW),
                new ComponentReference("Dynamic Bone", new string[]{"DynamicBone"}, ValidationLevel.ALLOW),
                new ComponentReference("Dynamic Bone Collider", new string[]{"DynamicBoneCollider"}, ValidationLevel.ALLOW),
                new ComponentReference("Skinned Mesh Renderer", new string[]{"UnityEngine.SkinnedMeshRenderer"}, ValidationLevel.ALLOW),
                new ComponentReference("Mesh Renderer ", new string[]{"UnityEngine.MeshRenderer"}, ValidationLevel.ALLOW),
                new ComponentReference("Mesh Filter", new string[]{"UnityEngine.MeshFilter"}, ValidationLevel.ALLOW),
                new ComponentReference("Particle System", new string[]{"UnityEngine.ParticleSystem", "UnityEngine.ParticleSystemRenderer"}, ValidationLevel.ALLOW),
                new ComponentReference("Trail Renderer", new string[]{"UnityEngine.TrailRenderer"}, ValidationLevel.ALLOW),
                new ComponentReference("Line Renderer", new string[]{"UnityEngine.LineRenderer"}, ValidationLevel.ALLOW),
                new ComponentReference("Light", new string[]{"UnityEngine.Light"}, AdvancedObjectValidationLevel),
                new ComponentReference("LightProbeGroup", new string[]{"UnityEngine.LightProbeGroup"}, AdvancedObjectValidationLevel),
                new ComponentReference("ReflectionProbe", new string[]{"UnityEngine.ReflectionProbe"}, AdvancedObjectValidationLevel),
                new ComponentReference("Animator", new string[]{"UnityEngine.Animator"}, ValidationLevel.ALLOW),
                new ComponentReference("Animation", new string[]{"UnityEngine.Animation"}, ValidationLevel.ALLOW),
                new ComponentReference("Audio Source", new string[]{"UnityEngine.AudioSource", "ONSPAudioSource", "VRCSDK2.VRC_SpatialAudioSource"}, ValidationLevel.DISALLOW),
                new ComponentReference("Canvas", new string[]{"UnityEngine.Canvas", "UnityEngine.CanvasGroup", "UnityEngine.RectTransform", "UnityEngine.UI.CanvasScaler", "UnityEngine.UI.GraphicRaycaster", "UnityEngine.UI.AspectRatioFitter", "UnityEngine.UI.LayoutElement", "UnityEngine.UI.ContentSizeFitter", "UnityEngine.UI.HorizontalLayoutGroup", "UnityEngine.UI.VerticalLayoutGroup", "UnityEngine.UI.GridLayoutGroup", "UnityEngine.UI.Text", "UnityEngine.UI.Image", "UnityEngine.UI.RawImage", "UnityEngine.UI.Mask", "UnityEngine.UI.RectMask2D", "UnityEngine.UI.Button", "UnityEngine.UI.InputField", "UnityEngine.UI.Toggle", "UnityEngine.UI.ToggleGroup", "UnityEngine.UI.Slider", "UnityEngine.UI.Scrollbar", "UnityEngine.UI.Dropdown", "UnityEngine.UI.ScrollRect", "UnityEngine.UI.Selectable", "UnityEngine.UI.Shadow", "UnityEngine.UI.Outline", "UnityEngine.UI.PositionAsUV1", "UnityEngine.RectTransform", "UnityEngine.CanvasRenderer"}, ValidationLevel.ALLOW),
                new ComponentReference("VRC_Station", new string[]{"VRCSDK2.VRC_Station"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_Mirror", new string[]{ "VRCSDK2.VRC_MirrorCamera", "VRCSDK2.VRC_MirrorReflection" }, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_PlayerAudioOverride", new string[]{"VRCSDK2.VRC_PlayerAudioOverride"}, ValidationLevel.DISALLOW),
                new ComponentReference("EventSystem", new string[]{"UnityEngine.EventSystems.EventSystem", "UnityEngine.EventSystems.StandaloneInputModule"}, ValidationLevel.DISALLOW),
                new ComponentReference("StandaloneInputModule", new string[]{"UnityEngine.EventSystems.StandaloneInputModule"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_SceneResetPosition", new string[]{"VRCSDK2.VRC_SceneResetPosition"}, ValidationLevel.NEGOTIABLE),
                new ComponentReference("PlayableDirector", new string[]{"UnityEngine.Playables.PlayableDirector" }, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_Panorama", new string[]{"VRCSDK2.scripts.Scenes.VRC_Panorama" }, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_SyncVideoPlayer", new string[]{"VRCSDK2.VRC_SyncVideoPlayer", "VRCSDK2.VRC_SyncVideoStream" }, ValidationLevel.DISALLOW),
            };
        }

        protected virtual ValidationLevel AdvancedObjectValidationLevel
        {
            get
            {
                return ValidationLevel.ALLOW;
            }
        }

        protected abstract LightConfigRule.LightConfig ApprovedPointLightConfig { get; }

        protected abstract LightConfigRule.LightConfig ApprovedSpotLightConfig { get; }

        protected abstract LightConfigRule.LightConfig ApprovedAreaLightConfig { get; }

        protected abstract LightmapBakeType[] unusablePointLightModes { get; }

        protected abstract LightmapBakeType[] unusableSpotLightModes { get; }

        protected abstract int AreaLightUsesLimit { get; }

        protected abstract int ChairPrefabUsesLimit { get; }

        protected abstract int PickupObjectSyncUsesLimit { get; }

    }
}