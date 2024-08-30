using System;
using UnityEngine;

namespace VLSaveSystemWithGPGSServices
{
    [CreateAssetMenu(fileName = "PlayTimeManager", menuName = "GPGS Services Assets/PlayTimeManager", order = 1)]
    public class PlayTimeManager : ScriptableObject
    {
        public static PlayTimeManager Instance;
        private DateTime sessionStartTime;
        private TimeSpan previousPlayTime;
        private TimeSpan currentPlayTime;
        private TimeSpan totalPlayTime;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }

            LoadPlayTime();
            sessionStartTime = DateTime.Now;
        }

        public void LoadedPreviousTimeSpan(TimeSpan timeSpan)
        {
            previousPlayTime = timeSpan;
        }

        public TimeSpan GetLoadedTimeSpan()
        {
            return previousPlayTime;
        }

        public TimeSpan GetLatestTimeSpan()
        {
            currentPlayTime = DateTime.Now - sessionStartTime;
            totalPlayTime = previousPlayTime + currentPlayTime;
            return totalPlayTime;
        }

        public void SavePlayTime()
        {
            GetLatestTimeSpan();
            string playTimeString = totalPlayTime.TotalSeconds.ToString();
            PlayerPrefs.SetString("TotalPlayTime", playTimeString);
            //Debug.Log("TotalPlayTime save: " + playTimeString);
        }

        void LoadPlayTime()
        {
            try
            {
                string playTimeString = PlayerPrefs.GetString("TotalPlayTime");
                double playTimeDouble = Convert.ToDouble(playTimeString);
                TimeSpan playTime = TimeSpan.FromSeconds(playTimeDouble);
                LoadedPreviousTimeSpan(playTime);
            }
            catch { }
        }
    }
}
