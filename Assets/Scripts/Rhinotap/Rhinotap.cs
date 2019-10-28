using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

using System.Collections;
using System;

/**
 * Rhinotap Gaming Studio
 * Helper Classes
 */
namespace Rhinotap
{

    /**
     * Randomly choose an array index BASED ON ITS CHANSE
     * Example: 
     * fruits = new string[3] {"Apple", "Orange", "Banana"};
     * chanse = new int[3] { 60, 30, 10 }; // %60 chanse for apple, 30 for orange 10 for banane
     * var fruitPicker = new Rhinotap.Choose( chanse ); //give an array of chanses during construct
     * Debug.Log( fruits[ fruitPicker.NextValue()] ); //Will pick a fruit based on chanse
     */
    public class Choose
    {
        public Choose(int probs)
        {
            this.prob = new List<long>();
            this.alias = new List<int>();
            this.total = 0;
            this.n = probs;
            this.even = true;
        }

        System.Random random = new System.Random();

        List<long> prob;
        List<int> alias;
        long total;
        int n;
        bool even;

        public Choose(IEnumerable<int> probs)
        {
            // Raise an error if nil
            if (probs == null) throw new System.Exception("Probabilities missing");
            this.prob = new List<long>();
            this.alias = new List<int>();
            this.total = 0;
            this.even = false;
            var small = new List<int>();
            var large = new List<int>();
            var tmpprobs = new List<long>();
            foreach (var p in probs)
            {
                tmpprobs.Add(p);
            }
            this.n = tmpprobs.Count;
            // Get the max and min choice and calculate total
            long mx = -1, mn = -1;
            foreach (var p in tmpprobs)
            {
                if (p < 0) throw new System.Exception("Probabilities contain a negative number.");
                mx = (mx < 0 || p > mx) ? p : mx;
                mn = (mn < 0 || p < mn) ? p : mn;
                this.total += p;
            }
            // We use a shortcut if all probabilities are equal
            if (mx == mn)
            {
                this.even = true;
                return;
            }
            // Clone the probabilities and scale them by
            // the number of probabilities
            for (var i = 0; i < tmpprobs.Count; i++)
            {
                tmpprobs[i] *= this.n;
                this.alias.Add(0);
                this.prob.Add(0);
            }
            // Use Michael Vose's alias method
            for (var i = 0; i < tmpprobs.Count; i++)
            {
                if (tmpprobs[i] < this.total)
                    small.Add(i); // Smaller than probability sum
                else
                    large.Add(i); // Probability sum or greater
            }
            // Calculate probabilities and aliases
            while (small.Count > 0 && large.Count > 0)
            {
                var l = small[small.Count - 1]; small.RemoveAt(small.Count - 1);
                var g = large[large.Count - 1]; large.RemoveAt(large.Count - 1);
                this.prob[l] = tmpprobs[l];
                this.alias[l] = g;
                var newprob = (tmpprobs[g] + tmpprobs[l]) - this.total;
                tmpprobs[g] = newprob;
                if (newprob < this.total)
                    small.Add(g);
                else
                    large.Add(g);
            }
            foreach (var g in large)
                this.prob[g] = this.total;
            foreach (var l in small)
                this.prob[l] = this.total;
        }

        // Returns the number of choices.
        public int Count
        {
            get
            {
                return this.n;
            }
        }
        // Chooses a choice at random, ranging from 0 to the number of choices
        // minus 1.
        public int NextValue()
        {
            var i = random.Next(this.n);
            return (this.even || random.Next((int)this.total) < this.prob[i]) ? i : this.alias[i];
        }
    }

    public class ChooseTest {
        public void start(int iterations = 10)
        {
            //TESTING
            Debug.Log("START probabilty test");
            int[] enemies = new int[] { -80, 15, 5 };
            int[] chosenEnemies = new int[] { 0, 0, 0 };

            Rhinotap.Choose chooseEnemy = new Rhinotap.Choose(enemies);
            //Choose 10 enemies
            for (int i = 0; i < iterations; i++)
            {
                int chosenIndex = chooseEnemy.NextValue();
                chosenEnemies[chosenIndex] += 1;
            }
            Debug.Log("Enemy Spawn Chanses: Enemy1: " + enemies[0] + " Enemy2: " + enemies[1] + " Enemy3: " + enemies[2]);
            Debug.Log("Choose Stats: Enemy1: " + chosenEnemies[0] + " Enemy2: " + chosenEnemies[1] + " Enemy3: " + chosenEnemies[2] + ".");
        }
    }



    public static class Tools
    {

        /**
         * Sort a keyvaluepair list by its values.
         * Used for index => value sorting
         * For example 3 enemy types with different spawn chanses, ordered by descending
         * 0: 11, 1: 66, 2: 33 //original pairs
         * 1: 66, 2: 33, 0: 11 //sorted pairs.
         */
        public static List<KeyValuePair<int, float>> SortPair(List<KeyValuePair<int, float>> pairs, bool ascending = true)
        {
            List<KeyValuePair<int, float>> result = new List<KeyValuePair<int, float>>();
            if( ascending )
                result = pairs.OrderBy(pair => pair.Value).ToList();
            else
                result = pairs.OrderByDescending(pair => pair.Value).ToList();


            return result;
        }

    }
}

