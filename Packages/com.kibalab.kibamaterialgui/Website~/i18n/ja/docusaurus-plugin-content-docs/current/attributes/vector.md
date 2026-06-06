# Vector

![Vector の例](/img/attributes/attribute-vector.png)

`[Vector]` は Vector プロパティの各成分に意味のあるラベルを付けます。

```shaderlab
[Vector(X, Y, Scale, Bias)] _Params ("Params", Vector) = (0,0,1,0)
```

packed value を扱うときや、`x/y/z/w` だけでは意味が分かりにくいときに使います。
