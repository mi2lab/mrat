using UnityEngine;
using UnityEngine.UI;

public class UIScriptController : MonoBehaviour {

    private Text buttonLabel;
    public ZoomClusteringController script;
    public string scriptName;

	// Use this for initialization
	void Start () {
        GetComponent<Button>().onClick.AddListener(OnButtonClick);
        buttonLabel = GetComponentInChildren<Text>();
	}

    void OnButtonClick()
    {
        script.enabled = !script.enabled;
    }
	
	// Update is called once per frame
	void Update () {
        if (script.enabled)
        {
            buttonLabel.text = $"Disable {scriptName}";
        }
        else {
            buttonLabel.text = $"Enable {scriptName}";
        }
	}
}
