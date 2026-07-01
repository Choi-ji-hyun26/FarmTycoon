# Farm Tycoon

<img width="204" height="498" alt="1" src="https://github.com/user-attachments/assets/59330f33-c2b7-4ada-bba7-fd7de0d806f2" />
<img width="204" height="498" alt="2" src="https://github.com/user-attachments/assets/9d8a3b52-3a0a-4c3a-a680-2aceb5b47ceb" />
<img width="204" height="498" alt="3" src="https://github.com/user-attachments/assets/5ea73f41-5c17-44bf-b9d0-961af112a90f" />
<img width="204" height="498" alt="4" src="https://github.com/user-attachments/assets/5fdaf110-9cfa-4b47-af72-326ce239b28b" />

## 개요

**Farm Tycoon**은 당근 수확 → 수프 조리 → 판매, 소 사육 → 우유 생산 → 판매,   
두 생산 루프를 플레이어와 NPC가 함께 운영하는 3D 하이퍼캐주얼 타이쿤 게임입니다.  

| 항목 | 내용 |
|------|------|
| 엔진 | Unity 2022.3.62f2, C# |
| 개발 기간 | 2026.04 – 06 |
| 인원 | 1인 (기획·구조·구현) |
| 플랫폼 | PC / Android |
| 패턴 | Interface 기반 확장, State Machine, Adapter, Object Pooling |
| 외부 라이브러리 | VContainer (DI), DOTween |

---

## 게임플레이 구조

두 루프는 독립적으로 동작하며 SaleDesk에서 합류합니다.

```
▼ CARROT LOOP                        ▼ MILK LOOP

      당근밭 수확                     AnimalController 우유 자동 생산
(플레이어 / FarmerWorker NPC)                  (플레이어)
         ↓                                      ↓
CookingMachine 당근 투입              PenCollectionBox 수거함 적재
         ↓                                      ↓
    수프 자동 생산                             플레이어 픽업
         ↓                                      ↓
         ↓                                 SaleDesk 납품
CourierWorker / 플레이어가                         ↓
  SaleDesk로 운반                     CustomerController 우유 소비
         ↓                                  → 보상 지급
CustomerController 수프 소비
    → 보상 지급
```

**PROGRESSION FLOW**

```
돈 획득 → Zone 해금 (ZoneRevealDirector 카메라 연출)
       → ToolUpgrade / FarmerHire / PenUpgrade / CourierHire
       → 생산 효율 증가 → 루프 반복
```

---

## 프로젝트 구조

```
Assets/Scripts/
├── Player/        # Action(Zone 디스패치), Movement(Controllable), Inventory
├── FieldArea/     # FarmerWorker, CarrotNode
├── Pen/           # AnimalController, PenCollectionBox
├── SaleArea/      # SaleDeskController, CourierWorker
├── Item/          # ItemStack, ItemVisualEffectPool (Object Pooling)
├── Interface/     # IPickupSource, IItemDepositTarget, IMoneyDepositTarget
├── Zone/          # ZoneRevealDirector, ZoneType
├── Quest/         # EventBus, QuestManager, QuestProgressTracker, VContainer DI
├── Core/          # MoneyManager
└── UI/
```

---

## 시스템

### 퀘스트 시스템 — ACTION vs COUNT 정책 분리

인게임 루프는 직접 구현했고 퀘스트 시스템은 EventBus·VContainer 같은 실무 표준 도구를 도입했습니다.   
순차 진행 퀘스트를 EventBus로 게임 로직과 분리해 구현했고 그 위에서 자유 진행 게임 특유의 문제를 풀었습니다.

**문제**: 플레이어가 특정 퀘스트를 받기 전에 이미 그 조건(도구 업그레이드, 손님 응대 등)을 끝내버리면  
이벤트는 이미 지나갔기 때문에 퀘스트가 영원히 완료되지 않습니다. 

**해결**: 조건을 두 카테고리로 나눴습니다.

- **COUNT** (당근 수확·수프 생산·판매·손님 응대) — 퀘스트 시작 시점의 누적값을 `baseValue`로 저장해두고 그 이후 진행분만 카운트합니다.   
- **ACTION** (도구 업그레이드·NPC 고용·축사 확장) — "몇 번 했는가"가 아니라 "이미 완료됐는가"라는 상태 문제이므로 완료 기록(`HashSet<FarmingToolTier>` 등)으로 보관합니다. 퀘스트가 시작되는 순간 기록에 있으면 즉시 Claimable 처리됩니다.   

```csharp
// QuestProgressTracker — 시작부터 항상 모든 이벤트를 구독해 진행 상태를 기록
if (data.Category == QuestEventCategory.Count)
{
    int progress = GetCount(data.TargetEvent) - _activeCountBase;
    _activeQuest.UpdateProgress(progress);
}
else // ACTION — "몇 번" 대신 "완료 여부"
{
    bool satisfied = IsActionSatisfied(data);
    _activeQuest.UpdateProgress(satisfied ? data.TargetValue : 0);
}
```

