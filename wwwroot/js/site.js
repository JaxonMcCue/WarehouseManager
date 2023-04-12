const toggleThemeBtn = document.querySelector('#toggle-theme-btn');

// Get the <html> element
const html = document.querySelector('html');

// Listen for a click event on the button
toggleThemeBtn.addEventListener('click', () => {
    // Toggle the data-bs-theme attribute on the <html> element
    if (html.getAttribute('data-bs-theme') === 'light') {
        html.setAttribute('data-bs-theme', 'dark');
        $('#toggle-theme-btn').text('Light Mode');
    } else {
        html.setAttribute('data-bs-theme', 'light');
        $('#toggle-theme-btn').text('Dark Mode');
    }
});