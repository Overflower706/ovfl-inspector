# CLAUDE.md

## 언어

모든 답변은 한국어로 작성한다.

## 패키지 개요

`com.ovfl.inspector`는 Inspector 필드를 조건부로 표시/숨기는 어트리뷰트 패키지.

- 사용 프로젝트: Catverse, Elis_In_Winterland

## 제공 어트리뷰트

| 어트리뷰트 | 동작 |
|-----------|------|
| `[ShowWhen("fieldName", value)]` | 지정한 필드가 value일 때만 Inspector에 표시 |
| `[HideWhen("fieldName", value)]` | 지정한 필드가 value일 때 Inspector에서 숨김 |

### 사용 예시

```csharp
public bool useCustomSpeed;

[ShowWhen("useCustomSpeed", true)]
public float customSpeed;
```

## 테스트

기능 구현 시 적절한 테스트도 함께 구현한다.
