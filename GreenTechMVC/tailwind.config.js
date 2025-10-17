module.exports = {
    content: [
        './Pages/**/*.cshtml',
        './Views/**/*.cshtml'
    ],
    theme: {
        extend: {
            fontFamily: {
                'sfu': ['SFU Angie', 'sans-serif'],
                'roboto': ['Roboto', 'sans-serif'],
                'roboto-condensed': ['Roboto Condensed', 'sans-serif'],
            },
        },
        fontFamily: {
            'sans': ['Roboto Condensed', 'ui-sans-serif', 'system-ui', 'sans-serif'],
        }
    },
    plugins: [],
}