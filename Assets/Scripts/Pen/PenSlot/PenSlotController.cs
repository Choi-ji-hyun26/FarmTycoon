using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
역할
1. 동물 슬롯 목록 관리
2. 해금된 슬롯에만 동물 배치
3. 동물 프리팹 생성 및 초기화
*/
public class PenSlotController : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private List<Transform> slots;         // 씬에 미리 배치된 슬롯 10개
    [SerializeField] private GameObject animalPrefab;

    [Header("References")]
    [SerializeField] private PenCollectionBox collectionBox;

    private int unlockedSlotCount = 0;
    private List<AnimalController> spawnedAnimals = new();

    // 해금 슬롯 수를 늘리고 새로운 슬롯에 동물 배치
    public void UnlockSlots(int count)
    {
        int from = unlockedSlotCount;
        int to = Mathf.Min(unlockedSlotCount + count, slots.Count);

        for (int i = from; i < to; i++)
        {
            SpawnAnimal(slots[i]);
        }

        unlockedSlotCount = to;
    }

    // 지정된 슬롯 위치에 동물 프리팹 생성 후 초기화
    private void SpawnAnimal(Transform slot)
    {
        if (animalPrefab == null) return;

        GameObject obj = Instantiate(animalPrefab, slot.position, slot.rotation);
        AnimalController animal = obj.GetComponent<AnimalController>();

        if (animal != null)
        {
            animal.Initialize(slot.position, collectionBox);
            spawnedAnimals.Add(animal);
        }
    }

    // 현재 배치된 동물 수 반환
    public int AnimalCount => spawnedAnimals.Count;
}
