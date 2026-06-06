# Inspector Features

KIBAMaterialGUI is designed for large shader inspectors. The UI stays close to Unity's material workflow while adding navigation and batch actions.

## Search

The search box matches:

- material property names,
- display labels,
- group paths,
- ShaderLab attribute names.

Condition-hidden properties stay hidden even when they match the search query.

## Filters

The filter dropdown can show subsets of the current visible properties:

- changed values,
- warnings,
- textures,
- numeric properties,
- color properties.

Filters are applied after `[ShowIf]` visibility.

## Groups

Groups can be expanded, collapsed, and reset. A group reset affects the properties currently visible in that group.

Group headers surface useful status:

- visible property count,
- mixed value state,
- diagnostics.

They do not show per-group dirty-count badges.

## Reset

Each property row has a reset button. Reset uses the shader default value and supports Unity undo.

Group reset and search-result reset use the same default-value path.

## Multi-Selection

When multiple materials are selected:

- mixed values use Unity's mixed-value display,
- `[ShowIf]` properties remain visible when at least one selected material satisfies the condition,
- edits apply through Unity material property APIs.

## Animation

KIBAMaterialGUI wraps property rows with Unity material property and animation scopes where Unity exposes them. This restores standard animation recording and preview behavior for built-in and custom renderers as much as Unity allows.

When recording animation, the property context menu can show keyframe actions such as add/update key and remove key.
