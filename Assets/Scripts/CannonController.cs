using UnityEngine;

public class CannonController : MonoBehaviour
{
    public GameObject objPrefab;            //発生させるPrefabデータ
    public float delayTime = 3.0f;          //遅延時間
    public float fireSpeed = 4.0f;          //発射速度
    public float length = 8.0f;             //範囲

    GameObject player;                      //プレイヤー
    Transform gateTransform;                //発射口のTransform
    float passedTimes = 0;                  //経過時間
                                            //距離チェック
                                            // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //発射口オブジェクトのTransformを取得
        gateTransform = transform.Find("gate");
        //プレイヤーを取得
        //playerをpublicにすれば不要の行
        //ただしPlayerをスポーン設定にするならlengthがInspectorを参照して、Update冒頭の処理を行わなくなるからpublicダメ!!
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        // ⬇️ Playerをスポーン設定にした為returnまでを追加！
        // もし player がいなければ、もう一度画面内を探しに行く
        if (player == null)
        {
            //見つかったらplayerに代入
            player = GameObject.FindGameObjectWithTag("Player");

            // それでもまだ見つからなければ（まだスポンしていない）、
            // このフレームの以降の処理（距離チェックなど）はスキップして次のフレームを待つ
            if (player == null)
            {
                return;
            }
        }

        //待機時間加算
        passedTimes += Time.deltaTime;
        //Playerとの距離チェック
        if (CheckLength(player.transform.position))
        {
            //待機時間経過
            if (passedTimes > delayTime)
            {
                passedTimes = 0;
                Vector2 pos = gateTransform.position; //gateの場所
                GameObject obj = Instantiate(objPrefab, pos, Quaternion.identity); //gateの場所に弾生成
                Rigidbody2D rbody = obj.GetComponent<Rigidbody2D>(); //弾に物理を利用

                float angleZ = transform.localEulerAngles.z;
                float x = Mathf.Cos(angleZ * Mathf.Deg2Rad);
                float y = Mathf.Sin(angleZ * Mathf.Deg2Rad);
                Vector2 v = new Vector3(x, y) * fireSpeed; //ラジアンは正規化込みで返してくれる
                //float以下の別の書き方
                //絶対座標同士の引き算したものを正規化し、それにスピードを乗算
                //(相対座標だと向きを変えても数値が変化しないからこの計算は無理)
                //Vector2 direction = pos - transform.position;
                //Vector2 v = direction.normalized * fireSpeed;

                rbody.AddForce(v, ForceMode2D.Impulse);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);
    }

    bool CheckLength(Vector2 targetPos)
    {
        return (Vector2.Distance(transform.position, targetPos) <= length);
        //上の行を分解したもの
        //float d = Vector2.Distance(transform.position, targetPos);
        //return (d<=length); 戻り値あり(trueかfalseかを返す)
    }

    //範囲表示
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, length);
    }
}
