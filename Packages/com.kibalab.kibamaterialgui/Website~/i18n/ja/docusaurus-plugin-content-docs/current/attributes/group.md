# Group

![Group の例](/img/attributes/attribute-group.png)

`[Group]` はプロパティをパスベースのグループに配置します。

```shaderlab
[Group(Surface)] _BaseColor ("Base Color", Color) = (1,1,1,1)
[Group(Surface, Detail)] _DetailNormal ("Detail Normal", 2D) = "bump" {}
```

## 構文

```shaderlab
[Group(Name)]
[Group(Parent, Child)]
```

階層はカンマで分けます。`/` は ShaderLab パーサーで問題になりやすいためサポートしません。
