using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheMazurkaStudio.SceneManagement
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] private bool loadOnStart;

        [SerializeField] private SceneLoadingManager.LoadSceneParameters loadParameters =
            new ()
            {
                loadSceneMode = LoadSceneMode.Additive,
                allowSceneActivation = true,
            };
        
        private void Start()
        {
            if (!loadOnStart) return;
            
            Execute();
        }

        public void Execute() =>  loadParameters.Execute();
        public void Execute(Action onCompleted) =>  loadParameters.Execute(onCompleted);
    }
}
