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
    public class SaveManagerUtil<T>
    {
        public readonly string savePath;
        private readonly string saveKey = "temp";

        public SaveManagerUtil(string savefileName, string saveKey)
        {
            this.saveKey = saveKey;
            savePath = Application.persistentDataPath + savefileName;
            GPGSSaveLoadUtil.InitializeAssets();
        }

        public void Save(object saveData)
        {
            GPGSSaveLoadUtil.SaveObject(saveData, savePath, saveKey, typeof(T));
        }

        public T Load(out SaveLoadMethod saveLoadMethod)
        {
            object loadedObj = GPGSSaveLoadUtil.LoadObject(savePath, saveKey, typeof(T), out saveLoadMethod);

            // using try catch here, since GPGS loading may have problems.
            try
            {
                T loadData = (T)loadedObj;
                // Data loaded successfully.
                return loadData;
            }
            catch (Exception e)
            {
                Debug.Log("Object type cast failed during loading. " + e.Message);
            }

            return default;
        }

        public void SaveToPlayerPrefs(object saveData)
        {
            GPGSSaveLoadUtil.SaveToPlayerPrefs(saveData, saveKey);
        }

        public T LoadFromPlayerPrefs()
        {
            return (T)GPGSSaveLoadUtil.LoadFromPlayerPrefs(saveKey, typeof(T));
        }
    }
}