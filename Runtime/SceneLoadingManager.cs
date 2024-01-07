using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheMazurkaStudio
{
    public class SceneLoadingManager : Singleton<SceneLoadingManager>
    {
        public bool IsLoadingScene => currentLoadCommand != null;
        public static event Action ScenesAreLoaded;
        public static event Action StartLoadingScenes;

        public Dictionary<string, Scene> ActiveScenes { get; private set; }

        private void Awake()
        {
            SceneManager.sceneLoaded += SceneManagerOnSceneLoaded;
            SceneManager.sceneUnloaded += SceneManagerOnSceneUnloaded;
            ActiveScenes = new Dictionary<string, Scene>();

        }
        protected override void OnDestroySpecific()
        {
            base.OnDestroySpecific();
            
            SceneManager.sceneLoaded -= SceneManagerOnSceneLoaded;
            SceneManager.sceneUnloaded -= SceneManagerOnSceneUnloaded;
        }
        
        
        private void SceneManagerOnSceneUnloaded(Scene scene)
        {
            if (ActiveScenes.ContainsKey(scene.name))
            {
                ActiveScenes.Remove(scene.name); //Remove from active scenes if unload by another processus
            }
        }
        private void SceneManagerOnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ActiveScenes.Add(scene.name, scene);
        }

        
        private bool SceneIsActive(string sceneName) => ActiveScenes.ContainsKey(sceneName);
        private bool SceneIsInCommandQueue(string sceneName, out Command queueCommand)
        {
            foreach (var load in LoadCommands.Where(load => !load.IsCompleted && load.SceneName == sceneName))
            {
                queueCommand = load;
                return true;
            }
            
            queueCommand = null;
            return false;
        }

        
        
        public void Load(ILoadSceneCommand command)
        {
            if (command.IsSingleScene)
            {
                LoadSingle(command.TargetScene, command.TargetLoadSceneMode);
            }
            else
            {
                foreach (var scene in command.TargetScenes)
                {
                    LoadSingle(scene, command.TargetLoadSceneMode);
                }
            }
        }
        public void Unload(ILoadSceneCommand command)
        {
            if (command.IsSingleScene)
            {
                UnloadSingle(command.TargetScene);
            }
            else
            {
                foreach (var scene in command.TargetScenes)
                {
                    UnloadSingle(scene);
                }
            }
        }

        private void LoadSingle(string targetScene, LoadSceneMode mode)
        {
            //SCENE IS LOADED
            if (SceneIsActive(targetScene)) 
            {
                //CHECK IS A COMMAND UNLOAD THE SCENE
                if (!SceneIsInCommandQueue(targetScene, out var c)) return;
                
                //CHECK COMMAND TYPE
                if (c is not UnloadCommand) return;
                
                //CANCEL UNLOADING
                Debug.Log("Cancel Unloading of scene " + targetScene);
                c.Cancel(); 
                return;
            }
                    
            //CHECK IF COMMAND ALREADY LOAD SCENE
            if (SceneIsInCommandQueue(targetScene, out var queueCommand))
            {
                if(queueCommand is LoadCommand) 
                {
                    Debug.Log("Scene " + targetScene + " is already in queue for loading");
                    return; //Already in queue !
                }
            }

            //IF CURRENTLY LOADING
            if (currentLoadCommand != null && currentLoadCommand.SceneName == targetScene && currentLoadCommand is LoadCommand) return;

            AddLoadCommand(new LoadCommand(targetScene, mode));
        }
        private void UnloadSingle(string targetScene)
        {
            //SCENE IS NOT ACTIVE
            if (!SceneIsActive(targetScene)) 
            {
                //CHECK IF LOADING COMMAND IS IN QUEUE
                if (!SceneIsInCommandQueue(targetScene, out var queueCommand)) return;
                        
                //CHECK COMMAND TYPE
                if (queueCommand is not LoadCommand) return;
                        
                //CANCEL LOADING
                Debug.Log("Cancel Loading of scene " + targetScene);
                queueCommand.Cancel();
                return; 
            }
                    
            //CHECK IF UNLOAD COMMAND ALREADY EXIST FOR THIS SCENE
            if (SceneIsInCommandQueue(targetScene, out var c))
            {
                if(c is UnloadCommand) 
                {
                    Debug.Log("Scene " + targetScene + " is already in queue for unloading");
                    return; //Already in queue !
                }
            }
            
            //IF CURRENTLY UNLOADING
            if (currentLoadCommand != null &&currentLoadCommand.SceneName == targetScene && currentLoadCommand is UnloadCommand) return;
            
            AddLoadCommand(new UnloadCommand(targetScene, LoadSceneMode.Additive));
        }
        
        private void Update()
        {
            UpdateLoadCommands();
        }

        
        #region Command Queue

        private Queue<Command> LoadCommands = new Queue<Command>();
        private Command currentLoadCommand;
        private bool haveStartCommand;
        
        private void AddLoadCommand(Command loadCommand)
        {
            LoadCommands.Enqueue(loadCommand);

            if (currentLoadCommand != null) return;
            
            haveStartCommand = true;
            currentLoadCommand = LoadCommands.Dequeue();
            StartLoadingScenes?.Invoke();
        }
        private void UpdateLoadCommands()
        {
            if (currentLoadCommand == null)
            {
                if (!haveStartCommand) return;
                
                haveStartCommand = false;
                ScenesAreLoaded?.Invoke();
                return;
            }
            
            if (currentLoadCommand.IsCompleted)
            {
                if (LoadCommands.Count <= 0) //Was Last
                {
                    currentLoadCommand = null;
                    return;
                }
                
                currentLoadCommand = LoadCommands.Dequeue(); //Next
            }
            
            currentLoadCommand.Execute();
        }

        private abstract class Command
        {
            public Command(string sceneName, LoadSceneMode sceneMode)
            {
                this.sceneName = sceneName;
                this.sceneMode = sceneMode;
            }
            
            protected readonly string sceneName;
            protected readonly LoadSceneMode sceneMode;
            protected AsyncOperation async;

            public string SceneName => sceneName;
            
            protected abstract void Start();
            protected abstract void Update();
            protected abstract void OnCompleted();
            protected abstract void OnCancel();

            private bool hasStart;
            public bool IsCompleted { get; private set; }
            
            
            public void Complete()
            {
                OnCompleted();
                IsCompleted = true;
            }
            public void Cancel()
            {
                OnCancel(); 
                IsCompleted = true;
            }
            public void Execute()
            {
                if (IsCompleted) return;
                
                if (!hasStart)
                {
                    hasStart = true;
                    Start();
                }
                
                Update();
            }
        }
        
        private class LoadCommand : Command
        {
            protected override void Start()
            {
                Debug.Log("Load Scene " + sceneName);
                async = SceneManager.LoadSceneAsync(sceneName, sceneMode);
                async.allowSceneActivation = false;
            }

            protected override void Update()
            {
                if (async.progress < 0.9f) return;
                
                Complete();
            }

            protected override void OnCancel()
            {
            }

            protected override void OnCompleted()
            {
                async.allowSceneActivation = true;
            }

            public LoadCommand(string sceneName, LoadSceneMode sceneMode) : base(sceneName, sceneMode)
            {
            }
        }

        private class UnloadCommand : Command
        {
            protected override void Start()
            {
                Debug.Log("Unload Scene " + sceneName);
                Instance.ActiveScenes.Remove(sceneName); //Remove from active scenes
                async = SceneManager.UnloadSceneAsync(sceneName);
                async.allowSceneActivation = false;
            }
            
            protected override void Update()
            {
                if (async.progress < 0.9f) return;
                
                Complete();
            }
            
            protected override void OnCancel()
            {
                
            }
            
            protected override void OnCompleted()
            {
                async.allowSceneActivation = true;
            }

            public UnloadCommand(string sceneName, LoadSceneMode sceneMode) : base(sceneName, sceneMode)
            {
            }
        }
        
        #endregion
    }
}

