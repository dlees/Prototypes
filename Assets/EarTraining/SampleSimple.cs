using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using MySpace;
using MySpace.Synthesizer.PM8;

public class SampleSimple : MonoBehaviour {
    private MySpace.MySyntheStation station;

    public RangeValue instrumentNumber;
    public RangeValue tempo;

    ToneParam pianoParam = new ToneParam("2,7, 28, 4, 0, 5, 1,37, 2, 1, 3, 0 22, 9, 1, 2, 1,47, 2,12, 0, 0 29, 4, 3, 6, 1,37, 1, 3, 7, 0 18, 8, 0, 6, 6, 0, 2, 1, 0, 0");
    ToneParam hornParam = new ToneParam("5,7, 15,10, 0, 6, 5,35, 0, 1, 0, 0 15, 5, 0, 8, 2, 6, 0, 2, 2, 0 15, 5, 0, 8, 2, 6, 0, 1, 5, 0 15, 5, 0, 8, 2, 6, 0, 1, 0, 0");

    private int instrument { get; set; }

    public RangeValue score;
    public RangeValue streak;

    public GameObject intervalPercentageContent;
    public GameObject intervalPercentageListItemPrefab;

    public ListHolder possibleIntervals;

    public Dictionary<int, Interval> intervals = new Dictionary<int,Interval>();

    public Text helpText;
    public Text playIntervalButtonText;
    public GameObject helpSongButton;


    public int level = 1;

    void OnEnable() {
            OnReady();
        }

    void Start() {
        OnReady();
    }
    
    private Dictionary<int, SongClip> songsForIntervals;

    void OnReady() {
        station = FindObjectOfType<MySpace.MySyntheStation>();

        station.Instrument[instrument].Channel[0].ProgramChange(hornParam);

        int neededForNextLevel = 2 * level * level;
        toNextLevelText.text = "Needed for next level: " + neededForNextLevel.ToString();

        songsForIntervals = new Dictionary<int, SongClip> 
        {        
            { 1, whiteChristmas }, 
            { 2, ff7 },
            { 3, greensleeves },
            { 4, vivaldi },
            { 5, harry  },
            { 6, simpsons },
            { 7, starWars },
            { 8, conquest },
            { 9, bonnie },
            { 10, starTrek },
            { 11, willyWonka },
            { 12, overTheRainbow}
        };

        for (int interval = 1  ; interval <= 12 ; interval++) {
            intervals[interval] = new Interval(interval, "mm", songsForIntervals[interval]);
        }


        foreach (int interval in possibleIntervals.list) {
            generateIntervalPercentage(interval);
        }

        helpSongButton.SetActive(false);
    }

    private void generateIntervalPercentage(int interval) {

        GameObject newListItem = Instantiate(intervalPercentageListItemPrefab) as GameObject;

        IntervalPercentageController listItemController = newListItem.GetComponent<IntervalPercentageController>();
        listItemController.interval = intervals[interval];

        newListItem.transform.SetParent(intervalPercentageContent.transform);
        newListItem.transform.localScale = Vector3.one;
    }

    void OnDisable() {
        station = null;
    }

    void OnGUI() {/*
        if (GUI.Button(new Rect(220, 30, 100, 20), "chNo Up")) {
            instrumentNumber.current++;
        }
        if (GUI.Button(new Rect(220, 60, 100, 20), "chNo Down")) {
            instrumentNumber.current--;
        } 
        if (GUI.Button(new Rect(330, 30, 100, 20), "tempo Down")) {
            tempo.current += .1f;
        }
        if (GUI.Button(new Rect(330, 60, 100, 20), "tempo Up")) {
            tempo.current -= .1f;
        }*/
    }


    private int actualDiff;
    public Text answerText;
    private int firstNote;

    private int challengesCreated = 0;

    public void createNextChallenge() {
        if (hasGuessed || challengesCreated == 0) {
            actualDiff = possibleIntervals.list[Random.Range(0, possibleIntervals.list.Count)];
            answerText.text = actualDiff.ToString();
            firstNote = Random.Range(60, 90);
            instrument = (int)instrumentNumber.current;

            helpSongButton.SetActive(false);
            hasGuessed = false;
            playIntervalButtonText.text = "Replay Interval";

            if (challengesCreated > 0) {
                helpText.text = "Can you guess this interval! Think of the songs you've associated with the intervals to help." ;
                helpText.color = Color.black;
            }

            challengesCreated++;
            spawnEnemy();
        }
        playSongInBackground(firstNote, new List<int>{ 0, actualDiff}, new List<int>{2,2});
    }

