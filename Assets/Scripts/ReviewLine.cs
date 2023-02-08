using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct LineData
{
    public Sprite icon;
    public TMPro.TMP_FontAsset font;
    public float fontSize;
    public RectTransform namePrefab;
}

public class ReviewLine : MonoBehaviour
{
    public TMPro.TMP_Text text;
    public Image icon;
    public Image[] stars;
    public RectTransform nameContainer;

    public LineData aya;
    public LineData racine;
    public LineData justine;

    public void SetData(Customer _customer, int _score)
    {
        LineData lineData = GetLineDataForCustomer(_customer);

        text.font = lineData.font;
        text.fontSize = lineData.fontSize;
        icon.sprite = lineData.icon;

        if (_score <= 1)
        {
            text.text = _customer.failSentences[Random.Range(0, _customer.failSentences.Length)];
        }
        else if (_score == 2)
        {
            text.text = _customer.neutralSentences[Random.Range(0, _customer.neutralSentences.Length)];
        }
        else if (_score >= 3)
        {
            text.text = _customer.successSentences[Random.Range(0, _customer.successSentences.Length)];
        }

        stars[0].gameObject.SetActive(true);
        stars[1].gameObject.SetActive(_score >= 2);
        stars[2].gameObject.SetActive(_score >= 3);

        foreach (RectTransform child in nameContainer)
        {
            Object.Destroy(child.gameObject);
        }
        Object.Instantiate(lineData.namePrefab, nameContainer);
    }

    LineData GetLineDataForCustomer(Customer _customer)
    {
        switch (_customer.id)
        {
            case 0: return justine;
            case 1: return aya;
            case 2: return racine;
        }
        return new LineData();
    }
}
