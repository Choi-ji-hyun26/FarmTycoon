# Farm Tycoon

<img width="204" height="498" alt="farmtycoon_start" src="https://github.com/user-attachments/assets/032fa97b-5475-4d86-afe3-bc0639ed3d66" />
<img width="204" height="498" alt="farmtycoon_zone" src="https://github.com/user-attachments/assets/8c074b15-81d1-4d3a-aab4-769d893ca43c" />
<img width="204" height="498" alt="farmtycoon_havest" src="https://github.com/user-attachments/assets/31d7be17-303e-4582-83d3-a81694ee41a1" />
<img width="204" height="498" alt="farmtycoon_money" src="https://github.com/user-attachments/assets/0509349a-a81c-4c35-bc7b-366b1c887e7b" />

## 개요

**Farm Tycoon**은 Unity 클라이언트 개발 포트폴리오 프로젝트입니다.

당근 수확 → 수프 조리 → 판매, 소 사육 → 우유 생산 → 판매,  
두 생산 루프를 플레이어와 NPC가 함께 운영하는 3D 하이퍼캐주얼 타이쿤 게임입니다.  
단순한 게임 구현을 넘어 **인터페이스 기반 아이템 플로우, NPC 자율 AI, Object Pooling, Zone 기반 액션 디스패치, 카메라 연출** 등 클라이언트 개발 전반을 다룹니다.

| 항목 | 내용 |
|------|------|
| 엔진 | Unity 2022.3.62f2, C# |
| 개발 기간 | 2026.04 – 05 |
| 인원 | 1인 (기획·구조·구현) |
| 플랫폼 | PC / Android |
| 패턴 | State Machine, Interface Design, Adapter Pattern, Object Pooling |
| 이동 | Controllable 추상화, InputDispatcher 런타임 교체 |
| 버전 관리 | Git |

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

## 아키텍처

### 프로젝트 구조

```
Assets/Scripts/
├── Player/
│   ├── Action/         # PlayerZoneActionHandler, PlayerZoneDetector
│   ├── Movement/       # Controllable, InputDispatcher, PlayerMovement, VehicleMountController, VehicleMovement
│   └── Inventory/      # PlayerInventory, PlayerStackController
├── FieldArea/          # FarmerWorker, CarrotNode
├── Pen/                # AnimalController, PenCollectionBox
├── SaleArea/           # SaleDeskController, CourierWorker
├── Item/               # ItemStack, ItemVisualEffectPool
├── Interface/          # IPickupSource, IItemDepositTarget, IMoneyDepositTarget
├── Zone/               # ZoneRevealDirector, ZoneType
├── Core/               # MoneyManager
└── UI/
```

---

## 핵심 시스템

### 1. 인터페이스 기반 아이템 플로우

플레이어와 NPC가 구체 타입을 모른 채 동일한 인터페이스로 픽업·납품을 처리합니다.

```
IPickupSource
├── AvailableCount { get; }
├── HasItem(int amount) / TryTakeItem(int amount)
└── 구현체
    ├── CookingMachineController  ← 수프 출력
    └── PenCollectionBox          ← 우유 출력

IItemDepositTarget
├── CanAddItem(int amount) / TryAddItem(int amount)
└── 구현체
    ├── CookingMachineController  ← 당근 입력
    └── SaleDeskDepositAdapter    ← Adapter Pattern

IMoneyDepositTarget
├── IsCompleted / RemainingCost / DepositMoney(int)
└── 구현체
    ├── ToolUpgradeController
    ├── FarmerHireController
    ├── CourierHireController
    └── PenUpgradeController
```

**설계 결정 — Dual Role:**  
`CookingMachineController`는 `IPickupSource`(수프 출력)와 `IItemDepositTarget`(당근 입력)을 동시에 구현합니다.   
입·출력을 역할에 따라 다른 인터페이스로 노출해 호출부가 목적에 맞는 인터페이스만 참조하도록 설계했습니다.

**설계 결정 — Adapter Pattern:**  
`SaleDeskDepositAdapter`가 Soup/Milk 납품을 `IItemDepositTarget`으로 감쌉니다.   
아이템 타입이 추가되어도 `PlayerZoneActionHandler` 수정 없이 Adapter만 추가하면 됩니다.

**설계 결정 — 일관성:**  
`CourierWorker` NPC도 `IPickupSource.TryTakeItem()`을 통해서만 `CookingMachineController`에 접근합니다.   
플레이어·NPC가 동일한 인터페이스로 픽업을 처리해 호출부 일관성을 유지합니다.

→ [`Interface/`](Assets/Scripts/Interface/)

---

### 2. FarmerWorker — 5-State Machine & Claim 시스템

여러 NPC가 같은 당근을 동시에 타격하는 충돌 문제를 Claim으로 해결합니다.

```
Inactive
  ↓ ActivateWorker()
MovingForward ──→ (당근 감지) ──→ Harvesting
    ↑                                  ↓ (수확 완료)
    │                        AdvancingAfterHarvest
    │                                  ↓ (전진 완료)
    └──── Returning ◀──────────── MovingForward
               (endPoint 도달 시)
```

**설계 결정 — Claim:**  
`CarrotNode.TryClaim(this)` / `ReleaseClaim(this)`로 당근을 선점합니다.    
Claim된 당근은 다른 FarmerWorker의 탐색에서 제외되어 동시 타격을 방지합니다.

