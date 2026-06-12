// Auth screen behaviours. Uses event delegation so it keeps working across Blazor
// enhanced navigations (the script is loaded once and the listeners persist).
(function () {
    // --- Show / hide password ---
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('.pw-toggle');
        if (!btn) return;
        e.preventDefault();
        var field = btn.closest('.pw-field');
        var input = field && field.querySelector('input');
        if (!input) return;
        var reveal = input.type === 'password';
        input.type = reveal ? 'text' : 'password';
        btn.classList.toggle('on', reveal);
        btn.setAttribute('aria-label', reveal ? 'Hide password' : 'Show password');
    });

    // --- Refresh CAPTCHA without reloading the page ---
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('.captcha-refresh');
        if (!btn) return;
        e.preventDefault();
        var form = btn.closest('form') || document;
        var img = form.querySelector('#captchaImg');
        var token = form.querySelector('#captchaToken');
        var input = form.querySelector('#captcha');

        btn.classList.add('spinning');
        fetch('/account/captcha', { headers: { 'Accept': 'application/json' } })
            .then(function (r) { return r.json(); })
            .then(function (d) {
                if (img) {
                    img.classList.remove('flash');
                    void img.offsetWidth; // restart the animation
                    img.src = d.image;
                    img.classList.add('flash');
                }
                if (token) token.value = d.token;
                if (input) input.value = '';
            })
            .catch(function () { /* keep current image on error */ })
            .finally(function () { btn.classList.remove('spinning'); });
    });
})();
