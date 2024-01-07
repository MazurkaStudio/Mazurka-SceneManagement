using Sirenix.OdinInspector;
using UnityEngine;

namespace TheMazurkaStudio
{
    [AddComponentMenu("The Mazurka Studio/Scene Management/Scene Loader")]
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] private LoadSceneCommand _command;
        

        [Button]
        public void Execute()
        {
            _command.Execute();
        }
    }
 
}

