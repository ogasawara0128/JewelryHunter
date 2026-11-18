using UnityEngine;
using UnityEngine.UI;               // UIを使うのに必要
using UnityEngine.SceneManagement;
using TMPro;

public enum GameState //ゲームの状態
{
    //初期状態、ゲーム中、ゲームクリア、ゲームオーバー、ゲーム終了
    Ready, InGame, GameClear, GameOver, GameEnd,
}

public class GameManager : MonoBehaviour
{
    //{ get; private set; }:読み取り専用のガード
    //                     (他スクリプトからget(読み取り)はOK、private set(書き換え)はﾀﾞﾒ)
    //Instanceをstaticにしている。仮にメソッドをstaticにするとその中からUIや画像を操作できなくなる為。
    //staticにアクセスするする際はinstanceの記述不要。staticそのものにはInstanceの記述が必要
    public static GameManager Instance { get; private set; } //★★

    [Header("---ゲーム状態---")]
    [Tooltip("現在のゲーム全体の進行状態(Ready/InGame/GameClear/GameOver/GameEnd)")]
    public static GameState gameState = GameState.Ready; //(static変数)Readyは安全策

    [Header("---UI Elements---")]
    [Tooltip("結果画面などで表示する大元のゲームオブジェクト")]
    public GameObject mainImage;
    [Tooltip("ゲームオーバー時に表示するスプライト画像")]
    public Sprite gameOverSpr;
    [Tooltip("ゲームクリア時に表示するスプライト画像")]
    public Sprite gameClearSpr;
    [Tooltip("ボタン類を載せている背景パネルオブジェクト")]
    public GameObject panel;
    public GameObject restartButton; //RESTARTボタン
    public GameObject nextButton; //NEXTボタン

    [Header("ステージ移行設定")]
    [Tooltip("NEXTボタンを押した時に読み込むシーン名")]
    public string nextSceneName;

    [Header("プレイヤー生成")]
    [Tooltip("生成するプレイヤーのプレハブ")]
    public GameObject playerPrefab;
    [Tooltip("プレイヤーが最初に出現する位置のTransform")]
    public Transform spawnPoint;

    [Header("---Camera---")]
    [Tooltip("ステージ内を映しているメインのCameraController")]
    public CameraController cameraController;

    [Header("---Timer UI---(一括管理用)")]
    [Tooltip("タイマーの見た目を表すバーのゲームオブジェクト")]
    public GameObject timeBar;
    [Tooltip("タイマーの数字を表すテキストのゲームオブジェクト(不要な時に消す用)")]
    public GameObject timeText;

    [Header("---Timer References---")]
    [Tooltip("時間を計算しているTimeControllerスクリプト(オブジェクトをアタッチ)")]
    [SerializeField] private TimeManager timeCnt;

    public GameObject scoreText; //スコアテキスト
    public static int totalScore; //合計スコア
    public int stageScore = 0; //ステージスコア

    [SerializeField] private AudioManager audioManager;

    // ★【追加】重い大捜索を完全に無くすため、プレイヤーの参照を保存しておく箱
    private PlayerController activePlayerCnt;

    private void Awake()
    {
        //シングルトン設計:世界に1つだけのGameManagerを確定させるガード処理
        if (Instance == null) { Instance = this; } //もしInstance(管理者の席)が空なら、自分が管理者になる
        else { Destroy(gameObject); } 　　　　     //既に管理者がいるなら、自分自身を消滅
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameState = GameState.InGame;  //ゲーム中にする(リトライ用)
        Invoke("InactiveImage", 1.0f); //1秒後に画像を非表示に
        panel.SetActive(false);        //パネルを非表示に

        SpawnPlayer(); //ゲームが始まったらプレイヤーを生成してカメラを設定

        //時間制限追加
        if (timeCnt != null)
        {
            if (!timeCnt.isCountDown && timeCnt.gameTime <= 0.0f) //カウントアップ(!isCountDown)かつ制限時間なしの場合
            {
                //バーもテキストもまとめて非表示に
                if (timeBar != null) timeBar.SetActive(false);
                if (timeText != null) timeText.SetActive(false);
            }
        }
        //スコア追加
        UpdateScore();
    }

    void SpawnPlayer()
    {
        //メソッド直下に記述することで、以下メソッドにおける全てのif内で使用可能
        Transform playerTransform = null; //初期化(変数を用意)

        if (playerPrefab != null && spawnPoint != null) //spawn設定ONのとき
        {
            //プレイヤーをスポーンポイントの位置・角度で生成 + 次行の為にspawnedPlayerに代入
            GameObject spawnedPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            //生成したプレイヤーのTransform(位置情報)を取得
            playerTransform = spawnedPlayer.transform; //最終行でCameraが参照できるよう代入

            // ★【追加】自動生成された瞬間に、直接プレイヤーのコンポーネントを記憶する
            activePlayerCnt = spawnedPlayer.GetComponent<PlayerController>();
        }
        else //spawn設定OFFのとき（最初から配置されているステージ）
        {
            Debug.Log("配置済みのプレイヤーからの自動登録を待機します。");
            return; // プレイヤー自身からの登録を待つので、ここは探さずに終了
        }

        //SpawnONのときのみターゲットに設定
        if (cameraController != null && playerTransform != null)
        {
            cameraController.SetTarget(playerTransform);
        }
    }

