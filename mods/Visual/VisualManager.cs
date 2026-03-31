using BepInEx;
using GorillaGameModes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ZenMenu.mods.Safety;
using ZenMenu_.mods.Advantage;
using static GorillaPaintbrawlManager;

namespace ZenMenu_.mods.Visual
{
    [BepInEx.BepInPlugin("zen.mods.visual", "visualmanager","0.0.0")]
    internal class VisualManager : BaseUnityPlugin
    {
        void Awake()
        {
            if (GameObject.Find("VisualManager(@Liex)") == null)
            {
                GameObject obj = new GameObject("VisualManager(@Liex)");
                obj.AddComponent<VisualManager>();
                obj.AddComponent<DataCollection>();
                obj.hideFlags = HideFlags.HideAndDontSave;
            }
        }
        public enum Mods
        {
            Chams,
            Tracers,
            NameEsp,
            DistanceEsp,
            BoxEsp,
            TrailEsp,
        }
        public static void ENableMod(Mods mod)
        {
            switch (mod)
            {
                case Mods.Chams:
                    Chams = !Chams;
                    break;
                case Mods.Tracers:
                    Tracers = !Tracers;
                    break;
                case Mods.NameEsp:
                    NameEsp = !NameEsp;
                    break;
                case Mods.DistanceEsp:
                    DistanceEsp = !DistanceEsp;
                    break;
                case Mods.BoxEsp:
                    BoxEsp = !BoxEsp;
                    break;
                case Mods.TrailEsp:
                    TrailEsp = !TrailEsp;
                    break;

            }
        }
        static bool Chams,
            Tracers,
            NameEsp,
            DistanceEsp,
            BoxEsp,
            TrailEsp;
        void Update()
        {
            if (Chams)
            {
                for (int i = 0; i < ZenMenu.patches.VrrigCache.Data.vrrigs.Count; i++)
                {
                    VRRig rig = ZenMenu.patches.VrrigCache.Data.vrrigs[i];
                    if (!Data.originalRenderers.ContainsKey(rig))
                    {
                        Data.originalRenderers[rig] = rig.mainSkin;
                    }
                    if (!Data.clonedRenderers.ContainsKey(rig) || Data.clonedRenderers[rig] == null)
                    {
                        SkinnedMeshRenderer clone = rig.mainSkin.gameObject.AddComponent<SkinnedMeshRenderer>();
                        clone.sharedMesh = rig.mainSkin.sharedMesh;
                        clone.bones = rig.mainSkin.bones;
                        clone.rootBone = rig.mainSkin.rootBone;
                        clone.localBounds = rig.mainSkin.localBounds;
                        clone.quality = rig.mainSkin.quality;
                        clone.updateWhenOffscreen = rig.mainSkin.updateWhenOffscreen;
                        clone.material = Mat(rig);
                        Data.originalRenderers[rig].enabled = false;
                        clone.enabled = true;

                        Data.clonedRenderers[rig] = clone;
                    }
                    else
                    {
                        Data.clonedRenderers[rig].material = Mat(rig);
                    }
                }
            }
            else if (!Data.RestoredMaterials)
            {
                for (int i = 0; i < ZenMenu.patches.VrrigCache.Data.vrrigs.Count; i++)
                {
                    VRRig rig = ZenMenu.patches.VrrigCache.Data.vrrigs[i];
                    if (Data.clonedRenderers.ContainsKey(rig) && Data.clonedRenderers[rig] != null)
                    {
                        if (Data.originalRenderers.ContainsKey(rig) && Data.originalRenderers[rig] != null)
                        {
                            Data.originalRenderers[rig].enabled = true;
                        }
                        Destroy(Data.clonedRenderers[rig]);
                        Data.clonedRenderers.Remove(rig);
                        Data.originalRenderers.Remove(rig);
                    }
                }

                Data.RestoredMaterials = true;
            }
            if (Tracers)
            {
                var toRemove = Data.tracerLines.Keys.Where(r => !ZenMenu.patches.VrrigCache.Data.vrrigs.Contains(r)).ToList();
                foreach (var r in toRemove)
                {
                    if (Data.tracerLines[r] != null)
                        Destroy(Data.tracerLines[r].gameObject);
                    Data.tracerLines.Remove(r);
                }
                for (int i = 0; i < ZenMenu.patches.VrrigCache.Data.vrrigs.Count; i++)
                {
                    VRRig rig = ZenMenu.patches.VrrigCache.Data.vrrigs[i];
                    if (!Data.tracerLines.ContainsKey(rig) || Data.tracerLines[rig] == null)
                    {
                        var _ = new GameObject($"tracer_{rig.Creator.NickName}");
                        var __ = _.AddComponent<LineRenderer>();
                        __.positionCount = 2; __.startWidth = 0.05f; __.endWidth = 0.05f; __.material = Mat(rig);
                        Data.tracerLines[rig] = __;
                    }

                    Data.tracerLines[rig].SetPosition(0, GorillaTagger.Instance.bodyCollider.transform.position);
                    Data.tracerLines[rig].SetPosition(1, rig.transform.position);
                    Data.tracerLines[rig].material = Mat(rig);
                }
            }
            else
            {
                foreach (var kvp in Data.tracerLines)
                {
                    if (kvp.Value != null)
                        Destroy(kvp.Value.gameObject);
                }
                Data.tracerLines.Clear();
            }
            if (DistanceEsp)
            {
                for (int i = 0; i < ZenMenu.patches.VrrigCache.Data.vrrigs.Count; i++)
                {
                    VRRig rig = ZenMenu.patches.VrrigCache.Data.vrrigs[i];

                    if (!Data.distanceLabels.ContainsKey(rig) || Data.distanceLabels[rig] == null)
                    {
                        GameObject labelObj = new GameObject($"distEsp_{rig.Creator.NickName}");
                        TextMesh tm = labelObj.AddComponent<TextMesh>();
                        tm.fontSize = 48;
                        tm.characterSize = 0.02f;
                        tm.anchor = TextAnchor.MiddleCenter;
                        tm.alignment = TextAlignment.Center;
                        Data.distanceLabels[rig] = tm;
                    }
                    float dist = Vector3.Distance(
                        GorillaTagger.Instance.bodyCollider.transform.position,
                        rig.transform.position
                    );
                    TextMesh label = Data.distanceLabels[rig];
                    label.text = $"{dist:F1}m";
                    label.color = Mat(rig).color;
                    label.transform.position = rig.transform.position + Vector3.up * 0.35f;
                    label.transform.LookAt(
                        label.transform.position + Camera.main.transform.rotation * Vector3.forward,
                        Camera.main.transform.rotation * Vector3.up
                    );
                }
                var toRemoveDist = Data.distanceLabels.Keys
                    .Where(r => !ZenMenu.patches.VrrigCache.Data.vrrigs.Contains(r)).ToList();
                foreach (var r in toRemoveDist)
                {
                    if (Data.distanceLabels[r] != null)
                        Destroy(Data.distanceLabels[r].gameObject);
                    Data.distanceLabels.Remove(r);
                }
            }
            else
            {
                foreach (var kvp in Data.distanceLabels)
                    if (kvp.Value != null) Destroy(kvp.Value.gameObject);
                Data.distanceLabels.Clear();
            }
            if (BoxEsp)
            {
                for (int i = 0; i < ZenMenu.patches.VrrigCache.Data.vrrigs.Count; i++)
                {
                    VRRig rig = ZenMenu.patches.VrrigCache.Data.vrrigs[i];

                    if (!Data.boxRenderers.ContainsKey(rig) || Data.boxRenderers[rig] == null)
                    {
                        GameObject boxObj = new GameObject($"boxEsp_{rig.Creator.NickName}");
                        LineRenderer lr = boxObj.AddComponent<LineRenderer>();
                        lr.positionCount = 16; 
                        lr.startWidth = 0.02f;
                        lr.endWidth = 0.02f;
                        lr.loop = false;
                        lr.useWorldSpace = true;
                        lr.material = Mat(rig);
                        Data.boxRenderers[rig] = lr;
                    }
                    LineRenderer box = Data.boxRenderers[rig];
                    box.material = Mat(rig);
                    Bounds bounds = rig.mainSkin.bounds;
                    Vector3 min = bounds.min;
                    Vector3 max = bounds.max;
                    Vector3 b0 = new Vector3(min.x, min.y, min.z);
                    Vector3 b1 = new Vector3(max.x, min.y, min.z);
                    Vector3 b2 = new Vector3(max.x, min.y, max.z);
                    Vector3 b3 = new Vector3(min.x, min.y, max.z);
                    Vector3 t0 = new Vector3(min.x, max.y, min.z);
                    Vector3 t1 = new Vector3(max.x, max.y, min.z);
                    Vector3 t2 = new Vector3(max.x, max.y, max.z);
                    Vector3 t3 = new Vector3(min.x, max.y, max.z);
                    Vector3[] pts = new Vector3[16]{b0, b1, b2, b3, b0,  t0, t1, t2, t3, t0,  t1, b1,  b2, t2,   t3, b3    };
                    box.positionCount = pts.Length;
                    box.SetPositions(pts);
                }

                var toRemoveBox = Data.boxRenderers.Keys
                    .Where(r => !ZenMenu.patches.VrrigCache.Data.vrrigs.Contains(r)).ToList();
                foreach (var r in toRemoveBox)
                {
                    if (Data.boxRenderers[r] != null) Destroy(Data.boxRenderers[r].gameObject);
                    Data.boxRenderers.Remove(r);
                }
            }
            else
            {
                foreach (var kvp in Data.boxRenderers)
                    if (kvp.Value != null) Destroy(kvp.Value.gameObject);
                Data.boxRenderers.Clear();
            }
            if (TrailEsp)
            {
                for (int i = 0; i < ZenMenu.patches.VrrigCache.Data.vrrigs.Count; i++)
                {
                    VRRig rig = ZenMenu.patches.VrrigCache.Data.vrrigs[i];

                    if (!Data.trailRenderers.ContainsKey(rig) || Data.trailRenderers[rig] == null)
                    {
                        GameObject trailObj = new GameObject($"trailEsp_{rig.Creator.NickName}");
                        TrailRenderer tr = trailObj.AddComponent<TrailRenderer>();
                        tr.time = 1.5f;
                        tr.startWidth = 0.08f;
                        tr.endWidth = 0.0f;
                        tr.minVertexDistance = 0.05f;
                        tr.material = Mat(rig);
                        Gradient gradient = new Gradient();
                        gradient.SetKeys(
                            new GradientColorKey[]  { new GradientColorKey(Mat(rig).color, 0f),
                                              new GradientColorKey(Mat(rig).color, 1f) },
                            new GradientAlphaKey[]  { new GradientAlphaKey(1f, 0f),
                                              new GradientAlphaKey(0f, 1f) }
                        );
                        tr.colorGradient = gradient;
                        Data.trailRenderers[rig] = tr;
                    }
                    Data.trailRenderers[rig].transform.position = rig.transform.position;
                    Color c = Mat(rig).color;
                    Gradient g = new Gradient();
                    g.SetKeys(
                        new GradientColorKey[] { new GradientColorKey(c, 0f), new GradientColorKey(c, 1f) },
                        new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
                    );
                    Data.trailRenderers[rig].colorGradient = g;
                }
                var toRemoveTrail = Data.trailRenderers.Keys
                    .Where(r => !ZenMenu.patches.VrrigCache.Data.vrrigs.Contains(r)).ToList();
                foreach (var r in toRemoveTrail)
                {
                    if (Data.trailRenderers[r] != null) Destroy(Data.trailRenderers[r].gameObject);
                    Data.trailRenderers.Remove(r);
                }
            }
            else
            {
                foreach (var kvp in Data.trailRenderers)
                    if (kvp.Value != null) Destroy(kvp.Value.gameObject);
                Data.trailRenderers.Clear();
            }

        }
        public static Material Mat(VRRig rig)
        {
            if (GameMode.CurrentGameModeType is GameModeType.Paintbrawl)
            {
                Color localTeam = (PaintbrawlStatus)typeof(GorillaPaintbrawlManager).GetMethod("GetPlayerStatus").Invoke(GorillaGameManager.instance, new object[] { VRRig.LocalRig.OwningNetPlayer }) == PaintbrawlStatus.BlueTeam ? Color.blue : Color.red;
                Color otherTeam = (PaintbrawlStatus)typeof(GorillaPaintbrawlManager).GetMethod("GetPlayerStatus").Invoke(GorillaGameManager.instance, new object[] { VRRig.LocalRig.OwningNetPlayer }) == PaintbrawlStatus.BlueTeam ? Color.red : Color.blue;
                return new Material(Shader.Find("GUI/Text Shader"))
                {
                    color = (bool)typeof(GorillaPaintbrawlManager).GetMethod("OnSameTeam").Invoke(GorillaGameManager.instance, new object[] { VRRig.LocalRig.OwningNetPlayer, rig.OwningNetPlayer }) ? localTeam : otherTeam
                };
            }

            if (GameMode.CurrentGameModeType is GameModeType.Infection ||
                GameMode.CurrentGameModeType is GameModeType.SuperInfect ||
                GameMode.CurrentGameModeType is GameModeType.Guardian ||
                GameMode.CurrentGameModeType is GameModeType.FreezeTag)
            {
                return new Material(Shader.Find("GUI/Text Shader"))
                {
                    color = RigInfected(rig) ? ImInfected() ? Color.green : Color.red : ImInfected() ? Color.red : Color.green
                };
            }

            return new Material(Shader.Find("GUI/Text Shader")) { color = Color.white };
        }
        public static bool ImInfected()
        {
            bool infected = GorillaTagger.Instance.offlineVRRig.mainSkin.material.name.Contains("infected") ||
                GorillaTagger.Instance.offlineVRRig.mainSkin.material.name.Contains("it") ||
                GorillaTagger.Instance.offlineVRRig.mainSkin.material.name.Contains("ice") ||
                GorillaGuardianZoneManager.zoneManagers.Any(x => x.CurrentGuardian == VRRig.LocalRig.OwningNetPlayer) || GorillaPropHuntGameManager.instance.IsInfected(VRRig.LocalRig.Creator);
            return infected;
        }
        public static bool RigInfected(VRRig target)
        {
            if (target.mainSkin.material.name.Contains("infected") || target.mainSkin.material.name.Contains("it") || target.mainSkin.material.name.Contains("ice") || GorillaGuardianZoneManager.zoneManagers.Any(x => x.CurrentGuardian == target.OwningNetPlayer) || GorillaPropHuntGameManager.instance.IsInfected(target.Creator))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
