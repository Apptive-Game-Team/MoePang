using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [Header("씬 연결")]
    [SerializeField] private string prevScene;

    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI goldText;

    private void Start()
    {
        goldText.text = $"Gold : {GoldManager.Instance.Gold}";
    }
    public void OnClickBack()
    {
        SceneManager.LoadScene(prevScene);
    }
}
