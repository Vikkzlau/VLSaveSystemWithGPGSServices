// Google Play Games plugin must be installed first.
// Read this page to install GPG plugin for Unity: https://developer.android.com/games/pgs/unity/unity-start
#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
#endif
using System;
using UnityEngine;

namespace GPGS_Services
{
    [CreateAssetMenu(fileName = "GPGSManager", menuName = "GPGS Services Assets/GPGSManager", order = 0)]
    public class GPGSManager : ScriptableObject
    {
        #if UNITY_ANDROID
        public static GPGSManager Instance;

        // get ID from https://developer.android.com/games/pgs/leaderboards#the_basics
        private const string leaderboardID = "";

        private Texture2D savedImage;
        private byte[] saveData;
        private byte[] loadData;

        private bool isSaving;
        private bool isLoading;
        private TimeSpan loadedTimeSpan;

        private bool saveSucceeded;

        private Action callbackAfterAuthentication;

        void ResetSaveLoadStatus()
        {
            isSaving = false;
            isLoading = false;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            if (!PlayGamesPlatform.Instance.IsAuthenticated())
                Authenticate();
        }

        void Authenticate()
        {
            if (!PlayGamesPlatform.Instance.IsAuthenticated())
            {
                callbackAfterAuthentication = null;
                PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
            }
        }

        void Authenticate(Action callback)
        {
            if (!PlayGamesPlatform.Instance.IsAuthenticated())
            {
                callbackAfterAuthentication = callback;
                PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
            }
        }

        internal void ProcessAuthentication(SignInStatus status)
        {
            if (status == SignInStatus.Success)
            {
                Debug.Log("Logged in to GPGS.");
                if (callbackAfterAuthentication != null)
                {
                    callbackAfterAuthentication();
                }
                // Continue with Play Games Services
            }
            else
            {
                // Disable your integration with Play Games Services or show a login button
                // to ask users to sign-in. Clicking it should call
                // PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication).
            }
        }

        public bool SaveGame(string filename, byte[] saveByte)
        {
            if (!PlayGamesPlatform.Instance.IsAuthenticated())
                return false;


            saveSucceeded = false;
            saveData = saveByte;
            ResetSaveLoadStatus();
            try
            {
                isSaving = true;
                OpenSavedGame(filename);
                if (saveSucceeded)
                {
                    Debug.Log("PlayGames saved successfully.");
                }
            }
            catch (Exception e) { Debug.LogException(e); }

            return saveSucceeded;
        }

        public byte[] LoadGame(string filename, out TimeSpan loadedGamePlayTime)
        {
            if (!PlayGamesPlatform.Instance.IsAuthenticated())
            {
                loadedGamePlayTime = TimeSpan.Zero;
                return null;
            }

            loadData = null;
            loadedTimeSpan = TimeSpan.Zero;
            ResetSaveLoadStatus();
            try
            {
                isLoading = true;
                OpenSavedGame(filename);
                loadedGamePlayTime = loadedTimeSpan;
                return loadData;
            }
            catch (Exception e) { Debug.LogException(e); }
            loadedGamePlayTime = loadedTimeSpan;
            return loadData;
        }

        #region GPGScode
        //Display the saved games UI
        void ShowSelectUI()
        {
            uint maxNumToDisplay = 5;
            bool allowCreateNew = false;
            bool allowDelete = true;

            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            savedGameClient.ShowSelectSavedGameUI("Select saved game",
                maxNumToDisplay,
                allowCreateNew,
                allowDelete,
                OnSavedGameSelected);
        }


        public void OnSavedGameSelected(SelectUIStatus status, ISavedGameMetadata game)
        {
            if (status == SelectUIStatus.SavedGameSelected)
            {
                // handle selected game save
            }
            else
            {
                // handle cancel or error
            }
        }

        //Open a saved game
        void OpenSavedGame(string filename)
        {
            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            savedGameClient.OpenWithAutomaticConflictResolution(filename, DataSource.ReadCacheOrNetwork,
                ConflictResolutionStrategy.UseLongestPlaytime, OnSavedGameOpened);
        }

