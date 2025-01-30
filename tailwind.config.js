/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Pages/**/*.{razor,html,cshtml}",
    "./Views/**/*.{razor,html,cshtml}",
    "./Components/**/*.{razor,html,cshtml}",
    "./wwwroot/**/*.js"
  ],
  theme: {
    extend: {},
  },
  plugins: [],
}

