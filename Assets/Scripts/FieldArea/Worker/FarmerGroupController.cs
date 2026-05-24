using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
역할
1. 마이너 여러 명 관리
2. 고용 완료 시 마이너들을 활성화
*/
public class FarmerGroupController : MonoBehaviour
{
    [SerializeField] private FarmerWorker[] farmers;

    private void Awake()
    {
        if (farmers == null)
            return;

        for (int i = 0; i < farmers.Length; i++)
        {
            if (farmers[i] != null)
            {
                farmers[i].gameObject.SetActive(false);
            }
        }
    }

    public void ActivateFarmers()
    {
        if(farmers == null)
            return;
        
        for(int i = 0; i < farmers.Length; i++)
        {
            if(farmers[i] != null)
            {
                farmers[i].gameObject.SetActive(true);
                farmers[i].ActivateWorker();
            }
        }
    }
}
