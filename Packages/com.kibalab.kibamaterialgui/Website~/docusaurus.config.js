const config = {
  title: 'KIBAMaterialGUI',
  tagline: 'Attribute-driven Unity material inspectors',
  url: 'https://kibalab.github.io',
  baseUrl: '/KIBA-MaterialGUI/',
  onBrokenLinks: 'throw',
  markdown: {
    hooks: {
      onBrokenMarkdownLinks: 'warn',
    },
  },
  organizationName: 'kibalab',
  projectName: 'KIBA-MaterialGUI',
  i18n: {
    defaultLocale: 'en',
    locales: ['en', 'ko', 'ja'],
    localeConfigs: {
      en: {
        label: 'English',
      },
      ko: {
        label: '한국어',
      },
      ja: {
        label: '日本語',
      },
    },
  },
  presets: [
    [
      'classic',
      {
        docs: {
          routeBasePath: '/',
          sidebarPath: require.resolve('./sidebars.js'),
        },
        blog: false,
        theme: {
          customCss: require.resolve('./src/css/custom.css'),
        },
      },
    ],
  ],
  themes: [
    [
      require.resolve('@easyops-cn/docusaurus-search-local'),
      {
        hashed: true,
        indexDocs: true,
        indexBlog: false,
        indexPages: true,
        docsRouteBasePath: '/',
        language: ['en', 'ja'],
        highlightSearchTermsOnTargetPage: true,
        explicitSearchResultPath: true,
      },
    ],
  ],
  themeConfig: {
    navbar: {
      title: 'KIBAMaterialGUI',
      items: [
        {
          type: 'docSidebar',
          sidebarId: 'guides',
          position: 'left',
          label: 'Docs',
        },
        {
          href: 'https://github.com/kibalab/KIBA-MaterialGUI',
          label: 'GitHub',
          position: 'right',
        },
        {
          type: 'localeDropdown',
          position: 'right',
        },
      ],
    },
    footer: {
      style: 'dark',
      copyright: `Copyright (c) ${new Date().getFullYear()} KIBA. Released under the MIT License.`,
    },
    prism: {
      additionalLanguages: ['csharp'],
    },
  },
};

module.exports = config;


