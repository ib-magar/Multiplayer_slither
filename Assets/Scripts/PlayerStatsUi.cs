
using UnityEngine;
using TMPro;
public class PlayerStatsUi : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI lengthText;

    private void OnEnable()
    {
        PlayerLength.changedLengthEvent += ChangeLengthText;
    }
    private void OnDisable()
    {
        PlayerLength.changedLengthEvent -= ChangeLengthText;
        
    }

    void ChangeLengthText(ushort legnth)
    {
        lengthText.text = legnth.ToString();    
    }

}
