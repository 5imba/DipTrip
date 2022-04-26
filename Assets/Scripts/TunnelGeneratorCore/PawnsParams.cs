using UnityEngine;

public enum PawnType { Empty, Obstacle, Coin, Fuel }

[System.Serializable]
public class Pattern
{
    //[NamedArray(new string[] { "Empty", "Obstacle", "Coin", "Fuel" })]
    public Transform[] pawns;

    public PatternSettings.Level[] levels;



    /*
    private int currentLvl, currentPatternIndex;
    private bool initialized = false;

    private void Initialize()
    {
        currentLvl = 0;
        currentPatternIndex = Random.Range(0, levels[currentLvl].patterns.Length);
    }

    private int index;
    private int Index
    {
        get
        {
            index++;
            if (index >= GetPatternCount) index = 0;

            return index;
        }
    }
    private int CurrentPatternIndex
    {
        get
        {
            currentPatternIndex = Random.Range(0, GetPatternCount);
            return currentPatternIndex;
        }
        set
        {
            currentPatternIndex = value;
        }
    }
    private int GetPatternCount
    {
        get
        {
            return levels[currentLvl].patterns.Length;
        }
    }
    private void ShufflePattern()
    {

    }

    private int GetPawnType(int side)
    {


        return 0;
    }

    


    // Public assessors & methods
    public Transform GetPatternPoint(int index, int side)
    {
        PatternSettings.PatternPoint patternPoint = levels[currentLvl].patterns[currentPatternIndex].points[index];

        int pawnType = 0;
        switch (side)
        {
            //case 0: pawnType = (int)patternPoint.side0; break;
            //case 1: pawnType = (int)patternPoint.side1; break;
            //case 2: pawnType = (int)patternPoint.side2; break;
            //case 3: pawnType = (int)patternPoint.side3; break;
        }

        return pawns[pawnType];
    }
    public int CurrentLvl
    {
        get
        {
            return currentLvl;
        }
        set
        {
            currentLvl = value;
        }
    }

    */
}

namespace PatternSettings
{
    [System.Serializable]
    public class Level
    {
        //[NamedArray("Level")]
        public Pattern[] patterns;
    }

    [System.Serializable]
    public class Pattern
    {
        //[NamedArray("Pattern")]
        public PatternPoint[] points;
    }

    [System.Serializable]
    public class PatternPoint
    {
        public PawnType side0;
        public PawnType side1;
        public PawnType side2;
        public PawnType side3;
    }
}




