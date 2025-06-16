using System.Linq;
using Sandbox.Developers.Importers;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class AssetViewer : MonoBehaviour
    {
        private GameObject asset;
        private SandboxAssetImporter importer;
        public GameObject stage;
        public Dropdown dropdownAnimations;
        public Toggle toggleLoop;
        
        public void Start()
        {
            Hide();
            dropdownAnimations.onValueChanged.AddListener(PlayAnimation);
            toggleLoop.onValueChanged.AddListener(PlayAnimation);
        }

        private void PlayAnimation(int clipIndex)
        {
            importer.PlayAnimation(importer.AnimationClips[clipIndex].name, toggleLoop.isOn);
        }

        private void PlayAnimation(bool value)
        {
            PlayAnimation(dropdownAnimations.value);
        }

        public async void Show(string assetId)
        {
            gameObject.SetActive(true);
            asset = new GameObject();
            asset.transform.parent = stage.transform;
            importer = asset.AddComponent<SandboxAssetImporter>();
            await importer.ImportAssetById(assetId);
            CenterAssetToCamera();
            if (importer.HasAnimations())
            {
                dropdownAnimations.ClearOptions();
                dropdownAnimations.options.AddRange(importer.AnimationClips.Select(clip =>
                    new Dropdown.OptionData(clip.name)));
                dropdownAnimations.value = 0;
                dropdownAnimations.RefreshShownValue();
                PlayAnimation(dropdownAnimations.value);
                dropdownAnimations.gameObject.SetActive(true);
                toggleLoop.gameObject.SetActive(true);
            }
        }

        private void CenterAssetToCamera()
        {
            Bounds bounds = GetObjectBounds(stage);
            float maxSize = 80f;
            float scale = 1f;
            if (bounds.size.x * scale > maxSize)
            {
                scale = maxSize / bounds.size.x;
            }

            if (bounds.size.y * scale > maxSize)
            {
                scale = maxSize / bounds.size.y;
            }

            if (bounds.size.z * scale > maxSize)
            {
                scale = maxSize / bounds.size.z;
            }

            stage.transform.localScale = Vector3.one * scale;
            stage.transform.position = -bounds.center * scale;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            stage.transform.position = Vector3.zero;
            stage.transform.localScale = Vector3.one;
            dropdownAnimations.gameObject.SetActive(false);
            toggleLoop.gameObject.SetActive(false);
            // Destroy all children
            foreach (Transform child in stage.transform)
            {
                Destroy(child.gameObject);
            }
        }
        
        Bounds GetObjectBounds(GameObject obj)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

            if (renderers.Length == 0)
                return new Bounds(obj.transform.position, Vector3.zero);

            Bounds bounds = renderers[0].bounds;
            foreach (Renderer r in renderers)
            {
                bounds.Encapsulate(r.bounds);
            }
            return bounds;
        }
    }
}