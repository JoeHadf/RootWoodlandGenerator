using System.Collections.Generic;
using UnityEngine;

namespace Extensions
{
    public static class ListExtensions
    {
        public static void Shuffle<T>(this List<T> listToShuffle)
        {
            for (int i = 0; i < listToShuffle.Count; i++)
            {
                int nextElementIndex = Random.Range(0, listToShuffle.Count - i);

                T nextElement = listToShuffle[nextElementIndex];
                T swappedElement = listToShuffle[^(i + 1)];

                listToShuffle[^(i + 1)] = nextElement;
                listToShuffle[nextElementIndex] = swappedElement;
            }
        }
    }
}