**설계 결정 — 다형성 회피**: 초기엔 조건별로 클래스를 나눠 다형성으로 처리하려 했지만 현재 게임 규모 대비 캐스팅과 클래스 분화가 과도하다고 판단해 되돌렸습니다. 조건 카테고리 2개로 단순화하는 쪽이 문제 본질에 더 맞았습니다.

**설계 결정 — 이벤트 버스와 DI**: EventBus는 `Dictionary<Type, Delegate>` 기반 비제네릭 중앙 관리로 구현하고, Domain Reload 비활성화 환경에서 이전 플레이의 구독이 남는 함정을 `[RuntimeInitializeOnLoadMethod]`로 방지했습니다. `QuestManager`/`QuestProgressTracker`는 VContainer로 주입하고 DI 컨테이너가 생성하지 않는 `ScriptableObject` 보상은 `IRewardContext`로 지급 시점에 의존성을 전달해 우회했습니다.

→ [`Quest/Event/EventBus.cs`](Assets/Scripts/Quest/Event/EventBus.cs) · [`Quest/Core/QuestProgressTracker.cs`](Assets/Scripts/Quest/Core/QuestProgressTracker.cs) · [`Quest/Core/GameLifetimeScope.cs`](Assets/Scripts/Quest/Core/GameLifetimeScope.cs)

---

### Vehicle 탑승 트러블슈팅

탑승 시 플레이어가 공중으로 튀거나 차 회전 시 위치가 틀어지는 문제를 세 단계에 걸쳐 풀었습니다.

| 단계 | 시도 | 결과 |
|------|------|------|
| 1 | CharacterController + 자식 오브젝트로 차량 배치, 레이어 분리 | CharacterController가 Physics Layer Matrix를 무시한다는 구조적 한계 발견 → 레이어로 해결 불가 |
| 2 | Rigidbody 전환, `SetParent`로 플레이어를 차량 하위로 귀속 | 차량 회전 시 플레이어도 같이 돌면서 물리 연산 충돌 |
| 3 (해결) | `SetParent` 제거, 매 `FixedUpdate`에서 플레이어 위치를 SeatPoint에 직접 동기화, `SetActive()` 시 `isKinematic` 전환으로 물리 제어권 분리 | 안정화 |

이 과정에서 플레이어-탈것 간 물리 의존을 완전히 분리하기로 하고 `Controllable` 추상 클래스로 `PlayerMovement`/`VehicleMovement`를 동일 인터페이스로 묶었습니다. `InputDispatcher.SetTarget()`만으로 런타임 입력 대상을 바꾸고 새 이동 수단이 추가돼도 `Controllable` 상속만으로 확장됩니다.

→ [`Player/Movement/`](Assets/Scripts/Player/Movement/)

---

### 그 외 시스템

| 시스템 | 요약 | 코드 |
|---|---|---|
| 인터페이스 기반 아이템 플로우 | `IPickupSource`/`IItemDepositTarget`으로 플레이어·NPC가 구체 타입 모른 채 픽업·납품 처리. `SaleDeskDepositAdapter`로 아이템 타입 확장 시 호출부 무변경 | [`Interface/`](Assets/Scripts/Interface/) |
| FarmerWorker Claim 시스템 | 5-State FSM으로 자율 수확. `CarrotNode.TryClaim()`으로 여러 NPC의 동시 타격 충돌 방지 | [`FieldArea/Worker/FarmerWorker.cs`](Assets/Scripts/FieldArea/Worker/FarmerWorker.cs) |
| 2단계 Object Pooling | `ItemStack`(정적 스택 풀링), `ItemVisualEffectPool`(포물선 이동 연출). 연출 완료 콜백 이후 데이터 반영으로 비주얼-수치 불일치 방지 | [`Item/`](Assets/Scripts/Item/) |
| Zone 기반 액션 디스패치 | `ZoneType` enum + switch로 컴파일 타임 검증. 탑승 중 Zone 감지가 끊기는 예외를 `IsMounted` 플래그로 우회 | [`Player/Action/PlayerZoneActionHandler.cs`](Assets/Scripts/Player/Action/PlayerZoneActionHandler.cs) |
| ZoneRevealDirector | Zone 해금 시 카메라 연출. `isPlaying` 플래그로 중복 실행 방지 | [`Zone/Core/ZoneRevealDirector.cs`](Assets/Scripts/Zone/Core/ZoneRevealDirector.cs) |

---

## 실행 방법

1. Unity Hub에서 `2022.3.62f2` 이상으로 열기
2. Package Manager에서 [VContainer](https://github.com/hadashiA/VContainer), [DOTween](https://dotween.demigiant.com/) 확인 (Packages/manifest.json에 포함)
3. `Assets/Scenes/Game` 씬 실행

---

## 링크

YouTube 게임 소개 영상
https://youtu.be/TWybSNSr_Ho

---

*Unity Client Developer · 최지현 · cjhyun26@gmail.com*