    // ★【追加】最初から配置されているプレイヤーが、Start時に自分自身をここに登録するための窓口
    public void RegisterPlayer(PlayerController player)
    {
        activePlayerCnt = player;
        if (cameraController != null)
        {
            cameraController.SetTarget(player.transform);
        }
        Debug.Log("GameManagerに配置済みプレイヤーが直接登録されました。");
    }

    public void OnPlayerClear() //★★自作メソッド Onは～の時にという意味で使う慣習
    {
        if (gameState == GameState.GameClear || gameState == GameState.GameEnd) return; //AudioManagerで2回鳴るor鳴らないのを防ぐ
        gameState = GameState.GameClear;

        if (audioManager != null)
        {
            audioManager.ClearSound();
        }
    }
    public void OnPlayerOver() //★★自作メソッド
    {
        if (gameState == GameState.GameOver || gameState == GameState.GameEnd) return; //Audio用 念の為の記述
        if (gameState == GameState.InGame)
        {
            gameState = GameState.GameOver;

            if (audioManager != null)
            {
                audioManager.OverSound();
            }

            // ★【修正】大捜索（FindGameObjectWithTag）を削除し、記憶しているプレイヤーに直接命令を出す
            if (activePlayerCnt != null)
            {
                activePlayerCnt.Over();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (gameState == GameState.GameClear)
        {
            //ゲームクリア
            mainImage.SetActive(true); //画像を表示
            panel.SetActive(true); //ボタン(パネル)を表示

            //RESTARTボタンを無効化
            if (restartButton != null && restartButton.TryGetComponent<Button>(out var bt))
            {
                bt.interactable = false;
            }

            if (mainImage != null && mainImage.TryGetComponent<Image>(out var img))
            {
                img.sprite = gameClearSpr;
            }

            gameState = GameState.GameEnd;

            if (timeCnt != null)
            {
                timeCnt.isTimeOver = true;

                //スコア追加
                int time = (int)timeCnt.displayTime;
                totalScore += time * 10;
            }
            //スコア追加
            totalScore += stageScore;
            stageScore = 0;
            UpdateScore();
        }
        else if (gameState == GameState.GameOver)
        {
            //ゲームオーバー
            mainImage.SetActive(true); //画像を表示する
            panel.SetActive(true); //ボタン(パネル)を表示する

            //NEXTボタンを無効化する
            if (nextButton != null && nextButton.TryGetComponent<Button>(out Button bt))
            {
                bt.interactable = false;
            }

            if (mainImage != null && mainImage.TryGetComponent<Image>(out Image img))
            {
                img.sprite = gameOverSpr;
            }

            gameState = GameState.GameEnd;

            if (timeCnt != null)
            {
                timeCnt.isTimeOver = true;
            }
        }
        else if (gameState == GameState.InGame)
        {
            // ❌ 起動を激重にしていた毎フレームの大捜索（FindGameObjectWithTag）は完全削除！
            // 💡 代わりに、登録されているプレイヤー（activePlayerCnt）を直接使用する
            if (activePlayerCnt != null)
            {
                if (activePlayerCnt.score != 0)
                {
                    stageScore += activePlayerCnt.score;
                    activePlayerCnt.score = 0;
                    UpdateScore();
                }
            }
        }
    }

    //画像を非表示に
    void InactiveImage()
    {
        if (mainImage != null) mainImage.SetActive(false);
    }

    //スコア更新
    void UpdateScore()
    {
        int score = stageScore + totalScore;
        if (scoreText != null && scoreText.TryGetComponent<TextMeshProUGUI>(out var textMesh))
        {
            textMesh.text = score.ToString();
        }
    }

    //リスタート
    public void Restart() { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    //次へ
    public void Next() { SceneManager.LoadScene(nextSceneName); }

    public void GameEnd()
    {
        if (nextButton != null && nextButton.TryGetComponent<Button>(out var nextBt))
        {
            // NEXTボタンが有効（＝ゲームクリア画面のとき）ならNextシーンへ
            if (nextBt.interactable)
            {
                Next();
            }
            // NEXTボタンが無効（＝ゲームオーバー画面のとき）ならリトライ
            else
            {
                Restart();
            }
        }
    }
}