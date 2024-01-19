using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheMazurkaStudio.SceneManagement
{
    /// <summary>
    /// Scene dependencies auto load/unload scenes when this scene is loaded or unloaded.
    /// </summary>
    public class SceneDependencies : MonoBehaviour
    {
        [SerializeField] private string[] sceneDependencies;
        [SerializeField] private UnloadSceneOptions unloadSceneOptions;
        
        private void Awake()
        {
            new SceneLoadingManager.LoadSceneParameters()
            {
                scenesToLoad = sceneDependencies,
                loadSceneMode = LoadSceneMode.Additive,
                allowSceneActivation = true
            }.Execute();
        }

        private void OnDestroy()
        {
            if (SceneLoadingManager.Instance == null) return;
            
            new SceneLoadingManager.UnloadSceneParameters()
            {
                scenesToUnload = sceneDependencies,
                unloadSceneMode =unloadSceneOptions
            }.Execute();
        }
    }
}
