using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Transform target; //追従対象

    public float minX = 0.0f;
    public float maxX = 0.0f;
    public float minY = 0.0f;
    public float maxY = 0.0f;

    private Camera mainCamera;

    //多重スクロール
    public GameObject subScreen; //サブスクリーン

    //強制スクロール
    public bool isForceScrollX = false; //強制スクロールフラグ
    public float forceScrollSpeedX = 0.5f; //1秒間で動かすX距離
    public bool isForceScrollY = false; //強制スクロールフラグ
    public float forceScrollSpeedY = 0.5f; //1秒間で動かすX距離

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// GameManagerからターゲットを設定するための自作メソッド
    /// </summary>
    /// <param name="player"></param>
    public void SetTarget(Transform player)
    {
        target = player;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float x = target.position.x;
        float y = target.position.y;

        //横強制スクロール
        if (isForceScrollX)
        {
            x = mainCamera.transform.position.x + (forceScrollSpeedX * Time.deltaTime);
        }

        //縦強制スクロール
        if (isForceScrollY)
        {
            y = mainCamera.transform.position.y + (forceScrollSpeedY * Time.deltaTime);
        }

        //Clampで範囲チェックかける
        x = Mathf.Clamp(x, minX, maxX);
        y = Mathf.Clamp(y, minY, maxY);

        //サブスクリーンスクロール
        if (subScreen != null)
        {
            //Mathf.Clamp要素は完全に上書きされている
            //下の行を短くするために記述
            float subY = subScreen.transform.position.y;
            subScreen.transform.position = new Vector3(x / 2.0f, subY, subScreen.transform.position.z);
        }

        //頭に何もつけずtransformと書くと、スクリプトのアタッチ先自身のことになる
        mainCamera.transform.position = new Vector3(x, y, mainCamera.transform.position.z);
    }
}
