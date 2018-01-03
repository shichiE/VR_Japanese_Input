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
    // 右手であるか
    public bool isRightHand;

    // 文字
    private GameObject[] _horizontalWords = new GameObject[10];
    private GameObject[] _verticalWords = new GameObject[5];
    // 選択文字を囲うフレーム
    private GameObject _frame;

    // 人差し指トリガーフラグ
    private bool _indexTrigerFlag;

    // 人差し指トリガー
    private OVRInput.RawButton _indexTrigger;
    // 中指トリガー
    private OVRInput.RawButton _handTrigger;
    // 下方ボタン
    private OVRInput.RawButton _downsideButton;
    // 上方ボタン
    private OVRInput.RawButton _upsideButton;

    // 文字間隔
    private static readonly float WORDS_INTERVAL = 0.045f;

    // 現在の文字
    private string _nowCharacter = "";

    // 位置
    private Vector3 _localHandPosition = new Vector3();
    private Vector3 _originHandPosition = new Vector3();

    // 回転
    private Quaternion _originHandRotation = new Quaternion();

    // 文字候補
    private static readonly string[,] _jpCharacters = new string[10, 5] {
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
    private static readonly string _preVoicedCharacters = "かきくけこさしすせそたちってとはひふへほ";
    private static readonly string _voicedCharacters = "がぎぐげござじずぜぞだぢづでどばびぶべぼ";
    private static readonly string _preSemiVoicedCharacters = "ばびぶべぼ";
    private static readonly string _semiVoicedCharacters = "ぱぴぷぺぽ";
    private static readonly string _preSmallCharacters = "あいうえおつやゆよ";
    private static readonly string _smallCharacters = "ぁぃぅぇぉっゃゅょ";
    private static readonly string _preUnvoicedCharacters = "がぎぐげござじずぜぞだぢづでどぱぴぷぺぽぁぃぅぇぉっゃゅょ";
    private static readonly string _unvoicedCharacters = "かきくけこさしすせそたちつてとはひふへほあいうえおつやゆよ";

    // 右手文字のタグ
    private static readonly string RIGHT_HAND_WORD_TAG = "RightHandWord";
    // 左手文字のタグ
    private static readonly string LEFT_HAND_WORD_TAG = "LeftHandWord";

    // コントローラーの振動
    private Vibrate _vibrate;

    /* -------------------------------------------------- */

    /// <summary>
    /// Use this for initialization
    /// </summary>
    void Start()
    {
        _indexTrigerFlag = false;

        _localHandPosition = Vector3.zero;
        _originHandPosition = Vector3.zero;

        _vibrate = new Vibrate();

        if (isRightHand)
        {
            _indexTrigger = OVRInput.RawButton.RIndexTrigger;
            _handTrigger = OVRInput.RawButton.RHandTrigger;
            _downsideButton = OVRInput.RawButton.A;
            _upsideButton = OVRInput.RawButton.B;
        }
        else
        {
            _indexTrigger = OVRInput.RawButton.LIndexTrigger;
            _handTrigger = OVRInput.RawButton.LHandTrigger;
            _downsideButton = OVRInput.RawButton.X;
            _upsideButton = OVRInput.RawButton.Y;
        }
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        // 人差し指トリガーを押した
        if (OVRInput.GetDown(_indexTrigger))
        {
            _indexTrigerFlag = true;
            CreateWordCubes();
        }
        // 人差し指トリガーを離した
        else if (OVRInput.GetUp(_indexTrigger))
        {
            _indexTrigerFlag = false;
            DeleteWordCubes();
        }

        // Aボタン/Xボタンを押した
        if (OVRInput.GetDown(_downsideButton))
        {
            // 文字確定
            if (_nowCharacter != "")
            {
                inputTextObj.GetComponent<TextMesh>().text += _nowCharacter;
            }
        }
        // Bボタン/Yボタンを押した
        else if (OVRInput.GetDown(_upsideButton))
        {
            string text = inputTextObj.GetComponent<TextMesh>().text;
            if (text.Length > 0)
            {
                inputTextObj.GetComponent<TextMesh>().text = text.Remove(text.Length - 1);
            }
        }
        // 中指トリガーを押した
        else if (OVRInput.GetDown(_handTrigger))
        {
            ChangeCharacter();
        }

        // 人差し指トリガー握っている
        if (_indexTrigerFlag)
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
        if (_originHandPosition == Vector3.zero)
        {
            _originHandPosition = this.transform.position;
            _originHandRotation = this.transform.rotation;
        }

        // 文字の親
        GameObject parentWords = new GameObject("Parent Words");
        string wordsTag = isRightHand ? RIGHT_HAND_WORD_TAG : LEFT_HAND_WORD_TAG;
        parentWords.tag = wordsTag;
        parentWords.transform.position = _originHandPosition;

        // 水平に展開する文字
        for (int h = 0; h < 10; h++)
        {
            _horizontalWords[h] = Instantiate(cubeEmptyObj,
                                 new Vector3(_originHandPosition.x - WORDS_INTERVAL * (float)(6 - h),
                                             _originHandPosition.y,
                                             _originHandPosition.z),
                                 Quaternion.identity);
            _horizontalWords[h].GetComponentInChildren<TextMesh>().text = _jpCharacters[h, 0];
            _horizontalWords[h].transform.parent = parentWords.transform;
        }
        // 垂直に展開する文字
        for (int v = 0; v < 5; v++)
        {
            _verticalWords[v] = Instantiate(cubeEmptyObj,
                               new Vector3(_originHandPosition.x - WORDS_INTERVAL,
                                           _originHandPosition.y,
                                           _originHandPosition.z + WORDS_INTERVAL * (float)(v <= 2 ? v : 2 - v)),
                               Quaternion.identity);
            _verticalWords[v].GetComponentInChildren<TextMesh>().text = _jpCharacters[0, v];
            _verticalWords[v].transform.parent = parentWords.transform;
        }
        // 水平と垂直で被っているので消す
        _verticalWords[0].GetComponent<MeshRenderer>().enabled = false;
        _verticalWords[0].GetComponentInChildren<TextMesh>().text = string.Empty;

        // フレーム
        _frame = Instantiate(frameObj,
                            new Vector3(_originHandPosition.x - WORDS_INTERVAL,
                                        _originHandPosition.y,
                                        _originHandPosition.z),
                            Quaternion.identity);
        _frame.transform.parent = parentWords.transform;

        _nowCharacter = _jpCharacters[5, 0];
        // Y軸回転のみ影響する。X軸は少し角度をつけて見やすくする
        parentWords.transform.rotation = Quaternion.Euler(-15.0f, _originHandRotation.eulerAngles.y, 0.0f);
    }


    /// <summary>
    /// 文字盤削除
    /// </summary>
    private void DeleteWordCubes()
    {
        _originHandPosition = Vector3.zero;
        _nowCharacter = "";

        // すべて削除
        string destroyTag = isRightHand ? RIGHT_HAND_WORD_TAG : LEFT_HAND_WORD_TAG;
        GameObject[] wordObject = GameObject.FindGameObjectsWithTag(destroyTag);
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
            if ((index = _preVoicedCharacters.IndexOf(lastChar)) != -1)
            {
                inputTextObj.GetComponent<TextMesh>().text = text.Remove(text.Length - 1);
                inputTextObj.GetComponent<TextMesh>().text += _voicedCharacters[index];
            }
            // 半濁点が使える文字の場合
            else if ((index = _preSemiVoicedCharacters.IndexOf(lastChar)) != -1)
            {
                inputTextObj.GetComponent<TextMesh>().text = text.Remove(text.Length - 1);
                inputTextObj.GetComponent<TextMesh>().text += _semiVoicedCharacters[index];
            }
            // 小文字が使える文字の場合
            else if ((index = _preSmallCharacters.IndexOf(lastChar)) != -1)
            {
                inputTextObj.GetComponent<TextMesh>().text = text.Remove(text.Length - 1);
                inputTextObj.GetComponent<TextMesh>().text += _smallCharacters[index];
            }
            // 元の文字に戻す場合
            else if ((index = _preUnvoicedCharacters.IndexOf(lastChar)) != -1)
            {
                inputTextObj.GetComponent<TextMesh>().text = text.Remove(text.Length - 1);
                inputTextObj.GetComponent<TextMesh>().text += _unvoicedCharacters[index];
            }
        }
    }


    /// <summary>
    /// 文字盤移動
    /// </summary>
    private void MoveWordCubes()
    {
        _localHandPosition = this.transform.position - _originHandPosition;

        // 水平に展開する文字
        float newCoodinateSystemPositionX = (_localHandPosition.x * Mathf.Cos(_originHandRotation.eulerAngles.y * Mathf.PI / 180)) +
            (_localHandPosition.z * Mathf.Sin(-_originHandRotation.eulerAngles.y * Mathf.PI / 180));
        newCoodinateSystemPositionX = Mathf.Min(Mathf.Max(newCoodinateSystemPositionX, WORDS_INTERVAL * -4), WORDS_INTERVAL * 5); // 飛び出し制限
        for (int h = 0; h < 10; h++)
        {
            _horizontalWords[h].GetComponentInChildren<TextMesh>().color = Color.black;
            _horizontalWords[h].transform.localPosition = new Vector3(-WORDS_INTERVAL * (float)(6 - h) + newCoodinateSystemPositionX,
                                                                     0,
                                                                     0);
        }

        int columnIowindex = 5 - (int)Mathf.Ceil((newCoodinateSystemPositionX - WORDS_INTERVAL / 2) / WORDS_INTERVAL);
        columnIowindex = Mathf.Min(Mathf.Max(columnIowindex, 0), 9);

        float newCoodinateSystemPositionZ = (_localHandPosition.x * Mathf.Sin(_originHandRotation.eulerAngles.y * Mathf.PI / 180)) +
            (_localHandPosition.z * Mathf.Cos(-_originHandRotation.eulerAngles.y * Mathf.PI / 180));
        newCoodinateSystemPositionZ = Mathf.Min(Mathf.Max(newCoodinateSystemPositionZ, WORDS_INTERVAL * -2), WORDS_INTERVAL * 2); // 飛び出し制限
        int rowIndex = (int)Mathf.Ceil((newCoodinateSystemPositionZ - WORDS_INTERVAL / 2) / WORDS_INTERVAL) + 2;
        rowIndex = rowIndex <= 2 ? 2 - rowIndex : rowIndex;
        rowIndex = Mathf.Min(Mathf.Max(rowIndex, 0), 4);

        // 垂直に展開する文字
        for (int v = 0; v < 5; v++)
        {
            if (v == 0)
            {
                _horizontalWords[columnIowindex].transform.localPosition = new Vector3(-WORDS_INTERVAL,
                                                                                      0,
                                                                                      newCoodinateSystemPositionZ);
                continue;
            }
            _verticalWords[v].GetComponentInChildren<TextMesh>().text = _jpCharacters[columnIowindex, v];
            _verticalWords[v].GetComponentInChildren<TextMesh>().color = Color.black;
            _verticalWords[v].transform.localPosition = new Vector3(-WORDS_INTERVAL,
                                                                   0,
                                                                   WORDS_INTERVAL * (float)(v <= 2 ? v : 2 - v) + newCoodinateSystemPositionZ);
        }

        if (rowIndex != 0)
        {
            _verticalWords[rowIndex].GetComponentInChildren<TextMesh>().color = Color.red;
        }
        else
        {
            _horizontalWords[columnIowindex].GetComponentInChildren<TextMesh>().color = Color.red;
        }
        // 選択中の文字が変化したとき
        if (_nowCharacter != _jpCharacters[columnIowindex, rowIndex])
        {
            _nowCharacter = _jpCharacters[columnIowindex, rowIndex];
            Debug.Log("_nowCharacter=" + _nowCharacter);

            // 振動
            _vibrate.VibrateHand(isRightHand, 16, 64);
        }
    }
}
