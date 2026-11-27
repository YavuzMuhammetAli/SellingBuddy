//light / dark theme kept in local storage
window.statusTheme = {
    key: 'sb-status-theme',

    get: function () {
        try { return localStorage.getItem(this.key) || 'light'; }
        catch (e) { return document.documentElement.getAttribute('data-bs-theme') || 'light'; }
    },

    set: function (theme) {
        document.documentElement.setAttribute('data-bs-theme', theme);
        try { localStorage.setItem(this.key, theme); } catch (e) { }
        return theme;
    },

    toggle: function () {
        return this.set(this.get() === 'dark' ? 'light' : 'dark');
    }
};
