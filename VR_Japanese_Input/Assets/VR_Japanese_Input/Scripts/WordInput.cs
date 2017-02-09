using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// VR日本語入力（アバターの右手にアタッチする）
/// </summary>
public class WordInput : MonoBehaviour
{
    // 文字盤オブジェクト
    public GameObject cubeEmptyObj;
    // フレームオブジェクト
    public GameObject frameObj;
    // 入力文字オブジェクト
    public GameObject inputTextObj;

    // 文字
    private GameObject[] horizontalWords = new GameObject[10];
    private GameObject[] verticalWords = new GameObject[5];
    // 選択文字を囲うフレーム
    private GameObject frame;

    // 右手人差し指トリガーフラグ
    private bool rIndexTrigerFlag;

    // 文字間隔
    private const float wordsInterval = 0.045f;

    // 現在の文字
    private string nowChar = "";

    // 位置
    private Vector3 localHandPosition = new Vector3();
    private Vector3 originHandPosition = new Vector3();

    // 回転
    private Quaternion originHandRotation = new Quaternion();

    // 文字候補
    private readonly string[,] jpChars = new string[10, 5] {
        { "わ", "ー", "を", "～", "ん" },
        { "ら", "り", "る", "れ", "ろ" },
        { "や", "（", "ゆ", "）", "よ" },
        { "ま", "み", "む", "め", "も" },
        { "は", "ひ", "ふ", "へ", "ほ" },
        { "あ", "い", "う", "え", "お" },
        { "か", "き", "く", "け", "こ" },
        { "さ", "し", "す", "せ", "そ" },
        { "た", "ち", "つ", "て", "と" },
        { "な", "に", "ぬ", "ね", "の" },
    };
    private string pre_voiced_characters = "かきくけこさしすせそたちってとはひふへほ";
    private string voiced_characters = "がぎぐげござじずぜぞだぢづでどばびぶべぼ";
    private string pre_semi_voiced_characters = "ばびぶべぼ";
    private string semi_voiced_characters = "ぱぴぷぺぽ";
    private string pre_small_characters = "あいうえおつやゆよ";
    private string small_characters = "ぁぃぅぇぉっゃゅょ";
    private string pre_unvoiced_characters = "がぎぐげござじずぜぞだぢづでどぱぴぷぺぽぁぃぅぇぉっゃゅょ";
    private string unvoiced_characters = "かきくけこさしすせそたちつてとはひふへほあいうえおつやゆよ";

    /* -------------------------------------------------- */

