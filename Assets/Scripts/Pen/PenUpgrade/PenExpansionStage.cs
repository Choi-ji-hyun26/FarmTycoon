/*
역할
: 동물 우리 확장 단계 식별
  퀘스트 ACTION 조건에서 특정 확장 단계 완료 여부 판정에 사용
*/

public enum PenExpansionStage
{
    None = 0,
    First = 1,   // 1차 확장
    Second = 2,  // 2차 확장 (게임 클리어 조건)
}
