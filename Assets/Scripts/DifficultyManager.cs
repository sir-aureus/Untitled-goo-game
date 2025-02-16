using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public enum Difficulty { Easy, Medium, Hard }
    public static Difficulty CurrentDifficulty { get; private set; }

    public void SetDifficulty(int difficultyIndex)
    {
        CurrentDifficulty = (Difficulty)difficultyIndex;
    }
}
