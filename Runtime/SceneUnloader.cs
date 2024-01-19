using System;
using UnityEngine;

namespace TheMazurkaStudio.SceneManagement
{
    public class SceneUnloader : MonoBehaviour
    {
        [SerializeField] private bool unloadOnStart;

        [SerializeField] private SceneLoadingManager.UnloadSceneParameters unloadParameters;
        private void Start()
        {
            if (!unloadOnStart) return;
            
            Execute();
        }

        public void Execute() =>  unloadParameters.Execute();
        public void Execute(Action onCompleted) =>  unloadParameters.Execute(onCompleted);
    }
}
