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
            if (ascending)
                result = pairs.OrderBy(pair => pair.Value).ToList();
            else
                result = pairs.OrderByDescending(pair => pair.Value).ToList();


            return result;
        }

        public static void testSort()
        {

            List<KeyValuePair<int, float>> test = new List<KeyValuePair<int, float>>();
            test.Add(new KeyValuePair<int, float>(0, 0.55f));
            test.Add(new KeyValuePair<int, float>(1, 0.42f));
            test.Add(new KeyValuePair<int, float>(2, 97.4f));

            //test = Rhinotap.Tools.SortPair(test, false);
            test = SortPair(test, false);

            foreach (KeyValuePair<int, float> tost in test)
            {
                Debug.Log(tost.Key + " -> " + tost.Value.ToString("n2"));
            }
        }

    }

    /**Calculate spawn chance for a list of enemies
     */
    public class SpawnChance
    {
        private float difficulty = 1f; //Difficulty of the level. 1-100

        private float[] enemyDifficulties; //Set enemy array here

        private int count = 0; //Array size

        private float highestDifficulty = 10000f; // a big number to divide

        

        private float[] result;
        


        private float ignoreBelow = 0f; //Ignore probabilities below this

        private int variety = 9999; //How many types of enemy supported

        public int Variety
        {
            get { return variety; }
            set
            {
                if( Variety > 0 && Variety <= 9999)
                {
                    
                    variety = value;
                } else
                {
                    
                    variety = 9999;
                }
            }
        }
        

        //Constructor
        public SpawnChance(float LevelDifficulty, float[] enemyDifficultiesArray)
        {
            Initialize(LevelDifficulty, enemyDifficultiesArray);
        }

        public SpawnChance(float LevelDifficulty, float[] enemyDifficultiesArray, float ignoreBelowPercentage = 0f)
        {
            Initialize(LevelDifficulty, enemyDifficultiesArray);
            if( ignoreBelowPercentage > 0f)
            {
                ignoreBelow = ignoreBelowPercentage;
            }
        }



        private void Initialize(float LevelDifficulty, float[] enemyDifficultiesArray)
        {
            if (LevelDifficulty < 0) { LevelDifficulty = 1f; }
            else if (LevelDifficulty == 0) { LevelDifficulty = 1f; }

            difficulty = LevelDifficulty;

            //Parse data

            if (enemyDifficultiesArray.Length > 0)
            {
                enemyDifficulties = enemyDifficultiesArray;
            }
            else
            {
                throw new System.Exception("Could not calculate SpawnChance. No enemy provided");
            }

            for (int i = 0; i < enemyDifficulties.Length; i++)
            {
                if (enemyDifficulties[i] <= 0f)
                {
                    enemyDifficulties[i] = 0.1f; //to overcome division zero
                }
                if (enemyDifficulties[i] > highestDifficulty)
                {
                    enemyDifficulties[i] = highestDifficulty;
                }
            }
            
            //Save count
            count = enemyDifficulties.Length;

            //Create result set
            result = new float[count];
        }

        private void Calculate()
        {
            //Calculate each enemies difficulty difference to level difficulty
            //The smaller the difference, the higer the spawn chance. 
            //Which means, we must convert small number to big number
            //To do this, we divide a big number by the delta
            //For example, difficulty differences are 0.1 0.2 and 0.3
            //We divide 1000 by these: 10,000 5,000 3,333.333 -> smaller is now bigger
            float[] difDelta = new float[count];
            float[] probability = new float[count];
            for(int i = 0; i < count; i++)
            {
                //Find difficulty difference (Smallest number will have highest spawn chance)
                difDelta[i] = Mathf.Abs(difficulty - enemyDifficulties[i]);
                if( difDelta[i] == 0f)
                {
                    difDelta[i] = 0.1f; //Stop division zero
                }
                //Convert small number to high number
                probability[i] = highestDifficulty / difDelta[i];
            }//EOL

            //Convert to percentage
            result = CalculatePercentage(probability);

            //Apply ignorance
            ApplyIgnore();



            //Apply variety
            ApplyVariety();

            //Re calc percentage
            result = CalculatePercentage(result);
        }

        private void ApplyIgnore()
        {
            int ignored = 0;
            float max = result.Max();
            int maxIndex = Array.IndexOf(result, max);

            if( result.Length > 0 && ignoreBelow > 0f)
            {
               for(int i = 0; i < count; i++)
                {
                    if( result[i] <= ignoreBelow)
                    {
                        result[i] = 0f;
                        ignored++;//Keep track of ignores
                    }
                }
            }
            
            if( ignored == count)
            {
                //Ignored all enemies. Add the max value back
                result[maxIndex] = max;
            }

        }
        

        private void ApplyVariety()
        {
            if( variety <= 0) { return; }

            if( variety >= count) {  return; }
            

            //Create new array for results
            float[] resultWithVariety = new float[count];
            //Debug.Log("Variety applied: " + variety.ToString());


            //Create list
            List<KeyValuePair<int, float>> finalResults = new List<KeyValuePair<int, float>>();
            
            //Save the spawn chances in list
            for(int i = 0; i < count; i++)
            {
                finalResults.Add(new KeyValuePair<int, float>(i, result[i]));
            }

            //Sort the list in Big->small order. So we can remove from the end
            finalResults = Tools.SortPair(finalResults, false);

            //Loop the list, if index reaches variety limit, Set the chance to 0
            for (int i = 0; i < finalResults.Count; i++)
            {
                //Debug.Log("SORT " + finalResults[i].Key + " chance: " + finalResults[i].Value.ToString("n2"));
                //Set spawn chance to zero to the elements after variety limit
                if (i >= variety)
                {
                    //Debug.Log("Enemy Variety is reached, Removing Enemy Index of " + finalizedSpawnChances[i].Key.ToString());
                    finalResults[i] = new KeyValuePair<int, float>(finalResults[i].Key, 0f);
                }
            }
            if( finalResults.Count == 0) { return; }

            
            //List is ready for returning
            for(int i = 0; i < finalResults.Count; i++)
            {
                resultWithVariety[finalResults[i].Key] = finalResults[i].Value;
            }


            //Calculate percentage
            //resultWithVariety = CalculatePercentage(resultWithVariety);

            result = resultWithVariety;
        }


        private void Refine()
        {
            result = CalculatePercentage(result);
            //Calculate how high the level is
            int minIndex = Array.IndexOf(result, result.Min());
            int maxIndex = Array.IndexOf(result, result.Min());
            float scale = result[maxIndex] / result[minIndex];
            if ( scale > 5f)
            {
                for(int i = 0; i < count; i++)
                {
                    if( result[i] < result[maxIndex])
                    {
                        result[i] *= 0.5f;
                    }
                }

            }

        }

        private float[] CalculatePercentage(float[] source)
        {
            if(source.Length == 0) { return new float[] { 100f }; }
            float sum = source.Sum();
            //Calculate percentage
            for (int i = 0; i < source.Length; i++)
            {
                //Convert to %
                source[i] = (source[i] / sum) * 100f;
            }
            return source;
        }


        //Return result int form
        public int[] Result()
        {
            int[] resultInteger = new int[count];

            Calculate();
            Refine();
            result = CalculatePercentage(result);
            
            for(int i = 0; i < count; i++)
            {
                resultInteger[i] = Mathf.RoundToInt(result[i]);
            }
            int sum = resultInteger.Sum();
            int indexMax = Array.IndexOf(resultInteger, resultInteger.Max());
            int min = resultInteger.Where(f => f > 0).Min();
            int indexMin = Array.IndexOf(resultInteger, min);
            if ( sum != 100)
            {
                int delta = 100 - sum;
                if( delta < 0 )
                {
                    //Delta < 0 means, sum was > 100
                    resultInteger[indexMin] -= Mathf.Abs(delta); //Reduce min spawn chance by delta
                } else if(delta > 0)
                {
                    resultInteger[indexMax] += Mathf.Abs(delta); //Increase max spawn chance by delta
                }
            }



            return resultInteger;
            

        }

        public void Report(int[] source)
        {
            string Message = "";
            string dateString = DateTime.Today.ToShortDateString();

            Message += "=====================| Spawn Chance Calculator |=====================\n";
            Message += dateString + "\n";
            Message += "Total objects: " + count.ToString() + "\n";
            Message += "Ignorence: %" + ignoreBelow.ToString("n2") + "\n";
            Message += "Variety: " + variety.ToString() + "\n";
            Message += "Difficulty: " + difficulty.ToString() + "\n";
            Message += "Spawn Chances: \n";

            
            for(int i= 0; i < count; i++)
            {
                Message += "Object [" + i.ToString() + "] Difficulty: " + enemyDifficulties[i].ToString("n2") + " ==> %" + source[i].ToString() + "\n";
            }
            Message += "============================================";

            Debug.Log(Message);
        }

    }


}



/* SPAWN CHANCE TEST
        float[] testenemy = new float[3] { 1f, 5f, 20f };
        float testdif = 20f;
        float ignore = 0f;
        int testvariety = 1;
        Rhinotap.SpawnChance newChance = new Rhinotap.SpawnChance(testdif, testenemy,ignore);
        newChance.Variety = testvariety;
        
        int[] intResult = newChance.Result();
        newChance.Report(intResult);

        Debug.Log("====TEST==| Dif: " + testdif.ToString("n2") + " ignore: " + ignore.ToString("n2") + " variety: " + testvariety);
        for(int i = 0; i < intResult.Length; i++)
        {
            Debug.Log(intResult[i]);
            Debug.Log("Enemy " + i.ToString() + "=> Dif: " + testenemy[i] + " Chance: " + intResult[i]);
        }
        */
