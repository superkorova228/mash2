using UnityEngine;

namespace mash2.Data
{
    // Это шаблон для создания ассетов сцен
    [CreateAssetMenu(fileName = "SceneData", menuName = "mash2/Scene Data")]
    public class SceneData : ScriptableObject
    {
        [Header("Scene Information")]
        public string sceneName;        // Имя сцены (должно совпадать с названием в Build Settings)
        public int sceneIndex;           // Индекс сцены для быстрого доступа
        
        [Header("Loading Settings")]
        public bool showLoadingScreen = true;  // Показывать ли экран загрузки
        public float minimumLoadTime = 0.5f;   // Минимальное время загрузки (для красоты)
        
        [Header("Audio")]
        public bool fadeOutMusic = true;       // Затухание музыки при смене сцены
        public float musicFadeDuration = 0.5f; // Длительность затухания
    }
}