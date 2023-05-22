using System;
using System.Collections.Generic;

namespace HandyVR.Utility
{
    public static class Collections
    {
        public static bool Best<T>(IEnumerable<T> list, out T best, Func<T, float> getScore, float startingScore = 0.0f)
        {
            best = default;
            var result = false;
            var bestScore = startingScore;
            foreach (var element in list)
            {
                var score = getScore(element);
                if (score < bestScore) continue;

                best = element;
                bestScore = score;
                result = true;
            }

            return result;
        }
        
        public static T Best<T>(IEnumerable<T> list, Func<T, float> getScore, float startingScore = 0.0f) where T : class
        {
            T best = null;
            var bestScore = startingScore;
            foreach (var element in list)
            {
                var score = getScore(element);
                if (score < bestScore) continue;

                best = element;
                bestScore = score;
            }

            return best;
        }
    }
}