**설계 결정 — AdvancingAfterHarvest:**  
수확 직후 즉시 같은 자리에서 다음 당근을 잡는 현상을 방지합니다.    
`postHarvestAdvanceDistance`만큼 짧게 강제 전진 후 다시 탐색합니다.

→ [`FieldArea/Worker/FarmerWorker.cs`](Assets/Scripts/FieldArea/Worker/FarmerWorker.cs)

---

### 3. 두 단계 Object Pooling

#### Section A — ItemStack (정적 스택 풀링)

`Instantiate` / `Destroy` 대신 `Queue<GameObject>`로 풀링, GC 부하 없이 빈번한 스택 생성·제거를 처리합니다.

```
TryAdd()     → GetFromPool() → SetActive(true) → visuals[]
TryConsume() → RemoveTopVisual() → ReturnToPool() → pool Queue
```

| 사용처 | 역할 |
|--------|------|
| `PlayerStackController` | 플레이어 머리 위 스택 |
| `SaleDeskController` | 데스크 수프 / 우유 / 돈 |
| `WorkerStackController` | Courier 적재 비주얼 |

#### Section B — ItemVisualEffectPool (포물선 연출)

포물선 이동 연출용 풀로, 연출 완료 후 자동으로 풀에 반환합니다.

```
PlayMoveAnimation() → Get() → MoveRoutine(Coroutine)
    → 포물선 이동 (Sin arc)
    → Return() + onArrived?.Invoke()
```

**설계 결정 — onArrived:**  
연출 완료 후 `onArrived` 콜백으로 실제 데이터를 반영합니다.    
연출 중 데이터가 먼저 반영되면 비주얼과 수치가 어긋나는 문제를 방지합니다.

→ [`Item/ItemStack.cs`](Assets/Scripts/Item/ItemStack.cs) · [`Item/ItemVisualEffectPool.cs`](Assets/Scripts/Item/ItemVisualEffectPool.cs)

---

### 4. Zone 기반 액션 디스패치 & 공통 타이머

`OnTriggerEnter`로 `CurrentZone`을 갱신하고 `ZoneType` switch로 액션을 분기합니다.     
모든 픽업·납품은 공통 타이머를 통해 일정 인터벌을 유지합니다.

```
OnTriggerEnter → CurrentZone 갱신
       ↓
Zone 변경 감지 → pickupTimer / depositTimer 리셋
       ↓
[ IsMounted 체크 ] → true: TryExecuteHarvest() 직접 호출
       ↓ false
ZoneType switch 분기
       ↓
TryPickupItem / TryDepositItem / HandleMoneyDeposit
       ↓
공통 타이머 체크 → 실행
```

**설계 결정 — Vehicle 예외:**  
탑승 중 `PlayerZoneDetector`가 `SeatPoint`로 이동하면서 `OnTriggerExit`가 발생해 `CurrentZone`이 `null`이 됩니다.    
`IsMounted` 플래그로 Zone 감지와 무관하게 수확을 직접 호출해 이 문제를 방지합니다.

**설계 결정 — 타이머 리셋:**  
존이 바뀔 때 공통 타이머를 리셋합니다. 존 전환 직후 첫 액션이 즉시 실행되지 않아 인터벌이 일관되게 유지됩니다.

→ [`Player/Action/PlayerZoneActionHandler.cs`](Assets/Scripts/Player/Action/PlayerZoneActionHandler.cs)

---

### 5. Vehicle 탑승 시스템 — Controllable 추상화

`Controllable` 추상 클래스로 `PlayerMovement`와 `VehicleMovement`를 동일한 인터페이스로 묶습니다.    
`InputDispatcher`가 구체 타입을 모른 채 `SetTarget()`만으로 런타임 입력 대상을 전환합니다.

```
Controllable (abstract)
├── Move(Vector2) / Rotate(Vector2)
├── GetSpeed() / SetActive(bool)
├── PlayerMovement    ← 기본 이동
└── VehicleMovement   ← 탑승 시 이동

InputDispatcher.SetTarget(Controllable) → 런타임 입력 대상 교체
```

새 이동 수단 추가 시 `Controllable`을 상속하는 것만으로 확장됩니다.

→ [`Player/Movement/`](Assets/Scripts/Player/Movement/)

---

### 6. ZoneRevealDirector — 카메라 연출

Zone 해금 시 카메라를 포커스 포인트로 이동한 뒤 플레이어로 복귀하는 연출을 담당합니다. 연출 중 입력을 차단하고 `CameraFollow`의 추적을 일시 정지합니다.

```
PlayReveal()
    → 입력 차단 / CameraFollow.PauseFollow()
    → MoveCamera() → focusPoint 이동
    → revealObject.SetActive(true)
    → holdAfterZoneOpen 대기
    → ReturnToPlayer() → CameraFollow.ResumeFollow()
    → 입력 해제
```

중복 실행 방지를 위해 `isPlaying` 플래그를 사용합니다. 포커스 전용 연출(`PlayFocusOnlyRoutine`)도 외부에서 `yield return`으로 대기할 수 있도록 별도로 제공합니다.

→ [`Zone/Core/ZoneRevealDirector.cs`](Assets/Scripts/Zone/Core/ZoneRevealDirector.cs)

---

<br>

## 링크

YouTube 게임 소개 영상
https://youtu.be/TWybSNSr_Ho

---

*Unity Client Developer · 최지현*
