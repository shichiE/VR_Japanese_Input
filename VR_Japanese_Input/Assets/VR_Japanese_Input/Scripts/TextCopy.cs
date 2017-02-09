using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 別のTextMeshのtextを複製する
/// </summary>
public class TextCopy : MonoBehaviour
{
    public TextMesh srcText;

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        this.GetComponent<TextMesh>().text = srcText.text;
    }
}
