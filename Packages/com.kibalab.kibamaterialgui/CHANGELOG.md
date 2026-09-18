# Changelog

## 0.1.1 - 2026-09-18

- Correct the minimum supported Unity version to 2022.3.
- Reuse inspector models and shader defaults, index property lookups, and reduce repeated attribute parsing and asset lookups.
- Preserve per-material values and keywords when validation rejects mixed-selection edits.
- Keep group popup search, filters, localization, and presets synchronized.
- Improve gradient resource lifetime, deferred saving, and transient-material persistence.
- Preserve texture None values, UV transforms, sub-assets, and integer properties in presets, including legacy preset import.
- Fix nested Group and signed MinMaxSlider arguments, hidden properties, search expansion, and fallback labels.
- Improve extension diagnostics, warning filters, exception handling, and batched reset Undo.
- Add regression tests and update shader-author and custom-renderer documentation.