    public void repeat() {
        playSongInBackground(firstNote, new List<int> { 0, actualDiff }, new List<int>{2,2});
    }

    private bool hasGuessed = false;
    public Text toNextLevelText;
    // minor2 = 1, major2 = 2, etc
    public void makeGuess(int guessDiff) {
        if (hasGuessed) {
            return;
        }
        
        hasGuessed = true;
        
        try {
            intervals[actualDiff].addAttempt(guessDiff);

            string helpTextTemplate = "The answer was: " + actualDiff + ". To help you identify the interval next time, " +
                "remember it's the first two notes in the song '" + songsForIntervals[actualDiff].name + "'";

            if (guessDiff == actualDiff) {
                score.current += streak.current;
                Debug.Log("Correct!");
                putEnemyOnStaff();
                helpText.text = "Correct! " + helpTextTemplate;
                helpText.color = Color.blue;
                

            } else {
                currentEnemy.defeat(positionToAttack.position);
                Debug.Log("Incorrect!");
                helpText.text = "Incorrect! " + helpTextTemplate; 
                helpText.color = Color.red;
                destroyEnemiesOnStaff();
            }

            helpSongButton.SetActive(true);
            playHelpSong(actualDiff);
            playIntervalButtonText.text = "Next Interval";

            int neededForNextLevel = 2 * level * level;
            toNextLevelText.text = "Needed for next level: " + neededForNextLevel.ToString();

            if (score.current >= neededForNextLevel) {
                goToNextLevel();
            }

        } catch (KeyNotFoundException e) {
            Debug.Log(e.Message + " " + "Interval: " + actualDiff);
        }
    }


    private Queue<int> intervalLevels = new Queue<int>(new[]{1, 11, 9, 5, 4, 12, 2, 8, 10, 6});
    private void goToNextLevel() {
        int nextLevelInterval = intervalLevels.Dequeue();
        possibleIntervals.list.Add(nextLevelInterval);
        generateIntervalPercentage(nextLevelInterval);
        level++;
    }
    
    SongClip whiteChristmas = new SongClip("White Christmas",
        new List<int> { 0, 1, -1, -1, 1, 1, 1, 1 },
       new List<int> { 1, 2, 2, 2, 2, 1, 2, 2 });
    SongClip ff7 = new SongClip("Final Fantasy VII Main Theme",
        new List<int> { 0, 2, 2, 7, -2 },
       new List<int> { 1, 2, 2, 1, 2 });
    SongClip greensleeves = new SongClip("Greensleeves",
        new List<int> { 0, 3, 2, 2, 1, -1, -2, -3, -4 },
       new List<int> { 4, 2, 4, 4, 4, 4, 2, 4, 4 });
    SongClip vivaldi = new SongClip("Vivaldi",
        new List<int> { 0, 4, 0, 0, -2, -2, 7 },
       new List<int> { 3, 3, 3, 3, 6, 6, 2 });
    SongClip harry = new SongClip("Harry Potter Theme",
        new List<int> { 0, 5, 3, -1, -2, 7, -2, -3 },
       new List<int> { 4, 2, 6, 5, 2, 2, 1, 1 });
    SongClip simpsons = new SongClip("The Simpsons",
        new List<int> { 0, 6, 1 },
       new List<int> { 2, 2, 2 });
    SongClip starWars = new SongClip("Star Wars",
        new List<int> { 0, 7, -2, -1, -2, 10, -5 },
       new List<int> { 2, 2, 4, 4, 4, 2, 2 });
    SongClip conquest = new SongClip("1492: Vangelis - Conquest of Paradise",
        new List<int> { 0, 8, -3, -1, 1, 2, -3, -4 },
       new List<int> { 4, 2, 2, 3, 4, 3, 4, 2 });
    SongClip bonnie = new SongClip("My Bonnie Lies over the Ocean",
        new List<int> { 0, 9, -2, -2, 2, -2, -3, -2, -3},
       new List<int> { 2, 1, 4, 2, 2, 2, 2, 2, 2, 2 });
    SongClip starTrek = new SongClip("Star Trek",
        new List<int> { 0,10,-1,-2,-2,-2},
       new List<int> { 1,1,2,2,2,1});
    SongClip willyWonka = new SongClip("Willy Wonka Imagination",
        new List<int> { 0, 11, 1, -1, 1, -1, 1, -1, -4 },
       new List<int> { 2,1,4,4,4,4,4,4,2});
    SongClip overTheRainbow = new SongClip("Over The Rainbow",
        new List<int> { 0, 12, -1, -4, 2, 2, 1 },
       new List<int> { 1, 1, 4, 4, 4, 2, 2 });

