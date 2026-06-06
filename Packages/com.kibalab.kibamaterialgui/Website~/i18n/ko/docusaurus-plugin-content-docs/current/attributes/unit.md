# Unit

![Unit 예시](/img/attributes/attribute-unit.png)

`[Unit]`은 숫자 프로퍼티 옆에 단위 텍스트를 표시합니다.

```shaderlab
[Unit(m)] _Distance ("Distance", Float) = 1
[Unit(deg)] _Angle ("Angle", Float) = 45
```

값의 의미가 단위 없이는 애매한 경우에 사용하세요.

## 팁

단위는 값을 변환하지 않습니다. 표시 힌트일 뿐이며 쉐이더에 전달되는 값은 그대로 유지됩니다.
