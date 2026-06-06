# Vector

![Vector 예시](/img/attributes/attribute-vector.png)

`[Vector]`는 Vector 프로퍼티의 각 컴포넌트 의미를 명확히 표시할 때 사용합니다.

```shaderlab
[Vector(X, Y, Scale, Bias)] _Params ("Params", Vector) = (0,0,1,0)
```

## 사용 시점

- 하나의 Vector에 여러 숫자 설정을 packed해서 넣을 때
- `x`, `y`, `z`, `w`보다 의미 있는 라벨이 필요할 때
- 커스텀 렌더러와 함께 타입별 UI를 만들 때

## 주의

Vector 값을 직접 수정하는 커스텀 렌더러는 `args.SetVectorValue`를 사용해야 Undo와 애니메이션 녹화가 유지됩니다.
