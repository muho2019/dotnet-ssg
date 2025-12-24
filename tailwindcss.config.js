module.exports = {
  content: [
    "./src/DotnetSsg/**/*.{razor,html,cshtml,cs}",
    "./templates/**/*.liquid"
  ],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        notion: {
          bg: '#ffffff',
          'bg-dark': '#191919',
          text: '#37352f',
          'text-dark':  '#ffffff',
          gray: '#787774',
          'gray-light': '#e3e2e0',
          'gray-dark':  '#373737',
          blue: '#0b6bcb',
          red: '#eb5757',
          yellow: '#f7b500',
          green: '#0f7b6c',
        }
      },
      fontFamily: {
        sans: ['Inter', 'ui-sans-serif', 'system-ui', '-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'sans-serif'],
        serif: ['ui-serif', 'Georgia', 'Cambria', 'Times New Roman', 'Times', 'serif'],
      },
      fontSize: {
        'notion-title': ['40px', { lineHeight: '1.2', fontWeight: '700' }],
        'notion-h1': ['30px', { lineHeight: '1.3', fontWeight: '600' }],
        'notion-h2': ['24px', { lineHeight: '1.4', fontWeight: '600' }],
        'notion-h3':  ['20px', { lineHeight:  '1.4', fontWeight: '600' }],
        'notion-body':  ['16px', { lineHeight:  '1.5' }],
      },
      maxWidth: {
        'notion':  '900px',
      }
    },
  },
  plugins: [
    require('@tailwindcss/typography'),
  ],
}