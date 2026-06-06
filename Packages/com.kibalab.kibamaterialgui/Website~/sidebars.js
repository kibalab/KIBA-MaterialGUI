const sidebars = {
  guides: [
    'intro',
    'quick-start',
    {
      type: 'category',
      label: 'Attributes',
      link: {
        type: 'doc',
        id: 'attribute-reference',
      },
      items: [
        'attributes/group',
        'attributes/show-if',
        'attributes/vector',
        'attributes/min-max-slider',
        'attributes/unit',
        'attributes/space',
        'attributes/divider',
        'attributes/gradient-texture',
        'attributes/flexible-range',
        'attributes/segmented-enum',
        'attributes/validate',
        'attributes/unity-built-ins',
      ],
    },
    'inspector-features',
    'presets-and-localization',
    {
      type: 'category',
      label: 'Custom Renderers',
      link: {
        type: 'doc',
        id: 'custom-renderers',
      },
      items: [
        'custom-renderers/first-renderer',
        'custom-renderers/shaderlab-bridge',
        'custom-renderers/matching-and-priority',
        'custom-renderers/typed-attribute-renderers',
        'custom-renderers/value-writes-and-context',
      ],
    },
    {
      type: 'category',
      label: 'Editor Injection',
      link: {
        type: 'doc',
        id: 'editor-injection',
      },
      items: [
        'editor-injection/choosing-extension-point',
        'editor-injection/hook-injection',
        'editor-injection/toolbar-contributions',
        'editor-injection/group-actions-and-menus',
        'editor-injection/diagnostics-and-filters',
      ],
    },
    'scripting-api',
    'troubleshooting',
  ],
};

module.exports = sidebars;
