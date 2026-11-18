using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    [Header("---タイマー設定---")]
    [Tooltip("true:カウントダウン/false:カウントアップ")]
    public bool isCountDown = true;

    [Tooltip("制限時間(カウントアップで制限なしにする場合は0以下の値を設定)")]
    public float gameTime = 60.0f;

    [Header("---ステータス(確認用)---")]
    [Tooltip("タイマーが停止しているか(trueで停止)")]
    public bool isTimeOver = false;

    [Tooltip("現在の表示時間(秒)")]
    public float displayTime = 0;

    private float times = 0; // 現在時間(内部処理用)

    [Header("---UI設定---")]
    [Tooltip("Canvas内にあるテキストオブジェクトをここにドラッグ＆ドロップ")]
    [SerializeField] private TextMeshProUGUI timerText;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // カウントダウン(三項演算子)
        displayTime = isCountDown ? gameTime : 0.0f;
        //上記は以下の書き換え
        //if (isCountDown)　　　　　　        
        //{
        //    displayTime = gameTime;
        //}
        //else
        //{
        //    displayTime = 0.0f;
        //}
    }

    // Update is called once per frame
    void Update()
    {
        if (isTimeOver) return;

        times += Time.deltaTime;

        if (isCountDown)            // カウントダウン
        {
            displayTime = gameTime - times;
            if (displayTime <= 0.0f)
            {
                displayTime = 0.0f;
                isTimeOver = true;
                OnTimeOver();       //タイムオーバー時のイベント
            }
        }
        else                        // カウントアップ
        {
            displayTime = times;
            if (gameTime > 0.0f && displayTime >= gameTime)
            {
                displayTime = gameTime;
                isTimeOver = true;
                OnTimeOver();
            }
        }

        if (timerText != null)
        {
            timerText.text = $"{(int)displayTime:000}";
        }

        //${変数・式}
        //:F2 小数点2桁まで表示
        //Debug.Log($"TIMES:{displayTime:F2} ");
    }

    /// <summary>
    /// 時間切れ(or目標時間到達)になった瞬間に1度だけ実行
    /// </summary>
    private void OnTimeOver()
    {
        //Debug.Log("タイムアップ！");

        //staticではない要素の呼び出しはInstanceの記述が必要
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerOver();
        }
    }
}