    public void playHelpSongButton() {
        playHelpSong(actualDiff);
    }

    private void playHelpSong(int helpSong) {
        playSongInBackground(firstNote, songsForIntervals[helpSong].intervals, songsForIntervals[helpSong].timings);
    }

    Coroutine currentPlayingSong = null;

    public void playSongInBackground(int baseNote, List<int> intervals, List<int> timings) {
        if (currentPlayingSong != null) {
            StopCoroutine(currentPlayingSong);
        }

        currentPlayingSong = StartCoroutine(playSong(baseNote, intervals, timings));
    }
    
    public IEnumerator playSong(int baseNote, List<int> intervals, List<int> timings) {
        station.Instrument[instrument].AllSoundOff(0);
        int curNote = baseNote;

        for (int i = 0; i < intervals.Count ; i++) {
            curNote += intervals[i];
            station.Instrument[instrument].Channel[0].NoteOn((byte)curNote, 127);

            yield return new WaitForSeconds(.5f * 2.0f * tempo.current / timings[i]);
            station.Instrument[instrument].AllSoundOff(0);
        }
    }


    // Enemy Controller

    public GameObject enemyPrefab;
    private Enemy currentEnemy;
    public Transform positionToAttack;
    public Transform positionToDance;
    public Transform positionToLeaveStaff;
    public List<Transform> positionsOnStaff;
    private int curStaffPos = 0;

    private Stack<List<Enemy>> enemiesOnStaff = new Stack<List<Enemy>>();

    public void spawnEnemy() {
        GameObject newEnemy = (GameObject)Instantiate(enemyPrefab);
        currentEnemy = newEnemy.GetComponent<Enemy>();
        currentEnemy.dancingStartPos = positionToDance;
    }

    public GameObject[] multipliers;

    public void putEnemyOnStaff() {
        currentEnemy.goToStaff(positionsOnStaff[curStaffPos++ % positionsOnStaff.Count].position);

        if (enemiesOnStaff.Count == 0 || enemiesOnStaff.Peek().Count == 4) {
            enemiesOnStaff.Push(new List<Enemy>(4));
        }

        enemiesOnStaff.Peek().Add(currentEnemy);

        if (enemiesOnStaff.Peek().Count == 4) {
            streak.current++;
            GameObject multiplier = multipliers[(int)streak.current - 2];
            multiplier.GetComponent<MetronomeController>().enabled = true;
            multiplier.GetComponent<Text>().color = Color.blue;
        }
    }

    public void destroyEnemiesOnStaff() {
        if (enemiesOnStaff.Count == 0) {
            return;
        }

        if (enemiesOnStaff.Peek().Count == 0 || enemiesOnStaff.Peek().Count == 4) {
            GameObject multiplier = multipliers[(int)streak.current - 2];
            multiplier.GetComponent<MetronomeController>().enabled = false; 
            multiplier.GetComponent<Text>().color = Color.black;

            streak.current--;
        }

        curStaffPos = (int)streak.current * 4 - 4;
        StartCoroutine(destroyEnemiesOnStaffInBackground());
    }
    public IEnumerator destroyEnemiesOnStaffInBackground() {

        List<Enemy> enemies = enemiesOnStaff.Pop();
        
        foreach (Enemy enemy in enemies ) {
            enemy.defeat(positionToLeaveStaff.position);
            yield return new WaitForSeconds(.1f);
        }
    }

}