using UnityEngine;

namespace Enlearn.Client
{
    /*! \mainpage Unity Client
     *
     * \section Intro
     *
     * This is the documentation for the Enlearn API to get adaptive problems for users and log the results.
     * 
     * The Enlearn Unity Package requires .NET 2.0.  To create an IUnityEnlearnClient, build one from the UnityEnlearnClientBuilder.
     *
     * \section Unity Integration Steps
     * 
     * 1. Import Enlearn.Client package to Unity
     * 2. Go to Edit -> Project Settings -> Player and change the Api compatibility level from [.NET 2.0 Subset] to [.NET 2.0]
     * 3. Attach a UnityEnlearnClientBuilder to a GameObject using AddComponent.
     * 4. Call UnityEnlearnClientBuilder.CreateClient() with a unique string identifying your game.
     * 5. Wait for UnityEnlearnClientBuilder.IsClientReady() to return true.
     * 6. Call UnityEnlearnClientBuilder.GetEnlearnClient() to get the IUnityEnlearnClient.
     *
     * \section Example
     *  @code
     *  IEnumerator CreateEnlearnClient()
     *  {
     *      IUnityEnlearnClient clientBuilder = gameObject.AddComponent<UnityEnlearnClientBuilder>();
     *      clientBuilder.CreateClient("com.company.appname");
     *      while (!clientBuilder.IsClientReady())
     *      {
     *          Debug.Log("Waiting for client to be ready...");
     *          yield return new WaitForFixedUpdate();
     *      }
     *      _client = clientBuilder.GetEnlearnClient();
     *  }
     *  @endcode
     */

    /// <summary>
    /// Builds an IUnityEnlearnClient for use in Unity.  Requires .NET 2.0
    /// </summary>
    public class UnityEnlearnClientBuilder : MonoBehaviour
    {
        private IUnityEnlearnClient _enlearnClient;
        private bool _isClientReady = false;
        
        /// <summary>
        ///  Begins creating an IUnityEnlearnClient associated with this instance.
        /// </summary>
        public void CreateClient(string gameId)
        {
            var logger = new UnityLogger();
#if UNITY_ANDROID
            _enlearnClient = new JavaBackedEnlearnClient(this, gameId, logger, () => { _isClientReady = true; Debug.Log("UnityEnlearnClient is now Ready!"); });
#endif
        }

        /// <summary>
        /// Returns whether the IUnityEnlearnClient is ready to be used.  If this is true, then GetEnlearnClient() will return a valid IUnityEnlearnClient
        /// </summary>
        /// <returns>Whether the IUnityEnlearnClient is ready to be used</returns>
        public bool IsClientReady()
        {
            return _isClientReady;
        }
        
        /// <summary>
        /// Returns an IUnityEnlearnClient.  This IUnityEnlearnClient may be invalid if IsClientReady() returns false;
        /// </summary>
        /// <returns> an IUnityEnlearnClient
        /// </returns>
        public IUnityEnlearnClient GetEnlearnClient()
        {
            return _enlearnClient;
        }
    }
}