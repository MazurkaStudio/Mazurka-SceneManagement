using System;
using UnityEngine;

namespace TheMazurkaStudio.SceneManagement
{
    [DefaultExecutionOrder(-100)]
    [AddComponentMenu("The Mazurka Studio/Scene Management/Scene Dependencies")]
    public class SceneDependencies : MonoBehaviour
    {
        [SerializeField] private LoadSceneCommand _sceneCommand;

        private void Awake()
        { 
           
        }

        private void Start()
        {
            LoadAllDependencies();
        }

        public void LoadAllDependencies()
        {
            _sceneCommand.loadType = LoadType.Load;
            _sceneCommand.Execute();
        }
   
        public void UnloadAllDependencies()
        {
            _sceneCommand.loadType = LoadType.Unload;
            _sceneCommand.Execute();
        }
    }

}

