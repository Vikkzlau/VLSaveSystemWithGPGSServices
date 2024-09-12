/*  VLSaveSystemWithGPGSServices (Save system for Unity projects, compatible with GPGS)
    Copyright (C) 2024  Vikkzlau

    VLSaveSystemWithGPGSServices is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    VLSaveSystemWithGPGSServices is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with VLSaveSystemWithGPGSServices.  If not, see <https://www.gnu.org/licenses/>.*/
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