        public void OnSavedGameOpened(SavedGameRequestStatus status, ISavedGameMetadata game)
        {
            if (status == SavedGameRequestStatus.Success)
            {
                // handle reading or writing of saved game.
                if (isSaving)
                {
                    SaveGame(game, saveData, PlayTimeManager.Instance.GetLatestTimeSpan());
                }
                if (isLoading)
                    LoadGameData(game);
            }
            else
            {
                // handle error
            }
        }

        //Write a saved game
        void SaveGame(ISavedGameMetadata game, byte[] savedData, TimeSpan totalPlaytime)
        {
            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;

            SavedGameMetadataUpdate.Builder builder = new();
            builder = builder
                .WithUpdatedPlayedTime(totalPlaytime)
                .WithUpdatedDescription("Saved game at " + DateTime.Now);

            //savedImage = GetScreenshot();

            if (savedImage != null)
            {
                // This assumes that savedImage is an instance of Texture2D
                // and that you have already called a function equivalent to
                // getScreenshot() to set savedImage
                // NOTE: see sample definition of getScreenshot() method below
                byte[] pngData = savedImage.EncodeToPNG();
                builder = builder.WithUpdatedPngCoverImage(pngData);
            }
            SavedGameMetadataUpdate updatedMetadata = builder.Build();
            savedGameClient.CommitUpdate(game, updatedMetadata, savedData, OnSavedGameWritten);
        }

        public void OnSavedGameWritten(SavedGameRequestStatus status, ISavedGameMetadata game)
        {
            if (status == SavedGameRequestStatus.Success)
            {
                // handle reading or writing of saved game.
                saveSucceeded = true;
            }
            else
            {
                // handle error
            }
        }

        public Texture2D GetScreenshot()
        {

            // Create a 2D texture that is 1024x700 pixels from which the PNG will be
            // extracted
            Texture2D screenShot = new(1024, 700);

            // Takes the screenshot from top left hand corner of screen and maps to top
            // left hand corner of screenShot texture
            screenShot.ReadPixels(
                new Rect(0, 0, Screen.width, (Screen.width / 1024) * 700), 0, 0);
            return screenShot;
        }

        //Read a saved game
        void LoadGameData(ISavedGameMetadata game)
        {
            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            savedGameClient.ReadBinaryData(game, OnSavedGameDataRead);
            PlayTimeManager.Instance.LoadedPreviousTimeSpan(game.TotalTimePlayed);

        }

        public void OnSavedGameDataRead(SavedGameRequestStatus status, byte[] data)
        {
            if (status == SavedGameRequestStatus.Success)
            {
                // handle processing the byte array data
                loadData = data;
            }
            else
            {
                // handle error
            }
        }

        //Delete a saved game
        void DeleteGameData(string filename)
        {
            // Open the file to get the metadata.
            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            savedGameClient.OpenWithAutomaticConflictResolution(filename, DataSource.ReadCacheOrNetwork,
                ConflictResolutionStrategy.UseLongestPlaytime, DeleteSavedGame);
        }

        public void DeleteSavedGame(SavedGameRequestStatus status, ISavedGameMetadata game)
        {
            if (status == SavedGameRequestStatus.Success)
            {
                ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
                savedGameClient.Delete(game);
            }
            else
            {
                // handle error
            }
        }
        #endregion

        public void ShowLeaderboard()
        {
            //Debug.LogFormat("ShowLeaderboard call. IsAuthenticated:{0}", PlayGamesPlatform.Instance.IsAuthenticated(), this);
            if (!PlayGamesPlatform.Instance.IsAuthenticated())
                Authenticate(() => PlayGamesPlatform.Instance.ShowLeaderboardUI(leaderboardID));
            else
            {
                PlayGamesPlatform.Instance.ShowLeaderboardUI(leaderboardID);
                //Debug.Log("ShowLeaderboard call.", this);
            }
        }
        
        #endif
    }
}
// Portions of this page are modifications based on work created and shared by the Android Open Source Project and used according to terms described
// in the Creative Commons 2.5 Attribution License. https://developer.android.com/games/pgs/unity/overview