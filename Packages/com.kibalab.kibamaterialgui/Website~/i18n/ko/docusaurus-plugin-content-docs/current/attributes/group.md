# Group

![Group 예시](/img/attributes/attribute-group.png)

`[Group]`은 프로퍼티를 인스펙터의 경로 기반 그룹에 배치합니다.

```shaderlab
[Group(Surface)] _BaseColor ("Base Color", Color) = (1,1,1,1)
[Group(Surface, Detail)] _DetailNormal ("Detail Normal", 2D) = "bump" {}
```

## 문법

```shaderlab
[Group(Name)]
[Group(Parent, Child)]
```

쉼표로 여러 경로 세그먼트를 지정하면 중첩 그룹이 됩니다. `/`는 ShaderLab 파서에서 문제가 될 수 있으므로 지원하지 않습니다.

## 권장 사용법

- 큰 기능 단위는 최상위 그룹으로 나눕니다.
- 자주 쓰는 값은 앞쪽 그룹에 둡니다.
- 조건부 값은 컨트롤러 프로퍼티와 같은 그룹에 두면 이해하기 쉽습니다.
