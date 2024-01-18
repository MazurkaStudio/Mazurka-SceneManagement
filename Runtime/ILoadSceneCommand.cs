using System;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;

namespace TheMazurkaStudio.SceneManagement
{
    public interface ILoadSceneCommand
    {
        public string[] TargetScenes { get; }
        public LoadSceneMode TargetLoadSceneMode { get; }
        public string TargetScene { get; }
        public bool IsSingleScene { get; }
        public void Execute();
    }

    
    [Serializable]
    public class LoadSceneCommand : ILoadSceneCommand
    {
        public LoadSceneCommand(string[] scenesToLoad, LoadType loadType,  LoadSceneMode sceneMode)
        {
            this.scenesToLoad = scenesToLoad;
            this.loadType = loadType;
            this.sceneMode = sceneMode;
        }
        public LoadSceneCommand(string scenesToLoad, LoadType loadType,  LoadSceneMode sceneMode)
        {
            this.scenesToLoad = new [] {scenesToLoad};
            this.loadType = loadType;
            this.sceneMode = sceneMode;
        }

        public event Action Completed; 
        
        [BoxGroup("Load Settings")] public string[] scenesToLoad;
        [BoxGroup("Load Settings")] public LoadType loadType;
        [HideIf("HideSceneMode"), BoxGroup("Load Settings")] public LoadSceneMode sceneMode;
        
        #if UNITY_EDITOR

        private bool IsSingle => scenesToLoad.Length <= 1;
        private bool HideSceneMode => loadType == LoadType.Unload || scenesToLoad.Length > 1;
        
        #endif
        
        private string sceneToLoad;
        private bool singleScene;
        
        public string[] TargetScenes => scenesToLoad;
        public LoadSceneMode TargetLoadSceneMode => sceneMode;
        public string TargetScene => sceneToLoad;
        public bool IsSingleScene => singleScene;

        public void Execute()
        {
            if (scenesToLoad.Length == 0) return;
            
            //Single ?
            if (scenesToLoad.Length <= 1)
            {
                sceneToLoad = scenesToLoad[0];
                singleScene = true;
            }
            else
            {
                sceneMode = LoadSceneMode.Additive;
            }
            
            switch (loadType)
            {
                case LoadType.Load:
                    SceneLoadingManager.Instance.Load(this);
                    break;
                case LoadType.Unload:
                    SceneLoadingManager.Instance.Unload(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    [Serializable]
    public enum LoadType
    {
        Load,
        Unload
    }
}

