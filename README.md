# OVFL Inspector

`[ShowWhen]` / `[HideWhen]` 속성으로 Inspector 필드를 조건부로 표시하는 패키지입니다.

## 사용법

### ShowWhen — 조건이 참일 때 표시
```csharp
public bool useCustomSpeed;

[ShowWhen(nameof(useCustomSpeed))]
public float customSpeed;
```

`useCustomSpeed`가 `true`일 때만 `customSpeed` 필드가 Inspector에 표시됩니다.

### HideWhen — 조건이 참일 때 숨김
```csharp
public bool isSimpleMode;

[HideWhen(nameof(isSimpleMode))]
public AnimationCurve advancedCurve;
```

`isSimpleMode`가 `true`이면 `advancedCurve` 필드가 숨겨집니다.

## 설치 방법

### Package Manager (git URL)
1. **Window > Package Manager** 열기
2. 좌상단 **+** → **Add package from git URL...**
3. 아래 URL 입력:
   ```
   https://github.com/Overflower706/inspector.git
   ```

### manifest.json 직접 편집
```json
{
  "dependencies": {
    "com.ovfl.inspector": "https://github.com/Overflower706/inspector.git"
  }
}
```

## 요구사항

- Unity 6000.1 이상
