using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float speed = 3.0f;          // 移動速度
    public bool isToRight = false;      // true=右向き　false=左向き
    public float revTime = 0;           // 反転時間
    public LayerMask groundLayer;       // 地面レイヤー
    private bool onGround = false;      // 地面フラグ
    private float time = 0;

    private Rigidbody2D rbody;

    //プレイヤーに当たったらテキスト表示する用
    [SerializeField]GameObject button;
    [SerializeField]GameObject container;

    // 💡 1.インスペクターには、新しく作った「TextContainer」をアタッチ！
    [Header("TextContainerオブジェクトを設定")]
    public Transform textContainer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
        UpdateScale();

        button.SetActive(false);
        container.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // 地上判定
        onGround = Physics2D.CircleCast(transform.position,    // 発射位置
                                        0.2f,                  // 円の半径
                                        Vector2.down,          // 発射方向
                                        0.0f,                  // 発射距離
                                        groundLayer);          // 検出するレイヤー

        if (revTime > 0) //時間経過で反転
        {
            time += Time.deltaTime;
            if (time >= revTime)
            {
                isToRight = !isToRight; //反転前準備:フラグ反転
                time = 0;               //反転前準備:経過時間リセット

                UpdateScale();          //反転メソッド
            }
        }
    }

    void FixedUpdate() //移動反転
    {
        if (onGround)
        {
            float direction = isToRight ? 1f : -1f; //画像は反転するが移動方向はシーンビュー通り
            rbody.linearVelocity = new Vector2(speed * direction, rbody.linearVelocity.y);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) //オブジェクトに接触で反転
    {
        isToRight = !isToRight;
        time = 0;
        UpdateScale();

        if (collision.CompareTag("Player"))
        {
            button.SetActive(true);
            container.SetActive(true);
        }
    }

    private void UpdateScale()
    {
        float scaleX = isToRight ? -1f : 1f; //三項演算子
        transform.localScale = new Vector2(scaleX, 1f);

        // 💡 2. 親（Enemy）のマイナス反転を、クッションである TextContainer の段階で「打ち消す」
        if (textContainer != null)
        {
            // 敵が右向き（-1）のときはコンテナを -1 倍して元に戻し、左向き（1）のときは 1 倍のままにする
            float textScaleX = isToRight ? -1f : 1f;
            textContainer.localScale = new Vector3(textScaleX, 1f, 1f);
            //textContainer.localScale = new Vector2(textScaleX, 1f);でも可
            //UIのRectTransformだとector2→3の自動修正機能がないからこれができない。V
        }
    }
}
