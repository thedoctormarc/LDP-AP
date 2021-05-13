using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{

    int index;
    int pCount;

    // Start is called before the first frame update
    void Start()
    {
        if (AppManager.instance._gMode() == AppManager.gameMode.AI)
        {
            index = 0;
            pCount = PlayerManager.instance.transform.childCount;
            PlayerManager.instance.GetChildByIndex(0).transform.Find("Line Of Sight").gameObject.SetActive(true);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (AppManager.instance._gMode() == AppManager.gameMode.AI)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) == true)
            {
                PlayerManager.instance.GetChildByIndex(index).transform.Find("Line Of Sight").gameObject.SetActive(false);
                index = (++index > pCount - 1) ? 0 : index;
                PlayerManager.instance.GetChildByIndex(index).transform.Find("Line Of Sight").gameObject.SetActive(true);
            }
        }
    }
}