    /// <summary>
    /// Use this for initialization
    /// </summary>
    void Start()
    {
        rIndexTrigerFlag = false;

        localHandPosition = Vector3.zero;
        originHandPosition = Vector3.zero;
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        // 右人差し指トリガーを押した
        if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
        {
            rIndexTrigerFlag = true;
            CreateWordCubes();
        }
        // 右人差し指トリガーを離した
        else if (OVRInput.GetUp(OVRInput.RawButton.RIndexTrigger))
        {
            rIndexTrigerFlag = false;
            DeleteWordCubes();
        }

        // Aボタンを押した
        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            // 文字確定
            if (nowChar != "")
            {
                inputTextObj.GetComponent<TextMesh>().text += nowChar;
            }
        }
        // Bボタンを押した
        else if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            string text = inputTextObj.GetComponent<TextMesh>().text;
            if (text.Length > 0)
            {
                inputTextObj.GetComponent<TextMesh>().text = text.Remove(text.Length - 1);
            }
        }
        // 右中指トリガーを押した
        else if (OVRInput.GetDown(OVRInput.RawButton.RHandTrigger))
        {
            ChangeCharacter();
        }

        // 右人差し指トリガー握っている
        if (rIndexTrigerFlag)
        {
            MoveWordCubes();
        }
    }


    /// <summary>
    /// 文字盤生成
    /// </summary>
    private void CreateWordCubes()
    {
        // 手の位置をオリジナルのローカル座標に落とし込む
        if (originHandPosition == Vector3.zero)
        {
            originHandPosition = this.transform.position;
            originHandRotation = this.transform.rotation;
        }

        // 文字の親
        GameObject parentWords = new GameObject("Parent Words");
        parentWords.tag = "Word";
        parentWords.transform.position = originHandPosition;

        // 水平に展開する文字
        for (int h = 0; h < 10; h++)
        {
            horizontalWords[h] = Instantiate(cubeEmptyObj,
                                 new Vector3(originHandPosition.x - wordsInterval * (float)(6 - h),
                                             originHandPosition.y,
                                             originHandPosition.z),
                                 Quaternion.identity);
            horizontalWords[h].GetComponentInChildren<TextMesh>().text = jpChars[h, 0];
            horizontalWords[h].transform.parent = parentWords.transform;
        }
        // 垂直に展開する文字
        for (int v = 0; v < 5; v++)
        {
            verticalWords[v] = Instantiate(cubeEmptyObj,
                               new Vector3(originHandPosition.x - wordsInterval,
                                           originHandPosition.y,
                                           originHandPosition.z + wordsInterval * (float)(v <= 2 ? v : 2 - v)),
                               Quaternion.identity);
            verticalWords[v].GetComponentInChildren<TextMesh>().text = jpChars[0, v];
            verticalWords[v].transform.parent = parentWords.transform;
        }
        // 水平と垂直で被っているので消す
        verticalWords[0].GetComponent<MeshRenderer>().enabled = false;
        verticalWords[0].GetComponentInChildren<TextMesh>().text = string.Empty;

        // フレーム
        frame = Instantiate(frameObj,
                            new Vector3(originHandPosition.x - wordsInterval,
                                        originHandPosition.y,
                                        originHandPosition.z),
                            Quaternion.identity);
        frame.transform.parent = parentWords.transform;

        nowChar = jpChars[5, 0];
        parentWords.transform.rotation = Quaternion.Euler(-15.0f, originHandRotation.eulerAngles.y, 0.0f);// Y軸回転のみ影響する。X軸は少し角度をつけて見やすくする
    }


    /// <summary>
    /// 文字盤削除
    /// </summary>
    private void DeleteWordCubes()
    {
        originHandPosition = Vector3.zero;
        nowChar = "";

        // すべて削除
        GameObject[] wordObject = GameObject.FindGameObjectsWithTag("Word");
        foreach (GameObject destroy in wordObject)
        {
            Destroy(destroy);
        }
    }


    /// <summary>
    /// 濁音、半濁音、小文字変換
    /// </summary>
    private void ChangeCharacter()
    {
        string text = inputTextObj.GetComponent<TextMesh>().text;
        if (text.Length > 0)
        {
            int index = -1;
            string lastChar = text.Substring(text.Length - 1);

            // 濁点が使える文字の場合
            if ((index = pre_voiced_characters.IndexOf(lastChar)) != -1)
            {
                inputTextObj.GetComponent<TextMesh>().text = text.Remove(text.Length - 1);
                inputTextObj.GetComponent<TextMesh>().text += voiced_characters[index];
            }
            // 半濁点が使える文字の場合
            else if ((index = pre_semi_voiced_characters.IndexOf(lastChar)) != -1)
            {
                inputTextObj.GetComponent<TextMesh>().text = text.Remove(text.Length - 1);
                inputTextObj.GetComponent<TextMesh>().text += semi_voiced_characters[index];
            }
            // 小文字が使える文字の場合
            else if ((index = pre_small_characters.IndexOf(lastChar)) != -1)
            {
                inputTextObj.GetComponent<TextMesh>().text = text.Remove(text.Length - 1);
                inputTextObj.GetComponent<TextMesh>().text += small_characters[index];
            }
            // 元の文字に戻す場合
            else if ((index = pre_unvoiced_characters.IndexOf(lastChar)) != -1)
            {
                inputTextObj.GetComponent<TextMesh>().text = text.Remove(text.Length - 1);
                inputTextObj.GetComponent<TextMesh>().text += unvoiced_characters[index];
            }
        }
    }


    /// <summary>
    /// 文字盤移動
    /// </summary>
    private void MoveWordCubes()
    {
        localHandPosition = this.transform.position - originHandPosition;

        // 水平に展開する文字
        float newCoodinateSystemPositionX = (localHandPosition.x * Mathf.Cos(originHandRotation.eulerAngles.y * Mathf.PI / 180)) + (localHandPosition.z * Mathf.Sin(-originHandRotation.eulerAngles.y * Mathf.PI / 180));
        newCoodinateSystemPositionX = Mathf.Min(Mathf.Max(newCoodinateSystemPositionX, wordsInterval * -4), wordsInterval * 5); // 飛び出し制限
        for (int h = 0; h < 10; h++)
        {
            horizontalWords[h].GetComponentInChildren<TextMesh>().color = Color.black;
            horizontalWords[h].transform.localPosition = new Vector3(-wordsInterval * (float)(6 - h) + newCoodinateSystemPositionX,
                                                                     0,
                                                                     0);
        }

        int columnIowindex = 5 - (int)Mathf.Ceil((newCoodinateSystemPositionX - wordsInterval / 2) / wordsInterval);
        columnIowindex = Mathf.Min(Mathf.Max(columnIowindex, 0), 9);

        float newCoodinateSystemPositionZ = (localHandPosition.x * Mathf.Sin(originHandRotation.eulerAngles.y * Mathf.PI / 180)) + (localHandPosition.z * Mathf.Cos(-originHandRotation.eulerAngles.y * Mathf.PI / 180));
        newCoodinateSystemPositionZ = Mathf.Min(Mathf.Max(newCoodinateSystemPositionZ, wordsInterval * -2), wordsInterval * 2); // 飛び出し制限
        int rowIndex = (int)Mathf.Ceil((newCoodinateSystemPositionZ - wordsInterval / 2) / wordsInterval) + 2;
        rowIndex = rowIndex <= 2 ? 2 - rowIndex : rowIndex;
        rowIndex = Mathf.Min(Mathf.Max(rowIndex, 0), 4);

        // 垂直に展開する文字
        for (int v = 0; v < 5; v++)
        {
            if (v == 0)
            {
                horizontalWords[columnIowindex].transform.localPosition = new Vector3(-wordsInterval,
                                                                                      0,
                                                                                      newCoodinateSystemPositionZ);
                continue;
            }
            verticalWords[v].GetComponentInChildren<TextMesh>().text = jpChars[columnIowindex, v];
            verticalWords[v].GetComponentInChildren<TextMesh>().color = Color.black;
            verticalWords[v].transform.localPosition = new Vector3(-wordsInterval,
                                                                   0,
                                                                   wordsInterval * (float)(v <= 2 ? v : 2 - v) + newCoodinateSystemPositionZ);
        }

        if (rowIndex != 0)
        {
            verticalWords[rowIndex].GetComponentInChildren<TextMesh>().color = Color.red;
        }
        else
        {
            horizontalWords[columnIowindex].GetComponentInChildren<TextMesh>().color = Color.red;
        }
        // 選択中の文字が変化したとき
        if (nowChar != jpChars[columnIowindex, rowIndex])
        {
            nowChar = jpChars[columnIowindex, rowIndex];
            Debug.Log("nowChar=" + nowChar);

            // 振動
            VibrateHand(true, 16, 64);
        }
    }


    /// <summary>
    /// ハンドコントローラー振動
    /// </summary>
    /// <param name="isRightHand">振動する手が右かどうか</param>
    /// <param name="length">振動の長さ</param>
    /// <param name="strength">振動の強さ</param>
    private void VibrateHand(bool isRightHand, int length, byte strength)
    {
        // 振動
        byte[] vibration = new byte[length];
        for (int i = 0; i < vibration.Length; i++)
        {
            vibration[i] = strength;
        }
        OVRHapticsClip hapticsClip = new OVRHapticsClip(vibration, vibration.Length);
        if (isRightHand)
        {
            OVRHaptics.RightChannel.Mix(hapticsClip);
        }
        else
        {
            OVRHaptics.LeftChannel.Mix(hapticsClip);
        }
    }
}
