using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ScannerRoomBeacons  // This is usually the name of your mod.
{
    [HarmonyPatch]
    internal class MapRoomFunctionality_UpdateCameraBlips_Patch
    {
        /* When beaconBlipRoot and beaconBlips is on the Outside of UpdateBeaconBlips: 
	     - The blips showup and disappear
	     - This appears dependent on a scan running or not almost? As in if you load in and a scan is running... the beacon blips never appear.
	     - Otherwise they appear for a few seconds and then disappear continually.
         - How long it shows depends as well on... something, idk yet. 
         - Probably has something to do with not caching or.. something..
         */

        public static GameObject beaconBlipRoot;
        public static List<GameObject> beaconBlips = new List<GameObject>();

        /* Doing this patch after/before UpdateCameraBlips() or UpdateBlips() is best. 
         * This isn't actually editing those functions just piggy backing off WHEN they run. 
         */
        [HarmonyPatch(typeof(MapRoomFunctionality))]
        [HarmonyPatch("UpdateBlips")] 
        [HarmonyPostfix]
        private static void UpdateBeaconBlips(MapRoomFunctionality __instance)
        {
            /* When beaconBlipRoot and beaconBlips is on the Inside of UpdateBeaconBlips:
             - The blips showup and never stop stacking: blips NEVER disappear.
             - Each update they just stack and stack causing the "blip" to appear brighter and bigger.
             */
            //GameObject beaconBlipRoot = new GameObject("BeaconBlipRoot");
            //List<GameObject> beaconBlips = new List<GameObject>();

            /* This block is taken from StartScanning() for some reason.
             * It did help clear up for a while old blips and is probably needed to be placed somewhere else.
             * It'll be needed if I don't come up with a way to check if a beaconBlip is already added and instead
             * am just clearing the blips each time. Is it inefficient, hell yea. Do I know a better way yet, hell nah.
             */
            beaconBlips.Clear();
            UnityEngine.Object.Destroy(beaconBlipRoot);
            beaconBlipRoot = new GameObject("BeaconBlipRoot");
            beaconBlipRoot.transform.SetParent(__instance.wireFrameWorld, false);

            /*  BeaconManager.beacons is a hashset and I could probably have left it but I don't know how to
             *  get the .transform.position using a hashset...
             *  Also down the road I'll probably need a separate array to tack if a beacon is added to a holo map or not.
             */
            List<Beacon> beaconList = BeaconManager.beacons.ToList();

            /* This code is based heavily on UpdateCameraBlips() because I can. It made sense at the time.*/
            float scanRange = __instance.GetScanRange();
            float num = scanRange * scanRange;
            Vector3 position = beaconBlipRoot.transform.position;
            int num2 = 0;
            for (int i = 0; i < BeaconManager.GetCount(); i++)
            {
                Vector3 position2 = beaconList[i].transform.position;
                if ((__instance.wireFrameWorld.position - position2).sqrMagnitude <= num)
                {
                    Vector3 vector = (position2 - position) * __instance.mapScale;
                    if (num2 >= beaconBlips.Count)
                    {
                        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(__instance.blipPrefab, vector, Quaternion.identity);
                        gameObject.transform.SetParent(beaconBlipRoot.transform, false);
                        beaconBlips.Add(gameObject);

                        /* "Borrowed from AutosortLockers to change the color
                         * It would be cool later on to make the color dependant on the beacon set color but idk how.
                         */
                        MeshRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<MeshRenderer>();
                        for (int k = 0; k < componentsInChildren.Length; k++)
                        { componentsInChildren[k].material.color = new Color(1f, 1f, 1f, 0.5f); }
                    }
                    beaconBlips[num2].transform.localPosition = vector;
                    beaconBlips[num2].SetActive(true); // Why this needed tho; possible prevent relooping something that hasn't moved?
                    num2++;
                }
            }
            for (int j = num2; j < beaconBlips.Count; j++)
            {
                beaconBlips[j].SetActive(false); // Why this needed tho; possible prevent relooping something that hasn't moved?
            }
        }
    }
}