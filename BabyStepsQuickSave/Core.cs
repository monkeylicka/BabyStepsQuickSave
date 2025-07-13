using Il2CppRootMotion.Dynamics;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(BabyStepsQuickSave.Core), "BabyStepsQuickSave", "1.0.0", "monkeylicker", null)]
[assembly: MelonGame("DefaultCompany", "BabySteps")]

namespace BabyStepsQuickSave
{
    public class Core : MelonMod
    {
        private bool isFrozen = false;
        private object lockCoroutine = null;

        private GameObject playerGlobal = null;

        private struct TransformState
        {
            public UnityEngine.Vector3 position { get; set; }
            public UnityEngine.Quaternion rotation { get; set; }
        }

        private Dictionary<Transform, TransformState> originalStates = new Dictionary<Transform, TransformState>();

        string charDir = "Dudest";///PuppetMaster/root.x";

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (Input.GetKeyDown(KeyCode.F9))
            {
                playerGlobal = GameObject.Find(charDir);
                originalStates.Clear();
                StoreChildTransforms(playerGlobal.transform);
                MelonLogger.Msg("Quicksaved");
            }

            if (Input.GetKeyDown(KeyCode.F10))
            {
                playerGlobal = GameObject.Find(charDir);
                MelonCoroutines.Start(ToggleTwice());
                MelonLogger.Msg("Quickloaded");
            }
        }

        private System.Collections.IEnumerator ToggleTwice() //This is its own coroutine so that it can be delayed
        {
            ToggleFreeze(playerGlobal);
            yield return new WaitForSecondsRealtime(0.1f);
            ToggleFreeze(playerGlobal);
        }

        private void ToggleFreeze(GameObject player)
        {
            isFrozen = !isFrozen;

            if (player == null) { return; }

            if (isFrozen)
            {
                lockCoroutine = MelonCoroutines.Start(LockObjectTransform());
                Time.timeScale = 0f;
            }
            else
            {
                if (lockCoroutine != null)
                {
                    MelonCoroutines.Stop(lockCoroutine);
                    lockCoroutine = null;
                }
                Time.timeScale = 1f;
            }
        }

        private void StoreChildTransforms(Transform parent)
        {
            originalStates[parent] = new TransformState
            {
                position = parent.position,
                rotation = parent.rotation,
            };

            for (int i = 0; i < parent.childCount; i++)
            {
                StoreChildTransforms(parent.GetChild(i));
            }
        }

        private System.Collections.IEnumerator LockObjectTransform()
        {
            while (true)
            {
                foreach (var pair in originalStates)
                {
                    if (pair.Key != null)
                    {
                        pair.Key.position = pair.Value.position;
                        pair.Key.rotation = pair.Value.rotation;
                    }
                }
                yield return null;
            }
        }

        private void LockTransformAndChildren(Transform parent, UnityEngine.Vector3 position)
        {
            parent.position = position;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                LockTransformAndChildren(child, position);
            }
        }
    }
}