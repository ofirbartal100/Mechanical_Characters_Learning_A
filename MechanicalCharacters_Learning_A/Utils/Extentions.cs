using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MechanicalCharacters_Learning_A.Utils
{
    public static class RegionManagerExtentions
    {
        private static List<NavigationInfo> _callbacks = new List<NavigationInfo>();

        /// <summary>
        /// Navigates the given regions into the desired views, waiting for region to be initialized
        /// </summary>
        /// <param name="regionManager">the region manager</param>
        /// <param name="regionViewDictionary">dictionary containing region names as keys and view names as values</param>
        public static void InitializeNavigations(this IRegionManager regionManager,
            Dictionary<string, string> regionViewDictionary)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var pair = new NavigationInfo(regionViewDictionary.Count, GenerateCallbackWithScopeToThis(regionManager, regionViewDictionary), regionViewDictionary);
                _callbacks.Add(pair);
                regionManager.Regions.CollectionChanged += (sender, args) =>
                {
                    if (_callbacks.Any(actionPair => actionPair.TimesToRun > 0))
                    {
                        if (_callbacks.First(actionPair => actionPair.TimesToRun > 0).Regions.All(regionName => regionManager.Regions.ContainsRegionWithName(regionName)))
                        {
                            var callback = _callbacks.First(actionPair => actionPair.TimesToRun > 0);
                            callback.TimesToRun = 0;
                            callback.Callback();
                        }
                    }
                };
            });
        }

        /// <summary>
        /// Navigates the given regions into the desired views
        /// </summary>
        /// <param name="regionManager">the region manager</param>
        /// <param name="regionViewDictionary">dictionary containing region names as keys and view names as values</param>
        public static void Navigate(this IRegionManager regionManager,
            Dictionary<string, string> regionViewDictionary)
        {
            InitializeNavigations(regionManager, regionViewDictionary);
            foreach (KeyValuePair<string, string> keyValuePair in regionViewDictionary)
            {
                regionManager.RequestNavigate(keyValuePair.Key, new Uri(keyValuePair.Value, UriKind.Relative));
            }
        }

        //create the callback and remove from the subscription
        private static Action GenerateCallbackWithScopeToThis(IRegionManager regionManager, Dictionary<string, string> regionViewDictionary)
        {
            return () =>
            {
                //request navigation for all pairs
                foreach (KeyValuePair<string, string> keyValuePair in regionViewDictionary)
                {
                    regionManager.RequestNavigate(keyValuePair.Key, new Uri(keyValuePair.Value, UriKind.Relative));
                }
            };
        }

        /// <summary>
        /// encapsulates int as changable key and action as value
        /// </summary>
        internal class NavigationInfo
        {
            public NavigationInfo(int timesToRun = 0, Action callback = null, Dictionary<string, string> regionsMapping = null)
            {
                TimesToRun = timesToRun;
                Callback = callback;
                Regions = regionsMapping?.Keys.ToList();
            }

            public Action Callback { get; set; }
            public List<string> Regions { get; set; }
            public int TimesToRun { get; set; }
        }
    }
}