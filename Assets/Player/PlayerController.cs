using System.Runtime.Serialization.Formatters;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rbody;  //Rigidbody2D型の変数
    float axisH = 0.0f; //入力
    [SerializeField] private float speed = 3.0f; //移動速度
    [SerializeField] private float jump = 9.0f; //ジャンプ力
    [SerializeField] private LayerMask groundLayer; //着地できるレイヤー
    bool goJump = false; //ジャンプ開始フラグ
    bool onGround = false; //地面フラグ

    //アニメーション対応
    Animator animator; //アニメーター
    [SerializeField] private string stopAnime = "PlayerStop";
    [SerializeField] private string moveAnime = "PlayerMove";
    [SerializeField] private string jumpAnime = "PlayerJump";
    [SerializeField] private string clearAnime = "PlayerClear";
    [SerializeField] private string deadAnime = "PlayerOver";
    private string nowAnime = ""; //基本的なデータ(string,int,bool等)は初期化が安全
    private string oldAnime = "";

    public int score = 0; //スコア

    // ★【大本命の裏ワザ】Unityの自動メッセージを使わず、コードで直接入力をコントロールする
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction submitAction;

    void Start()
    {
        rbody = this.GetComponent<Rigidbody2D>(); //Rigidbody2Dを取ってくる
        animator = GetComponent<Animator>(); //Animatorを取ってくる
        nowAnime = stopAnime; //停止から開始
        oldAnime = stopAnime; //停止から開始

        // ★最初から配置されているステージの場合、自分からGMのカメラに登録する
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.playerPrefab == null)
            {
                GameManager.Instance.RegisterPlayer(this);
            }
        }

        // ★【大修正】消えてしまっていたアクションの紐付け・有効化処理を完全補完！
        PlayerInput input = GetComponent<PlayerInput>();
        if (input != null)
        {
            // 💡アセットから各アクションを直接引っ張ってくる
            moveAction = input.actions.FindAction("Move");
            jumpAction = input.actions.FindAction("Jump");
            submitAction = input.actions.FindAction("Submit");
            if (submitAction == null) submitAction = input.actions.FindAction("submit");

            // 💡ゲーム開始時は、移動とジャンプだけをON（Enable）にして、決定キーはOFFにしておく
            if (moveAction != null) moveAction.Enable();
            if (jumpAction != null) jumpAction.Enable();
            if (submitAction != null) submitAction.Disable();
        }
    }

    void Update()
    {
        //クラス名.変数名という書き方はstaticだから許される
        if (GameManager.gameState != GameState.InGame)
        {
            return;
        }

        //地上判定
        //発射位置origin、円の半径radius、発射方向direction、発射距離distance、検出するレイヤーlayerMask
        onGround = Physics2D.CircleCast(transform.position, 0.2f, Vector2.down, 0.0f, groundLayer);

        // ★【修正】自動メッセージ（OnMove/OnJump）を完全にやめ、この軽量なUpdate内で入力を感知する
        if (moveAction != null)
        {
            axisH = moveAction.ReadValue<Vector2>().x;
        }

        if (jumpAction != null && jumpAction.WasPressedThisFrame())
        {
            goJump = true;
        }

        if (axisH > 0.0f) //向きの調整
        {
            transform.localScale = new Vector2(1, 1); //右移動
        }
        else if (axisH < 0.0f)
        {
            transform.localScale = new Vector2(-1, 1); //左右反転させる
        }

        //アニメーション更新
        if (onGround) //地面の上
        {
            if (axisH == 0)
            {
                nowAnime = stopAnime; //停止中
            }
            else
            {
                nowAnime = moveAnime; //移動
            }
        }
        else //空中
        {
            nowAnime = jumpAnime;
        }
        if (nowAnime != oldAnime)
        {
            oldAnime = nowAnime;
            animator.Play(nowAnime); //アニメーション再生
        }
    }

    void FixedUpdate()
    {
        if (GameManager.gameState != GameState.InGame) return;

        if (onGround || axisH != 0) //地面の上or速度が0ではない
        {
            //速度を更新する
            rbody.linearVelocity = new Vector2(axisH * speed, rbody.linearVelocity.y);
        }
        if (onGround && goJump) //地面の上でジャンプキーが押された
        {
            //ジャンプさせる(方向、力の種類)
            rbody.AddForce(new Vector2(0, jump), ForceMode2D.Impulse);
            goJump = false; //ジャンプフラグを下ろす
        }
    }

    //接触開始
    void OnTriggerEnter2D(Collider2D collision)
    {
        //プレイ中以外は多重ヒットを防ぐため無視
        if (GameManager.gameState != GameState.InGame) return;
        if (collision.gameObject.tag == "Goal")
        {
            Clear();  //クリアメソッド発動
        }
        else if (collision.gameObject.tag == "Dead")
        {
            Over(); //オーバーメソッド発動
        }
    }

    public void AddScore(ItemData data)
    {
        score += data.value;
    }

    //クリアメソッド
    private void Clear()
    {
        animator.Play(clearAnime);
        GameStop();
        //★★Instance:シングルトンパターン。GameManagerのようなクラスは事前準備なしに呼び出せる
        //GameManagerのインスタンスにアクセスしOnPlayerClearメソッドを呼び出し
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerClear();
        }
    }
    //オーバーメソッド
    public void Over()
    {
        animator.Play(deadAnime);
        GameStop(); //同フレーム内に記述することで、速度0(GameStop)に上書きされ跳ね上がらない事象を阻止

        //当たり判定を消して、上に少し跳ね上げる
        //★★Try:仕様変更等により生じるエラーの防御策
        //    out:結果を外の変数に出力する。変数colliderへの代入も含め1行で済ませられる
        //    var:ターゲットがはっきりしているから、型名CapsuleCollider2D→varに省略

        //out使わない書き方
        // CapsuleCollider2D collider = GetComponent<CapsuleCollider2D>();
        // if (collider != null)
        //  {
        //   collider.enabled = false;
        //  }
        if (TryGetComponent<CapsuleCollider2D>(out var collider))
        {
            collider.enabled = false; //もしCC2Dが見つかれば、当たり判定(CC2Dの機能)無効化
        }
        rbody.AddForce(new Vector2(0, 5), ForceMode2D.Impulse); //上に少し跳ね上げる

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerOver();
        }
    }

    void GameStop()
    {
        rbody.linearVelocity = Vector2.zero; //★★zeroを使うことで処理落ち減らせる(エコで安全)

        // マップは切り替えず、コードの力だけで直接「移動・ジャンプ」の電力を遮断し、決定ボタンだけを起動する！
        if (moveAction != null) moveAction.Disable();
        if (jumpAction != null) jumpAction.Disable();
        if (submitAction != null) submitAction.Enable(); // 決定キーのみONにする

        axisH = 0.0f;
        goJump = false;
    }

    // ★【追加】ゲームオーバー後、ONになった決定キーが押された瞬間をここでキャッチ
    // これにより、Unityのバグ付きメッセージ通知を一切通さずにGMのGameEndを実行します！
    private void LateUpdate()
    {
        if (GameManager.gameState != GameState.InGame)
        {
            if (submitAction != null && submitAction.WasPressedThisFrame())
            {
                if (GameManager.Instance != null) GameManager.Instance.GameEnd();
            }
        }
    }
}