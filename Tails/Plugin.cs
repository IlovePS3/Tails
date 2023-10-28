using BepInEx;
using BepInEx.Configuration;
using ExitGames.Client.Photon;
using Photon.Pun;
using System;
using UnityEngine;
using System.Reflection;
using Photon.Realtime;
using System.Collections.Generic;
using System.IO;
using HoneyLib.Utils;

namespace Tails
{
    [BepInDependency("org.legoandmars.gorillatag.utilla")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<int> Cosmetic;

        public static GameObject TailBase;
        public static GameObject NoLODObj;

        public static Vector3 place;
        public static Vector3 Rotate;

        static Tail LocalTail;

        Stream s;

        AssetBundle b;

        void Start()
        {
            Utilla.Events.GameInitialized += OnGameInitialized;
            Cosmetic = Config.Bind("Tail", "Cosmetic", 0);
        }
        void OnGameInitialized(object sender, EventArgs e)
        {
            NoLODObj = GameObject.Find("Player Objects/Local VRRig/Local Gorilla Player/rig/body/gorillachest");
            gameObject.AddComponent<PluginNet>();
            TailBase = EasyAssetLoading.InstantiateAsset(Assembly.GetExecutingAssembly(), "Tails.Asset.tail", "Tail");
            place = new Vector3(0, -0.306f, -0.215f);
            Rotate = new Vector3(85, 180, 180);
        }
        public static void MakeObj(GameObject NoLODobj, GameObject MeshToHave)
        {
            GameObject MeshToTake = Instantiate(MeshToHave);
            SkinnedMeshRenderer meshf = MeshToTake.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>();
            GameObject FixMesh()
            {
                GameObject thing = Instantiate(NoLODobj);
                thing.GetComponent<MeshFilter>().mesh = meshf.sharedMesh;
                thing.GetComponent<Renderer>().material = GorillaTagger.Instance.offlineVRRig.mainSkin.material;
                return thing;
            }
            Destroy(MeshToTake);
            TailBase = FixMesh();
            DontDestroyOnLoad(TailBase);

        }
        public static void LocalAddTail()
        {
            LocalTail = Instantiate(TailBase, GorillaTagger.Instance.offlineVRRig.transform).AddComponent<Tail>();
            LocalTail.transform.localPosition = Plugin.place;
            LocalTail.transform.localRotation = Quaternion.Euler(Plugin.Rotate);
            LocalTail.rig = GorillaTagger.Instance.offlineVRRig;
            LocalTail.player = PhotonNetwork.LocalPlayer;
        }
    }

    class PluginNet : MonoBehaviourPunCallbacks
    {
        Dictionary<Player, Tail> people = new Dictionary<Player, Tail>();
        GameObject TempTail;
        public void Start()
        {
            Hashtable tab = new Hashtable()
            {
                { "Tail", Plugin.Cosmetic.Value }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(tab);
            Plugin.LocalAddTail();
        }
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            try
            {
                if (newPlayer.CustomProperties.ContainsKey("Tail") && !people.ContainsKey(newPlayer))
                {
                    people.Add(newPlayer, MakeOtherTail(newPlayer));
                }
                TempTail = null;
            }
            catch
            {
                Debug.Log("Something Went Wrong In getting tails");
            }
        }
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            try
            {
                if (people.ContainsKey(otherPlayer))
                {
                    Destroy(people[otherPlayer].gameObject);
                    people.Remove(otherPlayer);
                }
            }
            catch
            {
                Debug.Log("Failed to check/reset Tails");
            }
        }
        public override void OnLeftRoom()
        {
            foreach (Tail t in people.Values)
            {
                Destroy(t.gameObject);
            }
            people.Clear();

        }
        Tail MakeOtherTail(Player p)
        {
            VRRig rig = GorillaGameManager.instance.FindPlayerVRRig(p);
            GameObject tail = Instantiate(Plugin.TailBase, rig.transform);
            tail.transform.localPosition = Plugin.place;
            tail.transform.localRotation = Quaternion.Euler(Plugin.Rotate);
            Tail t = TempTail.AddComponent<Tail>();
            t.rig = rig;
            t.player = p;
            return t;
        }
    }
}
